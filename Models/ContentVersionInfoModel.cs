#region

using System;
using System.Linq;
using System.Text;
using System.Web.DynamicData;
using HtmlAgilityPack;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.Fields;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Rijkshuisstijl.VersionManager.DataServices;
using Rijkshuisstijl.VersionManager.Services;

#endregion

namespace Rijkshuisstijl.VersionManager.Models
{
    public class ContentVersionInfoModel
    {
        private readonly int _contentItemId;
        private readonly int _contentItemVersionId;
        private readonly int _hoursOffSetUtc;
        private readonly IOrchardServices _orchardServices;
        private readonly IVersionManagerInfoDataService _versionManagerInfoDataService;

        public ContentVersionInfoModel(int contentItemId, int contentItemVersionId, IOrchardServices orchardServices, int hoursOffSetUtc, IVersionManagerInfoDataService versionManagerInfoDataService)
        {
            _contentItemId = contentItemId;
            _contentItemVersionId = contentItemVersionId;
            _orchardServices = orchardServices;
            _hoursOffSetUtc = hoursOffSetUtc;
            _versionManagerInfoDataService = versionManagerInfoDataService;
        }


        public String Description
        {
            get
            {
                string returnValue = String.Empty;

                VersionManagerRecord versionManagerInfo = _versionManagerInfoDataService.VersionManagerRecords.FirstOrDefault(r => r.ContentItemId == _contentItemId && r.ContentItemVersionId == _contentItemVersionId);
                if (versionManagerInfo != null)
                {
                    returnValue = versionManagerInfo.Description;
                }
                return returnValue;
            }
            set
            {
                _versionManagerInfoDataService.SetVersionManagerRecord(new VersionManagerRecord
                {
                    ContentItemId = _contentItemId,
                    ContentItemVersionId = _contentItemVersionId,
                    Description = value
                });
            }
        }

        public int Id
        {
            get
            {
                ContentItem contentItemVersion = ContentItemVersion;
                return contentItemVersion == null ? 0 : contentItemVersion.VersionRecord.Id;
            }
        }

        public int Version
        {
            get
            {
                ContentItem contentItemVersion = ContentItemVersion;
                return contentItemVersion == null ? 0 : contentItemVersion.VersionRecord.Number;
            }
        }

        public DateTime Created
        {
            get
            {
                CommonPart commonPart = CommonPart;
                if (commonPart == null || commonPart.VersionCreatedUtc == null)
                {
                    return DateTime.MinValue;
                }
                return commonPart.VersionCreatedUtc.Value.AddHours(_hoursOffSetUtc);
            }
        }

        public DateTime Published
        {
            get
            {
                CommonPart commonPart = CommonPart;
                if (commonPart == null || commonPart.VersionPublishedUtc == null)
                {
                    return DateTime.MinValue;
                }
                return commonPart.VersionPublishedUtc.Value.AddHours(_hoursOffSetUtc);
            }
        }

        public DateTime Modified
        {
            get
            {
                CommonPart commonPart = CommonPart;
                if (commonPart == null || commonPart.VersionModifiedUtc == null)
                {
                    return DateTime.MinValue;
                }
                return commonPart.VersionModifiedUtc.Value.AddHours(_hoursOffSetUtc);
            }
        }

        public String Body
        {
            get
            {
                ContentItem contentItemVersion = ContentItemVersion;
                if (contentItemVersion == null)
                {
                    return String.Empty;
                }
                BodyPart bodyPart = contentItemVersion.As<BodyPart>();
                return bodyPart == null ? null : bodyPart.Text;
            }
        }

        public String BodyText
        {
            get
            {
                String bodyHtml = Body;
                if (String.IsNullOrEmpty(Body))
                {
                    return String.Empty;
                }

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(bodyHtml);
                StringBuilder sb = new StringBuilder();
                foreach (string text in htmlDocument.DocumentNode.DescendantNodesAndSelf()
                    .Where(node => !node.HasChildNodes)
                    .Select(node => node.InnerText)
                    .Where(text => !string.IsNullOrEmpty(text)))
                {
                    sb.AppendLine(text.Trim());
                }
                return sb.ToString();
            }
        }


        public String IntroField
        {
            get
            {
                ContentItem contentItemVersion = ContentItemVersion;
                if (contentItemVersion == null)
                {
                    return String.Empty;
                }
                var introField = (TextField) contentItemVersion.Parts.SelectMany(p => p.Fields).FirstOrDefault(f => f.Name == "Intro");
                if (introField == null)
                {
                    return String.Empty;
                }
                return introField.Value;

            }
        }

        public String IntroFieldText
        {
            get
            {
                String introFieldHtml = IntroField;
                if (String.IsNullOrEmpty(IntroField))
                {
                    return String.Empty;
                }

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(introFieldHtml);
                StringBuilder sb = new StringBuilder();
                foreach (var text in htmlDocument.DocumentNode.DescendantNodesAndSelf()
                    .Where(node => !node.HasChildNodes)
                    .Select(node => node.InnerText)
                    .Where(text => !string.IsNullOrEmpty(text)))
                {
                    sb.AppendLine(text.Trim());
                }
                return sb.ToString();
            }
        }

        public String Title
        {
            get
            {
                ContentItem contentItemVersion = ContentItemVersion;
                if (contentItemVersion == null)
                {
                    return String.Empty;
                }
                TitlePart titlePart = contentItemVersion.As<TitlePart>();
                return titlePart == null ? null : titlePart.Title;
            }
        }


        public String Permalink
        {
            get
            {
                ContentItem contentItemVersion = ContentItemVersion;
                if (contentItemVersion == null)
                {
                    return String.Empty;
                }
                AutoroutePart autoRoutePart = contentItemVersion.As<AutoroutePart>();
                return autoRoutePart == null ? String.Empty : autoRoutePart.Path;
            }

        }

        public bool? IsLatest
        {
            get { return ContentItemVersion == null ? (bool?) null : ContentItemVersion.VersionRecord.Latest; }
        }

        public bool? IsPublished
        {
            get { return ContentItemVersion == null ? (bool?) null : ContentItemVersion.VersionRecord.Published; }
        }

        private ContentItem ContentItemVersion
        {
            get { return _orchardServices.ContentManager.Get(_contentItemId, VersionOptions.VersionRecord(_contentItemVersionId)); }
        }

        private CommonPart CommonPart
        {
            get
            {
                ContentItem contentItemVersion = ContentItemVersion;
                if (contentItemVersion == null)
                {
                    return null;
                }
                CommonPart commonPart = contentItemVersion.As<CommonPart>();
                return commonPart;
            }
        }
    }
}