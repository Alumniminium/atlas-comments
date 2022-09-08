public static class CgiVar
{
    public static string Version => Environment.GetEnvironmentVariable("GATEWAY_INTERFACE").ToUpperInvariant();
    public static string Protocol => Environment.GetEnvironmentVariable("SERVER_PROTOCOL").ToUpperInvariant();
    public static string StrPort => Environment.GetEnvironmentVariable("SERVER_PORT");
    public static string ServerSoftware => Environment.GetEnvironmentVariable("SERVER_SOFTWARE");
    public static string Url => Environment.GetEnvironmentVariable("URL");
    public static string ScriptName => Environment.GetEnvironmentVariable("SCRIPT_NAME");
    public static string PathInfo => Environment.GetEnvironmentVariable("PATH_INFO");
    public static string Query => Environment.GetEnvironmentVariable("QUERY_STRING");
    public static string FQDN => Environment.GetEnvironmentVariable("SERVER_NAME");
    public static string RemoteHost => Environment.GetEnvironmentVariable("REMOTE_HOST");
    public static string RemoteIp => Environment.GetEnvironmentVariable("REMOTE_ADDR");
    public static string AuthType => Environment.GetEnvironmentVariable("AUTH_TYPE").ToUpperInvariant();
    public static string StrTlsVersion => Environment.GetEnvironmentVariable("TLS_VERSION");
    public static string RemoteUser => Environment.GetEnvironmentVariable("REMOTE_USER");
    public static string StrCertValid => Environment.GetEnvironmentVariable("TLS_CLIENT_VALID");
    public static string StrCertTrusted => Environment.GetEnvironmentVariable("TLS_CLIENT_TRUSTED");
    public static string CertSubject => Environment.GetEnvironmentVariable("TLS_CLIENT_SUBJECT");
    public static string CertHash => Environment.GetEnvironmentVariable("TLS_CLIENT_HASH");
    public static string StrCertValidFrom => Environment.GetEnvironmentVariable("TLS_CLIENT_NOT_BEFORE");
    public static string StrCertValidTo => Environment.GetEnvironmentVariable("TLS_CLIENT_NOT_AFTER");
    public static string CertSerialNumber => Environment.GetEnvironmentVariable("TLS_CLIENT_SERIAL_NUMBER");


    public static bool IsGemini => Protocol == "GEMINI";
    public static bool IsSpartan => Protocol == "SPARTAN";
    public static ushort Port => ushort.Parse(StrPort);
    public static bool HasCert => AuthType == "CERTIFICATE";
    public static float TlsVersion => float.Parse(StrTlsVersion);
    public static bool CertValid => StrCertValid.ToUpperInvariant() == "TRUE";
    public static bool CertTrusted => StrCertTrusted.ToUpperInvariant() == "TRUE";
    public static DateTime CertValidFrom => DateTime.Parse(StrCertValidFrom);
    public static DateTime CertValidTo => DateTime.Parse(StrCertValidTo);
}
