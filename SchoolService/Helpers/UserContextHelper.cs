using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace SchoolService.Helpers
{
    public static class UserContextHelper
    {
        public class UserInfo
        {
            public int Id { get; set; }
            public string Email { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public static UserInfo? GetCurrentUser(HttpContext context)
        {
            var userJson = context.Items["User"] as string;
            if (string.IsNullOrEmpty(userJson)) return null;
            try
            {
                return JsonSerializer.Deserialize<UserInfo>(userJson);
            }
            catch
            {
                return null;
            }
        }
    }
}
