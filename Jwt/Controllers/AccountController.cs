using Jwt.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Jwt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost("Create")]
        public async Task<ActionResult<UserToken>> CreateAsync(UserInDTO userInDTO)
        {
            var user = new ApplicationUser() { UserName = userInDTO.Email, Email = userInDTO.Email };
            var result = await _userManager.CreateAsync(user, userInDTO.Password);
            if (result.Succeeded)
            {
                return BuildToken(userInDTO);
            }
            return BadRequest(result.Errors);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserToken>> LoginAsync(UserInDTO userInDTO)
        {
            var result = await _signInManager.PasswordSignInAsync(userInDTO.Email, userInDTO.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return BuildToken(userInDTO);
            }
            return BadRequest("Login failed attempt.");
        }

        private UserToken BuildToken(UserInDTO userInDTO)
        {
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, userInDTO.Email),
                new Claim("Mi Key", "Mi Valor"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddMinutes(5);

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                signingCredentials: creds,
                expires: expiration
                );

            return new UserToken
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token).ToString(),
                Expiration = expiration.ToString("dd/MM/yyyy hh:mm:ss")
            };
        }
    }
}
