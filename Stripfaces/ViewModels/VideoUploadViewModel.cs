using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace stripfaces.Models
{
    public class VideoUploadViewModel
    {
        [Required(ErrorMessage = "Please select a model")]
        [Display(Name = "Model")]
        public int ModelId { get; set; }

        [Required(ErrorMessage = "Please enter a video title")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a video file")]
        [Display(Name = "Video File")]
        public IFormFile VideoFile { get; set; }

        [Display(Name = "Thumbnail")]
        public IFormFile ThumbnailFile { get; set; }

        [Display(Name = "Tags (comma separated)")]
        public string Tags { get; set; } = string.Empty;

        [Display(Name = "Mark as Featured")]
        public bool IsFeatured { get; set; }

        [BindNever]
        public List<SelectListItem> Models { get; set; } = new List<SelectListItem>();
    }
}