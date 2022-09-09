using System.Text.Json;

namespace atlasComments
{
    public class Config
    {
        public const string ConfigName = "atlas-comments.json";
        public string FileSourcePath { get; set; } = "/srv/gemini/her.st/blog/";
        public Dictionary<string, List<Comment>> FileComments { get; set; } = new();
        
        public async ValueTask SaveAsync()
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(ConfigName, json);
        }
    }
}