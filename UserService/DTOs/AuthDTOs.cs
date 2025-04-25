namespace UserService.DTOs
{
    /// <summary>
    /// DTO для регистрации
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>Email пользователя</summary>
        public string Email { get; set; } = null!;
        /// <summary>Пароль</summary>
        public string Password { get; set; } = null!;
    }

    /// <summary>
    /// DTO для логина
    /// </summary>
    public class LoginRequest
    {
        /// <summary>Email пользователя</summary>
        public string Email { get; set; } = null!;
        /// <summary>Пароль</summary>
        public string Password { get; set; } = null!;
    }
}
