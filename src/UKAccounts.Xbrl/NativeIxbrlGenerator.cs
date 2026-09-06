using System.Xml;
using UKAccounts.Application.DTOs;
using UKAccounts.Domain.Entities;

namespace UKAccounts.Xbrl;

public class NativeIxbrlGenerator : IIxbrlGenerator
{
    public Task<GenerationResult> GenerateAsync(IxbrlRequest request, CancellationToken cancellationToken = default)
    {
        var result = new GenerationResult();

        try
        {
            var ixbrl = new IXbrlDocument();
            ixbrl.AddNamespace("html", "http://www.w3.org/1999/xhtml");
            ixbrl.AddNamespace("ix", "http://www.xbrl.org/2013/inlineXBRL");
            ixbrl.AddNamespace("uk-gaap", "http://www.xbrl.org/uk/gaap/core/2024-01-01");
            ixbrl.AddNamespace("xbrli", "http://www.xbrl.org/2003/instance");
            ixbrl.AddNamespace("iso", "http://www.iso.org/2001/XMLSchema");

            var contextId = "ctx1";
            var unitId = "GBP";

            ixbrl.AddContext(contextId, "http://www.companieshouse.gov.uk/", request.CompanyId.ToString(), request.PeriodStart, request.PeriodEnd);
            ixbrl.AddUnit(unitId, "iso:GBP");

            ixbrl.AddNonNumeric("uk-gaap:EntityLegalForm", contextId, "Limited company");
            ixbrl.AddNonNumeric("uk-gaap:NameOfEntity", contextId, request.CompanyName ?? "Unknown");
            ixbrl.AddNonNumeric("uk-gaap:CompanyRegistrationNumber", contextId, request.CompanyNumber ?? string.Empty);

            result.Success = true;
            result.OutputPath = $"accounts_{request.CompanyNumber}_{request.PeriodEnd:yyyyMMdd}.ixbrl";
            result.IxbrlContent = ixbrl.ToString();
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Failed to generate iXBRL: {ex.Message}";
        }

        return Task.FromResult(result);
    }
}

public class IXbrlDocument
{
    private readonly XmlDocument _document;
    private readonly XmlNamespaceManager? _nsManager;
    private readonly List<string> _namespaces = new();
    private readonly StringBuilder _content = new();

    public IXbrlDocument()
    {
        _document = new XmlDocument();
    }

    public void AddNamespace(string prefix, string uri)
    {
        _namespaces.Add($@"xmlns:{prefix}=""{uri}""");
    }

    public void AddContext(string id, string scheme, string identifier, DateTime? startDate, DateTime? endDate)
    {
        var period = endDate.HasValue && startDate.HasValue
            ? $@"<xbrli:startDate>{startDate:yyyy-MM-dd}</xbrli:startDate><xbrli:endDate>{endDate:yyyy-MM-dd}</xbrli:endDate>"
            : $@"<xbrli:instant>{endDate:yyyy-MM-dd}</xbrli:instant>";

        _content.Append($@"
<xbrli:context id=""{id}"">
    <xbrli:entity>
        <xbrli:identifier scheme=""{scheme}"">{identifier}</xbrli:identifier>
    </xbrli:entity>
    <xbrli:period>
        {period}
    </xbrli:period>
</xbrli:context>");
    }

    public void AddUnit(string id, string measure)
    {
        _content.Append($@"
<xbrli:unit id=""{id}"">
    <xbrli:measure>{measure}</xbrli:measure>
</xbrli:unit>");
    }

    public void AddNonNumeric(string name, string contextRef, string value)
    {
        _content.Append($@"<ix:nonNumeric name=""{name}"" contextRef=""{contextRef}"">{System.Security.SecurityElement.Escape(value)}</ix:nonNumeric>");
    }

    public void AddNonFraction(string name, string contextRef, string unitRef, int decimals, decimal value)
    {
        var sign = value < 0 ? "-" : "";
        _content.Append($@"<ix:nonFraction name=""{name}"" contextRef=""{contextRef}"" unitRef=""{unitRef}"" decimals=""{decimals}"">{sign}{Math.Abs(value)}</ix:nonFraction>");
    }

    public override string ToString()
    {
        var ns = string.Join(" ", _namespaces);
        return $"""
        <!DOCTYPE html>
        <html {ns}>
        <head>
            <title>Accounts</title>
        </head>
        <body>
            {_content}
        </body>
        </html>
        """;
    }
}
