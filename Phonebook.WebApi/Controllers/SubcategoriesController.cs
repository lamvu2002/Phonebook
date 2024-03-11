using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Phonebook.Shared;
using Phonebook.Application.Interfaces.Services;
using Phonebook.Caching;
using Phonebook.Caching.Common;
using Phonebook.IdentityJWT.Authentication;
namespace Phonebook.WebApi.Controllers;


[ApiController]
[Route("api/[controller]")]
public class SubcategoriesController : ControllerBase
{
    private readonly ISubcategoryService _SubcategoryService;
    private readonly IDataCached _dataCached;

    public SubcategoriesController(ISubcategoryService SubcategoryService, IDataCached dataCached)
    {
        _SubcategoryService = SubcategoryService;
        _dataCached = dataCached;
    }
    //1. method get return a list Subcategories
    // GET: api/Subcategories
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Subcategory>))]
    public async Task<IEnumerable<Subcategory>> GetSubcategories()
    {
        return await _SubcategoryService.GetAllAsync();
    }
    // GET: api/Subcategories/[id]
    // 2. get one by id
    [HttpGet("{id}", Name = nameof(GetSubcategory))] //name for route
    [ProducesResponseType(200, Type = typeof(Subcategory))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSubcategory(int? id)
    {
        Subcategory? p = await _SubcategoryService.GetOneAsync(id);
        if (p == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(p);
        }
    }
    // POST: api/Subcategories/(json/xml)
    // 3. using post method
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(Subcategory))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] Subcategory p)
    {
        try
        {
            if (p == null)
            {
                return BadRequest();
            }
            await _SubcategoryService.AddAsync(p);
            // return ok

            return CreatedAtRoute(routeName: nameof(GetSubcategory), routeValues: new { id = p.SubcategoryId }, value: p);
        }
        catch (Exception ex)
        {
            return BadRequest($"Repository failed to create the Subcategory {p.SubcategoryId}");
        }
    }
    //4.update
    //PUT : api/Subcategories/[id]
    [Authorize(Roles = UserRoles.AdminRole)]
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(int? id, [FromBody] Subcategory p)
    {
        if (p == null || p.SubcategoryId != id)
        {
            return BadRequest(); //400
        }
        Subcategory? existing = await _SubcategoryService.GetOneAsync(id);
        if (existing == null)
        {
            return NotFound(); //404
        }
        await _SubcategoryService.UpdateAsync(p);
        return new NoContentResult();
    }
    //5.Delete
    //DELETE: api/Subcategories/[id]
    [Authorize(Roles = UserRoles.AdminRole)]
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Delete(int? id)
    {
        try
        {
            Subcategory? existing = await _SubcategoryService.GetOneAsync(id);
            if (existing == null)
            {
                ProblemDetails problem = new()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://mydomain/api/Subcategories/failed-to-delete",
                    Title = $"Subcategory ID {id} found but failed to delete",
                    Detail = "More details",
                    Instance = HttpContext.Request.Path
                };
                return BadRequest(problem);
            }
            await _SubcategoryService.DeleteAsync(id);
            return new NoContentResult(); //204
        }
        catch (Exception ex)
        {
            return BadRequest($"Subcategory ID {id} was not found"); //400
        }
    }
}
