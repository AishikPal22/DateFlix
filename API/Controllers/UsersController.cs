using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class UsersController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;

    public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _photoService = photoService;
    }

    //synchronous
    // [HttpGet]
    // public ActionResult<IEnumerable<AppUser>> GetUsers()
    // {
    //     var users = _context.Users.ToList();
    //     return users;
    // }

    //asynchronous
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<PagedList<MemberDTO>>> GetUsers([FromQuery] UserParams userParams)
    {
        var currentUser = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
        userParams.CurrentUsername = currentUser.UserName;

        if (string.IsNullOrEmpty(userParams.Gender))
            userParams.Gender = currentUser.Gender == "male" ? "female" : "male";

        var users = await _userRepository.GetMembersAsync(userParams);
        // var usersToReturn = _mapper.Map<IEnumerable<MemberDTO>>(users);
        Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));
        return Ok(users);
    }

    //synchronous
    // [HttpGet("{id}")]
    // public ActionResult<AppUser> GetUser(int id)
    // {
    //     var user = _context.Users.Find(id);
    //     return user;
    // }

    //asynchronous

    // [Authorize]
    // [HttpGet("{id}")]
    // public async Task<ActionResult<AppUser>> GetUser(int id)
    // {
    //     return await _userRepository.GetUserByIdAsync(id);
    // }

    [Authorize]
    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDTO>> GetUser(string username)
    {
        return await _userRepository.GetMemberByUsernameAsync(username);
        //return _mapper.Map<MemberDTO>(user);
    }

    [Authorize]
    [HttpPut]
    public async Task<ActionResult> UpdateUser(UpdateDTO updateDTO)
    {
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null) return NotFound();

        _mapper.Map(updateDTO, user);

        if (await _userRepository.SaveAllAsync())
            return NoContent();
        else
            return BadRequest("Failed to make changes.");
    }

    [Authorize]
    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
    {
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null)
            return NotFound();

        var result = await _photoService.AddPhotoAsync(file);
        if (result.Error != null)
            return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        if (user.Photos.Count == 0)
            photo.IsMain = true;

        user.Photos.Add(photo);

        if (await _userRepository.SaveAllAsync())
            return CreatedAtAction(nameof(GetUser), new { username = user.UserName }, _mapper.Map<PhotoDTO>(photo));
        else
            return BadRequest("Problem adding photo.");
    }

    [Authorize]
    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null)
            return NotFound();

        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null)
            return NotFound();

        if (photo.IsMain)
            return BadRequest("THis is already your main photo.");

        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
        if (currentMain != null)
            currentMain.IsMain = false;

        photo.IsMain = true;

        if (await _userRepository.SaveAllAsync())
            return NoContent();
        else
            return BadRequest("Problem setting photo as Main.");
    }

    [Authorize]
    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

        if (photo == null)
            return NotFound();

        if (photo.IsMain)
            return BadRequest("Main photo cannot be deleted.");

        if (photo.PublicId != null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null)
                return BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);

        if (await _userRepository.SaveAllAsync())
            return Ok();
        else
            return BadRequest("Problem deleting photo.");
    }
}