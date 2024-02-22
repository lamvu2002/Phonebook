using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Phonebook.Shared;
using Phonebook.Application.Interfaces.Services;
using Phonebook.Caching;
using Phonebook.Caching.Common;
using Phonebook.IdentityJWT.Authentication;
namespace Phonebook.WebApi.Controllers;

//[Authorize(Roles = UserRoles.AdminRole)]
[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private readonly IContactService _ContactService;
    private readonly IDataCached _dataCached;

    public ContactsController(IContactService ContactService, IDataCached dataCached)
    {
        _ContactService = ContactService;
        _dataCached = dataCached;
    }
    //1. method get return a list Contacts
    // GET: api/Contacts
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Contact>))]
    public async Task<IEnumerable<Contact>> GetContacts()
    {
        return await _ContactService.GetAllAsync();
    }
    // GET: api/Contacts/[id]
    // 2. get one by id
    [HttpGet("{id}", Name = nameof(GetContact))] //name for route
    [ProducesResponseType(200, Type = typeof(Contact))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetContact(int? id)
    {
        Contact? p = await _ContactService.GetOneAsync(id);
        if (p == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(p);
        }
    }
    // POST: api/Contacts/(json/xml)
    // 3. using post method
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(Contact))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] Contact p)
    {
        try
        {
            if (p == null)
            {
                return BadRequest();
            }
            await _ContactService.AddAsync(p);
            // return ok

            return CreatedAtRoute(routeName: nameof(GetContact), routeValues: new { id = p.ContactId }, value: p);
        }
        catch (Exception ex)
        {
            return BadRequest($"Repository failed to create the Contact {p.ContactId}");
        }
    }
    //4.update
    //PUT : api/Contacts/[id]
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(int? id, [FromBody] Contact p)
    {
        if (p == null || p.ContactId != id)
        {
            return BadRequest(); //400
        }
        Contact? existing = await _ContactService.GetOneAsync(id);
        if (existing == null)
        {
            return NotFound(); //404
        }
        await _ContactService.UpdateAsync(p);
        return new NoContentResult();
    }
    //5.Delete
    //DELETE: api/Contacts/[id]
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Delete(int? id)
    {
        try
        {
            Contact? existing = await _ContactService.GetOneAsync(id);
            if (existing == null)
            {
                ProblemDetails problem = new()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://mydomain/api/Contacts/failed-to-delete",
                    Title = $"Contact ID {id} found but failed to delete",
                    Detail = "More details",
                    Instance = HttpContext.Request.Path
                };
                return BadRequest(problem);
            }
            await _ContactService.DeleteAsync(id);
            return new NoContentResult(); //204
        }
        catch (Exception ex)
        {
            return BadRequest($"Contact ID {id} was not found"); //400
        }
    }
}
