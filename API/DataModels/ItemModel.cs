namespace API.DataModels
{
    public class ItemModel
    {
        public int ID { get; set; }
        public int CreatorID { get; set; }
        public string? CreatorName { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int AvailableQuantity { get; set; }
        public string? Thumbnail { get; set; }
        public DateTime? CreationTime { get; set; }
    }
}
