using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly Services.AuthService _authService;

        public AuthController(Services.AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        /// <param name="request">Email и пароль</param>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var (success, error) = await _authService.RegisterAsync(request.Email, request.Password);
            if (!success)
                return BadRequest(error);
            return Ok("Регистрация успешна");
        }

        /// <summary>
        /// Авторизация пользователя (логин)
        /// </summary>
        /// <param name="request">Email и пароль</param>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var (success, error, token) = await _authService.LoginAsync(request.Email, request.Password);
            if (!success)
                return Unauthorized(error);
            return Ok(new { token });
        }


    }

}
