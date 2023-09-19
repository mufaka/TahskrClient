using TahskrApiClient;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var client = new ApiClient("http://10.10.9.108:8181");
            var systemInformation = client.SystemInformation();

            Console.WriteLine("SYSTEM INFORMATION");
            Console.WriteLine("".PadLeft(20, '-'));
            Console.WriteLine(systemInformation);
            Console.WriteLine();

            AuthToken loggedInUser = client.Authenticate("test2", "test2asdasdasdasd");
            Console.WriteLine("LOGGED IN USER");
            Console.WriteLine("".PadLeft(20, '-'));
            Console.WriteLine(loggedInUser);
            Console.WriteLine();

            client.UserCreate("BAD PASSWORD", "test3", "test3");

            return;
            //CheckCompletedYesterday(client);
            
            //CreateDailyToDos(client, incomplete);

            var toDoCategories = GetToDoCategories(GetConfigurationString());
            var incomplete = client.ToDoGetAll(null, false, false);

            foreach (ToDoCategory toDoCategory in toDoCategories)
            {
                ToDoList? toDoList = null;
                if (toDoCategory.Name != "Inbox")
                {
                    toDoList = client.ToDoListCreate(toDoCategory.Name, true);
                }

                CreateToDo(client, toDoList, toDoCategory.Summaries, incomplete);
            }
        }

        private static string GetConfigurationString()
        {
            return @"Test Inbox
[Daily]
Arm Care
Rauh Drills
Throwing
Workout
Glove Work

[Homework]
Check Email
Make Homework List
Do Homework

[Study]
Enter New Mochi Cards
Review Mochi Cards";
        }

        private class ToDoCategory
        {
            public string Name { get; set; } = String.Empty;
            public List<string> Summaries { get; set; } = new List<string>();
        }

        private static List<ToDoCategory> GetToDoCategories(string configurationString)
        {
            var list = new List<ToDoCategory>();

            StringReader reader = new StringReader(configurationString);
            string? line;

            list.Add(new ToDoCategory() { Name = "Inbox" });

            while ((line = reader.ReadLine()) != null)
            {
                var trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    list.Add(new ToDoCategory { 
                        Name = trimmedLine[1..^1].Trim() 
                    });
                }
                else
                {
                    if (!String.IsNullOrWhiteSpace(trimmedLine))
                    {
                        list[^1].Summaries.Add(trimmedLine);
                    }
                }
            };

            return list;
        }

        private static void CreateToDo(ApiClient client, ToDoList? toDoList, List<string> items, List<ToDo> incomplete)
        {
            foreach (string item in items)
            {
                // if there is a matching Summary in the incomplete list, don't add a duplicate
                var duplicate = incomplete.Where(t => t.Summary == item).FirstOrDefault();

                if (duplicate == null)
                {
                    client.ToDoCreate(new ToDo()
                    {
                        Summary = item,
                        ListId = toDoList?.Id
                    });
                }
                else
                {
                    // un-snooze if snoozed. The time of the snooze is captured
                    // by default so it needs to be cleared.
                    if (duplicate.SnoozeDatetime.HasValue)
                    {
                        duplicate.SnoozeDatetime = DateTime.Now.Date;
                        duplicate.ListId = toDoList?.Id;
                        client.ToDoUpdate(duplicate);
                    }
                }
            }
        }

        private static void ShowAllIncomplete(ApiClient client)
        {
            var allTodos = client.ToDoGetAll(null, false, false);

            foreach (var todo in allTodos)
            {
                Console.WriteLine(todo);
                Console.WriteLine();
            }
        }

        private static void CheckCompletedYesterday(ApiClient client)
        {
            var allTodos = client.ToDoGetAll(null, true, false);
            var yesterday = DateTime.Now.Date.AddDays(-1); // should be midnight

            foreach (var todo in allTodos)
            {
                if (todo.CompletedDatetime != null && todo.CompletedDatetime.Value.ToLocalTime().Date == yesterday)
                {
                    Console.WriteLine(todo);
                    Console.WriteLine();
                }
            }
        }

    }
}



