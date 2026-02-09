using stripfaces.Models;
using Stripfaces.Models;

namespace stripfaces.ViewModels
{
    public class ModelVideosViewModel
    {
        // This should reference Models.Model
        public Model Model { get; set; }
        public List<Video> Videos { get; set; }
    }
}