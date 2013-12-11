#region Usings

using System.Web.Mvc;
using Orchard.Core.Contents;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Rijkshuisstijl.VersionManager.InputModels.VersionManager;
using Rijkshuisstijl.VersionManager.ViewModels.Admin;
using Rijkshuisstijl.VersionManager.WorkerServices;

#endregion

namespace Rijkshuisstijl.VersionManager.Controllers
{
    [Admin]
    public class VersionManagerController : Controller
    {
        private readonly IVersionManagerWorkerService _versionManagerWorkerService;
        private readonly IAuthorizer _authorizer;


        public VersionManagerController(IVersionManagerWorkerService versionManagerWorkerService, IAuthorizer authorizer)
        {
            _versionManagerWorkerService = versionManagerWorkerService;
            _authorizer = authorizer;
            T = NullLocalizer.Instance;
        }


        public Localizer T { get; set; }

        public ActionResult List(int id)
        {
            if (!_authorizer.Authorize(Permissions.EditContent, T("Not authorized to manage versions.")))
            {
                return new HttpUnauthorizedResult();
            }
            ListViewModel viewModel = _versionManagerWorkerService.List(id);
            return View(viewModel);
        }


        public ActionResult Promote(int id, int versionId)
        {
            if (!_authorizer.Authorize(Permissions.EditContent, T("Not authorized to manage versions.")))
            {
                return new HttpUnauthorizedResult();
            }
            _versionManagerWorkerService.Promote(id, versionId);
            return RedirectToAction("List", new {id});
        }


        public ActionResult SetPublishedVersion(int id, int versionId)
        {
            if (!_authorizer.Authorize(Permissions.EditContent, T("Not authorized to manage versions.")))
            {
                return new HttpUnauthorizedResult();
            }
            _versionManagerWorkerService.SetPublishedVersion(id, versionId);
            return RedirectToAction("List", new {id});
        }


        public ActionResult UnsetPublishedVersion(int id, int versionId)
        {
            if (!_authorizer.Authorize(Permissions.EditContent, T("Not authorized to manage versions.")))
            {
                return new HttpUnauthorizedResult();
            }
            _versionManagerWorkerService.UnsetPublishedVersion(id, versionId);
            return RedirectToAction("List", new {id});
        }


        public ActionResult SetDescription(int id, int versionId)
        {
            if (!_authorizer.Authorize(Permissions.EditContent, T("Not authorized to manage versions.")))
            {
                return new HttpUnauthorizedResult();
            }
            SetDescriptionViewModel viewModel = _versionManagerWorkerService.SetDescription(id, versionId);
            return View("SetDescription", viewModel);
        }


        [HttpPost, ActionName("SetDescription")]
        public ActionResult SetDescriptionPost(SetDescriptionPostInputModel inputModel)
        {
            if (!_authorizer.Authorize(Permissions.EditContent, T("Not authorized to manage versions.")))
            {
                return new HttpUnauthorizedResult();
            }
            if (!ModelState.IsValid)
            {
                SetDescriptionViewModel viewModel = new SetDescriptionViewModel(inputModel);
                return View("SetDescription", viewModel);
            }
            _versionManagerWorkerService.SetDescriptionPost(inputModel);
            return RedirectToAction("List", new {id = inputModel.ContentItemId});
        }


        [HttpGet]
        public ActionResult ListDeletedItems()
        {
            if (!_authorizer.Authorize(Permissions.EditContent, T("Not authorized to manage versions.")))
            {
                return new HttpUnauthorizedResult();
            }
            ListDeletedItemsViewModel viewModel = _versionManagerWorkerService.ListDeletedItems();
            return View(viewModel);
        }


        [HttpGet]
        public ActionResult Undelete(int id)
        {
            if (!_authorizer.Authorize(Permissions.EditContent, T("Not authorized to manage versions.")))
            {
                return new HttpUnauthorizedResult();
            }
            _versionManagerWorkerService.Undelete(id);
            return RedirectToAction("ListDeletedItems");
        }
    }
}
