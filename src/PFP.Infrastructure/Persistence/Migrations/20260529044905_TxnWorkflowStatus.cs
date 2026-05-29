using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TxnWorkflowStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE fin_transactions
                SET status = N'new'
                WHERE status IN (N'pending', N'completed');

                UPDATE t
                SET status = N'transferred_to_installment'
                FROM fin_transactions t
                INNER JOIN fin_installment_plans p ON p.origin_transaction_id = t.id;

                UPDATE t
                SET
                    status = N'completed',
                    monthly_period_id = p.id
                FROM fin_transactions t
                INNER JOIN fin_monthly_periods p ON p.status = N'closed'
                WHERE t.status <> N'cancelled'
                  AND t.txn_date >= DATEFROMPARTS(p.year, p.month, 1)
                  AND t.txn_date <= EOMONTH(DATEFROMPARTS(p.year, p.month, 1));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE fin_transactions
                SET status = N'completed'
                WHERE status IN (N'new', N'transferred_to_installment', N'completed');

                UPDATE fin_transactions
                SET status = N'pending'
                WHERE status = N'new';
                """);
        }
    }
}
