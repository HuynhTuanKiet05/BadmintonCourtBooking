namespace BadmintonCourtBooking.Models;

public class VenueSearchCriteria
{
    public const string AllDistricts = "Tất cả";

    public string Query { get; set; } = string.Empty;
    public string District { get; set; } = AllDistricts;
    public int MaxPrice { get; set; } = 250_000;
    public string Hours { get; set; } = "any";
    public bool OnlyAvailable { get; set; }
    public string Sort { get; set; } = "relevance";

    public void Normalize()
    {
        Query = Query?.Trim() ?? string.Empty;
        District = string.IsNullOrWhiteSpace(District) ? AllDistricts : District.Trim();

        if (MaxPrice <= 0)
        {
            MaxPrice = 250_000;
        }
        else if (MaxPrice > 250_000)
        {
            MaxPrice = 250_000;
        }

        if (Hours is not ("any" or "morning" or "late"))
        {
            Hours = "any";
        }

        if (Sort is not ("relevance" or "price-asc" or "rating-desc"))
        {
            Sort = "relevance";
        }
    }
}

public class VenueIndexViewModel
{
    public List<Venue> Venues { get; set; } = new();
    public List<string> DistrictOptions { get; set; } = new();
    public VenueSearchCriteria Filters { get; set; } = new();
}
