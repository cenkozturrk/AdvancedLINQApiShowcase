namespace AdvancedLINQApiShowcase.Pagination
{
    public class PaginationFilter
    {
        public int PageNumber { get; set; } = 1; 
        public int PageSize { get; set; } = 10;
        public string? SearchQuery { get; set; }
        public string SortBy { get; set; } 
        public bool IsDescending { get; set; } = false; 
    }
}
