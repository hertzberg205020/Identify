using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identify.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;

        public TestController(ILogger<TestController> logger, RoleManager<Role> roleManager, UserManager<User> userManager)
        {
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
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

        [HttpPost]
        public async Task<ActionResult> Login(LoginRequest req)
        {
            string userName = req.userName;
            string password = req.password;

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound($"{userName}???????????????");
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return BadRequest($"????????????????????????");
            }

            var success = await _userManager.CheckPasswordAsync(user, password);
            if (success)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
                return Ok("Success");
            }

            var res = await _userManager.AccessFailedAsync(user);
            if (!res.Succeeded)
            {
                return BadRequest("AccessFailed failed");
            }

            return BadRequest("Fail");

        }
        
        [HttpPost]
        public async Task<ActionResult> SendResetPasswordToken([FromQuery]string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound($"??????????????????{userEmail}");
            }

            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            _logger.LogInformation($"???????????????{userEmail}??????token: {token}");
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> VerifyResetPasswordToken(string userEmail,
            string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                return NotFound($"??????????????????????????????{userEmail}?????????");
            }
            var res = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (res.Succeeded)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
                return Ok("?????????????????????");
            }

            // await _userManager.AccessFailedAsync(user);
            return BadRequest("??????????????????");
        }
    }
}
