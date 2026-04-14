using Dapper;
using SV22T1020670.DomainModels;
using SV22T1020670.DataLayers;
using System.Net.WebSockets;

namespace SV22T1020670.DataLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến tài khoản của nhân viên
    /// </summary>
    public class EmployeeUserAccountDAL : BaseDAL
    {
        public EmployeeUserAccountDAL(string connectionString) : base(connectionString)
        {
        }
        /// <summary>
        /// Kiểm tra tên đăng nhập và mật khẩu.
        /// Nếu hợp lệ trả về thông tin của tài khoản, ngược lại thì trả về null
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<UserAccount?> AuthenticateAsync(string userName, string password)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"SELECT	EmployeeID AS UserID, Email AS UserName, FullName, Email, Photo, RoleNames
                            FROM	Employees
                            WHERE	Email = @username AND Password = @password";
            var parameters = new { userName, password };
            return await connection.QueryFirstOrDefaultAsync<UserAccount>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
        }
        /// <summary>
        /// Thay đổi mật khẩu
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public async Task<bool> ChangePassword(string userID, string oldPassword, string newPassword)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"UPDATE	Employees 
                        SET		Password = @newPassword 
                        WHERE	EmployeeID = @userID AND Password = @oldPassword";
            var parameters = new { userID, oldPassword, newPassword };
            return (await connection.ExecuteAsync(sql: sql, param: parameters, commandType: System.Data.CommandType.Text)) > 0;
        }
        /// <summary>
        /// Reset mật khẩu về giá trị mặc định (dựa theo Email)
        /// </summary>
        /// <param name="email">Email người dùng cần reset</param>
        /// <param name="newPassword">Mật khẩu mới (ví dụ: "1")</param>
        /// <returns></returns>
        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            using var connection = await OpenConnectionAsync();
            // Lưu ý: Logic này update password trực tiếp dựa trên Email
            var sql = @"UPDATE Employees 
                SET Password = @newPassword 
                WHERE Email = @email";

            var parameters = new { email = email, newPassword = newPassword };

            // Nếu số dòng bị ảnh hưởng > 0 tức là Email có tồn tại và đã update thành công
            return (await connection.ExecuteAsync(sql: sql, param: parameters, commandType: System.Data.CommandType.Text)) > 0;
        }
    }
}