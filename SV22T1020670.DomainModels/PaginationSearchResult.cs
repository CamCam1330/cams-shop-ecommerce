namespace SV22T1020670.DomainModels
{
    /// <summary>
    /// Biểu diễn cho dữ liệu dưới dạng phân trang
    /// </summary>
    public class PaginationSearchResult<T> where T : class
    {
        /// <summary>
        /// Trang được hiển thị
        /// </summary>
        public int Page { get; set; }
        
        /// <summary>
        /// Số dòng trên mỗi trang
        /// </summary>
        public int PageSize { get; set; }
       
        /// <summary>
        /// Giá trị tìm kiếm
        /// </summary> 
        public string SearchValue { get; set; }

        /// <summary>
        /// Số dòng dữ liệu tìm được
        /// </summary>       
        public int RowCount { get; set; }

        /// <summary>
        /// Số trang
        /// </summary>
        public int PageCount 
        { 
            get
            {
                if (PageSize <= 0)
                    return 1;
                int p = RowCount / PageSize;
                if (RowCount % PageSize > 0)
                    p += 1;
                return p;
            }
        }
        
        public required IEnumerable<T> Data { get; set; }
    }
}
