namespace ToyStore.Models
{
    public class UserSession
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string UserType { get; set; } = null!; // Customer, Admin, Staff
        public string Role { get; set; } = null!;
        public bool IsAuthenticated { get; set; }
    }
}




