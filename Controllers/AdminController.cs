#region

using System;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Core.Contents;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.UI.Notify;
using Rijkshuisstijl.VersionManager.Models;
using Rijkshuisstijl.VersionManager.Services;
using Rijkshuisstijl.VersionManager.ViewModels;
using Rijkshuisstijl.VersionManager.ViewModels.Admin;

#endregion

namespace Rijkshuisstijl.VersionManager.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthorizer _authorizer;
        private readonly IRepository<ContentItemVersionRecord> _contentItemVersionRepository;
        private readonly IContentManager _contentManager;
        private readonly IVersionManager _versionManager;
        private readonly IVersionManagerInfo _versionManagerInfo;

        public AdminController(IOrchardServices services, IShapeFactory shapeFactory, IVersionManager versionManager, IContentManager contentManager,
            IRepository<ContentItemVersionRecord> contentItemVersionRepository, IVersionManagerInfo versionManagerInfo, IAuthorizer authorizer)
        {
            _versionManager = versionManager;
            _contentManager = contentManager;
            _contentItemVersionRepository = contentItemVersionRepository;
            _versionManagerInfo = versionManagerInfo;
            _authorizer = authorizer;
            Services = services;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Shape = shapeFactory;
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult List(int id)
        {
            ListViewModel viewModel;
            try
            {
                if (!_authorizer.Authorize(Permissions.EditContent, T("Not authorized to list previous versions")))
                {
                    return new HttpUnauthorizedResult();
                }
                viewModel = new ListViewModel
                {
                    ContentInfo = _versionManager.GetContentInfo(id)
                };
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Failed to list version items. Id:{0}", id);
                throw;
            }
            return View(viewModel);
        }


        public ActionResult Promote(int id, int versionId)
        {
            try
            {
                if (!_authorizer.Authorize(Permissions.EditContent, T("Not authorized to promote previous versions")))
                {
                    return new HttpUnauthorizedResult();
                }

                //Get the last version number to create a new version
                int lastversionNumber = _contentManager.GetAllVersions(id).Max(r => r.VersionRecord.Number);

                //Retrieve the content item to clone
                ContentItem contentItem = _contentManager.Get(id, VersionOptions.VersionRecord(versionId));


                if (contentItem == null)
                {
                    return HttpNotFound();
                }
                int versionOfContentItemToClone = contentItem.VersionRecord.Number;

                //Unfortunately it creates not a new record but updates the old record
                _contentManager.Create(contentItem, VersionOptions.Number(lastversionNumber + 1));

                //Update the table that a new version is added
                ContentItemVersionRecord previousLastRecord = _contentItemVersionRepository.Table.FirstOrDefault(r => r.ContentItemRecord.Id == id && r.Latest);
                ContentItemVersionRecord newLastRecord = _contentItemVersionRepository.Table.Where(r => r.ContentItemRecord.Id == contentItem.Id).OrderByDescending(r => r.Number).FirstOrDefault();
                if (previousLastRecord != null && newLastRecord != null)
                {
                    previousLastRecord.Latest = false;
                    newLastRecord.Latest = true;
                    _contentItemVersionRepository.Update(previousLastRecord);
                    _contentItemVersionRepository.Update(newLastRecord);
                }

                Services.Notifier.Information(T("Successfully promoted version {0} to {1}.", versionOfContentItemToClone, lastversionNumber + 1));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Failed to promote content item. Id:{0}, versionId:{1}", id, versionId);
                throw;
            }
            return RedirectToAction("List", new {id});
        }

        public ActionResult SetPublishedVersion(int id, int versionId)
        {
            ContentItem contentItem;
            try
            {
                if (!_authorizer.Authorize(Permissions.PublishContent, T("Not authorized to publish previous versions")))
                {
                    return new HttpUnauthorizedResult();
                }

                contentItem = Services.ContentManager.Get(id, VersionOptions.VersionRecord(versionId));
                if (contentItem == null)
                {
                    return HttpNotFound();
                }

                Services.ContentManager.Publish(contentItem);
                Services.Notifier.Add(NotifyType.Information, T("Version {0} of content item published.", contentItem.Version));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Failed to publish content item. Id:{0}, versionId:{1}", id, versionId);
                throw;
            }
            return RedirectToAction("List", new {contentItem.Id});
        }

        public ActionResult UnsetPublishedVersion(int id, int versionId)
        {
            try
            {
                if (!_authorizer.Authorize(Permissions.PublishContent, T("Not authorized to unpublish pages")))
                {
                    return new HttpUnauthorizedResult();
                }

                ContentItem contentItem = Services.ContentManager.Get(id, VersionOptions.VersionRecord(versionId));
                if (contentItem == null)
                {
                    return HttpNotFound();
                }

                Services.ContentManager.Unpublish(contentItem);
                Services.Notifier.Add(NotifyType.Information, T("Version {0} of content item un-published.", contentItem.Version));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Failed to unpublish content item. Id:{0}, versionId:{1}", id, versionId);
                throw;
            }
            return RedirectToAction("List", new {id});
        }


        public ActionResult SetDescription(int id, int versionId)
        {
            SetDescriptionViewModel viewModel;
            try
            {
                if (!_authorizer.Authorize(Permissions.ViewContent, T("Not authorized to edit description of previous versions")))
                {
                    return new HttpUnauthorizedResult();
                }

                VersionManagerRecord existingInfo = _versionManagerInfo.VersionManagerRecords.FirstOrDefault(r => r.ContentItemId == id && r.ContentItemVersionId == versionId);

                VersionManagerRecord versionManagerRecord = new VersionManagerRecord();
                if (existingInfo != null)
                {
                    versionManagerRecord.Description = existingInfo.Description;
                }
                versionManagerRecord.ContentItemId = id;
                versionManagerRecord.ContentItemVersionId = versionId;

                viewModel = new SetDescriptionViewModel
                {
                    VersionManagerRecord = versionManagerRecord,
                };
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Failed to edit description. Id:{0}, versionId:{1}", id, versionId);
                throw;
            }

            return View("SetDescription", viewModel);
        }

        [HttpPost]
        public ActionResult SetDescription(SetDescriptionViewModel viewModel)
        {
            try
            {
                if (!_authorizer.Authorize(Permissions.ViewContent, T("Not authorized to edit description of previous versions")))
                {
                    return new HttpUnauthorizedResult();
                }

                _versionManagerInfo.Update(viewModel.VersionManagerRecord);
            }
            catch (Exception exception)
            {
                Logger.Error(exception,"Failed to save description. Id:{0}, versionId:{1}");
                throw;
            }
            return RedirectToAction("List", new {id = viewModel.VersionManagerRecord.ContentItemId});
        }
    }
}