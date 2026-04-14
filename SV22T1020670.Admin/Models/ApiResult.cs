namespace SV22T1020670.DomainModels.Models
{
    /// <summary>
    /// Dữ liệu trả về cho các API dưới dạng JSON
    /// </summary>
    public class ApiResult
    {
        /// <summary>
        /// Trả về 1 nếu thành công, 0 nếu không thành công
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// Chuỗi thông báo kết quả hoặc lý do không thành công/lỗi
        /// </summary>
        public string Message { get; set; } = "";
        /// <summary>
        /// Dữ liệu trả về (nếu có)
        /// </summary>
        public object? Data { get; set; } = null;

        public static ApiResult CreateFail(string message)
        {
            return new ApiResult { Code = 0, Message = message };
        }

        public static ApiResult CreateSuccess(string message = "", object? data = null)
        {
            return new ApiResult { Code = 1, Message = message, Data = data };
        }
    }
}
