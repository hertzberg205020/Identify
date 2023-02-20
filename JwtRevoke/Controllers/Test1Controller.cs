using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtRevoke.DTO;
using JwtRevoke.Helper;
using JwtRevoke.Identity;
using JwtRevoke.Settings;


namespace JwtRevoke.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class Test1Controller: ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public Test1Controller(UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpPost]
    public async Task<ActionResult> Login(LoginRequest req,
        [FromServices] IOptionsSnapshot<JwtSetting> jwtSettingOpt)
    {
        string userName = req.UserName;
        string password = req.Password;

        var user = await _userManager.FindByNameAsync(userName);
        if (user is null)
        {
            return NotFound($"不存在{userName}名稱的帳戶");
        }

        var success = await _userManager.CheckPasswordAsync(user, password);
        if (!success)
        {
            return BadRequest($"密碼錯誤");
        }

        user.JWTVersion++;  // 更新JWT版本號
        await _userManager.UpdateAsync(user);

        var claims = new List<Claim>();
        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        claims.Add(new Claim(ClaimTypes.Name, user.UserName));
        claims.Add(new Claim(ClaimTypes.Version, user.JWTVersion.ToString()));
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        string jwtToken = JwtTokenHelper.BuildToken(claims, jwtSettingOpt.Value);
        return Ok(jwtToken);
    }
    
    [HttpPost]
    public async Task<ActionResult> CreateUserRole()
    {
        bool isRoleExist = await _roleManager.RoleExistsAsync("admin");
        if (!isRoleExist)
        {
            Role role = new Role() { Name = "admin" };
            var res = await _roleManager.CreateAsync(role);
            if (!res.Succeeded)
            {
                return BadRequest(res.Errors);
            }
        }

        User user = await _userManager.FindByNameAsync("Alice");
        if (user == null)
        {
            user = new User() { UserName = "Alice", Email = "Alice@gmail.com", EmailConfirmed = true };
            var res = await _userManager.CreateAsync(user, "123456");
            if (!res.Succeeded)
            {
                return BadRequest(res.Errors);
            }

            res = await _userManager.AddToRoleAsync(user, "admin");
            if (!res.Succeeded)
            {
                return BadRequest(res.Errors);
            }
        }

        return Ok();

    }
}