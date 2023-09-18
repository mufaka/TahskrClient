namespace TahskrApiClient
{
    public class TahskrUser
    {
        public UserConfig Config { get; set; } = new UserConfig();
        public DateTime Created { get; set; }
        public string Username { get; set; }
        public int Id { get; set; }

        public override string ToString()
        {
            return $@"Created: {Created}
Username: {Username}
Id: {Id}
Config: {Config?.ToString()}";
        }
    }
}
