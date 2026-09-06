namespace UKAccounts.Xbrl;

public static class TaxonomyMapper
{
    private static readonly Dictionary<string, TaxonomyConcept> _concepts = new()
    {
        ["uk-gaap:NameOfEntity"] = new TaxonomyConcept { Id = "uk-gaap:NameOfEntity", Name = "Name of Entity", DataType = "string", Taxonomy = "UK" },
        ["uk-gaap:CompanyRegistrationNumber"] = new TaxonomyConcept { Id = "uk-gaap:CompanyRegistrationNumber", Name = "Company Registration Number", DataType = "string", Taxonomy = "UK" },
        ["uk-gaap:EntityLegalForm"] = new TaxonomyConcept { Id = "uk-gaap:EntityLegalForm", Name = "Entity Legal Form", DataType = "string", Taxonomy = "UK" },
        ["uk-gaap:ShareCapital"] = new TaxonomyConcept { Id = "uk-gaap:ShareCapital", Name = "Share Capital", DataType = "monetary", PeriodType = "instant", Balance = "credit", Taxonomy = "UK" },
        ["uk-gaap:TotalAssets"] = new TaxonomyConcept { Id = "uk-gaap:TotalAssets", Name = "Total Assets", DataType = "monetary", PeriodType = "instant", Balance = "debit", Taxonomy = "UK" },
        ["uk-gaap:TotalLiabilities"] = new TaxonomyConcept { Id = "uk-gaap:TotalLiabilities", Name = "Total Liabilities", DataType = "monetary", PeriodType = "instant", Balance = "credit", Taxonomy = "UK" },
        ["uk-gaap:TotalEquity"] = new TaxonomyConcept { Id = "uk-gaap:TotalEquity", Name = "Total Equity", DataType = "monetary", PeriodType = "instant", Balance = "credit", Taxonomy = "UK" },
        ["uk-gaap:Turnover"] = new TaxonomyConcept { Id = "uk-gaap:Turnover", Name = "Turnover", DataType = "monetary", PeriodType = "duration", Balance = "credit", Taxonomy = "UK" },
        ["uk-gaap:ProfitLoss"] = new TaxonomyConcept { Id = "uk-gaap:ProfitLoss", Name = "Profit or Loss", DataType = "monetary", PeriodType = "duration", Balance = "credit", Taxonomy = "UK" }
    };

    public static TaxonomyConcept? GetConcept(string conceptId)
    {
        return _concepts.TryGetValue(conceptId, out var concept) ? concept : null;
    }

    public static IReadOnlyList<TaxonomyConcept> GetAllConcepts()
    {
        return _concepts.Values.ToList();
    }
}
