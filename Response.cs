public static class Response
{
    public static void Ok(string content, string mimeType = "text/gemini", string language = "en")
    {
        if (CgiVar.IsGemini)
            Console.Write($"20 {mimeType}\r\n");
        if (CgiVar.IsSpartan)
            Console.Write($"2 {mimeType}\r\n");

        Console.Write(content);
    }
    public static void Input(string prompt)
    {
        if (CgiVar.IsGemini)
            Console.Write($"10 {prompt}\r\n");
    }
    public static void Redirect(string destination)
    {
        if (CgiVar.IsGemini)
            Console.Write($"30 {destination}\r\n");
        if (CgiVar.IsSpartan)
            Console.Write($"3 {destination}\r\n");
    }
    public static void NotFound(string file)
    {
        if (CgiVar.IsGemini)
            Console.Write($"51 {file}\r\n");
        if (CgiVar.IsSpartan)
            Console.Write($"5 {file}\r\n");
    }
    public static void BadRequest(string reason = "")
    {
        if (CgiVar.IsGemini)
            Console.Write($"50 {reason}\r\n");
        if (CgiVar.IsSpartan)
            Console.Write($"5 {reason}\r\n");
    }
    public static void CertRequired()
    {
        if (CgiVar.IsGemini)
            Console.Write($"60 Commenting requires a client certificate\r\n");
        if (CgiVar.IsSpartan)
            Console.Write($"5 Commenting requires a client certificate\r\n");
    }
    public static void CertInvalid()
    {
        if (CgiVar.IsGemini)
            Console.Write($"62 Commenting requires a *valid* client certificate\r\n");
        if (CgiVar.IsSpartan)
            Console.Write($"5 Commenting requires a *valid* client certificate\r\n");
    }
}