#region

using Orchard;
using Rijkshuisstijl.VersionManager.Models;

#endregion

namespace Rijkshuisstijl.VersionManager.Services
{
    public interface IVersionManager : IDependency
    {
        ContentInfoModel GetContentInfo(int contentItemId);
    }
}