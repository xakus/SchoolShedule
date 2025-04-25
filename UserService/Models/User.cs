using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    /// <summary>
    /// Модель пользователя для регистрации и авторизации по email/паролю
    /// </summary>
    public class User
    {
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Email пользователя (уникальный)
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Хеш пароля пользователя
        /// </summary>
        [Required]
        public string PasswordHash { get; set; } = null!;

        /// <summary>
        /// Дата регистрации
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
