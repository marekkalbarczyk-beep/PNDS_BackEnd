using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PNDS_Ship_Mngr.Interfaces;

namespace PNDS_Ship_Mngr.Models
{
    public class shipList : shipListInterface
    {
        private static List<shipData> _localShipList = new();

        public  shipList()
        {
            Console.WriteLine("shipList Constructor - trying to load ship database");
            try
            {
                StreamReader _reader = new StreamReader(@"c:\PNDS\ships.json");
                var _json = _reader.ReadToEnd();
                _reader.Close();
                _reader.Dispose();
                var _jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var loadedData = JsonSerializer.Deserialize<List<shipData>>(_json, _jsonOptions);
                if (loadedData != null)
                {
                    _localShipList = loadedData;
                }
                else
                {
                    _localShipList = new();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load ship database file");
                Console.WriteLine(ex.Message);
                _localShipList = new();
            }
            
        }

        private bool writeShipList()
        {
            Console.WriteLine("ShipData JSON file write to disk");
            try
            {
                var jsonOptions = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                };
                string json = JsonSerializer.Serialize(_localShipList, jsonOptions);
                File.WriteAllText(@"c:\PNDS\ships.json", json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return true;

        }

        public IEnumerable<shipData> GetShips()
        {
            return _localShipList;
        }

        public shipData GetShip(string _shipName)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return _localShipList.Where(shipData => shipData.shipName == _shipName).SingleOrDefault();
#pragma warning restore CS8603 // Possible null reference return.
        }

        public bool UpdateShipPass(string _shipName, string _shipPassword, DateTime _shipExpire, string _shipOwner)
        {
            var _tmpShip = _localShipList.FirstOrDefault(shipData => shipData.shipName == _shipName);
            //return adam.setOutput(output, value);
            if (_tmpShip == null)
            {
                return false;
            }
            _tmpShip.shipPassword = ComputeSha256Hash(_shipPassword);
            _tmpShip.shipExpire = _shipExpire;
            _tmpShip.shipOwner = _shipOwner;
            writeShipList();
            return true;
        }

        public bool UpdateShipNoPass(string _shipName, DateTime _shipExpire, string _shipOwner)
        {
            var _tmpShip = _localShipList.FirstOrDefault(shipData => shipData.shipName == _shipName);
            //return adam.setOutput(output, value);
            if (_tmpShip == null)
            {
                return false;
            }
            _tmpShip.shipPassword = _tmpShip.shipPassword;
            _tmpShip.shipExpire = _shipExpire;
            _tmpShip.shipOwner = _shipOwner;
            writeShipList();
            return true;
        }

        public bool CreateShip(string _shipName, string _shipPassword, DateTime _shipExpire, string _shipOwner)
        {
            shipData _tmpShip = new();
            _tmpShip.shipName = _shipName;
            _tmpShip.shipPassword = ComputeSha256Hash(_shipPassword);
            _tmpShip.shipExpire = _shipExpire;
            _tmpShip.shipOwner = _shipOwner;
            _localShipList.Add(_tmpShip);
            writeShipList() ;
            return true;
        }

        public bool DeleteShip(string _shipName)
        {
            if (_localShipList != null)
            {
                var itemToRemove = _localShipList.SingleOrDefault(shipData => shipData.shipName == _shipName);
                if (itemToRemove != null)
                {
                    _localShipList.Remove(itemToRemove);
                    writeShipList();
                }
            }
            return true;
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
