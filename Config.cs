using System.Text.Json;
using System.Text.Json.Serialization;

namespace atlasComments
{
    [JsonSerializable(typeof(Config))]
    internal partial class MyJsonContext : JsonSerializerContext
    {
    }
    public class Config
    {
        public Dictionary<string,string> FileSourcePath { get; set; } = new()
        {
            { "/srv/gemini/her.st/blog/", "blog/" },
            { "/srv/gemini/her.st/pages/", "pages/" },
        };
        public Dictionary<string, List<Comment>> FileComments { get; set; } = new();
        public Dictionary<string, string> OriginalFiles { get; set; } = new();
        
        public async ValueTask SaveAsync(string path)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(this, MyJsonContext.Default.Config);
            await File.WriteAllBytesAsync(path, json);
        }
    }
}