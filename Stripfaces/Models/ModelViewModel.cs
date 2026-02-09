using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace stripfaces.Models
{
    public class ModelViewModel
    {
        [Required(ErrorMessage = "Model name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Model Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Bio/Description")]
        public string Bio { get; set; } = string.Empty;

        [Display(Name = "Profile Picture")]
        public IFormFile ProfilePicFile { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}