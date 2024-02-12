using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BuggyController : BaseApiController
{
    private readonly DataContext _context;
 
    public BuggyController(DataContext context)
    {
        _context = context;
    }
 
    [Authorize]
    [HttpGet("auth")]
    public ActionResult<string> GetSecret()
    {
        return "secret text";
    }
 
    [HttpGet("server-error")]
    public ActionResult<string> GetServerError()
    {
        return _context.Users.Find(-1).ToString();
    }
 
    [HttpGet("not-found")]
    public ActionResult<AppUser> GetNotFound()
    {
        var thing = _context.Users.Find(-1);
 
        if (thing == null)
            return NotFound();
        else
            return thing;
    }
 
    [HttpGet("bad-request")]
    public ActionResult<string> GetBadRequest()
    {
        return BadRequest("Not a valid request.");
    }
}
