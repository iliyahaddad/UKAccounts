using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UKAccounts.Application.DTOs;

namespace UKAccounts.Reporting;

public static class PdfExporter
{
    public static byte[] ExportTrialBalance(TrialBalanceReport report)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A4);
                page.PageTitle("Trial Balance");
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Code");
                        header.Cell().Element(CellStyle).Text("Account Name");
                        header.Cell().Element(CellStyle).AlignRight().Text("Debit");
                        header.Cell().Element(CellStyle).AlignRight().Text("Credit");
                    });

                    foreach (var line in report.Lines)
                    {
                        table.Cell().Element(CellStyle).Text(line.AccountCode);
                        table.Cell().Element(CellStyle).Text(line.AccountName);
                        table.Cell().Element(CellStyle).AlignRight().Text(line.Debit.ToString("N2"));
                        table.Cell().Element(CellStyle).AlignRight().Text(line.Credit.ToString("N2"));
                    }
                });
            });
        });

        return document.GeneratePdf();
    }

    public static byte[] ExportProfitLoss(ProfitLossReport report)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A4);
                page.PageTitle("Profit and Loss");
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Account");
                        header.Cell().Element(CellStyle).AlignRight().Text("Amount");
                    });

                    foreach (var section in report.Sections)
                    {
                        table.Cell().ColumnSpan(2).Background(Colors.Grey.Lighten3).Padding(5).Text(section.Title).Bold();
                        foreach (var line in section.Lines)
                        {
                            table.Cell().Element(CellStyle).Text($"{line.AccountCode} - {line.AccountName}");
                            table.Cell().Element(CellStyle).AlignRight().Text(line.Amount.ToString("N2"));
                        }
                    }
                });
            });
        });

        return document.GeneratePdf();
    }

    public static byte[] ExportBalanceSheet(BalanceSheetReport report)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A4);
                page.PageTitle("Balance Sheet");
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Account");
                        header.Cell().Element(CellStyle).AlignRight().Text("Amount");
                    });

                    foreach (var section in report.Sections)
                    {
                        table.Cell().ColumnSpan(2).Background(Colors.Grey.Lighten3).Padding(5).Text(section.Title).Bold();
                        foreach (var line in section.Lines)
                        {
                            table.Cell().Element(CellStyle).Text($"{line.AccountCode} - {line.AccountName}");
                            table.Cell().Element(CellStyle).AlignRight().Text(line.Amount.ToString("N2"));
                        }
                    }
                });
            });
        });

        return document.GeneratePdf();
    }

    public static byte[] ExportCashFlow(CashFlowReport report)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A4);
                page.PageTitle("Cash Flow Statement");
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Description");
                        header.Cell().Element(CellStyle).AlignRight().Text("Amount");
                    });

                    foreach (var section in report.Sections)
                    {
                        table.Cell().ColumnSpan(2).Background(Colors.Grey.Lighten3).Padding(5).Text(section.Title).Bold();
                        foreach (var line in section.Lines)
                        {
                            table.Cell().Element(CellStyle).Text(line.Description);
                            table.Cell().Element(CellStyle).AlignRight().Text(line.Amount.ToString("N2"));
                        }
                    }
                });
            });
        });

        return document.GeneratePdf();
    }

    private static IContainer CellStyle(IContainer container) => container.PaddingVertical(2).PaddingHorizontal(5);
}
