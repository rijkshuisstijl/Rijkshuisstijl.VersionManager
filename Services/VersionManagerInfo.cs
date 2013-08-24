#region

using System;
using System.Collections.Generic;
using System.Linq;
using Orchard;
using Orchard.Caching;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Services;
using Orchard.UI.Notify;
using Rijkshuisstijl.VersionManager.Models;

#endregion

namespace Rijkshuisstijl.VersionManager.Services
{
    public class VersionManagerInfo : IVersionManagerInfo
    {
        public const string SignalUpdateVersionManagerTrigger = "SignalUpdateVersionManagerRecords";
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly INotifier _notifier;
        private readonly ISignals _signals;
        private readonly IRepository<VersionManagerRecord> _versionManagerRecords;

        public VersionManagerInfo(IRepository<VersionManagerRecord> versionManagerRecords, ICacheManager cacheManager, ISignals signals, IClock clock, INotifier notifier)
        {
            _versionManagerRecords = versionManagerRecords;
            _cacheManager = cacheManager;
            _signals = signals;
            _clock = clock;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        #region IVersionManagerInfo Members

        public List<VersionManagerRecord> VersionManagerRecords
        {
            get
            {
                List<VersionManagerRecord> versionManagerRecords = _cacheManager.Get("Rijkshuisstijl.VersionManager.VersionManagerRecords", ctx =>
                {
                    ctx.Monitor(_clock.When(TimeSpan.FromMinutes(60)));
                    ctx.Monitor(_signals.When(SignalUpdateVersionManagerTrigger));
                    return _versionManagerRecords.Table.ToList();
                });
                return versionManagerRecords;
            }
        }

        public void Update(VersionManagerRecord record)
        {
            if (record.ContentItemId == 0 || record.ContentItemVersionId == 0)
            {
                _notifier.Error(T("Could not update VersionManagerRecord because contentItemId or ContentItemVersionId is 0"));
                return;
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
            _signals.Trigger(SignalUpdateVersionManagerTrigger);
            _notifier.Information(T("Versionmanager information updated."));
        }

        #endregion
    }
}