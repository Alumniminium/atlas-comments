using System.Text.Json;
using System.Web;

internal class Program
{
    public static readonly string path = Environment.GetEnvironmentVariable("PATH");
    public static readonly string cgiVersion = Environment.GetEnvironmentVariable("GATEWAY_INTERFACE");
    public static readonly string protocol = Environment.GetEnvironmentVariable("SERVER_PROTOCOL");
    public static readonly string port = Environment.GetEnvironmentVariable("SERVER_PORT");
    public static readonly string serverSoftware = Environment.GetEnvironmentVariable("SERVER_SOFTWARE");
    public static readonly string spartanUrl = Environment.GetEnvironmentVariable("SPARTAN_URL");
    public static readonly string scriptName = Environment.GetEnvironmentVariable("SCRIPT_NAME");
    public static readonly string pathInfo = Environment.GetEnvironmentVariable("PATH_INFO");
    public static readonly string query = Environment.GetEnvironmentVariable("QUERY_STRING");
    public static readonly string serverName = Environment.GetEnvironmentVariable("SERVER_NAME");
    public static readonly string remoteHost = Environment.GetEnvironmentVariable("REMOTE_HOST");
    public static readonly string remoteIp = Environment.GetEnvironmentVariable("REMOTE_ADDR");
    public static readonly string authType = Environment.GetEnvironmentVariable("AUTH_TYPE");
    public static readonly string geminiUrl = Environment.GetEnvironmentVariable("GEMINI_URL");
    public static readonly string tlsVersion = Environment.GetEnvironmentVariable("TLS_VERSION");
    public static readonly string remoteUser = Environment.GetEnvironmentVariable("REMOTE_USER");
    public static readonly bool certValid = Environment.GetEnvironmentVariable("TLS_CLIENT_VALID") != null && Environment.GetEnvironmentVariable("TLS_CLIENT_VALID").ToLowerInvariant() == "true";
    public static readonly bool certTrusted = Environment.GetEnvironmentVariable("TLS_CLIENT_TRUSTED") != null && Environment.GetEnvironmentVariable("TLS_CLIENT_TRUSTED").ToLowerInvariant() == "true";
    public static readonly string certSubject = Environment.GetEnvironmentVariable("TLS_CLIENT_SUBJECT");
    public static readonly string certHash = Environment.GetEnvironmentVariable("TLS_CLIENT_HASH");
    public static readonly string certFrom = Environment.GetEnvironmentVariable("TLS_CLIENT_NOT_BEFORE");
    public static readonly string certTo = Environment.GetEnvironmentVariable("TLS_CLIENT_NOT_AFTER");
    public static readonly string certSerialNumber = Environment.GetEnvironmentVariable("TLS_CLIENT_SERIAL_NUMBER");


    public static string SourcePath = "/srv/gemini/her.st/blog/";
    public static string CommentStoragePath = "/srv/gemini/her.st/blog/comments.json";
    public static Dictionary<string, List<Comment>> Posts = new();

    private static void Main()
    {
        var kvp = pathInfo.Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (var str in kvp)
            Console.Error.WriteLine(str);

        var type = kvp[1];
        var target = kvp[0];

        if (File.Exists(CommentStoragePath))
        {
            var json = File.ReadAllText(CommentStoragePath);
            Posts = JsonSerializer.Deserialize<Dictionary<string, List<Comment>>>(json);
        }

        foreach (var file in Directory.GetFiles(SourcePath))
        {
            var post = Path.GetFileName(file);

            if (post != target)
                continue;

            switch (type)
            {
                case "view":
                    {
                        Console.Write("20 text/gemini\r\n");
                        Console.Write("### Comments\n");
                        Console.Write("> Your Certificate Subject (CN=) will be use for your name and the Certificate Thumbprint will be saved & publicly accessible.\n");
                        Console.Write($"=> {geminiUrl.Replace("view", "add")} add one!\n");
                        foreach (var comments in Posts.Where(x => x.Key == target).Select(x => x.Value))
                        {
                            foreach (var comment in comments)
                            {
                                foreach (var line in comment.Text.Split('\n'))
                                    Console.Write($"> {line}\n");
                                Console.Write($"> -- {comment.Username} at {comment.TimeStamp}\n");
                                Console.WriteLine();
                                Console.WriteLine();
                            }
                        }
                        Console.Write($"### End\n");
                        break;
                    }
                case "add":
                    {
                        if (authType.ToLowerInvariant() != "certificate")
                        {
                            Console.Write($"60 Commenting requires a client certificate\r\n");
                            break;
                        }
                        if (!certValid)
                        {
                            Console.Write($"62 Commenting requires a *valid* client certificate\r\n");
                            break;
                        }

                        if (string.IsNullOrWhiteSpace(query))
                        {
                            Console.Write($"10 write your comment:\r\n");
                        }
                        else
                        {
                            if (!Posts.TryGetValue(target, out var commentList))
                            {
                                commentList = new();
                                Posts.Add(target, commentList);
                            }

                            var text = HttpUtility.UrlDecode(query[1..]);

                            commentList.Add(new Comment(certSubject, certHash, text));

                            var json = JsonSerializer.Serialize(Posts, new JsonSerializerOptions { WriteIndented = true });
                            File.WriteAllText(CommentStoragePath, json);
                            Console.Write($"30 {geminiUrl.Replace("add", "view").Split('?')[0]}\r\n");
                        }
                        break;
                    }
                case "delete":
                    {
                        break;
                    }
                case "update":
                    {
                        break;
                    }
            }
            break;
        }
    }
}