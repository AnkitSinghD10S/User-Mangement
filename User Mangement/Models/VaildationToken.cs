namespace User_Mangement.Models
{
    public class VaildationToken
    {

        
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Token { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
}
