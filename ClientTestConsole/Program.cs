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

            AuthToken loggedInUser = client.Authenticate("test2", "test2");
            Console.WriteLine("LOGGED IN USER");
            Console.WriteLine("".PadLeft(20, '-'));
            Console.WriteLine(loggedInUser);
            Console.WriteLine();

            //CheckCompletedYesterday(client);
            var incomplete = client.ToDoGetAll(null, false, false);
            CreateDailyToDos(client, incomplete);
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

        // TODO: How to make this configurable for a plugin to KronoMata?
        private static void CreateDailyToDos(ApiClient client, List<ToDo> incomplete)
        {
            var dailyToDoList = client.ToDoListCreate("Daily", true);
            var homeworkToDoList = client.ToDoListCreate("Homework", true);
            var studyToDoList = client.ToDoListCreate("Study", true);

            var dailyList = new List<string>()
            {
                "Arm Care",
                "Rauh Drills",
                "Throwing",
                "Workout",
                "Glove Work"
            };

            var homeworkList = new List<string>()
            {
                "Check Email",
                "Make Homework List",
                "Do Homework"
            };

            var mochiList = new List<string>()
            {
                "Enter New Mochi Cards",
                "Review Mochi Cards"
            };

            CreateToDo(client, dailyToDoList, dailyList, incomplete);
            CreateToDo(client, homeworkToDoList, homeworkList, incomplete);
            CreateToDo(client, studyToDoList, mochiList, incomplete);
        }

        private static void CreateToDo(ApiClient client, ToDoList toDoList, List<string> items, List<ToDo> incomplete)
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
                        ListId = toDoList.Id
                    });

                    Console.WriteLine($"Added {item}");
                }
                else
                {
                    // un-snooze if snoozed. The time of the snooze is captured
                    // by default so it needs to be cleared.
                    if (duplicate.SnoozeDatetime.HasValue)
                    {
                        duplicate.SnoozeDatetime = DateTime.Now.Date;
                        client.ToDoUpdate(duplicate);
                        Console.WriteLine($"Un-snoozed {item}");
                    }
                }
            }
        }
    }
}



