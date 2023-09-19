# TahskrClient
A .NET Core (6.0) Api Client for [Tahskr](https://github.com/Dullage/tahskr-server) covering all [endpoints.](https://github.com/dullage/tahskr-server/blob/master/docs/api.md)

### System Information
```
ApiClient client = new ApiClient("http://[Server IP or Name]:[Server Port]");
SystemInformation systemInformation = client.SystemInformation();
```

### Create User
```
ApiClient client = new ApiClient("http://[Server IP or Name]:[Server Port]");
TahskrUser user = client.UserCreate("ADMIN PASSWORD", "Username", "Password");
```

### Authorization
The client maintains a reference to an AuthToken for a successful login for use with subsequent calls that need it. It also returns the AuthToken for debugging purposes.
```
ApiClient client = new ApiClient("http://[Server IP or Name]:[Server Port]");
AuthToken loggedInUser = client.Authenticate("Username", "Password");
```

### Create a ToDo List and Add a ToDo to it
```
ApiClient client = new ApiClient("http://[Server IP or Name]:[Server Port]");
AuthToken loggedInUser = client.Authenticate("Username", "Password");
ToDoList toDoList = client.ToDoListCreate("List Name", true); // prevent duplicate list name
ToDo toDo = new ToDo()
{
    Summary = "Take the dog for a walk.",
    ListId = toDoList.Id
};
client.ToDoCreate(toDo);
```

All other [endpoints](https://github.com/dullage/tahskr-server/blob/master/docs/api.md) have been implemented and follow the naming convention of [Object][Action]. 
