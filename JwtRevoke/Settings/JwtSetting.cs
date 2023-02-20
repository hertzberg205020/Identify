namespace JwtRevoke.Settings;

public class JwtSetting
{
    public string SecretKey { get; set; }
    public int ExpireSeconds { get; set; }
}