using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController : BaseApiController
{
private readonly IUserRepository _userRepository;
    private readonly ILikesRepository _likesRepository;
 
    public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
    {
        _userRepository = userRepository;
        _likesRepository = likesRepository;
    }
 
    [HttpPost("{username}")]
    public async Task<ActionResult> AddLike(string username)
    {
        var sourcerUserId = User.GetUserId();
        var likedUser = await _userRepository.GetUserByUsernameAsync(username);
        var sourceUser = await _likesRepository.GetUserWithLikes(sourcerUserId);
 
        if (likedUser == null) return NotFound();
 
        if (sourceUser.UserName == username) return BadRequest("Self Like is prohibited.");
 
        var userLike = await _likesRepository.GetUserLike(sourcerUserId, likedUser.Id);
 
        if (userLike != null) return BadRequest("You already liked this person.");
 
        userLike = new UserLike
        {
            SourceUserId = sourcerUserId,
            TargetUserId = likedUser.Id
        };
 
        sourceUser.LikedUsers.Add(userLike);
 
        if (await _userRepository.SaveAllAsync())
            return Ok();
        else
            return BadRequest("Failed to like user.");
    }
 
    [HttpGet]
    public async Task<ActionResult<PagedList<LikeDTO>>> GetUserLikes([FromQuery] LikesParams likesParams)
    {
        likesParams.UserId = User.GetUserId();
        var users = await _likesRepository.GetUserLikes(likesParams);
        Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));
        return Ok(users);
    }
}
