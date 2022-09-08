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

    public static string Target = "";

    private static void Main()
    {
        var kvp = pathInfo.Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (var str in kvp)
            Console.Error.WriteLine(str);

        Console.Error.WriteLine("Cert from: " + certFrom);

        var type = kvp[1];
        Target = kvp[0];

        Console.Error.WriteLine($"Type {type} Target {Target}");

        if (File.Exists(CommentStoragePath))
        {
            var json = File.ReadAllText(CommentStoragePath);
            Posts = JsonSerializer.Deserialize<Dictionary<string, List<Comment>>>(json);
        }

        switch (type)
        {
            case "view":
                {
                    foreach (var file in Directory.GetFiles(SourcePath))
                    {
                        var post = Path.GetFileName(file);

                        if (post != Target)
                            continue;

                        Console.Write("20 text/gemini\r\n");
                        if (authType.ToUpperInvariant() == "CERTIFICATE")
                        {
                            Console.Write($"> 🪪 Subject: {remoteUser}\n");
                            Console.Write($"> 🪪 Thumbprint: {certHash}\n");
                            Console.Write($"Your comments will be attributed to the Subject of your certificate as shown above. The Thumbprint will be stored along with it so you can edit/delete your comment later.\n");
                            Console.Write($"=> {geminiUrl.Replace("view", "add")} add one!\n");
                        }
                        else
                            Console.Write($"> 🪪 No Certificate! You can only read comments but not write any.\n");

                        Console.Write("## Comments\n");
                        foreach (var comments in Posts.Where(x => x.Key == Target).Select(x => x.Value))
                        {
                            foreach (var comment in comments)
                            {
                                foreach (var line in comment.Text.Split('\n'))
                                    Console.Write($"> {line}\n");
                                Console.Write($"> -- {comment.Username} at {comment.TimeStamp}\n");
                                if (comment.Thumbprint == certHash)
                                    Console.WriteLine($"=> {geminiUrl.Replace("view", "delete").Replace(Target, comment.Id)} delete\n");
                                Console.WriteLine();
                            }
                        }
                        break;
                    }
                    break;
                }
            case "add":
                {
                    foreach (var file in Directory.GetFiles(SourcePath))
                    {
                        var post = Path.GetFileName(file);

                        if (post != Target)
                            continue;
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
                            if (!Posts.TryGetValue(Target, out var commentList))
                            {
                                commentList = new();
                                Posts.Add(Target, commentList);
                            }

                            var text = HttpUtility.UrlDecode(query[1..]);
                            commentList.Add(new Comment(certSubject, certHash, text, Target));

                            var json = JsonSerializer.Serialize(Posts, new JsonSerializerOptions { WriteIndented = true });
                            File.WriteAllText(CommentStoragePath, json);
                            Console.Write($"30 {geminiUrl.Replace("add", "view").Split('?')[0]}\r\n");
                        }
                        break;
                    }
                    break;
                }
            case "delete":
                {
                    var postList = default(List<Comment>);
                    var oldComment = default(Comment);

                    Console.Error.WriteLine("Searching " + Target);
                    foreach (var list in Posts.Values)
                        foreach (var comment in list)
                            if (comment.Id == Target)
                            {
                                oldComment = comment;
                                postList = list;
                                Console.Error.WriteLine("Found " + Target);
                                break;
                            }

                    if (oldComment != null)
                    {
                        Console.Error.WriteLine("Deleting " + Target);
                        if (oldComment.Thumbprint == certHash)
                            postList.Remove(oldComment);
                        var newUrl = geminiUrl.Replace("delete", "view").Replace(oldComment.Id, oldComment.File);
                        var json = JsonSerializer.Serialize(Posts, new JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText(CommentStoragePath, json);
                        Console.Error.WriteLine("Redirect to " + newUrl);
                        Console.Write($"30 {newUrl}\r\n");
                    }
                    else
                    {
                        Console.Error.WriteLine("Redirect to  " + serverName);
                        Console.Write($"30 {serverName}\r\n");
                    }
                    break;
                }
        }
    }
}