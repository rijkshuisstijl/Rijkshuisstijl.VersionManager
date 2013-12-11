#region Usings

using System;
using Orchard;
using Orchard.Time;
using Rijkshuisstijl.VersionManager.DataServices;
using Rijkshuisstijl.VersionManager.Models;

#endregion

namespace Rijkshuisstijl.VersionManager.Services
{
    public class VersionManager : IVersionManager
    {
        private readonly IOrchardServices _orchardServices;
        private readonly ITimeZoneSelector _timeZoneSelector;
        private readonly IVersionManagerInfoDataService _versionManagerInfoDataService;
        private readonly IWorkContextAccessor _workContextAccessor;

        public VersionManager(IOrchardServices orchardServices, ITimeZoneSelector timeZoneSelector,
            IWorkContextAccessor workContextAccessor,
            IVersionManagerInfoDataService versionManagerInfoDataService)
        {
            _orchardServices = orchardServices;
            _timeZoneSelector = timeZoneSelector;
            _workContextAccessor = workContextAccessor;
            _versionManagerInfoDataService = versionManagerInfoDataService;
        }

        public ContentInfoModel GetContentInfo(int contentItemId)
        {
            return new ContentInfoModel(contentItemId, _orchardServices, GetHoursOffSet(), _versionManagerInfoDataService);
        }


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