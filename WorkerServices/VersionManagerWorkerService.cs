#region Usings

using System.Globalization;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Records;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Notify;
using Rijkshuisstijl.VersionManager.DataServices;
using Rijkshuisstijl.VersionManager.InputModels.VersionManager;
using Rijkshuisstijl.VersionManager.Models;
using Rijkshuisstijl.VersionManager.Services;
using Rijkshuisstijl.VersionManager.ViewModels.Admin;

#endregion

namespace Rijkshuisstijl.VersionManager.WorkerServices
{
    public class VersionManagerWorkerService : IVersionManagerWorkerService
    {
        private readonly IRepository<ContentItemVersionRecord> _contentItemVersionRepository;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;
        private readonly IVersionManager _versionManager;
        private readonly IVersionManagerDataService _versionManagerDataService;
        private readonly IVersionManagerInfoDataService _versionManagerInfoDataService;


        public VersionManagerWorkerService(IVersionManager versionManager, IContentManager contentManager,
                                           IVersionManagerDataService versionManagerDataService,
                                           INotifier notifier,
                                           IVersionManagerInfoDataService versionManagerInfoDataService,
                                           IRepository<ContentItemVersionRecord> contentItemVersionRepository)
        {
            _versionManager = versionManager;
            _contentManager = contentManager;
            _versionManagerDataService = versionManagerDataService;
            _notifier = notifier;
            _versionManagerInfoDataService = versionManagerInfoDataService;
            _contentItemVersionRepository = contentItemVersionRepository;
            T = NullLocalizer.Instance;
        }


        public Localizer T { get; set; }


        public ListViewModel List(int id)
        {
            ListViewModel viewModel = new ListViewModel
            {
                ContentInfoModel = _versionManager.GetContentInfo(id)
            };
            return viewModel;
        }


        public bool Promote(int id, int versionId)
        {
            //Get the last version number to create a new version
            int lastversionNumber = _contentManager.GetAllVersions(id).Max(r => r.VersionRecord.Number);

            //Retrieve the content item to clone
            ContentItem contentItem = _contentManager.Get(id, VersionOptions.VersionRecord(versionId));


            if (contentItem == null)
            {
                _notifier.Error(T("Could not find content with id {0}", id));
                return false;
            }

            int versionOfContentItemToClone = contentItem.VersionRecord.Number;

            //Unfortunately it creates not a new record but updates the old record
            _contentManager.Create(contentItem, VersionOptions.Number(lastversionNumber + 1));

            //var newContent = BuildNewVersion(contentItem);



            //Update the table that a new version is added
            ContentItemVersionRecord previousLastRecord =
                _versionManagerDataService.ContentItemVersionRecords.FirstOrDefault(
                    r => r.ContentItemRecord.Id == id && r.Latest);
            ContentItemVersionRecord newLastRecord =
                _versionManagerDataService.ContentItemVersionRecords.Where(r => r.ContentItemRecord.Id == contentItem.Id)
                    .OrderByDescending(r => r.Number)
                    .FirstOrDefault();
            if (previousLastRecord != null && newLastRecord != null)
            {
                previousLastRecord.Latest = false;
                newLastRecord.Latest = true;
                _versionManagerDataService.SetContentItemVersionRecord(previousLastRecord);
                _versionManagerDataService.SetContentItemVersionRecord(newLastRecord);
            }

            _notifier.Information(T("Successfully promoted version {0} to {1}.", versionOfContentItemToClone,
                lastversionNumber + 1));
            return true;
        }


        public bool SetPublishedVersion(int id, int versionId)
        {
            ContentItem contentItem = _contentManager.Get(id, VersionOptions.VersionRecord(versionId));
            if (contentItem == null)
            {
                _notifier.Error(T("Could not find version id {0} of content with id {0}", versionId, id));
                return false;
            }

            _contentManager.Publish(contentItem);
            _notifier.Information(T("Version {0} of content item published.", contentItem.Version));

            return true;
        }


        public bool UnsetPublishedVersion(int id, int versionId)
        {
            ContentItem contentItem = _contentManager.Get(id, VersionOptions.VersionRecord(versionId));
            if (contentItem == null)
            {
                _notifier.Error(T("Could not find version id {0} of content with id {0}", versionId, id));
                return false;
            }

            _contentManager.Unpublish(contentItem);
            _notifier.Add(NotifyType.Information, T("Version {0} of content item un-published.", contentItem.Version));

            return true;
        }


        public SetDescriptionViewModel SetDescription(int id, int versionId)
        {
            VersionManagerRecord existingInfo =
                _versionManagerInfoDataService.VersionManagerRecords.FirstOrDefault(
                    r => r.ContentItemId == id && r.ContentItemVersionId == versionId);

            VersionManagerRecord versionManagerRecord = new VersionManagerRecord();
            if (existingInfo != null)
            {
                versionManagerRecord.Description = existingInfo.Description;
            }
            versionManagerRecord.ContentItemId = id;
            versionManagerRecord.ContentItemVersionId = versionId;

            SetDescriptionViewModel viewModel = new SetDescriptionViewModel
            {
                ContentItemId = versionManagerRecord.ContentItemId,
                ContentItemVersionId = versionManagerRecord.ContentItemVersionId,
                Description = versionManagerRecord.Description,
                Id = versionManagerRecord.Id
            };

            return viewModel;
        }


        public bool SetDescriptionPost(SetDescriptionPostInputModel inputModel)
        {
            VersionManagerRecord record = new VersionManagerRecord
            {
                ContentItemId = inputModel.ContentItemId,
                ContentItemVersionId = inputModel.ContentItemVersionId,
                Description = inputModel.Description,
                Id = inputModel.Id
            };

            if (_versionManagerDataService.SetVersionManagerRecord(record))
            {
                _notifier.Information(T("Versionmanager information updated."));
                return false;
            }

            _notifier.Error(
                T("Could not update VersionManagerRecord. Make sure that contentItemId or ContentItemVersionId is 0"));
            return true;
        }


        public ListDeletedItemsViewModel ListDeletedItems()
        {
            ListDeletedItemsViewModel viewModel = new ListDeletedItemsViewModel
            {
                RemovedItems = _contentManager
                    // all versions of all content items...
                    .Query(VersionOptions.AllVersions).List()
                    // ... group by content item id
                    .GroupBy(item => item.Id)
                    // ...that don't not have a "Latest" flag set in their list of revisions
                    .Where(g => !g.Any(item => item.VersionRecord.Latest))
                    // ...order by version #
                    .Select(g => g.OrderBy(item => item.Version).Last()).ToList()
            };
            return viewModel;
        }


        public bool Undelete(int id)
        {
            ContentItem contentItem = _contentManager.Get(-1, VersionOptions.VersionRecord(id));
            if (contentItem == null)
            {
                return false;
            }

            string title = contentItem.Id.ToString(CultureInfo.InvariantCulture);
            if (contentItem.Has<TitlePart>())
            {
                title = contentItem.As<TitlePart>().Title;
            }

            // Undelete and publish 
            contentItem.VersionRecord.Latest = true;
            _contentManager.Publish(contentItem);
            _notifier.Information(T("Content item '{0}' has been un-deleted and re-published.", title));

            return true;
        }


        protected virtual ContentItem BuildNewVersion(ContentItem existingContentItem)
        {
            ContentItemRecord contentItemRecord = existingContentItem.Record;

            // locate the existing and the current latest versions, allocate building version
            ContentItemVersionRecord existingItemVersionRecord = existingContentItem.VersionRecord;

            ContentItemVersionRecord buildingItemVersionRecord = new ContentItemVersionRecord
            {
                ContentItemRecord = contentItemRecord,
                Latest = true,
                Published = false,
                Data = existingItemVersionRecord.Data,
            };

            //get the latest version
            ContentItemVersionRecord latestVersion = contentItemRecord.Versions.SingleOrDefault(x => x.Latest);

            if (latestVersion != null)
            {
                latestVersion.Latest = false;
                buildingItemVersionRecord.Number = latestVersion.Number + 1;
            }
            else
            {
                buildingItemVersionRecord.Number = contentItemRecord.Versions.Max(x => x.Number) + 1;
            }

            //Add new version record
            contentItemRecord.Versions.Add(buildingItemVersionRecord);

            //Save the version record to the database
            _contentItemVersionRepository.Create(buildingItemVersionRecord);

            //Create a new contentitem record
            ContentItem buildingContentItem = _contentManager.New(existingContentItem.ContentType);
            buildingContentItem.VersionRecord = buildingItemVersionRecord;

            VersionContentContext context = new VersionContentContext
            {
                Id = existingContentItem.Id,
                ContentType = existingContentItem.ContentType,
                ContentItemRecord = contentItemRecord,
                ExistingContentItem = existingContentItem,
                BuildingContentItem = buildingContentItem,
                ExistingItemVersionRecord = existingItemVersionRecord,
                BuildingItemVersionRecord = buildingItemVersionRecord,
            };

            return context.BuildingContentItem;
        }
    }
}
