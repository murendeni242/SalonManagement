namespace Salon.Application.DTOs;

/// <summary>
/// Standard wrapper for paginated list responses.
/// Every GET list endpoint should return this shape so the frontend
/// always knows how many total records exist and can render pagination controls.
///
/// Usage in a handler:
///   var items = await _repo.GetPagedAsync(skip, take);
///   var total = await _repo.GetCountAsync();
///   return new PagedResult&lt;BookingDto&gt;(items.Select(ToDto), total, skip, take);
///
/// Frontend receives:
/// {
///   "data":       [ ... ],
///   "total":      143,
///   "skip":       0,
///   "take":       50,
///   "totalPages": 3,
///   "hasNext":    true,
///   "hasPrev":    false
/// }
/// </summary>
public class PagedResult<T>
{
    /// <summary>The records for this page.</summary>
    public IEnumerable<T> Data { get; set; }

    /// <summary>Total number of records in the database (not just this page).</summary>
    public int Total { get; set; }

    /// <summary>Records skipped (current page offset).</summary>
    public int Skip { get; set; }

    /// <summary>Page size requested.</summary>
    public int Take { get; set; }

    /// <summary>Total number of pages at this page size.</summary>
    public int TotalPages => Take > 0 ? (int)Math.Ceiling((double)Total / Take) : 0;

    /// <summary>True when there are more records after this page.</summary>
    public bool HasNext => Skip + Take < Total;

    /// <summary>True when there are records before this page.</summary>
    public bool HasPrev => Skip > 0;

    public PagedResult(IEnumerable<T> data, int total, int skip, int take)
    {
        Data = data;
        Total = total;
        Skip = skip;
        Take = take;
    }
}