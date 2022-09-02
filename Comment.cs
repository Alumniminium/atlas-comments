public class Comment
{
    public string Username { get; set; }
    public string Thumbprint { get; set; }
    public string Text{ get; set; }
    public DateTime TimeStamp { get; set; }

    public Comment() { }
    public Comment(string username, string thumbprint, string text)
    {
        TimeStamp= DateTime.Now;
        Username = username;
        Thumbprint = thumbprint;
        Text = text;
    }
}