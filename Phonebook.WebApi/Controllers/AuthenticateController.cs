using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Phonebook.IdentityJWT.Authentication;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace Phonebook.WebApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthenticateController : ControllerBase
{
    //fields
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthenticateController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
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
        IdentityUser user = new()
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
        IdentityUser user = new()
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

            // return
            return Ok(new
            {
                token = tokenValue,
                expiration = tokenValue.ValidTo
            });
        }
        return Unauthorized();
    }

    private JwtSecurityToken GeneratedToken(List<Claim> authClaims)
    {
        // get and convert private key to byte array
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
        // render token value (hash value)
        var tokenValue = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddHours(1),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );
        return tokenValue;
    }
}
