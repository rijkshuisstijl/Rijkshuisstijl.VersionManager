#region

using System.Collections.Generic;
using Orchard;
using Rijkshuisstijl.VersionManager.Models;

#endregion

namespace Rijkshuisstijl.VersionManager.Services
{
    public interface IVersionManagerInfo : IDependency
    {
        List<VersionManagerRecord> VersionManagerRecords { get; }
        void Update(VersionManagerRecord record);
    }
}