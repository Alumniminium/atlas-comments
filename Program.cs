using System.Text.Json;
using System.Web;

namespace atlasComments
{
    public static class Program
    {
        public static readonly Config Config = JsonSerializer.Deserialize<Config>(File.ReadAllText(Config.ConfigName));
        public static readonly TextWriter Error = Console.Error;
        public static readonly TextWriter Output = Console.Out;
        private static async Task Main()
        {
            var kvp = CgiVar.PathInfo.Split('/', StringSplitOptions.RemoveEmptyEntries);
             var type = kvp[1];
            var target = kvp[0];

            switch (type)
            {
                case "view":
                    View(target);
                    break;
                case "add":
                    await Add(target);
                    break;
                case "delete":
                    await Delete(target);
                    break;
            }
        }

        private static async ValueTask Delete(string target)
        {
            var comments = default(List<Comment>);
            var oldComment = default(Comment);

            //find list and comment based on Id
            foreach (var file in Config.FileComments)
            {
                comments = file.Value;
                oldComment = comments.Find(x => x.Id == target);

                if (oldComment != null)
                    break;
            }

            if (oldComment is null || comments is null)
            {
                Response.BadRequest($"Comment not found");
                return;
            }

            if (oldComment.Thumbprint != CgiVar.CertHash)
            {
                Response.BadRequest("You are not authorized to delete this comment");
                return;
            }

            comments.Remove(oldComment);
            await Config.SaveAsync();
            var newUrl = CgiVar.Url.Replace("delete", "view").Replace(oldComment.Id, oldComment.File);
            Response.Redirect($"{newUrl}");
        }

        private static async ValueTask Add(string target)
        {
            if (!File.Exists(Path.Join(Config.FileSourcePath, target)))
                Response.NotFound(target);
            else if (!CgiVar.HasCert)
                Response.CertRequired();
            else if (!CgiVar.CertValid)
                Response.CertInvalid();
            else if (string.IsNullOrWhiteSpace(CgiVar.Query))
                Response.Input("write your comment:");
            else
            {
                if (!Config.FileComments.TryGetValue(target, out var commentList))
                    Config.FileComments.TryAdd(target, commentList = new());

                var text = HttpUtility.UrlDecode(CgiVar.Query[1..]);
                commentList.Add(new Comment(CgiVar.CertSubject, CgiVar.CertHash, text, target));
                await Config.SaveAsync();
                Response.Redirect($"{CgiVar.Url.Replace("add", "view").Split('?')[0]}");
            }
        }

        private static void View(string target)
        {
            if (File.Exists(Path.Join(Config.FileSourcePath, target)))
            {
                Response.Ok($"### {Path.GetFileNameWithoutExtension(target)}\n");
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
                var list = Config.FileComments.Where(x => x.Key == target).Select(x => x.Value).FirstOrDefault(new List<Comment>());
                foreach (var comment in list.OrderByDescending(x => x.TimeStamp))
                {
                    foreach (var line in comment.Text.Split('\n'))
                        Output.WriteLine($"> {line}");
                    Output.WriteLine($"> -- {comment.Username} at {comment.TimeStamp}");
                    if (comment.Thumbprint == CgiVar.CertHash)
                        Output.WriteLine($"=> {CgiVar.Url.Replace("view", "delete").Replace(target, comment.Id)} ☢️delete");
                    Output.WriteLine();
                }
            }
            else
                Response.NotFound(target);
        }
    }
}