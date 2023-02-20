using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

string path = @"c:\temp\jwt.txt";
string jwt = File.ReadAllText(path, Encoding.UTF8);

string secKey = Environment.GetEnvironmentVariable("jwt_secret");
JwtSecurityTokenHandler tokenHandler = new();
TokenValidationParameters valParam = new();
var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secKey));
valParam.IssuerSigningKey = securityKey;
valParam.ValidateIssuer = false;
valParam.ValidateAudience = false;
ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(jwt,
    valParam, out SecurityToken secToken);
foreach (var claim in claimsPrincipal.Claims)
{
    Console.WriteLine($"{claim.Type}={claim.Value}");
}