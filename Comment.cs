namespace atlasComments
{
    public class Comment
    {
        public string Id { get; set; }
        public string File { get; set; }
        public string Username { get; set; }
        public string Thumbprint { get; set; }
        public string Text { get; set; }
        public DateTime TimeStamp { get; set; }

        public Comment() { }
        public Comment(string username, string thumbprint, string text, string file)
        {
            TimeStamp = DateTime.Now;
            Username = username;
            Thumbprint = thumbprint;
            Text = text;
            Id = Guid.NewGuid().ToString().Replace("-", "");
            File = file;
        }
    }
}