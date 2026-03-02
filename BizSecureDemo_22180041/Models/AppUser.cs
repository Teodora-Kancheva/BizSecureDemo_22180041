namespace BizSecureDemo_22180041.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;
    }
}
