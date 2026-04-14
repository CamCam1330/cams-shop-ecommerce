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
    /// <summary>
    /// Xử lý dữ liệu cho bảng Categories
    /// </summary>
    public class CategoryDAL : BaseDAL
    {
        public CategoryDAL(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Tìm kiếm và lấy danh sách loại hàng dưới dạng phân trang
        /// </summary>
        public async Task<IEnumerable<Category>> ListAsync(int page = 1, int pageSize = 0, string searchValue = "")
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
                                SELECT *, ROW_NUMBER() OVER(ORDER BY CategoryName) AS RowNumber
                                FROM Categories
                                WHERE CategoryName LIKE @searchValue OR Description LIKE @searchValue
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

                return await connection.QueryAsync<Category>(sql, param: parameters, commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// Đếm tổng số loại hàng phù hợp điều kiện tìm kiếm
        /// </summary>
        public async Task<int> CountAsync(string searchValue = "")
        {
            searchValue = $"%{searchValue}%";
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT COUNT(*) 
                            FROM Categories
                            WHERE CategoryName LIKE @searchValue OR Description LIKE @searchValue;";

                var parameters = new { searchValue };
                return await connection.ExecuteScalarAsync<int>(sql, param: parameters, commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một loại hàng
        /// </summary>
        public async Task<Category?> GetAsync(int id)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT * FROM Categories WHERE CategoryID = @CategoryID;";
                var parameters = new { CategoryID = id };

                return await connection.QueryFirstOrDefaultAsync<Category>(sql, param: parameters, commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// Thêm mới một loại hàng
        /// </summary>
        public async Task<int> AddAsync(Category data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"INSERT INTO Categories
                            (
                                CategoryName,
                                Description
                            )
                            VALUES
                            (
                                @CategoryName,
                                @Description
                            );
                            SELECT SCOPE_IDENTITY();";

                var parameters = new
                {
                    data.CategoryName,
                    data.Description
                };

                return await connection.ExecuteScalarAsync<int>(sql, param: parameters, commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// Cập nhật thông tin loại hàng
        /// </summary>
        public async Task<bool> UpdateAsync(Category data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"UPDATE Categories
                            SET CategoryName = @CategoryName,
                                Description = @Description,
                                Photo = @Photo
                            WHERE CategoryID = @CategoryID;";

                var parameters = new
                {
                    data.CategoryID,
                    data.CategoryName,
                    data.Description,
                    Photo = data.Photo ?? ""
                };

                return await connection.ExecuteAsync(sql, param: parameters, commandType: CommandType.Text) > 0;
            }
        }

        /// <summary>
        /// Xoá một loại hàng
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"DELETE FROM Categories WHERE CategoryID = @CategoryID;";
                var parameters = new { CategoryID = id };

                return await connection.ExecuteAsync(sql, param: parameters, commandType: CommandType.Text) > 0;
            }
        }

        /// <summary>
        /// Kiểm tra xem loại hàng có đang được sử dụng trong bảng khác hay không
        /// </summary>
        public async Task<bool> InUsedAsync(int id)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT CASE 
                               WHEN EXISTS (SELECT * FROM Products WHERE CategoryID = @CategoryID) THEN 1
                               ELSE 0
                           END";
                var parameters = new { CategoryID = id };

                return await connection.ExecuteScalarAsync<int>(sql, param: parameters, commandType: CommandType.Text) > 0;
            }
        }
    }
}
