using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace InventorySystem.Models
{
    public class ChangeLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string UserId { get; set; }

        [Required]
        [StringLength(255)]
        public string TypeAction { get; set; }

        [Required]
        [StringLength(255)]
        public string TableName { get; set; }

        [Required]
        public DateTime DateMod { get; set; } = DateTime.Now;

        [StringLength(255)]
        public string OldValues { get; set; }

        [StringLength(255)]
        public string NewValues { get; set; }

        [StringLength(255)]
        public string AffectedColumns { get; set; }

        [StringLength(255)]
        public string PrimaryKey { get; set; }
    }
}

