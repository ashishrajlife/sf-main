using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Stripfaces.Models
{
    public class VideoUploadViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        [Display(Name = "Select Model")]
        public int ModelId { get; set; }

        [Required]
        [Display(Name = "Video File")]
        [DataType(DataType.Upload)]
        public IFormFile VideoFile { get; set; }

        [Display(Name = "Thumbnail (Optional)")]
        [DataType(DataType.Upload)]
        public IFormFile ThumbnailFile { get; set; }

        public string Tags { get; set; }

        [Display(Name = "Featured Video")]
        public bool IsFeatured { get; set; }

        // For dropdown
        public List<SelectListItem> Models { get; set; }
    }
}
