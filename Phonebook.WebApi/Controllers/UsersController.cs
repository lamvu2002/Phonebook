using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Phonebook.Shared;
using Phonebook.Application.Interfaces.Services;
using Phonebook.Caching;
using Phonebook.Caching.Common;
using Phonebook.IdentityJWT.Authentication;
namespace Phonebook.WebApi.Controllers;

[Authorize(Roles = UserRoles.AdminRole)]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IDataCached _dataCached;
    public class UserSummaryDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
    }

    public UsersController(IUserService UserService, IDataCached dataCached)
    {
        _userService = UserService;
        _dataCached = dataCached;
    }
    //1. method get return a list Users
    // GET: api/Users
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<AspNetUser>))]
    public async Task<IEnumerable<UserSummaryDto>> GetUsers()
    {
        var users = await _userService.GetAllAsync();
        return users.Select(user => new UserSummaryDto
        {
            UserName = user.UserName,
            Email = user.Email
        });
    }

}