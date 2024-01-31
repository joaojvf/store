using Identity.Api.Extensions;
using Identity.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Api.Controllers
{
    [Route("api/identity")]
    public class AuthController(SignInManager<IdentityUser> _signInManager, UserManager<IdentityUser> _userManager, IOptions<AppSettings> _appSettings) : MainController
    {
        [HttpPost("new-account")]
        public async Task<ActionResult> Register(UserRegister viewModel)
        {
            if (!ModelState.IsValid)
            {
                return CustomResponse(ModelState);
            }

            var user = new IdentityUser
            {
                UserName = viewModel.Email,
                Email = viewModel.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, viewModel.Password);
            if (!result.Succeeded)
            {
                result.Errors.ToList().ForEach(x => AddError(x.Description));
                return CustomResponse();
            }

            return CustomResponse(await JwtGenerate(viewModel.Email));
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult> Login(UserLogin viewModel)
        {
            if (!ModelState.IsValid)
            {
                return CustomResponse(ModelState);
            }

            var res = await _signInManager.PasswordSignInAsync(
                viewModel.Email,
                viewModel.Password,
                isPersistent: false,
                lockoutOnFailure: true);

            if (!res.Succeeded)
            {
                if (res.IsLockedOut)
                {
                    AddError("User temporary blocked. Too many tries.");
                }
                else
                {
                    AddError("User or Password incorrect");
                }

                return CustomResponse();
            }


            return CustomResponse(await JwtGenerate(viewModel.Email));
        }

        private async Task<UserLoginResponse> JwtGenerate(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);
            var identityClaims = await GetUserClaims(user, claims);
            var encodedToken = EncodeToken(identityClaims);       
            return BuildResponse(user, claims, encodedToken);
        }

        private async Task<ClaimsIdentity?> GetUserClaims(IdentityUser? user, IList<Claim> claims)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim("role", userRole));
            }

            return new ClaimsIdentity(claims);
        }

        private string EncodeToken(ClaimsIdentity identityClaims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Value.Secret);
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _appSettings.Value.Sender,
                Audience = _appSettings.Value.Domain,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(_appSettings.Value.ExperitationHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            });

            return tokenHandler.WriteToken(token);
        }

        private UserLoginResponse BuildResponse(IdentityUser user, IList<Claim> claims, string encodedToken) => new UserLoginResponse
        {
            AccessToken = encodedToken,
            ExpirateIn = TimeSpan.FromHours(_appSettings.Value.ExperitationHours).TotalSeconds,
            UserToken = new UserToken
            {
                Id = user.Id,
                Email = user.Email!,
                Claims = claims.Select(x => new UserClaim
                {
                    Type = x.Type,
                    Value = x.Value,
                })
            }
        };

        private static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

    }
}
