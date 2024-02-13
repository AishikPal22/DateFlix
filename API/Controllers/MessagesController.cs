using API.Controllers;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API;

public class MessagesController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _messageRepository = messageRepository;
        _mapper = mapper;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
    {
        var username = User.GetUsername();
        if (username == createMessageDTO.RecipientUsername.ToLower())
            return BadRequest("Self-message cannot be sent.");

        var sender = await _userRepository.GetUserByUsernameAsync(username);
        var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDTO.RecipientUsername);
        if (recipient == null)
            return NotFound();

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDTO.Content
        };

        _messageRepository.AddMessage(message);

        if (await _messageRepository.SaveAllAsync())
            return Ok(_mapper.Map<MessageDTO>(message));
        else
            return BadRequest("Failed to send message.");
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<PagedList<MessageDTO>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
        messageParams.Username = User.GetUsername();

        var messages = await _messageRepository.GetMessagesForUser(messageParams);

        Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.PageSize));

        return messages;
    }

    [Authorize]
    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessageThread(string username)
    {
        var currentUserName = User.GetUsername();
        return Ok(await _messageRepository.GetMessageThread(currentUserName, username));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUsername();
        var message = await _messageRepository.GetMessage(id);

        if (message.SenderUsername != username && message.RecipientUsername != username)
            return Unauthorized();

        if (message.SenderUsername == username)
            message.SenderDeleted = true;

        if (message.RecipientUsername == username)
            message.RecipientDeleted = true;

        if (message.SenderDeleted && message.RecipientDeleted)
            _messageRepository.DeleteMessage(message);

        if (await _messageRepository.SaveAllAsync())
            return Ok();
        else
            return BadRequest("Message could not be deleted.");
    }
}
