namespace E_Commerce_BackendAPI.Model
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string Token { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; }

        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

    }
}
