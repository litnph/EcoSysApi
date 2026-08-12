using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RepairInstallmentSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int?>(
                name: "schedule_version",
                table: "fin_installment_plans",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly?>(
                name: "statement_date",
                table: "fin_installment_pays",
                type: "date",
                nullable: true);

            // Schedule v1's exact fingerprint: past rows were marked paid at 12:00 UTC on the
            // nominal due date without a transaction. Reset only those inferred payments.
            migrationBuilder.Sql(
                """
                UPDATE fin_installment_pays
                SET status = 'upcoming',
                    paid_amount = 0,
                    paid_at = NULL
                WHERE status = 'paid'
                  AND transaction_id IS NULL
                  AND paid_at = (due_date::timestamp + interval '12 hours') AT TIME ZONE 'UTC';
                """);

            // Schedule v2 stores both the statement date and the actual deadline. Purchases made
            // on a statement date enter the following cycle. Day 29-31 is clamped independently
            // in each month, and the configured payment offset is applied after that.
            migrationBuilder.Sql(
                """
                WITH schedule_rows AS (
                    SELECT pay.id,
                           corrected.statement_date,
                           (
                               corrected.statement_date
                               + make_interval(days => source.payment_due_day)
                           )::date AS due_date
                    FROM fin_installment_pays AS pay
                    JOIN fin_installment_plans AS plan
                      ON plan.id = pay.installment_plan_id
                    JOIN fin_transactions AS txn
                      ON txn.id = plan.origin_transaction_id
                    JOIN fin_sources AS source
                      ON source.id = plan.source_id
                    CROSS JOIN LATERAL (
                        SELECT date_trunc('month', txn.txn_date)::date AS txn_month
                    ) AS transaction_month
                    CROSS JOIN LATERAL (
                        SELECT (
                            transaction_month.txn_month
                            + make_interval(
                                days => LEAST(
                                    source.statement_day,
                                    EXTRACT(
                                        DAY FROM (
                                            transaction_month.txn_month
                                            + interval '1 month'
                                            - interval '1 day'
                                        )
                                    )::integer
                                ) - 1
                            )
                        )::date AS candidate_statement
                    ) AS candidate
                    CROSS JOIN LATERAL (
                        SELECT CASE
                            WHEN txn.txn_date < candidate.candidate_statement
                                THEN transaction_month.txn_month
                            ELSE (transaction_month.txn_month + interval '1 month')::date
                        END AS first_statement_month
                    ) AS first_statement
                    CROSS JOIN LATERAL (
                        SELECT (
                            first_statement.first_statement_month
                            + make_interval(months => pay.installment_number - 1)
                        )::date AS target_month
                    ) AS target
                    CROSS JOIN LATERAL (
                        SELECT (
                            target.target_month
                            + make_interval(
                                days => LEAST(
                                    source.statement_day,
                                    EXTRACT(
                                        DAY FROM (
                                            target.target_month
                                            + interval '1 month'
                                            - interval '1 day'
                                        )
                                    )::integer
                                ) - 1
                            )
                        )::date AS statement_date
                    ) AS corrected
                    WHERE source.type = 'credit_card'
                      AND source.statement_day BETWEEN 1 AND 31
                      AND source.payment_due_day BETWEEN 1 AND 60
                )
                UPDATE fin_installment_pays AS pay
                SET statement_date = schedule.statement_date,
                    due_date = schedule.due_date
                FROM schedule_rows AS schedule
                WHERE pay.id = schedule.id;
                """);

            migrationBuilder.Sql(
                """
                UPDATE fin_installment_plans AS plan
                SET status = 'active'
                WHERE plan.status = 'completed'
                  AND EXISTS (
                      SELECT 1
                      FROM fin_installment_pays AS pay
                      WHERE pay.installment_plan_id = plan.id
                        AND pay.status <> 'paid'
                  );

                UPDATE fin_installment_pays AS pay
                SET status = CASE
                    WHEN pay.due_date < (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Bangkok')::date
                        THEN 'overdue'
                    WHEN pay.due_date = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Bangkok')::date
                        THEN 'due'
                    ELSE 'upcoming'
                END
                FROM fin_installment_plans AS plan
                WHERE plan.id = pay.installment_plan_id
                  AND plan.status = 'active'
                  AND pay.status <> 'paid';

                UPDATE fin_installment_plans AS plan
                SET schedule_version = 2
                FROM fin_sources AS source
                WHERE source.id = plan.source_id
                  AND source.type = 'credit_card'
                  AND source.statement_day BETWEEN 1 AND 31
                  AND source.payment_due_day BETWEEN 1 AND 60;

                UPDATE fin_installment_pays
                SET statement_date = due_date
                WHERE statement_date IS NULL;

                UPDATE fin_installment_plans
                SET schedule_version = 1
                WHERE schedule_version IS NULL;
                """);

            // Rebuild card balances from auditable facts. A direct installment payment has a
            // linked transaction; a statement payment is already represented by cycle.paid_amount.
            migrationBuilder.Sql(
                """
                UPDATE fin_sources AS source
                SET balance = GREATEST(
                    0,
                    COALESCE((
                        SELECT SUM(
                            CASE txn.type
                                WHEN 'deferred' THEN txn.amount
                                WHEN 'direct' THEN txn.amount
                                WHEN 'transfer' THEN txn.amount
                                WHEN 'income' THEN -txn.amount
                                WHEN 'balance_adjustment' THEN txn.amount
                                ELSE 0
                            END
                        )
                        FROM fin_transactions AS txn
                        WHERE txn.source_id = source.id
                          AND txn.status <> 'cancelled'
                          AND txn.type <> 'reversal'
                          AND NOT txn.is_deleted
                    ), 0)
                    - COALESCE((
                        SELECT SUM(cycle.paid_amount)
                        FROM fin_billing_cycles AS cycle
                        WHERE cycle.source_id = source.id
                    ), 0)
                    - COALESCE((
                        SELECT SUM(pay.paid_amount)
                        FROM fin_installment_plans AS plan
                        JOIN fin_installment_pays AS pay
                          ON pay.installment_plan_id = plan.id
                        JOIN fin_transactions AS payment_txn
                          ON payment_txn.id = pay.transaction_id
                        WHERE plan.source_id = source.id
                          AND plan.status <> 'cancelled'
                          AND pay.status = 'paid'
                          AND payment_txn.purpose = 'installment_payment'
                    ), 0)
                )
                WHERE source.type = 'credit_card';
                """);

            migrationBuilder.AlterColumn<int>(
                name: "schedule_version",
                table: "fin_installment_plans",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "statement_date",
                table: "fin_installment_pays",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Do not recreate inferred payments or stale balances on rollback. Only schema added by
            // this migration is removed; the corrected financial evidence is intentionally kept.
            migrationBuilder.DropColumn(
                name: "schedule_version",
                table: "fin_installment_plans");

            migrationBuilder.DropColumn(
                name: "statement_date",
                table: "fin_installment_pays");
        }
    }
}
