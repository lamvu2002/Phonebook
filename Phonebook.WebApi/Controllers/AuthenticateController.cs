using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Phonebook.IdentityJWT.Authentication;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;


namespace Phonebook.WebApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthenticateController : ControllerBase
{
    //fields
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthenticateController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }
    // create api to register user has roles admin
    [HttpPost]
    [Route("register-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] UserRegisterModel model)
    {
        // check user (model) da ton tai hay chua?
        var userExist = await _userManager.FindByNameAsync(model.UserName);
        if (userExist != null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseHeader { Status = "Error", Message = "User already exists..." });
        }
        // create new identityUser
        // khai bao 1 object user
        ApplicationUser user = new()
        {
            Email = model.Email,
            UserName = model.UserName,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        // apply into db via UserManager
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseHeader { Status = "Error", Message = "User created fail, please check user information input and try again..." });
        }
        // if register ok then add roles (Admin, User and Client) (tao ra 3 nhom quyen vao db)
        if (!await _roleManager.RoleExistsAsync(UserRoles.AdminRole))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.AdminRole));
        }
        if (!await _roleManager.RoleExistsAsync(UserRoles.UserRole))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.UserRole));
        }
        if (!await _roleManager.RoleExistsAsync(UserRoles.ClientRole))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.ClientRole));
        }
        // add user for each role
        if (await _roleManager.RoleExistsAsync(UserRoles.AdminRole))
        {
            await _userManager.AddToRoleAsync(user, UserRoles.AdminRole);
        }
        if (await _roleManager.RoleExistsAsync(UserRoles.UserRole))
        {
            await _userManager.AddToRoleAsync(user, UserRoles.UserRole);
        }
        if (await _roleManager.RoleExistsAsync(UserRoles.ClientRole))
        {
            await _userManager.AddToRoleAsync(user, UserRoles.ClientRole);
        }
        return Ok(new ResponseHeader { Status = "Success", Message = "User created ok..." });
    }
    [HttpPost]
    [Route("register-user")]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegisterModel model)
    {
        // check user (model) da ton tai hay chua?
        var userExist = await _userManager.FindByNameAsync(model.UserName);
        if (userExist != null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseHeader { Status = "Error", Message = "User already exists..." });
        }
        // create new user
        ApplicationUser user = new()
        {
            Email = model.Email,
            UserName = model.UserName,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        // apply into db via UserManager
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseHeader { Status = "Error", Message = "User created fail, please check user information input and try again..." });
        }
        if (!await _roleManager.RoleExistsAsync(UserRoles.UserRole))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.UserRole));
        }
        if (!await _roleManager.RoleExistsAsync(UserRoles.ClientRole))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.ClientRole));
        }
        if (await _roleManager.RoleExistsAsync(UserRoles.UserRole))
        {
            await _userManager.AddToRoleAsync(user, UserRoles.UserRole);
        }
        if (await _roleManager.RoleExistsAsync(UserRoles.ClientRole))
        {
            await _userManager.AddToRoleAsync(user, UserRoles.ClientRole);
        }
        return Ok(new ResponseHeader { Status = "Success", Message = "User created ok..." });
    }
    // login and generate token value
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> UserLogin([FromBody] UserLoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.UserName);
        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            var userRoles = await _userManager.GetRolesAsync(user);// lay ra all roles cho user
            var authClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            // if user has roles then add roles to claim
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            // render token
            var tokenValue = GeneratedToken(authClaims);
            var refreshToken = GenerateRefreshToken();
            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);

            await _userManager.UpdateAsync(user);

            var tokenHandler = new JwtSecurityTokenHandler();
            var encodedToken = tokenHandler.WriteToken(tokenValue);
            // return
            return Ok(new
            {
                token = encodedToken,
                RefreshToken = refreshToken,
                expiration = tokenValue.ValidTo
            });
        }
        return Unauthorized();
    }
    [HttpPost]
    [Route("refresh")]
    public async Task<IActionResult> RefreshToken(TokenModel tokenModel)
    {
        if (tokenModel is null)
        {
            return BadRequest("Invalid client request");
        }

        string? accessToken = tokenModel.AccessToken;
        string? refreshToken = tokenModel.RefreshToken;

        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal == null)
        {
            return BadRequest("Invalid access token or refresh token");
        }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        string username = principal.Identity.Name;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        var user = await _userManager.FindByNameAsync(username);

        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
        {
            return BadRequest("Invalid access token or refresh token");
        }

        var newAccessToken = GeneratedToken(principal.Claims.ToList());
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        await _userManager.UpdateAsync(user);

        return new ObjectResult(new
        {
            accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
            refreshToken = newRefreshToken
        });
    }

    [Authorize]
    [HttpPost]
    [Route("revoke/{username}")]
    public async Task<IActionResult> Revoke(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null) return BadRequest("Invalid user name");

        user.RefreshToken = null;
        await _userManager.UpdateAsync(user);

        return NoContent();
    }

    [Authorize]
    [HttpPost]
    [Route("revoke-all")]
    public async Task<IActionResult> RevokeAll()
    {
        var users = _userManager.Users.ToList();
        foreach (var user in users)
        {
            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);
        }

        return NoContent();
    }
    private JwtSecurityToken GeneratedToken(List<Claim> authClaims)
    {
        // get and convert private key to byte array
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
        _ = int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out int tokenValidityInMinutes);
        // render token value (hash value)
        var tokenValue = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddMinutes(tokenValidityInMinutes),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );
        return tokenValue;
    }
    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;

    }
}
