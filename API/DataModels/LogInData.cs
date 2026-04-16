namespace API.DataModels
{
    public class LogInData()
    {
        public int ID { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        public string? DisplayName { get; set; }
        public string? DeviceID { get; set; }
    }
}