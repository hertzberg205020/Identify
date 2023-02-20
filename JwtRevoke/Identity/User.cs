using Microsoft.AspNetCore.Identity;

namespace JwtRevoke.Identity;

public class User: IdentityUser<long>
{
    public DateTime CreationTime { get; set; } = DateTime.Now;
    public string? NickName { get; set; }
    public long JWTVersion { get; set; }
}