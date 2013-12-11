using Orchard;
using Rijkshuisstijl.VersionManager.InputModels.VersionManager;
using Rijkshuisstijl.VersionManager.ViewModels.Admin;

namespace Rijkshuisstijl.VersionManager.WorkerServices
{
    public interface IVersionManagerWorkerService : IDependency
    {
        ListViewModel List(int id);
        bool Promote(int id, int versionId);
        bool SetPublishedVersion(int id, int versionId);
        bool UnsetPublishedVersion(int id, int versionId);
        SetDescriptionViewModel SetDescription(int id, int versionId);
        bool SetDescriptionPost(SetDescriptionPostInputModel inputModel);

        ListDeletedItemsViewModel ListDeletedItems();

        bool Undelete(int id);
    }
}