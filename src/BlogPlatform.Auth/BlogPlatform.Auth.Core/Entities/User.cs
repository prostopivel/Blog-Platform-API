namespace BlogPlatform.Auth.Core.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public User(Guid id, string username, string email, string passwordHash)
        {
            Id = id;
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
        }
    }
}
