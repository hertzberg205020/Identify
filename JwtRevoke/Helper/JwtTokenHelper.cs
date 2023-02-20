using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtRevoke.Settings;
using Microsoft.IdentityModel.Tokens;

namespace JwtRevoke.Helper;

public class JwtTokenHelper
{
    public static string BuildToken(IEnumerable<Claim> claims, JwtSetting jwtSetting)
    {
        DateTime expires = DateTime.Now.AddSeconds(jwtSetting.ExpireSeconds);
        byte[] keyBytes = Encoding.UTF8.GetBytes(jwtSetting.SecretKey);
        var secKey = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(secKey,
            SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new JwtSecurityToken(expires: expires,
            signingCredentials: credentials, claims: claims);
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}