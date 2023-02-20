using System.Text;
using JwtRevoke.Filters;
using JwtRevoke.Identity;
using JwtRevoke.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace JwtRevoke
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
            
            # region 讀取設定
            
            builder.Services.Configure<JwtSetting>(builder.Configuration.GetSection("JWT:JwtSetting"));
            
            # endregion
            
            # region register JWT service

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    var JwtSetting = builder.Configuration.GetSection("JWT:JwtSetting").Get<JwtSetting>();
                    byte[] keyBytes = Encoding.UTF8.GetBytes(JwtSetting.SecretKey);
                    var secKey = new SymmetricSecurityKey(keyBytes);
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = secKey
                    };
                });
            
            # endregion
            
            # region register identity service

            IServiceCollection services = builder.Services;
            services.AddDbContext<IdDbContext>(opt =>
            {
                string connStr = builder.Configuration.GetConnectionString("Demo3");
                opt.UseSqlServer(connStr);
            });

            services.AddDataProtection();
            
            services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
            });
            var idBuilder = new IdentityBuilder(typeof(User), typeof(Role), services);
            idBuilder.AddEntityFrameworkStores<IdDbContext>()
                .AddDefaultTokenProviders()
                .AddRoleManager<RoleManager<Role>>()
                .AddUserManager<UserManager<User>>();
            
            # endregion

            # region 使用記憶體快取
            
            builder.Services.AddMemoryCache();
            
            # endregion
            
            # region 註冊JWTValidationFilter

            builder.Services.Configure<MvcOptions>(opt =>
            {
                opt.Filters.Add<JWTValidationFilter>();
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
            
            # region JWT middleware

            app.UseAuthentication();
            
            # endregion

            app.UseAuthorization();
            
            app.MapControllers();

            app.Run();
        }
    }
}