using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Xml.Linq;

namespace TahskrApiClient
{
    public class ApiClient
    {
        private HttpClient _httpClient;
        private string _serverUrl;
        private AuthToken _authToken;

        public ApiClient(string serverUrl)
        {
            _httpClient = new HttpClient();

            if (serverUrl.EndsWith('/'))
            {
                serverUrl = serverUrl.Substring(0, serverUrl.Length - 1);
            }
            _serverUrl = serverUrl;
        }

        private string BuildUrl(string endpoint)
        {
            return _serverUrl + endpoint;
        }

        private void AddBody(HttpRequestMessage message, object body)
        {
            message.Content = JsonContent.Create(body);
        }

        private string GetUrlWithQueryStringParameters(string url, Dictionary<string, string> parameters)
        {
            var uriBuilder = new UriBuilder(url);

            foreach (var key in parameters.Keys)
            {
                string query = $"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(parameters[key])}";

                if (uriBuilder.Query != null && uriBuilder.Query.Length > 1)
                {
                    uriBuilder.Query = uriBuilder.Query + "&" + query;
                }
                else
                {
                    // .NET Core version of UriBuilder automatically adds the ?
                    uriBuilder.Query = query;
                }
            }

            return uriBuilder.Uri.AbsoluteUri;
        }

        public SystemInformation SystemInformation()
        {
            var message = new HttpRequestMessage(HttpMethod.Get, BuildUrl("/"));
            message.Headers.Add("Accept", "application/json");
            var response = _httpClient.Send(message);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                using var reader = new StreamReader(response.Content.ReadAsStream());
                var result = reader.ReadToEnd();

                return JsonConvert.DeserializeObject<SystemInformation>(result);
            }
            else
            {
                throw new Exception($"Invalid request sent to /. Received {response.StatusCode}");
            }
        }

        public AuthToken Authenticate(string username, string password)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, BuildUrl("/auth"));
            message.Headers.Add("Accept", "application/json");
            AddBody(message, new {  username, password });
            var response = _httpClient.Send(message);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                using var reader = new StreamReader(response.Content.ReadAsStream());
                var result = reader.ReadToEnd();

                _authToken = JsonConvert.DeserializeObject<AuthToken>(result);
                return _authToken;
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Invalid credentials provided.");
                }
                else
                {
                    throw new Exception($"Invalid request sent to /auth. Received {response.StatusCode}");
                }
            }
        }

        public TahskrUser UserCreate(string adminPassword, string username, string password)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, BuildUrl("/user"));
            message.Headers.Add("Accept", "application/json");
            message.Headers.Add("x-admin", adminPassword);
            AddBody(message, new { username, password });

            var response = _httpClient.Send(message);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                using var reader = new StreamReader(response.Content.ReadAsStream());
                var result = reader.ReadToEnd();

                return JsonConvert.DeserializeObject<TahskrUser>(result);
            }
            else
            {
                throw new Exception($"Invalid request sent to /user. Received {response.StatusCode}");
            }
        }

        public TahskrUser UserGet(string? adminPassword, int userId)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"/user/{userId}"));
            message.Headers.Add("Accept", "application/json");

            if (!string.IsNullOrEmpty(adminPassword))
            {
                message.Headers.Add("x-admin", adminPassword);
            }
            else
            {
                message.Headers.Add("x-token", _authToken.Token);
            }
            var response = _httpClient.Send(message);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                using var reader = new StreamReader(response.Content.ReadAsStream());
                var result = reader.ReadToEnd();

                return JsonConvert.DeserializeObject<TahskrUser>(result);
            }
            else
            {
                throw new Exception($"Invalid request sent to /user. Received {response.StatusCode}");
            }
        }

        // TODO: It appears that you are unable to update a users password because the new one will 
        //       not be hashed before saving. Removing password for now. 
        public TahskrUser UserUpdate(string? adminPassword, TahskrUser user)
        {
            var message = new HttpRequestMessage(HttpMethod.Patch, BuildUrl($"/user/{user.Id}"));
            message.Headers.Add("Accept", "application/json");

            if (!string.IsNullOrEmpty(adminPassword))
            {
                message.Headers.Add("x-admin", adminPassword);
            }
            else
            {
                message.Headers.Add("x-token", _authToken.Token);
            }

            var updateBody = new
            {
                username = user.Username,
                config = user.Config

                //password = user.Password <-- password will not be re-hashed on saving. no way to update password.
            };

            AddBody(message, updateBody);

            var response = _httpClient.Send(message);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                using var reader = new StreamReader(response.Content.ReadAsStream());
                var result = reader.ReadToEnd();

                return JsonConvert.DeserializeObject<TahskrUser>(result);
            }
            else
            {
                throw new Exception($"Invalid request sent to /user. Received {response.StatusCode}");
            }
        }

        public void UserDelete(string? adminPassword, int userId)
        {
            var message = new HttpRequestMessage(HttpMethod.Delete, BuildUrl($"/user/{userId}"));
            message.Headers.Add("Accept", "application/json");

            if (!string.IsNullOrEmpty(adminPassword))
            {
                message.Headers.Add("x-admin", adminPassword);
            }
            else
            {
                message.Headers.Add("x-token", _authToken.Token);
            }

            var response = _httpClient.Send(message);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Invalid request sent to /user. Received {response.StatusCode}");
            }
        }

        public ToDoList ToDoListCreate(string name, bool preventDuplicate)
        {
            if (preventDuplicate)
            {
                var existingToDoLists = ToDoListGetAll();
                var existingByName = existingToDoLists.Where(t => t.Name.Trim().ToLower() == name.Trim().ToLower()).FirstOrDefault();

                if (existingByName != null)
                {
                    return existingByName;
                }
            }
            
            var message = new HttpRequestMessage(HttpMethod.Post, BuildUrl($"/todolist"));
            message.Headers.Add("x-token", _authToken.Token);
            message.Headers.Add("Accept", "application/json");

            AddBody(message, new { name });

            var response = _httpClient.Send(message);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                using var reader = new StreamReader(response.Content.ReadAsStream());
                var result = reader.ReadToEnd();

                return JsonConvert.DeserializeObject<ToDoList>(result);
            }
            else
            {
                throw new Exception($"Invalid request sent to /todolist. Received {response.StatusCode}");
            }
        }

        public List<ToDoList> ToDoListGetAll()
        {
            var message = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"/todolist"));
            message.Headers.Add("x-token", _authToken.Token);
            message.Headers.Add("Accept", "application/json");

            var response = _httpClient.Send(message);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                using var reader = new StreamReader(response.Content.ReadAsStream());
                var result = reader.ReadToEnd();

                return JsonConvert.DeserializeObject<List<ToDoList>>(result);
            }
            else
            {
                throw new Exception($"Invalid request sent to /todolist. Received {response.StatusCode}");
            }
        }

        public ToDoList ToDoListGet(int toDoListId)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"/todolist/{toDoListId}"));
            message.Headers.Add("x-token", _authToken.Token);
            message.Headers.Add("Accept", "application/json");

            var response = _httpClient.Send(message);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                using var reader = new StreamReader(response.Content.ReadAsStream());
                var result = reader.ReadToEnd();

                return JsonConvert.DeserializeObject<ToDoList>(result);
            }
            else
            {
                throw new Exception($"Invalid request sent to /todolist. Received {response.StatusCode}");
            }
        }

        public ToDoList ToDoListUpdate(ToDoList toDoList)
        {
            var message = new HttpRequestMessage(HttpMethod.Patch, BuildUrl($"/todolist/{toDoList.Id}"));
            message.Headers.Add("Accept", "application/json");
            message.Headers.Add("x-token", _authToken.Token);

            var updateBody = new
            {
                name = toDoList.Name
            };

            AddBody(message, updateBody);

            var response = _httpClient.Send(message);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                using var reader = new StreamReader(response.Content.ReadAsStream());
                var result = reader.ReadToEnd();

                return JsonConvert.DeserializeObject<ToDoList>(result);
            }
            else
            {
                throw new Exception($"Invalid request sent to /todolist/. Received {response.StatusCode}");
            }
        }

        public void ToDoListDelete(int toDoListId)
        {
            var message = new HttpRequestMessage(HttpMethod.Delete, BuildUrl($"/todolist/{toDoListId}"));
            message.Headers.Add("x-token", _authToken.Token);
            message.Headers.Add("Accept", "application/json");

            var response = _httpClient.Send(message);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Invalid request sent to /todolist. Received {response.StatusCode}");
            }
        }

        public ToDo ToDoCreate(ToDo toDo)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, BuildUrl($"/todo"));
            message.Headers.Add("x-token", _authToken.Token);
            message.Headers.Add("Accept", "application/json");

            AddBody(message, new { 
                summary = toDo.Summary,
                parentId = toDo.ParentId,
                listId = toDo.ListId,
                notes = toDo.Notes,
                important = toDo.Important,
                snoozeDatetime = toDo.SnoozeDatetime,
                completedDatetime = toDo.CompletedDatetime
            });

            var response = _httpClient.Send(message);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                using var reader = new StreamReader(response.Content.ReadAsStream());
                var result = reader.ReadToEnd();

                return JsonConvert.DeserializeObject<ToDo>(result);
            }
            else
            {
                throw new Exception($"Invalid request sent to /todo. Received {response.StatusCode}");
            }
        }

        public List<ToDo> ToDoGetAll(int? parentId, bool? completed, bool excludeSnoozed = false)
        {
            var parameters = new Dictionary<string, string>();
            if (parentId != null) parameters.Add("parentId", parentId.Value.ToString());
            if (completed != null) parameters.Add("completed", completed.Value ? "true" : "false");
            parameters.Add("excludeSnoozed", excludeSnoozed ? "true" : "false");

            var url = GetUrlWithQueryStringParameters(BuildUrl("/todo"), parameters);

            var message = new HttpRequestMessage(HttpMethod.Get, url);
            message.Headers.Add("x-token", _authToken.Token);
            message.Headers.Add("Accept", "application/json");

            var response = _httpClient.Send(message);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                using var reader = new StreamReader(response.Content.ReadAsStream());
                var result = reader.ReadToEnd();

                return JsonConvert.DeserializeObject<List<ToDo>>(result);
            }
            else
            {
                throw new Exception($"Invalid request sent to /todo. Received {response.StatusCode}");
            }
        }

        public ToDo ToDoGet(int toDoId)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"/todo/{toDoId}"));
            message.Headers.Add("x-token", _authToken.Token);
            message.Headers.Add("Accept", "application/json");

            var response = _httpClient.Send(message);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                using var reader = new StreamReader(response.Content.ReadAsStream());
                var result = reader.ReadToEnd();

                return JsonConvert.DeserializeObject<ToDo>(result);
            }
            else
            {
                throw new Exception($"Invalid request sent to /todo. Received {response.StatusCode}");
            }
        }

        public ToDo ToDoUpdate(ToDo toDo)
        {
            var message = new HttpRequestMessage(HttpMethod.Patch, BuildUrl($"/todo/{toDo.Id}"));
            message.Headers.Add("x-token", _authToken.Token);
            message.Headers.Add("Accept", "application/json");

            AddBody(message, new
            {
                summary = toDo.Summary,
                parentId = toDo.ParentId,
                listId = toDo.ListId,
                notes = toDo.Notes,
                important = toDo.Important,
                snoozeDatetime = toDo.SnoozeDatetime,
                completedDatetime = toDo.CompletedDatetime
            });

            var response = _httpClient.Send(message);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                using var reader = new StreamReader(response.Content.ReadAsStream());
                var result = reader.ReadToEnd();

                return JsonConvert.DeserializeObject<ToDo>(result);
            }
            else
            {
                throw new Exception($"Invalid request sent to /todo. Received {response.StatusCode}");
            }
        }

        public void ToDoDelete(int toDoId)
        {
            var message = new HttpRequestMessage(HttpMethod.Delete, BuildUrl($"/todo/{toDoId}"));
            message.Headers.Add("x-token", _authToken.Token);
            message.Headers.Add("Accept", "application/json");

            var response = _httpClient.Send(message);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Invalid request sent to /todo. Received {response.StatusCode}");
            }
        }

    }
}
