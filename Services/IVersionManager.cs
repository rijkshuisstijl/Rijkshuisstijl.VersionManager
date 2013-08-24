#region

using Orchard;
using Rijkshuisstijl.VersionManager.Models;

#endregion

namespace Rijkshuisstijl.VersionManager.Services
{
    public interface IVersionManager : IDependency
    {
        ContentInfo GetContentInfo(int contentItemId);
    }
}