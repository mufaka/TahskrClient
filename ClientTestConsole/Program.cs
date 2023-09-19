// See https://aka.ms/new-console-template for more information
using TahskrApiClient;

var client = new ApiClient("http://10.10.9.108:8181");
var systemInformation = client.SystemInformation();

Console.WriteLine("SYSTEM INFORMATION");
Console.WriteLine("".PadLeft(20, '-'));
Console.WriteLine(systemInformation);
Console.WriteLine();

AuthToken loggedInUser = client.Authenticate("bnickel", "bnickel");
Console.WriteLine("LOGGED IN USER");
Console.WriteLine("".PadLeft(20, '-'));
Console.WriteLine(loggedInUser);
Console.WriteLine();

var lists = client.ToDoListGetAll();

foreach (ToDoList list in lists)
{
    Console.WriteLine("TODO LIST");
    Console.WriteLine("".PadLeft(20, '-'));
    Console.WriteLine(list);
    Console.WriteLine();
}


var toDoList = client.ToDoListGet(1);

ToDo toDo = new ToDo()
{
    Summary = "Take the dog for a walk.",
    ListId = toDoList.Id
};

client.ToDoCreate(toDo);

var toDos = client.ToDoGetAll(null, null);

foreach (ToDo todo in toDos)
{
    Console.WriteLine("TODO");
    Console.WriteLine("".PadLeft(20, '-'));
    Console.WriteLine(todo);
    Console.WriteLine();
}

