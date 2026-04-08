namespace Tesis.Models;

public class User
{
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public int FingerId { get; set; }
        public UserRole Role { get; set; }
        public string RfidUid { get; set; } = "";
}

public enum UserRole
{
    User,
    Police,
    Doctor,
}