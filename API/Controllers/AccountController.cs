using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    [HttpPost("Register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO) //[FromBody] not required due to [ApiController] in BaseApiController
    {
        if (await UserExists(registerDTO.Username))
            return BadRequest("Username is already taken.");

        var user = _mapper.Map<AppUser>(registerDTO);
        user.UserName = registerDTO.Username.ToLower();

        var result = await _userManager.CreateAsync(user, registerDTO.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var roleResult = await _userManager.AddToRoleAsync(user, "Member");
        if(!roleResult.Succeeded)
            return BadRequest(result.Errors);

        return new UserDTO
        {
            Username = user.UserName,
            Token = await _tokenService.CreateToken(user),
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    [HttpPost("Login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO) //[FromBody] not required due to [ApiController] in BaseApiController
    {
        var user = await _userManager.Users.Include(p => p.Photos).SingleOrDefaultAsync(x => x.UserName == loginDTO.Username.ToLower());
        if (user == null) return Unauthorized("Invalid username.");

        var result = await _userManager.CheckPasswordAsync(user, loginDTO.Password);
        if (!result)
            return Unauthorized("Invalid password.");

        return new UserDTO
        {
            Username = user.UserName,
            Token = await _tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
}
