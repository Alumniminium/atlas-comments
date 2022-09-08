using System.Text.Json;
using System.Web;

internal class Program
{
    public static string SourcePath = "/srv/gemini/her.st/blog/";
    public static string CommentStoragePath = "/srv/gemini/her.st/blog/comments.json";
    public static Dictionary<string, List<Comment>> Posts = new();

    public static string Target = "";

    private static void Main()
    {
        try
        {
            var kvp = CgiVar.PathInfo.Split('/', StringSplitOptions.RemoveEmptyEntries);
            foreach (var str in kvp)
                Console.Error.WriteLine(str);

            Console.Error.WriteLine("Cert from: " + CgiVar.CertValidFrom);

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
                            if (CgiVar.HasCert)
                            {
                                Console.Write($"> 🪪 Subject: {CgiVar.RemoteUser}\n");
                                Console.Write($"> 🪪 Thumbprint: {CgiVar.CertHash}\n");
                                Console.Write($"Your comments will be attributed to the Subject of your certificate as shown above. The Thumbprint will be stored along with it so you can edit/delete your comment later.\n");
                                Console.Write($"=> {CgiVar.Url.Replace("view", "add")} add one!\n");
                            }
                            else
                                Console.Write($"> 🪪 No Certificate! You can only read comments but not write any.\n");
                            
                            Console.Write($"### {string.Join("", Path.GetFileNameWithoutExtension(Target[11..]))}\n");
                            foreach (var comments in Posts.Where(x => x.Key == Target).Select(x => x.Value))
                            {
                                foreach (var comment in comments)
                                {
                                    foreach (var line in comment.Text.Split('\n'))
                                        Console.Write($"> {line}\n");
                                    Console.Write($"> -- {comment.Username} at {comment.TimeStamp}\n");
                                    if (comment.Thumbprint == CgiVar.CertHash)
                                        Console.WriteLine($"=> {CgiVar.Url.Replace("view", "delete").Replace(Target, comment.Id)} delete\n");
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
                            if (!CgiVar.HasCert)
                            {
                                Console.Write($"60 Commenting requires a client certificate\r\n");
                                break;
                            }
                            if (!CgiVar.CertValid)
                            {
                                Console.Write($"62 Commenting requires a *valid* client certificate\r\n");
                                break;
                            }

                            if (string.IsNullOrWhiteSpace(CgiVar.Query))
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

                                var text = HttpUtility.UrlDecode(CgiVar.Query[1..]);
                                commentList.Add(new Comment(CgiVar.CertSubject, CgiVar.CertHash, text, Target));

                                var json = JsonSerializer.Serialize(Posts, new JsonSerializerOptions { WriteIndented = true });
                                File.WriteAllText(CommentStoragePath, json);
                                Console.Write($"30 {CgiVar.Url.Replace("add", "view").Split('?')[0]}\r\n");
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
                            if (oldComment.Thumbprint == CgiVar.CertHash)
                                postList.Remove(oldComment);
                            var newUrl = CgiVar.Url.Replace("delete", "view").Replace(oldComment.Id, oldComment.File);
                            var json = JsonSerializer.Serialize(Posts, new JsonSerializerOptions { WriteIndented = true });
                            File.WriteAllText(CommentStoragePath, json);
                            Console.Error.WriteLine("Redirect to " + newUrl);
                            Console.Write($"30 {newUrl}\r\n");
                        }
                        else
                        {
                            Console.Error.WriteLine("Redirect to  " + CgiVar.FQDN);
                            Console.Write($"30 {CgiVar.FQDN}\r\n");
                        }
                        break;
                    }
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            Console.Error.WriteLine(e.Source);
            Console.Error.WriteLine(e.StackTrace);
        }
    }
}