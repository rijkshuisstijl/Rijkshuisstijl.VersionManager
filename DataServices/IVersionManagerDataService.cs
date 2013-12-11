using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement.Records;
using Rijkshuisstijl.VersionManager.Models;

namespace Rijkshuisstijl.VersionManager.DataServices
{
    public interface IVersionManagerDataService : IDependency
    {
        List<ContentItemVersionRecord> ContentItemVersionRecords { get; }
        bool SetContentItemVersionRecord(ContentItemVersionRecord record);
        List<VersionManagerRecord> VersionManagerRecords { get; }
        bool SetVersionManagerRecord(VersionManagerRecord record);
    }
}