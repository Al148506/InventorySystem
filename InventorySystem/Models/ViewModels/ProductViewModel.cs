using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Models.ViewModels
{
    public class ProductViewModel
    {

        public int IdProd { get; set; }
        [Required]
        [Display(Name = "Poduct Name")]
        public string? ProductName { get; set; }
        [Required]
        public string? Description { get; set; }
        [Required]
        public int? Quantity { get; set; }
        [Required]
        public string? State { get; set; }
        [Required]
        [Display(Name = "Category")]
        public int? IdCategory { get; set; }
        [Required]
        [Display(Name = "Location")]
        public int? IdLocation { get; set; }

        public DateTime? CreationDate { get; set; }

        public DateTime? LastModDate { get; set; }

        public string? ImageRoot { get; set; }
    }
}
