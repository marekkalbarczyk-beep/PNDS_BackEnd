using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using PNDS_BackEnd_Dev.Services;
using System.Reflection.Metadata.Ecma335;

namespace PNDS_BackEnd_Dev.Services
{


    public class shipData
    {
        public string? shipName { get; set; }
        public string? shipPassword { get; set; }
        public DateTime? shipExpire { get; set; }
        public string? shipOwner { get; set; }
    }


    public class ShipService
    {
        private readonly string _filePath;

        public ShipService(IConfiguration configuration)
        {
            // Odczytujemy wartość z sekcji UserSettings:JsonFilePath
            // Jeśli nie zostanie znaleziona, domyślnie używamy "users.json"
            _filePath = configuration["ShipSettings:ShipJsonFile"] ?? "ships.json";
        }

        public async Task<int> ValidateUser(string username, string password)
        {
            if (!File.Exists(_filePath)) 
            {
                Console.WriteLine(DateTime.Now.ToString() + " ship database file is missing");
                return -99; 
            }

            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                var ships = JsonSerializer.Deserialize<List<shipData>>(json);
                //string doubleHashedPassword = ComputeSha256Hash(password);

                var dateNow = DateTime.UtcNow;
                if (ships != null)
                {
#pragma warning disable CS8602 // Wyłuskanie odwołania, które może mieć wartość null.
                    shipData? shipTMP = ships.FirstOrDefault(u => u.shipName.Equals(username, StringComparison.CurrentCultureIgnoreCase));
#pragma warning restore CS8602 // Wyłuskanie odwołania, które może mieć wartość null.
                    if (shipTMP == null) {
                        Console.WriteLine(DateTime.Now.ToString() + " Ship validation " + username + " -1 : no ship name in database");
                        return -1;
                    }
                    if ((shipTMP.shipPassword ?? String.Empty ) != ComputeSha256Hash(password))
                    {
                        Console.WriteLine(DateTime.Now.ToString() + " Ship validation " + username + " -2 : wrong password");
                        return -2;
                    }
                    if (!shipTMP.shipExpire.HasValue)
                    {
                        Console.WriteLine(DateTime.Now.ToString() + " Ship validation " + username + " -3 : no ship exp date in database");
                        return -3;
                    }
                    if (shipTMP.shipExpire < dateNow)
                    {
                        Console.WriteLine(DateTime.Now.ToString() + " Ship validation " + username + " -4 : ship account expired");
                        return -4;
                    }
                    Console.WriteLine(DateTime.Now.ToString() + " Ship validation " + username + " 1 : Success"); 
                    return 1;
                    //return ships?.Any(u => u.shipName == username && u.shipPassword == password && u.shipExpire >= dateNow) ?? false;
                }
                return -97;
            }
            catch (JsonException)
            {
                // Błąd formatu pliku JSON
                Console.WriteLine(DateTime.Now.ToString() + " Błod formatu pliku ships.json") ;
                return -98;
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
