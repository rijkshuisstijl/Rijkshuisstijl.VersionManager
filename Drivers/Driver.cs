#region

using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

#endregion

namespace Rijkshuisstijl.VersionManager.Drivers
{
    public class Driver : ContentPartDriver<ContentPart>
    {
        protected override DriverResult Display(ContentPart part, string displayType, dynamic shapeHelper)
        {
            if (displayType == "SummaryAdmin")
            {
                return ContentShape("Parts_Versions_SummaryAdmin",
                    () => shapeHelper.Parts_Versions_SummaryAdmin(ContentPart: part));
            }
            return null;
        }
    }
}