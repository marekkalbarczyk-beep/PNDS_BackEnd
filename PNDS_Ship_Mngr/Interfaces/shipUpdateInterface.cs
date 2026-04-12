using System.ComponentModel.DataAnnotations;

namespace PNDS_Ship_Mngr.Interfaces
{
    public class shipUpdateInterface
    {
        //[Required]
        //public required string shipName { get; init; }

        [Required]
        public required DateTime shipExpire { get; set; }
        
        public string? shipPassword { get; init; }
        public required string shipOwner { get; set; }
    }
}

