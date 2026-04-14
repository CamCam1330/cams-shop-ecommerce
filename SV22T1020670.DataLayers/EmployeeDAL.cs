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
    public class EmployeeDAL : BaseDAL
    {
        public EmployeeDAL(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Tìm kiếm và lấy danh sách nhân viên dưới dạng phân trang
        /// </summary>
        public async Task<IEnumerable<Employee>> ListAsync(int page = 1, int pageSize = 0, string searchValue = "")
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
                                SELECT *, ROW_NUMBER() OVER(ORDER BY FullName) AS RowNumber
                                FROM Employees
                                WHERE FullName LIKE @searchValue OR Phone LIKE @searchValue OR Email LIKE @searchValue
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

                return await connection.QueryAsync<Employee>(sql, param: parameters, commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// Đếm tổng số nhân viên phù hợp điều kiện tìm kiếm
        /// </summary>
        public async Task<int> CountAsync(string searchValue = "")
        {
            searchValue = $"%{searchValue}%";
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT COUNT(*) 
                            FROM Employees
                            WHERE FullName LIKE @searchValue OR Phone LIKE @searchValue OR Email LIKE @searchValue;";

                var parameters = new { searchValue };
                return await connection.ExecuteScalarAsync<int>(sql, param: parameters, commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một nhân viên
        /// </summary>
        public async Task<Employee?> GetAsync(int id)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT * FROM Employees WHERE EmployeeID = @EmployeeID;";
                var parameters = new { EmployeeID = id };

                return await connection.QueryFirstOrDefaultAsync<Employee>(sql, param: parameters, commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// Thêm mới một nhân viên
        /// </summary>
        public async Task<int> AddAsync(Employee data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"INSERT INTO Employees
                            (
                                FullName,
                                BirthDate,
                                Address,
                                Phone,
                                Email,
                                Password,
                                Photo,
                                IsWorking
                            )
                            VALUES
                            (
                                @FullName,
                                @BirthDate,
                                @Address,
                                @Phone,
                                @Email,
                                @Password,
                                @Photo,
                                @IsWorking
                            );
                            SELECT SCOPE_IDENTITY();";

                var parameters = new
                {
                    data.FullName,
                    data.BirthDate,
                    data.Address,
                    data.Phone,
                    data.Email,
                    data.Password,
                    data.Photo,
                    data.IsWorking
                };

                return await connection.ExecuteScalarAsync<int>(sql, param: parameters, commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// Cập nhật thông tin nhân viên
        /// </summary>
        public async Task<bool> UpdateAsync(Employee data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"UPDATE Employees
                            SET FullName = @FullName,
                                BirthDate = @BirthDate,
                                Address = @Address,
                                Phone = @Phone,
                                Email = @Email,
                                Password = @Password,
                                Photo = @Photo,
                                IsWorking = @IsWorking
                            WHERE EmployeeID = @EmployeeID;";

                var parameters = new
                {
                    data.EmployeeID,
                    data.FullName,
                    data.BirthDate,
                    data.Address,
                    data.Phone,
                    data.Email,
                    data.Password,
                    data.Photo,
                    data.IsWorking
                };

                return await connection.ExecuteAsync(sql, param: parameters, commandType: CommandType.Text) > 0;
            }
        }

        /// <summary>
        /// Xoá một nhân viên
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"DELETE FROM Employees WHERE EmployeeID = @EmployeeID;";
                var parameters = new { EmployeeID = id };

                return await connection.ExecuteAsync(sql, param: parameters, commandType: CommandType.Text) > 0;
            }
        }

        /// <summary>
        /// Kiểm tra xem nhân viên có đang được sử dụng trong bảng khác hay không
        /// </summary>
        public async Task<bool> InUsedAsync(int id)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT CASE 
                               WHEN EXISTS (SELECT * FROM Orders WHERE EmployeeID = @EmployeeID) THEN 1
                               ELSE 0
                           END";
                var parameters = new { EmployeeID = id };

                return await connection.ExecuteScalarAsync<int>(sql, param: parameters, commandType: CommandType.Text) > 0;
            }
        }
    }
}
