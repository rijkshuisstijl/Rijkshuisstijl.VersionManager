#region Usings

using Rijkshuisstijl.VersionManager.InputModels.VersionManager;

#endregion

namespace Rijkshuisstijl.VersionManager.ViewModels.Admin
{
    public class SetDescriptionViewModel
    {
        public SetDescriptionViewModel()
        {}

        public SetDescriptionViewModel(SetDescriptionPostInputModel inputModel)
        {
            Id = inputModel.Id;
            ContentItemId = inputModel.ContentItemId;
            ContentItemVersionId = inputModel.ContentItemVersionId;
            Description = inputModel.Description;
        }

        public int Id { get; set; }
        public int ContentItemId { get; set; }
        public int ContentItemVersionId { get; set; }
        public string Description { get; set; }
    }
}