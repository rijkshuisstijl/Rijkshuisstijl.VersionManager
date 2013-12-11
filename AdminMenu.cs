#region Usings

using Orchard;
using Orchard.Core.Contents;
using Orchard.Data.Migration.Schema;
using Orchard.UI.Navigation;

#endregion

namespace Rijkshuisstijl.VersionManager
{
    public class AdminMenu : Component, INavigationProvider
    {
        public string MenuName
        {
            get { return "admin"; }
        }


        public void GetNavigation(NavigationBuilder builder)
        {

            builder.Add(T("Content"),
               menu => menu
                   .Add(T("Recycle Bin"), "2.5", item => item
                       .Action("ListDeletedItems", "VersionManager", new { area = "Rijkshuisstijl.VersionManager" })
                       .Permission(Permissions.EditContent)
                       .LocalNav())
               );
        }


    }
}
