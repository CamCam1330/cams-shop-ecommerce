using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020670.BusinessLayers
{
    /// <summary>
    /// Dùng để khởi tạo & lưu các thông tin cấu hình cho tầng nghiệp vụ
    /// </summary>
    public static class Configuration
    {
        private static string connectionString = "";

        /// <summary>
        /// khởi tạo chuỗi kết nối CSDL cho tầng tác nghiệp ,khởi tạo chuỗi cấu hình
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Initialize(string connectionString)
        {
            Configuration.connectionString = connectionString;
        }
        /// <summary>
        /// Chuỗi tham số kết nối đến Cơ sở dữ liệu
        /// </summary>
        public static string ConnectionString
        { 
            get 
            { 
                return connectionString; 
            } 
        }
    }
}
