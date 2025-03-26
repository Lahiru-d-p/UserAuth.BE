using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserAuth.BE.Application.Services;
using UserAuth.BE.Domain.Dto;

namespace UserAuth.BE.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            var result = await _userService.RegisterAsync(userDto.Username, userDto.Password, userDto.Role);
            return Ok(new { Message = result });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userDto)
        {
            var token = await _userService.LoginAsync(userDto.Username, userDto.Password);
            if (token == "Invalid credentials") return Unauthorized(new { Message = token });
            return Ok(new { Token = token });
        }
    }
}
