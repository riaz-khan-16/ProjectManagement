namespace ProjectManagementAPI.Models
{
    public class RegisterModel
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Role { get; set; } = "User";      // optional
        public string? Department { get; set; }          // optional
    }

}
