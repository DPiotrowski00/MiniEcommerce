namespace API.DataModels
{
    public class UserModel
    {
        public int ID { get; set; }
        public required string Username { get; set; }
        public required string DisplayName { get; set; }
        public required string Email { get; set; }
    }
}
