// See https://aka.ms/new-console-template for more information
using TahskrApiClient;

/*
tanner
bnickel
blaise
test1
test2
*/

var client = new ApiClient("http://10.10.9.108:8181");

var systemInformation = client.SystemInformation();
Console.WriteLine("SYSTEM INFORMATION");
Console.WriteLine("".PadLeft(20, '-'));
Console.WriteLine(systemInformation);
Console.WriteLine();

var loggedInUser = client.Authenticate("bnickel", "bnickel");
Console.WriteLine("LOGGED IN USER");
Console.WriteLine("".PadLeft(20, '-'));
Console.WriteLine(loggedInUser);
Console.WriteLine();

var lists = client.ToDoListGetAll();

foreach (ToDoList toDoList in lists)
{
    Console.WriteLine("TODO LIST");
    Console.WriteLine("".PadLeft(20, '-'));
    Console.WriteLine(toDoList);
    Console.WriteLine();
}


var toDos = client.ToDoGetAll(null, null);

foreach (ToDo todo in toDos)
{
    Console.WriteLine("TODO");
    Console.WriteLine("".PadLeft(20, '-'));
    Console.WriteLine(todo);
    Console.WriteLine();
}

var toDo = client.ToDoGet(4);

toDo.ListId = 1;
toDo = client.ToDoUpdate(toDo);

Console.WriteLine("TODO");
Console.WriteLine("".PadLeft(20, '-'));
Console.WriteLine(toDo);
Console.WriteLine();

client.ToDoDelete(4);

