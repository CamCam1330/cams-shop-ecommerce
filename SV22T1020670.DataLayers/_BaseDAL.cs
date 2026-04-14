using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020670.DataLayers
{
    /// <summary>
    ///  lớp cơ sở cho các lớp xử lý dữ liệu trên CSDL SQL Server
    /// </summary>
    public abstract class BaseDAL
    {
        protected string connectionString;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="connectionString">Chuỗi tham số kết nối đến CSDL</param>
        public BaseDAL(string connectionString)
        {
            this.connectionString = connectionString;
        }
        /// <summary>
        /// Mo ket noi đến CSDL
        /// </summary>
        /// <returns></returns>
        protected SqlConnection OpenConnection()
        {
            try
            {
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = connectionString;
                connection.Open();
                return connection;
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể kết nối đến cơ sở dữ liệu. Chi tiết lỗi: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// mở kết nối đến cơ sở dữ liệu 
        /// </summary>
        /// <returns></returns>
        protected async Task<SqlConnection> OpenConnectionAsync()
        {
            try
            {
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = connectionString;
                await connection.OpenAsync();
                return connection;
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể kết nối đến cơ sở dữ liệu. Chi tiết lỗi: {ex.Message}", ex);
            }
        }
    }
}
