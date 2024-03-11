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
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _CategoryService;
    private readonly IDataCached _dataCached;

    public CategoriesController(ICategoryService CategoryService, IDataCached dataCached)
    {
        _CategoryService = CategoryService;
        _dataCached = dataCached;
    }
    //1. method get return a list Categories
    // GET: api/Categories
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Category>))]
    public async Task<IEnumerable<Category>> GetCategories()
    {
        return await _CategoryService.GetAllAsync();
    }
    // GET: api/Categories/[id]
    // 2. get one by id
    [HttpGet("{id}", Name = nameof(GetCategory))] //name for route
    [ProducesResponseType(200, Type = typeof(Category))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCategory(int? id)
    {
        Category? p = await _CategoryService.GetOneAsync(id);
        if (p == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(p);
        }
    }
    // POST: api/Categories/(json/xml)
    // 3. using post method
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(Category))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] Category p)
    {
        try
        {
            if (p == null)
            {
                return BadRequest();
            }
            await _CategoryService.AddAsync(p);
            // return ok

            return CreatedAtRoute(routeName: nameof(GetCategory), routeValues: new { id = p.CategoryId }, value: p);
        }
        catch (Exception ex)
        {
            return BadRequest($"Repository failed to create the Category {p.CategoryId}");
        }
    }
    //4.update
    //PUT : api/Categories/[id]
    [Authorize(Roles = UserRoles.AdminRole)]
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(int? id, [FromBody] Category p)
    {
        if (p == null || p.CategoryId != id)
        {
            return BadRequest(); //400
        }
        Category? existing = await _CategoryService.GetOneAsync(id);
        if (existing == null)
        {
            return NotFound(); //404
        }
        await _CategoryService.UpdateAsync(p);
        return new NoContentResult();
    }
    //5.Delete
    //DELETE: api/Categories/[id]
    [Authorize(Roles = UserRoles.AdminRole)]
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Delete(int? id)
    {
        try
        {
            Category? existing = await _CategoryService.GetOneAsync(id);
            if (existing == null)
            {
                ProblemDetails problem = new()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://mydomain/api/Categories/failed-to-delete",
                    Title = $"Category ID {id} found but failed to delete",
                    Detail = "More details",
                    Instance = HttpContext.Request.Path
                };
                return BadRequest(problem);
            }
            await _CategoryService.DeleteAsync(id);
            return new NoContentResult(); //204
        }
        catch (Exception ex)
        {
            return BadRequest($"Category ID {id} was not found"); //400
        }
    }
}
