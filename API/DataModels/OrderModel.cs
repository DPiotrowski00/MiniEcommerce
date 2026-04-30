namespace API.DataModels
{
    public class OrderPosition
    {
        public int ItemID { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderModel
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public required List<OrderPosition> Positions { get; set; }
        public required AddressModel Address { get; set; }
    }
}
