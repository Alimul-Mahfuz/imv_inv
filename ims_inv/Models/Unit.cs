using System;
using System.ComponentModel.DataAnnotations;

namespace ims_inv.Models
{
    public class Unit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // Kilogram, Piece, Liter

        [Required]
        public string Symbol { get; set; } // kg, pcs, L

        [Required]
        public string Type { get; set; } // weight, count, volume

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class UnitViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Symbol { get; set; }

        [Required]
        public string Type { get; set; }
    }
}
