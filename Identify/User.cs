using Microsoft.AspNetCore.Identity;

namespace Identify;

public class User: IdentityUser<long>
{
    public DateTime CreateTime { get; set; }
    public string? NickName { get; set; }
}