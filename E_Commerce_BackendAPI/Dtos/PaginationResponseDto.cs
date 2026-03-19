using System.Collections.Generic;

namespace E_Commerce_BackendAPI.Dtos
{
    public class PaginationResponseDto<T>
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<T> Items { get; set; } = new();
    }
}

