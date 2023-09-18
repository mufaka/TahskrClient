using Newtonsoft.Json.Linq;

namespace TahskrApiClient
{
    public class SystemInformation
    {
        public string AppVersion { get; set; }
        public string SchemaVersion { get; set; }

        public override string ToString()
        {
            return $@"AppVersion: {AppVersion}
SchemaVersion: {SchemaVersion}";
        }
    }
}
