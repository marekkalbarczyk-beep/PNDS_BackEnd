namespace PNDS_Auth.Services
{
    using System.Text.Json;
    using Microsoft.Extensions.Configuration;

    public class UserEntry
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UserService
    {
        private readonly string _filePath;

        public UserService(IConfiguration configuration)
        {
            // Odczytujemy wartość z sekcji UserSettings:JsonFilePath
            // Jeśli nie zostanie znaleziona, domyślnie używamy "users.json"
            _filePath = configuration["UserSettings:JsonFilePath"] ?? "users.json";
        }

        public async Task<bool> ValidateUser(string username, string password)
        {
            if (!File.Exists(_filePath)) return false;

            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                var users = JsonSerializer.Deserialize<List<UserEntry>>(json);

                return users?.Any(u => u.Username == username && u.Password == password) ?? false;
            }
            catch (JsonException)
            {
                // Błąd formatu pliku JSON
                return false;
            }
        }
    }
}
