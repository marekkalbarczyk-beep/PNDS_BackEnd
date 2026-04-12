namespace PNDS_Ship_Mngr.Services
{
    using System.Security.Cryptography;
    using System.Text;
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
                string doubleHashedPassword = ComputeSha256Hash(password);

                return users?.Any(u => u.Username == username && u.Password == doubleHashedPassword) ?? false;
            }
            catch (JsonException)
            {
                // Błąd formatu pliku JSON
                return false;
            }
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
