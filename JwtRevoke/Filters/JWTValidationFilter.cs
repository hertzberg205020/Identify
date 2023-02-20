using System.Net;
using System.Security.Claims;
using JwtRevoke.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace JwtRevoke.Filters;

public class JWTValidationFilter: IAsyncActionFilter
{
    private readonly IMemoryCache _memoryCache;
    private readonly UserManager<User> _userManager;

    public JWTValidationFilter(IMemoryCache memoryCache, UserManager<User> userManager)
    {
        _memoryCache = memoryCache;
        _userManager = userManager;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var claimUserId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        // 略過沒有傳送Token的請求
        // 沒有Token的話，會被有標註[Authorize]特性的Controller, Action擋住
        if (claimUserId is null)
        {
            await next();
            return;
        }

        // 根據JWT內Payload所提供的Id找出使用者
        long userId = long.Parse(claimUserId!.Value);
        string cacheKey = $"JWTValidationFilter.UserInfo.{userId}";
        User user = await _memoryCache.GetOrCreateAsync(cacheKey,  async e =>
        {
            e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
            return await _userManager.FindByIdAsync(userId.ToString());
        });
        
        // 找不到Payload提供Id所對應的使用者
        if (user is null)
        {
            var res = new ObjectResult($"UserId({userId}) not found");
            res.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Result = res;
            return;
        }

        var claimVersion = context.HttpContext.User.FindFirst(ClaimTypes.Version);
        // jwt中所保存的版本資訊
        long jwtVersionFromReq = long.Parse(claimVersion!.Value);
        
        // 由於使用記憶體快取所導致的併發問題
        // 假如集群中的A伺服器中的記憶體快取版本為5的數據
        // 但客戶端提交過來可能是版本6的數據，
        // 因此只要客戶端所提交的版本號 >= 伺服器上取出來的(可以是從DB 或是 快取 取出)版本號
        if (jwtVersionFromReq >= user.JWTVersion)
        {
            await next();
            return;
        }
        else
        {
            var res = new ObjectResult("JWTVersion mismatch");
            res.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Result = res;
            return;
        }
    }
}