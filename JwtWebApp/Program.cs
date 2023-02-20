using System.Text;
using JwtWebApp.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace JwtWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            # region swagger設置在請求中加入JWT授權
            
            builder.Services.AddSwaggerGen(c =>
            {
                var scheme = new OpenApiSecurityScheme()
                {
                    Description = "Authorization header. \r\nExample: 'Bearer 12345abcdef'",
                    Reference = new OpenApiReference{Type = ReferenceType.SecurityScheme,
                        Id = "Authorization"},
                    Scheme = "oauth2",Name = "Authorization",
                    In = ParameterLocation.Header,Type = SecuritySchemeType.ApiKey,
                };
                c.AddSecurityDefinition("Authorization", scheme);
                var requirement = new OpenApiSecurityRequirement();
                requirement[scheme] = new List<string>();
                c.AddSecurityRequirement(requirement);
            }); 
            
            # endregion
            
            # region 讀取設置
            
            // GetSection("僅能讀取第一層的JSON的key")
            // 若要讀取多層JSON下的結構要使用 ":"
            
            // {
            //     "Jwt": {
            //         "JwtSetting": {
            //             "SecretKey": "adsjfhlafkjhbe221f",
            //             "ExpireSeconds": "3600"
            //         }
            //     }
            // }
            
            // 例如: GetSection("JWT:JwtSetting")
            builder.Services.Configure<JwtSetting>(builder.Configuration.GetSection("JWT:JwtSetting"));
            
            # endregion
            
            # region JWT

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    var JwtSetting = builder.Configuration.GetSection("JWT:JwtSetting").Get<JwtSetting>();
                    byte[] keyBytes = Encoding.UTF8.GetBytes(JwtSetting.SecretKey);
                    var secKey = new SymmetricSecurityKey(keyBytes);
                    opt.TokenValidationParameters = new()
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = secKey
                    };
                });
            
            
            # endregion
            
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            # region JWT

            app.UseAuthentication();
            
            # endregion
            
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}