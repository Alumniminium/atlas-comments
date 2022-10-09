using System.Text;
using System.Text.Json;
using System.Web;

namespace atlasComments
{
    public static class Program
    {
        public const string CfgPath = "/etc/atlas/atlas-comments.json";
        static Config cfg = new();
        public static readonly TextWriter Error = Console.Error;
        public static readonly TextWriter Output = Console.Out;
        private static async Task Main()
        {
            if(File.Exists(CfgPath))
                cfg = JsonSerializer.Deserialize(File.ReadAllText(CfgPath), MyJsonContext.Default.Config);
            else
                await cfg.SaveAsync(CfgPath);

            var kvp = CgiVar.PathInfo.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var type = kvp[1];
            var target = kvp[0];

            var filePath = "";
            var uriPath = "";

            foreach(var (dir, rel) in cfg.FileSourcePath)
            {
                foreach(var file in Directory.EnumerateFileSystemEntries(dir))
                {
                    var fileName = Path.GetFileName(file);
                    if(Path.GetFileName(fileName) == target)
                    {
                        filePath = file;
                        uriPath = $"{CgiVar.Protocol}://{CgiVar.FQDN}/{rel}{fileName}";
                    }
                }
            }

            switch (type)
            {
                case "view":
                    View(filePath, uriPath);
                    break;
                case "add":
                    await Add(filePath, uriPath);
                    break;
                case "delete":
                    await Delete(filePath, uriPath);
                    break;
            }
        }

        private static async ValueTask Delete(string filePath, string uriPath)
        {
            var oldComment = cfg.FileComments[filePath]?.Find(x => x.File == filePath);

            if (oldComment is null)
            {
                Response.BadRequest($"Comment not found");
                return;
            }

            if (oldComment.Thumbprint != CgiVar.CertHash)
            {
                Response.BadRequest("You are not authorized to delete this comment");
                return;
            }

            cfg.FileComments[filePath].Remove(oldComment);

            var actualFile = filePath;
            var actualFileText = Encoding.UTF8.GetString(Convert.FromBase64String(cfg.OriginalFiles[oldComment.File]));

            foreach (var comment in cfg.FileComments[oldComment.File])
            {
                foreach (var line in comment.Text.Split('\n'))
                    actualFileText += $"> {line}\n";
                actualFileText += $"> -- {comment.Username} at {comment.TimeStamp}\n\n";
            }

            File.WriteAllText(actualFile, actualFileText);

            await cfg.SaveAsync(CfgPath);
            var newUrl = CgiVar.Url.Replace("delete", "view").Replace(oldComment.Id, oldComment.File);
            Response.Redirect($"{newUrl}");
        }

        private static async ValueTask Add(string filePath, string fileUri)
        {
            if (!File.Exists(filePath))
                Response.NotFound(fileUri);
            else if (!CgiVar.HasCert)
                Response.CertRequired();
            else if (!CgiVar.CertValid)
                Response.CertInvalid();
            else if (string.IsNullOrWhiteSpace(CgiVar.Query))
                Response.Input("write your comment:");
            else
            {
                if (!cfg.FileComments.TryGetValue(filePath, out var commentList))
                {
                    cfg.FileComments.TryAdd(filePath, commentList = new());
                    cfg.OriginalFiles.TryAdd(filePath, Convert.ToBase64String(File.ReadAllBytes(filePath)));
                }

                var text = HttpUtility.UrlDecode(CgiVar.Query[1..]);
                var newComment = new Comment(CgiVar.CertSubject, CgiVar.CertHash, text, filePath);
                commentList.Add(newComment);
            await cfg.SaveAsync(CfgPath);

                var actualFile = filePath;
                var actualFileText = Encoding.UTF8.GetString(Convert.FromBase64String(cfg.OriginalFiles[filePath]));

                foreach (var comment in cfg.FileComments[filePath])
                {
                    foreach (var line in comment.Text.Split('\n'))
                        actualFileText += $"> {line}\n";
                    actualFileText += $"> -- {comment.Username} at {comment.TimeStamp}\n\n";
                }

                File.WriteAllText(actualFile, actualFileText);

                Response.Redirect($"{CgiVar.Url.Replace("add", "view").Split('?')[0]}");
            }
        }

        private static void View(string filePath, string uriPath)
        {
            if (File.Exists(filePath))
            {
                Response.Ok($"=> {uriPath} {Path.GetFileNameWithoutExtension(filePath)}\n");
                if (CgiVar.HasCert)
                {
                    Output.WriteLine($"> 🪪 Subject: {CgiVar.RemoteUser}");
                    Output.WriteLine($"> 🪪 Thumbprint: {CgiVar.CertHash}");
                    Output.WriteLine($"Subject acts as Username. Thumbprint acts as token to delete your comment.");
                    Output.WriteLine($"=> {CgiVar.Url.Replace("view", "add")} ⌨️add one!");
                }
                else
                    Output.WriteLine($"> 🪪 No Certificate! You can only read comments but not write any.");

                Output.WriteLine($"### Comments");
                var list = cfg.FileComments.Where(x => x.Key == filePath).Select(x => x.Value).FirstOrDefault(new List<Comment>());
                foreach (var comment in list.OrderByDescending(x => x.TimeStamp))
                {
                    foreach (var line in comment.Text.Split('\n'))
                        Output.WriteLine($"> {line}");
                    Output.WriteLine($"> -- {comment.Username} at {comment.TimeStamp}");
                    if (comment.Thumbprint == CgiVar.CertHash)
                        Output.WriteLine($"=> {CgiVar.Url.Replace("view", "delete").Replace(filePath, comment.Id)} ☢️delete");
                    Output.WriteLine();
                }
            }
            else
                Response.NotFound(uriPath);
        }
    }
}