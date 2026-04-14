using SV22T1020670.DataLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020670.BusinessLayers
{
    public static class ProductDataService
    {
        private static readonly ProductDAL productDB;

        /// <summary>
        /// Ctor
        /// </summary>
        static ProductDataService()
        {
            productDB = new ProductDAL(Configuration.ConnectionString);
        }

        /// <summary>
        /// 
        /// </summary>
        public static ProductDAL ProductDB => productDB;
    }
}
