namespace API.DataModels
{
    public class OrderPosition
    {
        public int ItemID { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderModel
    {
        public int UserID { get; set; }
        public List<OrderPosition>? Positions { get; set; }
        public AddressModel? Address { get; set; }
    }
}
