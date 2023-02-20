// See https://aka.ms/new-console-template for more information

// 將密鑰存到環境變數中

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var key = Environment.GetEnvironmentVariable("JWT:JwtSetting:SecretKey");

List<Claim> claims = new List<Claim>();
claims.Add(new Claim("Passport", "123456"));
claims.Add(new Claim(ClaimTypes.NameIdentifier, "1"));
claims.Add(new Claim(ClaimTypes.Name, "Alice"));
claims.Add(new Claim(ClaimTypes.Role, "6"));
claims.Add(new Claim(ClaimTypes.MobilePhone, "0932839361"));

DateTime expireTime = DateTime.Now.AddHours(1);

byte[] secBytes = Encoding.UTF8.GetBytes(key);
var secKey = new SymmetricSecurityKey(secBytes);
var credentials = new SigningCredentials(secKey,SecurityAlgorithms.HmacSha256Signature);
var tokenDescriptor = new JwtSecurityToken(claims: claims,
    expires: expireTime, signingCredentials: credentials);
string jwt = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

string path = @"c:\temp\jwt.txt";

File.WriteAllText(path, jwt, Encoding.UTF8);