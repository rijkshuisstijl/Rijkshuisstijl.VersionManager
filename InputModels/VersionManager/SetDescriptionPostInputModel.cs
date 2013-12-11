using System.ComponentModel.DataAnnotations;

namespace Rijkshuisstijl.VersionManager.InputModels.VersionManager
{
    public class SetDescriptionPostInputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Content item id is required")]
        public int ContentItemId { get; set; }

        [Required(ErrorMessage = "Content item version id is required")]
        public int ContentItemVersionId { get; set; }

        [StringLength(255, ErrorMessage = "Description may not be longer than 255 characters")]
        public string Description { get; set; }
    }
}