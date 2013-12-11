#region

using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Services;
using Orchard.UI.Notify;
using Rijkshuisstijl.VersionManager.Models;

#endregion

namespace Rijkshuisstijl.VersionManager.DataServices
{
    public class VersionManagerInfoDataService : IVersionManagerInfoDataService
    {
        public const string VersionManagerCache = "Rijkshuisstijl.VersionManager.VersionManager";
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly ISignals _signals;
        private readonly IRepository<VersionManagerRecord> _versionManagerRecords;

        public VersionManagerInfoDataService(IRepository<VersionManagerRecord> versionManagerRecords, ICacheManager cacheManager, ISignals signals, IClock clock, INotifier notifier)
        {
            _versionManagerRecords = versionManagerRecords;
            _cacheManager = cacheManager;
            _signals = signals;
            _clock = clock;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        #region IVersionManagerInfo Members

        public List<VersionManagerRecord> VersionManagerRecords
        {
            get
            {
                List<VersionManagerRecord> versionManagerRecords = _cacheManager.Get(VersionManagerCache, ctx =>
                {
                    ctx.Monitor(_clock.When(TimeSpan.FromHours(24)));
                    ctx.Monitor(_signals.When(VersionManagerCache));
                    return _versionManagerRecords.Table.ToList();
                });
                return versionManagerRecords;
            }
        }

        public bool SetVersionManagerRecord(VersionManagerRecord record)
        {
            if (record.ContentItemId == 0 || record.ContentItemVersionId == 0)
            {
                return false;
            }

            //Resolve current record if exist
            VersionManagerRecord newRecord = VersionManagerRecords.FirstOrDefault(r => r.ContentItemId == record.ContentItemId && r.ContentItemVersionId == record.ContentItemVersionId) ?? new VersionManagerRecord();
            newRecord.ContentItemId = record.ContentItemId;
            newRecord.ContentItemVersionId = record.ContentItemVersionId;
            if (!String.IsNullOrEmpty(record.Description))
            {
                newRecord.Description = record.Description;
            }
            _versionManagerRecords.Update(newRecord);
            _signals.Trigger(VersionManagerCache);
            return true;
        }

        #endregion
    }
}