#region

using System;
using System.Collections.Generic;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Rijkshuisstijl.VersionManager.Services;

#endregion

namespace Rijkshuisstijl.VersionManager.Models
{
    public class ContentInfo
    {
        private readonly int _contentItemId;
        private readonly IRepository<ContentItemVersionRecord> _contentItemVersionRepository;
        private readonly IOrchardServices _orchardServices;
        private readonly int _timeOffSet;
        private readonly IVersionManagerInfo _versionManagerInfo;
        private List<ContentVersionInfo> _versions = null;

        public ContentInfo(int contentItemId, IOrchardServices orchardServices, int timeOffSet, IRepository<ContentItemVersionRecord> contentItemVersionRepository, IVersionManagerInfo versionManagerInfo)
        {
            _contentItemId = contentItemId;
            _orchardServices = orchardServices;
            _timeOffSet = timeOffSet;
            _contentItemVersionRepository = contentItemVersionRepository;
            _versionManagerInfo = versionManagerInfo;
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
                ContentVersionInfo contentVersionInfo = Versions.FirstOrDefault(r => r.IsPublished == true);
                if (contentVersionInfo != null)
                {
                    return contentVersionInfo.Title;
                }
                contentVersionInfo = Versions.FirstOrDefault(r => r.IsLatest == true);
                return contentVersionInfo == null ? "<No title found>" : String.Format("{0} (not published)", contentVersionInfo.Title);
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
                ContentVersionInfo contentVersionInfo = Versions.FirstOrDefault(r => r.IsPublished == true);
                if (contentVersionInfo == null)
                {
                    return "<No active permalink found>";
                }
                return contentVersionInfo.Permalink;
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

        public List<ContentVersionInfo> Versions
        {
            get
            {
                if (_versions != null)
                {
                    return _versions;
                }

                _versions = new List<ContentVersionInfo>();
                foreach (ContentItem contentItem in _orchardServices.ContentManager.GetAllVersions(_contentItemId))
                {
                    _versions.Add(new ContentVersionInfo(_contentItemId, contentItem.VersionRecord.Id, _orchardServices, _timeOffSet, _versionManagerInfo));
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
            get { return _orchardServices.ContentManager.Get(_contentItemId); }
        }

    }
}