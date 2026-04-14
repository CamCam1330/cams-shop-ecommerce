namespace SV22T1020670.Shop.Models
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public string Photo { get; set; } = "";

        public string Unit { get; set; } = "";
        public decimal Price { get; set; } = 0;

        public int Quantity { get; set; } = 0;

        public decimal TotalPrice => Price * Quantity;
    }
}