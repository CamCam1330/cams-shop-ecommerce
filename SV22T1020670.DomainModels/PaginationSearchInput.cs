using System;
using System.Collections.Generic;
using System.Globalization;

namespace SV22T1020670.DomainModels.Models
{
    /// <summary>
    /// Đầu vào tìm kiếm chung (Phân trang)
    /// </summary>
    public class PaginationSearchInput
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SearchValue { get; set; } = string.Empty;
    }
}

