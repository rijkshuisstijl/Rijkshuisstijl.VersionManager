#region

using System.Collections.Generic;
using Orchard;
using Rijkshuisstijl.VersionManager.Models;

#endregion

namespace Rijkshuisstijl.VersionManager.DataServices
{
    public interface IVersionManagerInfoDataService : IDependency
    {
        List<VersionManagerRecord> VersionManagerRecords { get; }
        bool SetVersionManagerRecord(VersionManagerRecord record);
    }
}