namespace TahskrApiClient
{
    public class ToDoList
    {
        public DateTime Created { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $@"Created: {Created}
Id: {Id}
Name: {Name}";
        }

    }
}
