using Dapper;
using SV22T1020670.DomainModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020670.DataLayers
{
    public class ShipperDAL : BaseDAL
    {
        public ShipperDAL(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Tìm kiếm và lấy danh sách người giao hàng dưới dạng phân trang
        /// </summary>
        public async Task<IEnumerable<Shipper>> ListAsync(int page = 1, int pageSize = 0, string searchValue = "")
        {
            if (page < 1)
                page = 1;
            if (pageSize < 0)
                pageSize = 0;
            searchValue = $"%{searchValue}%";

            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"WITH cte AS
                            (
                                SELECT *, ROW_NUMBER() OVER(ORDER BY ShipperName) AS RowNumber
                                FROM Shippers
                                WHERE ShipperName LIKE @searchValue OR Phone LIKE @searchValue
                            )
                            SELECT * FROM cte
                            WHERE (@pageSize = 0)
                                OR (RowNumber BETWEEN (@page - 1) * @pageSize + 1 AND @page * @pageSize)
                            ORDER BY RowNumber;";

                var parameters = new
                {
                    page,
                    pageSize,
                    searchValue
                };

                return await connection.QueryAsync<Shipper>(sql, param: parameters, commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// Đếm tổng số shipper phù hợp điều kiện tìm kiếm
        /// </summary>
        public async Task<int> CountAsync(string searchValue = "")
        {
            searchValue = $"%{searchValue}%";
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT COUNT(*) 
                            FROM Shippers
                            WHERE ShipperName LIKE @searchValue OR Phone LIKE @searchValue;";

                var parameters = new { searchValue };
                return await connection.ExecuteScalarAsync<int>(sql, param: parameters, commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một shipper
        /// </summary>
        public async Task<Shipper?> GetAsync(int id)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT * FROM Shippers WHERE ShipperID = @ShipperID;";
                var parameters = new { ShipperID = id };

                return await connection.QueryFirstOrDefaultAsync<Shipper>(sql, param: parameters, commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// Thêm mới một shipper
        /// </summary>
        public async Task<int> AddAsync(Shipper data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"INSERT INTO Shippers
                            (
                                ShipperName,
                                Phone
                            )
                            VALUES
                            (
                                @ShipperName,
                                @Phone
                            );
                            SELECT SCOPE_IDENTITY();";

                var parameters = new
                {
                    data.ShipperName,
                    data.Phone
                };

                return await connection.ExecuteScalarAsync<int>(sql, param: parameters, commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// Cập nhật thông tin shipper
        /// </summary>
        public async Task<bool> UpdateAsync(Shipper data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"UPDATE Shippers
                            SET ShipperName = @ShipperName,
                                Phone = @Phone
                            WHERE ShipperID = @ShipperID;";

                var parameters = new
                {
                    data.ShipperID,
                    data.ShipperName,
                    data.Phone
                };

                return await connection.ExecuteAsync(sql, param: parameters, commandType: CommandType.Text) > 0;
            }
        }

        /// <summary>
        /// Xoá một shipper
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"DELETE FROM Shippers WHERE ShipperID = @ShipperID;";
                var parameters = new { ShipperID = id };

                return await connection.ExecuteAsync(sql, param: parameters, commandType: CommandType.Text) > 0;
            }
        }

        /// <summary>
        /// Kiểm tra xem shipper có đang được sử dụng trong bảng khác hay không
        /// </summary>
        public async Task<bool> InUsedAsync(int id)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT CASE 
                               WHEN EXISTS (SELECT * FROM Orders WHERE ShipperID = @ShipperID) THEN 1
                               ELSE 0
                           END";
                var parameters = new { ShipperID = id };

                return await connection.ExecuteScalarAsync<int>(sql, param: parameters, commandType: CommandType.Text) > 0;
            }
        }
    }
}
