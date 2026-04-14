using SV22T1020670.DataLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020670.BusinessLayers
{
    /// <summary>
    /// Các chức năng tác nghiệp liên quan đến Quảng cáo
    /// </summary>
    public static class AdvertisementDataService
    {
        private static readonly AdvertisementDAL advertisementDB;

        static AdvertisementDataService()
        {
            advertisementDB = new AdvertisementDAL(Configuration.ConnectionString);
        }

        /// <summary>
        /// Giao tiếp trực tiếp với DAL Quảng cáo
        /// </summary>
        public static AdvertisementDAL AdvertisementDB => advertisementDB;
    }
}
