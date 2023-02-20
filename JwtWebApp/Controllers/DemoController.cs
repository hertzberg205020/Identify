using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtWebApp.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JwtWebApp.Controllers;


[Route("api/[controller]/[action]")]
[ApiController]
public class DemoController: ControllerBase
{
    private readonly IOptionsSnapshot<JwtSetting> _jwtSettingOpt;

    public DemoController(IOptionsSnapshot<JwtSetting> jwtSettingOpt)
    {
        _jwtSettingOpt = jwtSettingOpt;
    }

    [HttpPost]
    public  ActionResult<string> Login(string userName, string password)
    {
        if (userName == "Alice" || password == "123456")
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, "1"));
            claims.Add(new Claim(ClaimTypes.Name, userName));

            JwtSetting jwtSetting = _jwtSettingOpt.Value;

            string key = jwtSetting.SecretKey;
            
            DateTime expireTime = DateTime.Now.AddMinutes(jwtSetting.ExpireSeconds);
            
            byte[] secBytes = Encoding.UTF8.GetBytes(key);
            var secKey = new SymmetricSecurityKey(secBytes);
            var credentials = new SigningCredentials(secKey,SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new JwtSecurityToken(claims: claims,
                expires: expireTime, signingCredentials: credentials);
            string jwt = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
            return Ok(jwt);
        }

        return BadRequest();
    }
}