using Dapper;
using SV22T1020670.DomainModels;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SV22T1020670.DataLayers
{
    public class SupplierDAL : BaseDAL
    {
        public SupplierDAL(string connectionString) : base(connectionString) { }

        /// <summary>
        /// Tìm kiếm và lấy danh sách nhà cung cấp (phân trang)
        /// </summary>
        public async Task<IEnumerable<Supplier>> ListAsync(int page = 1, int pageSize = 0, string searchValue = "")
        {
            if (page < 1) page = 1;
            if (pageSize < 0) pageSize = 0;

            // chuẩn hóa wildcard
            searchValue = $"%{(searchValue ?? string.Empty)}%";

            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"
WITH cte AS
(
    SELECT  SupplierID, SupplierName, ContactName, Province, [Address], Phone, Email,
            ROW_NUMBER() OVER (ORDER BY SupplierName, SupplierID) AS RowNumber
    FROM    Suppliers
    WHERE   SupplierName LIKE @searchValue
        OR  ContactName  LIKE @searchValue
        OR  Province     LIKE @searchValue
        OR  [Address]    LIKE @searchValue
        OR  Phone        LIKE @searchValue
        OR  Email        LIKE @searchValue
)
SELECT SupplierID, SupplierName, ContactName, Province, [Address], Phone, Email
FROM cte
WHERE   (@pageSize = 0)
    OR  (RowNumber BETWEEN (@page - 1) * @pageSize + 1 AND @page * @pageSize)
ORDER BY RowNumber;";

                var parameters = new { page, pageSize, searchValue };
                return await connection.QueryAsync<Supplier>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// Đếm số lượng nhà cung cấp tìm được
        /// </summary>
        public async Task<int> CountAsync(string searchValue = "")
        {
            searchValue = $"%{(searchValue ?? string.Empty)}%";

            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"
SELECT COUNT(*)
FROM Suppliers
WHERE   SupplierName LIKE @searchValue
    OR  ContactName  LIKE @searchValue
    OR  Province     LIKE @searchValue
    OR  [Address]    LIKE @searchValue
    OR  Phone        LIKE @searchValue
    OR  Email        LIKE @searchValue;";
                return await connection.ExecuteScalarAsync<int>(sql: sql, param: new { searchValue }, commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// Lấy thông tin 1 nhà cung cấp theo ID
        /// </summary>
        public async Task<Supplier?> GetAsync(int id)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT * FROM Suppliers WHERE SupplierID = @id;";
                return await connection.QueryFirstOrDefaultAsync<Supplier>(sql: sql, param: new { id }, commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// Thêm nhà cung cấp mới → trả về ID
        /// </summary>
        public async Task<int> AddAsync(Supplier data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"
INSERT INTO Suppliers (SupplierName, ContactName, Province, Address, Phone, Email)
VALUES (@SupplierName, @ContactName, @Province, @Address, @Phone, @Email);
SELECT CAST(SCOPE_IDENTITY() AS int);";

                // Truyền trực tiếp 'data' để map các @Param trong SQL
                return await connection.ExecuteScalarAsync<int>(sql: sql, param: data, commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// Cập nhật thông tin nhà cung cấp
        /// </summary>
        public async Task<bool> UpdateAsync(Supplier data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"
UPDATE  Suppliers
SET     SupplierName = @SupplierName,
        ContactName  = @ContactName,
        Province     = @Province,
        Address      = @Address,
        Phone        = @Phone,
        Email        = @Email
WHERE   SupplierID   = @SupplierID;";

                var affected = await connection.ExecuteAsync(sql: sql, param: data, commandType: CommandType.Text);
                return affected > 0;
            }
        }

        /// <summary>
        /// Xóa nhà cung cấp
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"DELETE FROM Suppliers WHERE SupplierID = @id;";
                return (await connection.ExecuteAsync(sql: sql, param: new { id }, commandType: CommandType.Text)) > 0;
            }
        }

        /// <summary>
        /// Kiểm tra đang được sử dụng (VD: Products.SupplierID)
        /// </summary>
        public async Task<bool> InUsedAsync(int id)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"IF EXISTS(SELECT 1 FROM Products WHERE SupplierID = @id)
                                SELECT 1
                            ELSE
                                SELECT 0;";
                var parameters = new { id };
                return await connection.ExecuteScalarAsync<bool>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
            }
        }
    }
}
