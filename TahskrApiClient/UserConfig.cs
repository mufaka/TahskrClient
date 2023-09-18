namespace TahskrApiClient
{
    public class UserConfig
    {
        public bool ShowCompleted { get; set; }
        public override string ToString()
        {
            return $@"ShowCompleted: {ShowCompleted}";
        }

    }
}
