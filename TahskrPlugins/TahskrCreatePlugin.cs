using KronoMata.Public;
using System.Text;
using TahskrApiClient;

namespace TahskrPlugins
{
    public class TahskrCreatePlugin : IPlugin
    {
        public string Name { get { return "Tahskr Create Plugin"; } }

        public string Description { get { return "Creates configured Tahskr To Dos."; } }

        public string Version { get { return "1.1"; } }

        public List<PluginParameter> Parameters
        {
            get
            {
                var parameters = new List<PluginParameter>();

                parameters.Add(new PluginParameter()
                {
                    Name = "Server Url",
                    Description = "The Tahskr Server URL. (http://xyz.somedomain.com:8080)",
                    DataType = ConfigurationDataType.String,
                    IsRequired = true
                });

                parameters.Add(new PluginParameter()
                {
                    Name = "Username",
                    Description = "The Tahskr Username",
                    DataType = ConfigurationDataType.String,
                    IsRequired = true
                });

                parameters.Add(new PluginParameter()
                {
                    Name = "Password",
                    Description = "The Tahskr Password.",
                    DataType = ConfigurationDataType.Password,
                    IsRequired = true
                });

                parameters.Add(new PluginParameter()
                {
                    Name = "To Do Configuration",
                    Description = "Lists are defined as [List Name] on a separate line. To Dos are added below; one per line.",
                    DataType = ConfigurationDataType.Text,
                    IsRequired = true
                });

                return parameters;
            }
        }

        private PluginResult? ValidateRequiredParameters(Dictionary<string, string> pluginConfig)
        {
            PluginResult? missingRequiredParameterResult = null;

            foreach (PluginParameter parameter in Parameters)
            {
                if (parameter.IsRequired && !pluginConfig.ContainsKey(parameter.Name))
                {
                    missingRequiredParameterResult ??= new PluginResult()
                    {
                        IsError = true,
                        Message = "Missing required parameter(s).",
                        Detail = "The plugin configuration is missing the following parameters:"
                    };

                    missingRequiredParameterResult.Detail = missingRequiredParameterResult.Detail + Environment.NewLine + parameter.Name;
                }
            }

            return missingRequiredParameterResult;
        }

        public List<PluginResult> Execute(Dictionary<string, string> systemConfig, Dictionary<string, string> pluginConfig)
        {
            var log = new List<PluginResult>();

            try
            {
                var invalidConfigurationResult = ValidateRequiredParameters(pluginConfig);

                if (invalidConfigurationResult != null)
                {
                    log.Add(invalidConfigurationResult);
                }
                else
                {
                    var serverUrl = pluginConfig["Server Url"];
                    var userName = pluginConfig["Username"];
                    var password = pluginConfig["Password"];
                    var configuration = pluginConfig["To Do Configuration"];

                    var client = new ApiClient(serverUrl);
                    client.Authenticate(userName, password);

                    var toDoCategories = GetToDoCategories(configuration);
                    var incomplete = client.ToDoGetAll(null, false, false);

                    var buff = new StringBuilder();

                    foreach (ToDoCategory toDoCategory in toDoCategories)
                    {
                        ToDoList? toDoList = null;
                        if (toDoCategory.Name != "Inbox")
                        {
                            toDoList = client.ToDoListCreate(toDoCategory.Name, true);
                        }

                        CreateToDo(client, toDoList, toDoCategory.Summaries, incomplete, buff);
                    }

                    if (buff.Length == 0)
                    {
                        buff.Append("No ToDos were created.");
                    }

                    log.Add(new PluginResult()
                    {
                        IsError = false,
                        Message = $"Finished creating ToDos for {userName}",
                        Detail = buff.ToString()
                    });

                }
            }
            catch (Exception ex)
            {
                log.Add(new PluginResult()
                {
                    IsError = true,
                    Message = ex.Message,
                    Detail = ex.StackTrace ?? String.Empty
                });
            }

            return log;
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
                    list.Add(new ToDoCategory
                    {
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

        private static void CreateToDo(ApiClient client, ToDoList? toDoList, List<string> items, List<ToDo> incomplete, StringBuilder buff)
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
                    buff.AppendLine($"Created {item}");
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
                        buff.AppendLine($"Un-Snoozed {item}");
                    }
                    else
                    {
                        buff.AppendLine($"{item} already existed.");
                    }
                }
            }
        }

    }
}
