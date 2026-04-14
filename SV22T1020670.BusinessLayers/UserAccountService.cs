using SV22T1020670.DataLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020670.BusinessLayers
{
    public static class UserAccountService
    {
        private static readonly EmployeeUserAccountDAL employeeUserAccountDB;
        private static readonly CustomerUserAccountDAL customerUserAccountDB;

        static UserAccountService()
        {
            employeeUserAccountDB = new EmployeeUserAccountDAL(Configuration.ConnectionString);
            customerUserAccountDB = new CustomerUserAccountDAL(Configuration.ConnectionString);
        }

        /// <summary>
        /// Giao tiếp với dữ liệu tài khoản của nhân viên
        /// </summary>
        public static EmployeeUserAccountDAL EmployeeUserAccountDB => employeeUserAccountDB;

        public static CustomerUserAccountDAL CustomerUserAccountDB => customerUserAccountDB;
    }
}
