using System.Xml.Linq;

namespace TahskrApiClient
{
    public class ToDo
    {
        public DateTime? CompletedDatetime { get; set; }
        public DateTime Created { get; set; }
        public int Id { get; set; }
        public bool Important { get; set; }
        public int? ListId { get; set; }
        public string? Notes { get; set; }
        public int? ParentId { get; set; }
        public DateTime? SnoozeDatetime { get; set; }
        public string Summary { get; set; }

        public override string ToString()
        {
            return $@"CompletedDatetime: {CompletedDatetime}
Created: {Created}
Id: {Id}
Important: {Important}
ListId: {ListId}
Notes: {Notes}
ParentId: {ParentId}
SnoozeDatetime: {SnoozeDatetime}
Summary: {Summary}";
        }

    }
}
