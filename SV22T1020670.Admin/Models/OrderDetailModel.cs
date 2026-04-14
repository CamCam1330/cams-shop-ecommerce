using SV22T1020670.DomainModels;

namespace SV22T1020670.Admin.Models
{
    public class OrderDetailModel
    {
        public Order Order { get; set; }
        public List<OrderDetail> Details { get; set; }
    }
}
