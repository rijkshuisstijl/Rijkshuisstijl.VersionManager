#region Usings

using Orchard;
using Orchard.Core.Contents;
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
            builder
                .Add(T("Rijkshuisstijl"), "1", LinkSubMenu);
        }


        private void LinkSubMenu(NavigationBuilder menu)
        {
            menu.Add(item => item
                .Position("9")
                .Caption(T("Recycle Bin"))
                .Action("ListDeletedItems", "VersionManager", new {area = "Rijkshuisstijl.VersionManager"})
                .Permission(Permissions.EditContent)
                );
        }
    }
}
