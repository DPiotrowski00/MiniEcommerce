using API.DataTransferObjects;

namespace API.DataModels
{
    public class OrderModel
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public required List<OrderPositionDto> Positions { get; set; }
    }
}
