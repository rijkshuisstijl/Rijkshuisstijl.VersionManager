#region

using System;
using System.Collections.Generic;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Rijkshuisstijl.VersionManager.DataServices;
using Rijkshuisstijl.VersionManager.Services;

#endregion

namespace Rijkshuisstijl.VersionManager.Models
{
    public class ContentInfoModel
    {
        private readonly int _contentItemId;
        private readonly IOrchardServices _orchardServices;
        private readonly int _timeOffSet;
        private readonly IVersionManagerInfoDataService _versionManagerInfoDataService;
        private List<ContentVersionInfoModel> _versions = null;

        public ContentInfoModel(int contentItemId, IOrchardServices orchardServices, int timeOffSet, IVersionManagerInfoDataService versionManagerInfoDataService)
        {
            _contentItemId = contentItemId;
            _orchardServices = orchardServices;
            _timeOffSet = timeOffSet;
            _versionManagerInfoDataService = versionManagerInfoDataService;
        }

        public int Id
        {
            get
            {
                ContentItem contentItem = ContentItem;
                if (contentItem == null)
                {
                    return 0;
                }
                return ContentItem.Id;
            }
        }

        public int HighestVersion
        {
            get
            {
                if (Versions == null || !Versions.Any())
                {
                    return 0;
                }
                return Versions.Max(r => r.Version);
            }
        }

        /// <summary>
        /// Returns the title of the published version. If not version is published return the title of the latest version
        /// </summary>
        public string PublishedTitle
        {
            get
            {
                if (Versions == null || !Versions.Any())
                {
                    return "<No version found>";
                }
                ContentVersionInfoModel contentVersionInfoModel = Versions.FirstOrDefault(r => r.IsPublished == true);
                if (contentVersionInfoModel != null)
                {
                    return contentVersionInfoModel.Title;
                }
                contentVersionInfoModel = Versions.FirstOrDefault(r => r.IsLatest == true);
                return contentVersionInfoModel == null ? "<No title found>" : String.Format("{0} (not published)", contentVersionInfoModel.Title);
            }
        }

        public string PublishedPermalink
        {
            get
            {
                if (Versions == null || !Versions.Any())
                {
                    return "<No version found>";
                }
                ContentVersionInfoModel contentVersionInfoModel = Versions.FirstOrDefault(r => r.IsPublished == true);
                if (contentVersionInfoModel == null)
                {
                    return "<No active permalink found>";
                }
                return contentVersionInfoModel.Permalink;
            }
        }

        public String Owner
        {
            get
            {
                CommonPart commonPart = CommonPart;
                return commonPart == null ? null : commonPart.Owner.UserName;
            }
        }

        public DateTime Created
        {
            get
            {
                CommonPart commonPart = CommonPart;
                if (commonPart == null || commonPart.CreatedUtc == null)
                {
                    return DateTime.MinValue;
                }
                return commonPart.CreatedUtc.Value.AddHours(_timeOffSet);
            }
        }

        public DateTime Modified
        {
            get
            {
                CommonPart commonPart = CommonPart;
                if (commonPart == null || commonPart.ModifiedUtc == null)
                {
                    return DateTime.MinValue;
                }
                return commonPart.ModifiedUtc.Value.AddHours(_timeOffSet);
            }
        }

        public DateTime Published
        {
            get
            {
                CommonPart commonPart = CommonPart;
                if (commonPart == null || commonPart.PublishedUtc == null)
                {
                    return DateTime.MinValue;
                }
                return commonPart.PublishedUtc.Value.AddHours(_timeOffSet);
            }
        }

        public List<ContentVersionInfoModel> Versions
        {
            get
            {
                if (_versions != null)
                {
                    return _versions;
                }

                _versions = new List<ContentVersionInfoModel>();
                foreach (ContentItem contentItem in _orchardServices.ContentManager.GetAllVersions(_contentItemId))
                {
                    _versions.Add(new ContentVersionInfoModel(_contentItemId, contentItem.VersionRecord.Id, _orchardServices, _timeOffSet, _versionManagerInfoDataService));
                }
                return _versions;
            }
        }

        private CommonPart CommonPart
        {
            get
            {
                ContentItem contentItem = ContentItem;
                return contentItem == null ? null : contentItem.As<CommonPart>();
            }
        }

        private ContentItem ContentItem
        {
            get { return _orchardServices.ContentManager.Get(_contentItemId, VersionOptions.AllVersions); }
        }

    }
}