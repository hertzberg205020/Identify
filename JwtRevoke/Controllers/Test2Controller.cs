using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtRevoke.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
[Authorize]
public class Test2Controller: ControllerBase
{
    [HttpGet]
    public ActionResult Hello()
    {
        string id = this.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        string userName = this.User.FindFirst(ClaimTypes.Name)!.Value;
        IEnumerable<Claim> roleClaims = User.FindAll(ClaimTypes.Role);
        string roleNames = string.Join(',', roleClaims.Select(c => c.Value));
        return Ok($"id={id},userName={userName},roleNames ={roleNames}");
    }
}