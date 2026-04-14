using Dapper;
using SV22T1020670.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020670.DataLayers
{
    /// <summary>
    /// Cài đặt các phép xử lý dữ liệu liên quan đến tỉnh thành
    /// </summary>
    public class ProvinceDAL : BaseDAL
    {
        public ProvinceDAL(string connectionString) : base(connectionString)
        {
        }
        /// <summary>
        /// Lấy danh sách các tỉnh thành
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Province>> ListAsync()
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = "SELECT * FROM Provinces";
                return await connection.QueryAsync<Province>(sql);
            }
        }
    }
}
