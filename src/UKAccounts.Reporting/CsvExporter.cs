using System.Globalization;
using UKAccounts.Application.DTOs;

namespace UKAccounts.Reporting;

public static class CsvExporter
{
    public static byte[] ExportTrialBalance(TrialBalanceReport report)
    {
        var lines = new List<string>
        {
            "Code,Account Name,Category,Debit,Credit"
        };

        foreach (var line in report.Lines)
        {
            lines.Add($"{line.AccountCode},{line.AccountName},{line.Category},{line.Debit.ToString("N2", CultureInfo.InvariantCulture)},{line.Credit.ToString("N2", CultureInfo.InvariantCulture)}");
        }

        return System.Text.Encoding.UTF8.GetBytes(string.Join("\n", lines));
    }

    public static byte[] ExportProfitLoss(ProfitLossReport report)
    {
        var lines = new List<string>
        {
            "Section,Account Code,Account Name,Category,Amount"
        };

        foreach (var section in report.Sections)
        {
            foreach (var line in section.Lines)
            {
                lines.Add($"{section.Title},{line.AccountCode},{line.AccountName},{line.Category},{line.Amount.ToString("N2", CultureInfo.InvariantCulture)}");
            }
        }

        return System.Text.Encoding.UTF8.GetBytes(string.Join("\n", lines));
    }

    public static byte[] ExportBalanceSheet(BalanceSheetReport report)
    {
        var lines = new List<string>
        {
            "Section,Account Code,Account Name,Amount"
        };

        foreach (var section in report.Sections)
        {
            foreach (var line in section.Lines)
            {
                lines.Add($"{section.Title},{line.AccountCode},{line.AccountName},{line.Amount.ToString("N2", CultureInfo.InvariantCulture)}");
            }
        }

        return System.Text.Encoding.UTF8.GetBytes(string.Join("\n", lines));
    }

    public static byte[] ExportGeneralLedger(GeneralLedgerReport report)
    {
        var lines = new List<string>
        {
            "Date,Reference,Description,Debit,Credit"
        };

        foreach (var entry in report.Entries)
        {
            lines.Add($"{entry.Date:yyyy-MM-dd},{entry.JournalReference},{entry.Description},{entry.Debit.ToString("N2", CultureInfo.InvariantCulture)},{entry.Credit.ToString("N2", CultureInfo.InvariantCulture)}");
        }

        return System.Text.Encoding.UTF8.GetBytes(string.Join("\n", lines));
    }

    public static byte[] ExportAgedReceivables(AgedReceivablesReport report)
    {
        var lines = new List<string>
        {
            "Invoice Number,Customer Name,Invoice Date,Due Date,Current,1-30 Days,31-60 Days,61-90 Days,Over 90 Days,Total"
        };

        foreach (var line in report.Lines)
        {
            lines.Add($"{line.InvoiceNumber},{line.CustomerName},{line.InvoiceDate:yyyy-MM-dd},{line.DueDate:yyyy-MM-dd},{line.Current:N2},{line.Days1to30:N2},{line.Days31to60:N2},{line.Days61to90:N2},{line.Over90:N2},{line.Total:N2}");
        }

        return System.Text.Encoding.UTF8.GetBytes(string.Join("\n", lines));
    }

    public static byte[] ExportAgedPayables(AgedPayablesReport report)
    {
        var lines = new List<string>
        {
            "Bill Number,Supplier Name,Bill Date,Due Date,Current,1-30 Days,31-60 Days,61-90 Days,Over 90 Days,Total"
        };

        foreach (var line in report.Lines)
        {
            lines.Add($"{line.BillNumber},{line.SupplierName},{line.BillDate:yyyy-MM-dd},{line.DueDate:yyyy-MM-dd},{line.Current:N2},{line.Days1to30:N2},{line.Days31to60:N2},{line.Days61to90:N2},{line.Over90:N2},{line.Total:N2}");
        }

        return System.Text.Encoding.UTF8.GetBytes(string.Join("\n", lines));
    }

    public static byte[] ExportCashFlow(CashFlowReport report)
    {
        var lines = new List<string>
        {
            "Section,Description,Amount"
        };

        foreach (var section in report.Sections)
        {
            foreach (var line in section.Lines)
            {
                lines.Add($"{section.Title},{line.Description},{line.Amount.ToString("N2", CultureInfo.InvariantCulture)}");
            }
        }

        return System.Text.Encoding.UTF8.GetBytes(string.Join("\n", lines));
    }
}
