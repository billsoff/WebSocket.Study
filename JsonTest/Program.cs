using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Script.Serialization;

namespace JsonTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Person person = new Person
            {
                Name = "Jane",
                Age = 19,
            };
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string json = serializer.Serialize(person);

            Console.WriteLine(json);

            string name = "print";
            string data = serializer.Serialize(new string[] { "A", "B" });

            string query = $"name={name}&data={HttpUtility.UrlEncode(data)}";

            Console.WriteLine(query);
            Console.WriteLine();

            NameValueCollection pairs = HttpUtility.ParseQueryString(query);

            foreach (string key in pairs.AllKeys)
            {
                Console.WriteLine($"name: {pairs[key]}");
            }

            Console.WriteLine();

            PrintCommand command = new PrintCommand
            {
                Name = name,
                Data = data,
            };

            Console.WriteLine(command.Name);

            foreach (string path in command.Paths)
            {
                Console.WriteLine(path);
            }

            Console.WriteLine();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }


        private class Command
        {
            public Command()
            {
            }

            public string Name { get; set; }

            public string Data { get; set; }

            protected T Deserialzie<T>()
                where T : class
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                return serializer.Deserialize<T>(Data);
            }
        }

        private sealed class PrintCommand : Command
        {
            [ScriptIgnore]
            public string[] Paths { get => Deserialzie<string[]>(); }
        }

        private sealed class Person
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }
    }
}
