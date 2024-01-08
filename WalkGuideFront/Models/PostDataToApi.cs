using System.ComponentModel.DataAnnotations;

namespace WalkGuideFront.Models
{
    public class PostDataToApi
    {
        [Required]
        public string Message {  get; set; }
    }
}
