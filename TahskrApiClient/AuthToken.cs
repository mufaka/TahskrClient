namespace TahskrApiClient
{
    public class AuthToken
    {
        public DateTime Created { get; set; }
        public DateTime Expiry { get; set; }
        public DateTime LastUsed { get; set; }
        public string Token { get; set; }
        public int UserId { get; set; }

        public override string ToString()
        {
            return $@"Created: {Created}
Expiry: {Expiry}
LastUsed: {LastUsed}
Token: {Token}
UserId: {UserId}";
        }
    }
}
