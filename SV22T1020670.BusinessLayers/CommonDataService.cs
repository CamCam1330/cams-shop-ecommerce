                                                                                                                                                                                                                                                                                                                                                                                                                                                        using SV22T1020670.DataLayers;
using SV22T1020670.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020670.BusinessLayers
{
    /// <summary>
    /// Cung cấp các tính năng giao tiếp, xử lý dữ liệu chung
    /// (Province, Customer, Supplier, Shipper, Employee, Category)
    /// </summary>
    public static class CommonDataService
    {
        private static readonly ProvinceDAL provinceDB;
        private static readonly SupplierDAL supplierDB;
        private static readonly CustomerDAL customerDB;
        private static readonly ShipperDAL shipperDB;
        private static readonly EmployeeDAL employeeDB;                                                                                                     
        private static readonly CategoryDAL categoryDB;
        // Khai báo biến ProductDB
        private static readonly ProductDAL productDB;

        // Khai báo thêm DAL cho Quảng cáo
        private static readonly AdvertisementDAL advertisementDB;
        /// <summary>
        /// Constructor
        /// (Câu hỏi: Constructor của một lớp static có đặc điểm gì trong C#)               
        /// </summary>
        static CommonDataService()
        {
            provinceDB = new ProvinceDAL(Configuration.ConnectionString);
            supplierDB = new SupplierDAL(Configuration.ConnectionString);
            customerDB = new CustomerDAL(Configuration.ConnectionString);
            shipperDB = new ShipperDAL(Configuration.ConnectionString);
            employeeDB = new EmployeeDAL(Configuration.ConnectionString);
            categoryDB = new CategoryDAL(Configuration.ConnectionString);
            // Khởi tạo ProductDB
            productDB = new ProductDAL(Configuration.ConnectionString);
            // Khởi tạo DAL quảng cáo
            advertisementDB = new AdvertisementDAL(Configuration.ConnectionString);
        }

        /// <summary>
        /// Dữ liệu tỉnh thành
        /// </summary>
        public static ProvinceDAL ProvinceDB => provinceDB; 
        /// <summary>
        /// Dữ liệu nhà cung  cấp
        /// </summary>
        public static SupplierDAL SupplierDB => supplierDB;
        
        /// <summary>
        /// Dữ liệu khách hàng
        /// </summary>
        public static CustomerDAL CustomerDB => customerDB;

        /// <summary>
        /// Dữ liệu người giao hàng
        /// </summary>
        public static ShipperDAL ShipperDB => shipperDB;
        /// <summary>
        /// Dữ liệu nhân viên
        /// </summary>
        public static EmployeeDAL EmployeeDB => employeeDB;
        /// <summary>
        /// Dữ liệu loại hàng
        /// </summary>
        public static CategoryDAL CategoryDB => categoryDB;

        // Public ra bên ngoài
        public static ProductDAL ProductDB => productDB;

        public static AdvertisementDAL AdvertisementDB => advertisementDB;
    }
}
