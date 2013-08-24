#region

using System;
using System.Diagnostics;
using Orchard;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Time;
using Rijkshuisstijl.VersionManager.Models;

#endregion

namespace Rijkshuisstijl.VersionManager.Services
{
    public class VersionManager : IVersionManager
    {
        private readonly IRepository<ContentItemVersionRecord> _contentItemVersionRepository;
        private readonly IOrchardServices _orchardServices;
        private readonly ITimeZoneSelector _timeZoneSelector;
        private readonly IVersionManagerInfo _versionManagerInfo;
        private readonly IWorkContextAccessor _workContextAccessor;

        public VersionManager(IOrchardServices orchardServices, ITimeZoneSelector timeZoneSelector,
            IWorkContextAccessor workContextAccessor, IRepository<ContentItemVersionRecord> contentItemVersionRepository,
            IVersionManagerInfo versionManagerInfo)
        {
            _orchardServices = orchardServices;
            _timeZoneSelector = timeZoneSelector;
            _workContextAccessor = workContextAccessor;
            _contentItemVersionRepository = contentItemVersionRepository;
            _versionManagerInfo = versionManagerInfo;
        }

        #region IVersionManager Members

        public ContentInfo GetContentInfo(int contentItemId)
        {
            return new ContentInfo(contentItemId, _orchardServices, GetHoursOffSet(),
                _contentItemVersionRepository, _versionManagerInfo);
        }

        #endregion

        private int GetHoursOffSet()
        {
            DateTime currentTime = DateTime.Now;
            TimeZoneSelectorResult timeZone =
                _timeZoneSelector.GetTimeZone(_workContextAccessor.GetContext().HttpContext);
            TimeSpan dateTimeOffSet =
                (TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(currentTime, DateTimeKind.Utc), timeZone.TimeZone) -
                 currentTime);
            return dateTimeOffSet.Hours;
        }
    }
}