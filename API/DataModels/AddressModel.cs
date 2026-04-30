namespace API.DataModels
{
    public class AddressModel
    {
        public int ID { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? BuildingNumber { get; set; }
        public string? ApartmentNumber { get; set; }
    }
}
