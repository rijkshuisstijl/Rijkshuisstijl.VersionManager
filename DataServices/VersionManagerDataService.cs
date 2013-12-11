using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Services;
using Rijkshuisstijl.VersionManager.Models;

namespace Rijkshuisstijl.VersionManager.DataServices
{
    public class VersionManagerDataService : IVersionManagerDataService
    {
        public const string VersionManagerCache = "Rijkshuisstijl.VersionManager.VersionManager";

        private readonly IRepository<ContentItemVersionRecord> _contentItemVersionRepository;
        private readonly IRepository<VersionManagerRecord> _versionManagerRecords;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IClock _clock;

        public VersionManagerDataService(IRepository<ContentItemVersionRecord> contentItemVersionRepository, IRepository<VersionManagerRecord> versionManagerRecords, ICacheManager cacheManager, ISignals signals, IClock clock)
        {
            _contentItemVersionRepository = contentItemVersionRepository;
            _versionManagerRecords = versionManagerRecords;
            _cacheManager = cacheManager;
            _signals = signals;
            _clock = clock;
        }

        public List<ContentItemVersionRecord> ContentItemVersionRecords
        {
            get
            {
                //Do not use cache, because the data is not in our control
                return _contentItemVersionRepository.Table.ToList();
            }
        }


        public bool SetContentItemVersionRecord(ContentItemVersionRecord record)
        {
            _contentItemVersionRepository.Update(record);
            return true;
        }

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
          //  _versionManagerRecords.Flush();
            _signals.Trigger(VersionManagerCache);
            return true;
        }


    }
}