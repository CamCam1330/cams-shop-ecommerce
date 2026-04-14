using Dapper;
using SV22T1020670.DomainModels;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SV22T1020670.DataLayers
{
    public class AdvertisementDAL : BaseDAL
    {
        public AdvertisementDAL(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Lấy danh sách quảng cáo (có phân trang và tìm kiếm)
        /// </summary>
        public async Task<IEnumerable<Advertisement>> ListAsync(int page = 1, int pageSize = 0, string searchValue = "")
        {
            if (page < 1) page = 1;
            if (pageSize < 0) pageSize = 0;
            searchValue = $"%{searchValue}%";

            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"WITH cte AS
                            (
                                SELECT *, ROW_NUMBER() OVER(ORDER BY DisplayOrder) AS RowNumber
                                FROM Advertisements
                                WHERE (@searchValue = N'%%' OR Title LIKE @searchValue)
                            )
                            SELECT * FROM cte
                            WHERE (@pageSize = 0)
                                OR (RowNumber BETWEEN (@page - 1) * @pageSize + 1 AND @page * @pageSize)
                            ORDER BY DisplayOrder;";

                var parameters = new
                {
                    page,
                    pageSize,
                    searchValue
                };

                return await connection.QueryAsync<Advertisement>(sql, param: parameters, commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// Đếm số lượng quảng cáo
        /// </summary>
        public async Task<int> CountAsync(string searchValue = "")
        {
            searchValue = $"%{searchValue}%";
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT COUNT(*) FROM Advertisements 
                            WHERE (@searchValue = N'%%' OR Title LIKE @searchValue);";
                return await connection.ExecuteScalarAsync<int>(sql, new { searchValue }, commandType: CommandType.Text);
            }
        }

        public async Task<Advertisement?> GetAsync(int id)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT * FROM Advertisements WHERE BannerID = @BannerID;";
                return await connection.QueryFirstOrDefaultAsync<Advertisement>(sql, new { BannerID = id }, commandType: CommandType.Text);
            }
        }

        public async Task<int> AddAsync(Advertisement data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"INSERT INTO Advertisements(Title, Photo, Link, DisplayOrder, IsHidden)
                            VALUES(@Title, @Photo, @Link, @DisplayOrder, @IsHidden);
                            SELECT SCOPE_IDENTITY();";

                var parameters = new
                {
                    data.Title,
                    data.Photo,
                    data.Link,
                    data.DisplayOrder,
                    data.IsHidden
                };

                return await connection.ExecuteScalarAsync<int>(sql, param: parameters, commandType: CommandType.Text);
            }
        }

        public async Task<bool> UpdateAsync(Advertisement data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"UPDATE Advertisements
                            SET Title = @Title,
                                Photo = @Photo,
                                Link = @Link,
                                DisplayOrder = @DisplayOrder,
                                IsHidden = @IsHidden
                            WHERE BannerID = @BannerID;";

                return await connection.ExecuteAsync(sql, data, commandType: CommandType.Text) > 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"DELETE FROM Advertisements WHERE BannerID = @BannerID;";
                return await connection.ExecuteAsync(sql, new { BannerID = id }, commandType: CommandType.Text) > 0;
            }
        }
    }
}