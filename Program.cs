using System.Text;
using System.Text.Json;
using System.Web;

public static class Program
{
    public static Config Config = JsonSerializer.Deserialize<Config>(File.ReadAllText(Config.ConfigName));
    public static TextWriter Error = Console.Error;
    public static TextWriter Output = Console.Out;
    private static void Main()
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
                Add(target);
                break;
            case "delete":
                Delete(target);
                break;
        }
    }

    private static void Delete(string target)
    {
        var postList = default(List<Comment>);
        var oldComment = default(Comment);

        foreach (var list in Config.FileComments.Values)
        {
            foreach (var comment in list)
            {
                if (comment.Id != target || comment.Thumbprint != CgiVar.CertHash)
                    continue;
                
                oldComment = comment;
                postList = list;
                break;
            }
            
            if (oldComment != null)
                break;
        }

        if (oldComment == null)
            Response.BadRequest();
        else
        {
            postList.Remove(oldComment);
            Save();
            var newUrl = CgiVar.Url.Replace("delete", "view").Replace(oldComment.Id, oldComment.File);
            Response.Redirect($"{newUrl}");
        }
    }

    private static void Add(string target)
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
            Save();
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
                Output.WriteLine($"Your comment will be attributed to the Subject. The Thumbprint will be stored so you can delete your comment.");
                Output.WriteLine($"=> {CgiVar.Url.Replace("view", "add")} add one!");
            }
            else
                Output.WriteLine($"> 🪪 No Certificate! You can only read comments but not write any.");

            Output.WriteLine($"### Comments");
            var list = Config.FileComments.Where(x => x.Key == target).Select(x => x.Value).FirstOrDefault(new List<Comment>());
            foreach (var comment in list.OrderByDescending(x=> x.TimeStamp))
            {
                foreach (var line in comment.Text.Split('\n'))
                    Output.WriteLine($"> {line}");
                Output.WriteLine($"> - {comment.Username} at {comment.TimeStamp}");
                if (comment.Thumbprint == CgiVar.CertHash)
                    Output.WriteLine($"=> {CgiVar.Url.Replace("view", "delete").Replace(target, comment.Id)} delete");
                Output.WriteLine();
            }
        }
        else
            Response.NotFound(target);
    }

    private static void Save()
    {
        var json = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Config.ConfigName, json);
    }
}