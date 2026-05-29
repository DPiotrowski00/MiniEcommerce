namespace API.DataTransferObjects
{
    public class OrderPositionDto
    {
        public int ItemID { get; set; }
        public int Quantity { get; set; }
    }

    public class PlaceOrderRequest
    {
        public required List<OrderPositionDto> Items { get; set; }
    }
}
