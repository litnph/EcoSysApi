using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.Persistence;

/// <summary>
/// Minh họa tài chính cá nhân: Minh Nguyễn, 28 tuổi, nhân viên IT tại TP.HCM (tháng 5/2026).
/// Bật <c>Seed:DemoFinance</c>; dùng <c>Seed:DemoFinance:Reset</c> để xóa dữ liệu cũ trước khi seed lại.
/// </summary>
public static class DemoFinanceSeeder
{
    private const string DemoMarker = "demo-ecosys";

    public static async Task EnsureAsync(AppDbContext db, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (!configuration.GetValue("Seed:DemoFinance", false))
            return;

        var reset = configuration.GetValue("Seed:DemoFinance:Reset", false);
        if (reset)
        {
            await DemoFinanceDataReset.ClearAllFinanceDataAsync(db, cancellationToken).ConfigureAwait(false);
        }
        else if (await db.FinCategories.AnyAsync(c => c.Code == DemoMarker, cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var utcNow = DateTime.UtcNow;
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var prevMonth = today.AddMonths(-1);
        var prevMonthStart = new DateOnly(prevMonth.Year, prevMonth.Month, 1);
        var twoMonthsAgoStart = prevMonthStart.AddMonths(-1);

        // Cập nhật tên admin cho đúng persona demo
        var admin = await db.Users.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (admin is not null)
        {
            admin.FullName = "Minh Nguyễn";
            var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == admin.Id, cancellationToken).ConfigureAwait(false);
            if (profile is not null)
                profile.LanguageCode = "vi";
        }

        // ---- Danh mục ----
        var catFood = new FinCategory { Name = "Ăn uống", Code = DemoMarker, Kind = CategoryKind.Expense, Depth = 0, SortOrder = 1, IsDefault = true };
        var catTransport = new FinCategory { Name = "Di chuyển", Code = "demo-transport", Kind = CategoryKind.Expense, Depth = 0, SortOrder = 2 };
        var catBills = new FinCategory { Name = "Hóa đơn", Code = "demo-bills", Kind = CategoryKind.Expense, Depth = 0, SortOrder = 3 };
        var catFun = new FinCategory { Name = "Giải trí", Code = "demo-fun", Kind = CategoryKind.Expense, Depth = 0, SortOrder = 4 };
        var catShop = new FinCategory { Name = "Mua sắm", Code = "demo-shop", Kind = CategoryKind.Expense, Depth = 0, SortOrder = 5 };
        var catSalary = new FinCategory { Name = "Lương", Code = "demo-salary", Kind = CategoryKind.Income, Depth = 0, SortOrder = 1, IsDefault = true };
        var catFreelance = new FinCategory { Name = "Freelance", Code = "demo-freelance", Kind = CategoryKind.Income, Depth = 0, SortOrder = 2 };
        db.FinCategories.AddRange(catFood, catTransport, catBills, catFun, catShop, catSalary, catFreelance);

        // ---- Nguồn tiền (số dư khớp kịch bản tháng 5) ----
        var bank = new FinSource
        {
            Name = "VCB — Lương",
            Type = SourceType.BankAccount,
            Balance = 42_300_000,
            Currency = "VND",
            SortOrder = 1,
        };
        var eWallet = new FinSource
        {
            Name = "MoMo",
            Type = SourceType.EWallet,
            Balance = 1_250_000,
            Currency = "VND",
            SortOrder = 2,
        };
        var cash = new FinSource
        {
            Name = "Ví tiền mặt",
            Type = SourceType.Cash,
            Balance = 850_000,
            Currency = "VND",
            SortOrder = 3,
        };
        var creditCard = new FinSource
        {
            Name = "Thẻ TC Visa",
            Type = SourceType.CreditCard,
            Balance = 8_200_000,
            Currency = "VND",
            CreditLimit = 80_000_000,
            StatementDay = 15,
            PaymentDueDay = 25,
            MinInstallmentAmt = 3_000_000,
            SortOrder = 4,
        };
        db.FinSources.AddRange(bank, eWallet, cash, creditCard);

        var currentPeriod = new FinMonthlyPeriod
        {
            Year = today.Year,
            Month = today.Month,
            TotalIncome = 37_500_000,
            TotalExpense = 12_175_000,
            Net = 25_325_000,
            Status = PeriodStatus.Open,
        };
        var closedPeriod = new FinMonthlyPeriod
        {
            Year = prevMonth.Year,
            Month = prevMonth.Month,
            TotalIncome = 35_000_000,
            TotalExpense = 28_400_000,
            Net = 6_600_000,
            Status = PeriodStatus.Closed,
            ClosedAt = utcNow.AddDays(-12),
        };
        db.FinMonthlyPeriods.AddRange(currentPeriod, closedPeriod);

        // Kỳ sao kê thẻ: đang mở / đã đóng chờ trả / đã trả hết
        var openCycle = new FinBillingCycle
        {
            SourceId = creditCard.Id,
            PeriodStart = monthStart,
            PeriodEnd = today,
            StatementDate = DayInMonth(today.Year, today.Month, 15),
            PaymentDueDate = DayInMonth(today.Year, today.Month, 15).AddDays(25),
            TotalAmount = 8_200_000,
            PaidAmount = 0,
            Status = BillingCycleStatus.Open,
        };
        var closedCycle = new FinBillingCycle
        {
            SourceId = creditCard.Id,
            PeriodStart = prevMonthStart,
            PeriodEnd = monthStart.AddDays(-1),
            StatementDate = monthStart.AddDays(14),
            PaymentDueDate = monthStart.AddDays(39),
            TotalAmount = 3_500_000,
            PaidAmount = 0,
            Status = BillingCycleStatus.Closed,
            ClosedAt = utcNow.AddDays(-3),
        };
        var paidCycle = new FinBillingCycle
        {
            SourceId = creditCard.Id,
            PeriodStart = twoMonthsAgoStart,
            PeriodEnd = prevMonthStart.AddDays(-1),
            StatementDate = prevMonthStart.AddDays(14),
            PaymentDueDate = prevMonthStart.AddDays(39),
            TotalAmount = 4_100_000,
            PaidAmount = 4_100_000,
            Status = BillingCycleStatus.Paid,
            ClosedAt = utcNow.AddDays(-45),
            PaidAt = utcNow.AddDays(-40),
        };
        db.FinBillingCycles.AddRange(openCycle, closedCycle, paidCycle);

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // ---- Giao dịch tháng hiện tại ----
        var txnSalary = new FinTransaction
        {
            Type = TransactionType.Income,
            Status = TxnStatus.Completed,
            Amount = 35_000_000,
            Currency = "VND",
            TxnDate = monthStart.AddDays(4),
            SourceId = bank.Id,
            CategoryId = catSalary.Id,
            MonthlyPeriodId = currentPeriod.Id,
            Description = "Income: Lương",
            Note = "Lương tháng 5 — Công ty ABC Tech",
        };

        var txnFreelance = new FinTransaction
        {
            Type = TransactionType.Income,
            Status = TxnStatus.Completed,
            Amount = 2_500_000,
            Currency = "VND",
            TxnDate = today.AddDays(-8),
            SourceId = bank.Id,
            CategoryId = catFreelance.Id,
            MonthlyPeriodId = currentPeriod.Id,
            Description = "Income: Freelance",
            Note = "Thiết kế landing page — khách US",
        };

        var txnGrab = new FinTransaction
        {
            Type = TransactionType.Direct,
            Status = TxnStatus.Completed,
            Amount = 45_000,
            Currency = "VND",
            TxnDate = today.AddDays(-6),
            SourceId = eWallet.Id,
            CategoryId = catTransport.Id,
            MonthlyPeriodId = currentPeriod.Id,
            Description = "Expense: Di chuyển",
            Note = "Grab đi làm",
        };

        var txnElectric = new FinTransaction
        {
            Type = TransactionType.Direct,
            Status = TxnStatus.Completed,
            Amount = 680_000,
            Currency = "VND",
            TxnDate = today.AddDays(-5),
            SourceId = bank.Id,
            CategoryId = catBills.Id,
            MonthlyPeriodId = currentPeriod.Id,
            Description = "Expense: Hóa đơn",
            Note = "Tiền điện EVN tháng 4",
        };

        var txnNetflix = new FinTransaction
        {
            Type = TransactionType.Direct,
            Status = TxnStatus.Completed,
            Amount = 260_000,
            Currency = "VND",
            TxnDate = today.AddDays(-4),
            SourceId = creditCard.Id,
            CategoryId = catFun.Id,
            MonthlyPeriodId = currentPeriod.Id,
            Description = "Expense: Giải trí",
            Note = "Netflix + Spotify",
        };

        var txnHeadphone = new FinTransaction
        {
            Type = TransactionType.Deferred,
            Status = TxnStatus.Completed,
            Amount = 2_400_000,
            Currency = "VND",
            TxnDate = today.AddDays(-2),
            SourceId = creditCard.Id,
            CategoryId = catShop.Id,
            BillingCycleId = openCycle.Id,
            MonthlyPeriodId = currentPeriod.Id,
            Description = "Expense: Mua sắm",
            Note = "Tai nghe Sony WH-1000XM5 — Shopee, trả góp 0%",
        };

        var txnShopeeSmall = new FinTransaction
        {
            Type = TransactionType.Deferred,
            Status = TxnStatus.Completed,
            Amount = 890_000,
            Currency = "VND",
            TxnDate = today.AddDays(-10),
            SourceId = creditCard.Id,
            CategoryId = catShop.Id,
            BillingCycleId = openCycle.Id,
            MonthlyPeriodId = currentPeriod.Id,
            Description = "Expense: Mua sắm",
            Note = "Phụ kiện laptop",
        };

        var txnTeamLunch = new FinTransaction
        {
            Type = TransactionType.Split,
            Status = TxnStatus.Completed,
            Amount = 720_000,
            Currency = "VND",
            TxnDate = today.AddDays(-7),
            SourceId = bank.Id,
            CategoryId = catFood.Id,
            MonthlyPeriodId = currentPeriod.Id,
            Description = "Expense: Ăn uống",
            Note = "Tiệc team cuối tuần — Minh trả trước",
        };

        var txnBorrow = new FinTransaction
        {
            Type = TransactionType.DebtBorrow,
            Status = TxnStatus.Completed,
            Amount = 2_000_000,
            Currency = "VND",
            TxnDate = today.AddDays(-14),
            SourceId = bank.Id,
            MonthlyPeriodId = currentPeriod.Id,
            Description = "Borrowed: Trung IT",
            Note = "Vay tạm chờ lương",
        };

        var txnLoanGive = new FinTransaction
        {
            Type = TransactionType.LoanGive,
            Status = TxnStatus.Completed,
            Amount = 1_500_000,
            Currency = "VND",
            TxnDate = today.AddDays(-12),
            SourceId = bank.Id,
            MonthlyPeriodId = currentPeriod.Id,
            Description = "Loan given: Hương",
            Note = "Ứng tiền đặt cọc thuê nhà",
        };

        var txnCoffee = new FinTransaction
        {
            Type = TransactionType.Direct,
            Status = TxnStatus.Completed,
            Amount = 55_000,
            Currency = "VND",
            TxnDate = today,
            SourceId = cash.Id,
            CategoryId = catFood.Id,
            MonthlyPeriodId = currentPeriod.Id,
            Description = "Expense: Ăn uống",
            Note = "Cà phê sáng",
        };

        db.FinTransactions.AddRange(
            txnSalary, txnFreelance, txnGrab, txnElectric, txnNetflix,
            txnHeadphone, txnShopeeSmall, txnTeamLunch, txnBorrow, txnLoanGive, txnCoffee);

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var txnTransferOut = new FinTransaction
        {
            Type = TransactionType.Transfer,
            Status = TxnStatus.Completed,
            Amount = -2_000_000,
            Currency = "VND",
            TxnDate = today.AddDays(-3),
            SourceId = bank.Id,
            DestSourceId = eWallet.Id,
            MonthlyPeriodId = currentPeriod.Id,
            Description = "Transfer: VCB — Lương → MoMo",
            Note = "Nạp ví chi tiêu tháng",
        };
        var txnTransferIn = new FinTransaction
        {
            Type = TransactionType.Transfer,
            Status = TxnStatus.Completed,
            Amount = 2_000_000,
            Currency = "VND",
            TxnDate = today.AddDays(-3),
            SourceId = eWallet.Id,
            DestSourceId = bank.Id,
            MonthlyPeriodId = currentPeriod.Id,
            Description = "Transfer: VCB — Lương → MoMo",
        };
        db.FinTransactions.AddRange(txnTransferOut, txnTransferIn);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        txnTransferOut.RefTxnId = txnTransferIn.Id;
        txnTransferIn.RefTxnId = txnTransferOut.Id;
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var txnSplitRefund = new FinTransaction
        {
            Type = TransactionType.Income,
            Status = TxnStatus.Completed,
            Amount = 240_000,
            Currency = "VND",
            TxnDate = today.AddDays(-5),
            SourceId = eWallet.Id,
            CategoryId = catFreelance.Id,
            Description = "Income: Freelance",
            Note = "Hoàn tiền split từ Tuấn",
        };
        db.FinTransactions.Add(txnSplitRefund);

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        db.FinTxnSplits.AddRange(
            new FinTxnSplit
            {
                TransactionId = txnTeamLunch.Id,
                PersonName = "Tuấn (Dev)",
                PersonContact = "0903123456",
                Amount = 240_000,
                Status = SplitStatus.Settled,
                SettledAt = utcNow.AddDays(-5),
                SettledTxnId = txnSplitRefund.Id,
            },
            new FinTxnSplit
            {
                TransactionId = txnTeamLunch.Id,
                PersonName = "Linh (Design)",
                Amount = 240_000,
                Status = SplitStatus.Pending,
            },
            new FinTxnSplit
            {
                TransactionId = txnTeamLunch.Id,
                PersonName = "Huy (QA)",
                Amount = 240_000,
                Status = SplitStatus.Pending,
            });

        db.FinDebtRecords.AddRange(
            new FinDebtRecord
            {
                Direction = DebtDirection.Borrowed,
                PersonName = "Trung IT",
                PersonContact = "0918765432",
                OriginalTxnId = txnBorrow.Id,
                OriginalAmount = 2_000_000,
                RemainingAmount = 1_500_000,
                Currency = "VND",
                DueDate = today.AddDays(10),
                Status = DebtStatus.Active,
                Note = "Đã trả 500k ngày 10/5",
            },
            new FinDebtRecord
            {
                Direction = DebtDirection.Lent,
                PersonName = "Hương",
                PersonContact = "0988111222",
                OriginalTxnId = txnLoanGive.Id,
                OriginalAmount = 1_500_000,
                RemainingAmount = 1_000_000,
                Currency = "VND",
                Status = DebtStatus.Active,
                Note = "Chờ thu hồi khi nhận lại cọc",
            },
            new FinDebtRecord
            {
                Direction = DebtDirection.Borrowed,
                PersonName = "An (bạn học)",
                OriginalAmount = 800_000,
                RemainingAmount = 0,
                Currency = "VND",
                Status = DebtStatus.Completed,
                Note = "Vay mua sách — đã trả hết tháng 3",
            });

        var installmentPlan = new FinInstallmentPlan
        {
            OriginalTxnId = txnHeadphone.Id,
            SourceId = creditCard.Id,
            TotalAmount = 2_400_000,
            TotalMonths = 6,
            MonthlyAmount = 400_000,
            InterestRate = 0,
            ConversionFeeRate = 1.5m,
            ConversionFeeAmount = 36_000,
            ConversionFeeStatus = ConversionFeeStatus.Pending,
            StartDate = today.AddDays(-2),
            Status = InstallmentStatus.Active,
        };
        db.FinInstallmentPlans.Add(installmentPlan);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        db.FinInstallmentPays.AddRange(
            new FinInstallmentPay
            {
                PlanId = installmentPlan.Id,
                InstallmentNumber = 1,
                DueDate = today.AddDays(28),
                Amount = 400_000,
                Status = InstallmentPayStatus.Due,
            },
            new FinInstallmentPay
            {
                PlanId = installmentPlan.Id,
                InstallmentNumber = 2,
                DueDate = today.AddMonths(1).AddDays(28),
                Amount = 400_000,
                Status = InstallmentPayStatus.Upcoming,
            },
            new FinInstallmentPay
            {
                PlanId = installmentPlan.Id,
                InstallmentNumber = 3,
                DueDate = today.AddMonths(2).AddDays(28),
                Amount = 400_000,
                Status = InstallmentPayStatus.Upcoming,
            },
            new FinInstallmentPay
            {
                PlanId = installmentPlan.Id,
                InstallmentNumber = 4,
                DueDate = today.AddMonths(3).AddDays(28),
                Amount = 400_000,
                Status = InstallmentPayStatus.Upcoming,
            },
            new FinInstallmentPay
            {
                PlanId = installmentPlan.Id,
                InstallmentNumber = 5,
                DueDate = today.AddMonths(4).AddDays(28),
                Amount = 400_000,
                Status = InstallmentPayStatus.Upcoming,
            },
            new FinInstallmentPay
            {
                PlanId = installmentPlan.Id,
                InstallmentNumber = 6,
                DueDate = today.AddMonths(5).AddDays(28),
                Amount = 400_000,
                Status = InstallmentPayStatus.Upcoming,
            });

        db.FinSavings.AddRange(
            new FinSaving
            {
                SourceId = bank.Id,
                Name = "Quỹ khẩn cấp 6 tháng",
                TargetAmount = 50_000_000,
                CurrentAmount = 18_000_000,
                InterestRate = 4.8m,
                StartDate = today.AddMonths(-8),
                Type = SavingType.Flexible,
                Status = SavingStatus.Active,
                Note = "Mục tiêu 6 tháng chi phí sinh hoạt",
            },
            new FinSaving
            {
                SourceId = bank.Id,
                Name = "Tiết kiệm kỳ hạn 12T",
                TargetAmount = 20_000_000,
                CurrentAmount = 20_000_000,
                InterestRate = 6.5m,
                StartDate = today.AddYears(-1),
                MaturityDate = today.AddDays(-5),
                Type = SavingType.FixedTerm,
                Status = SavingStatus.Matured,
                Note = "Đáo hạn — có thể rút về VCB",
            });

        var investment = new FinInvestment
        {
            Name = "CCQ FUEVFVND",
            Type = InvestmentType.Fund,
            Currency = "VND",
            CurrentValue = 11_200_000,
            TotalInvested = 10_000_000,
            TotalReturned = 0,
            Note = "DCA 2 triệu/tháng từ tháng 1",
        };
        db.FinInvestments.Add(investment);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        db.FinInvestmentTxns.AddRange(
            new FinInvestmentTxn
            {
                InvestmentId = investment.Id,
                TxnType = InvestmentTxnType.Buy,
                Amount = 6_000_000,
                Quantity = 60,
                PricePerUnit = 100_000,
                TxnDate = today.AddMonths(-4),
                Note = "Mua đợt 1",
            },
            new FinInvestmentTxn
            {
                InvestmentId = investment.Id,
                TxnType = InvestmentTxnType.Buy,
                Amount = 4_000_000,
                Quantity = 38,
                PricePerUnit = 105_263,
                TxnDate = today.AddMonths(-2),
                Note = "Mua đợt 2",
            });

        var tagWork = new Tag { Name = "Công việc", Color = "#2563EB", UsageCount = 0 };
        var tagFamily = new Tag { Name = "Gia đình", Color = "#DC2626", UsageCount = 0 };
        db.Tags.AddRange(tagWork, tagFamily);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (admin is not null)
        {
            db.EntityTags.AddRange(
                new EntityTag
                {
                    TagId = tagWork.Id,
                    EntityType = nameof(FinTransaction),
                    EntityId = txnElectric.Id,
                    TaggedBy = admin.Id,
                },
                new EntityTag
                {
                    TagId = tagFamily.Id,
                    EntityType = nameof(FinTransaction),
                    EntityId = txnTeamLunch.Id,
                    TaggedBy = admin.Id,
                });
            tagWork.UsageCount = 1;
            tagFamily.UsageCount = 1;
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static DateOnly DayInMonth(int year, int month, int dayOfMonth)
    {
        var last = DateTime.DaysInMonth(year, month);
        var day = dayOfMonth > last ? last : dayOfMonth;
        return new DateOnly(year, month, day);
    }
}
