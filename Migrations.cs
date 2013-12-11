#region

using Orchard.Data.Migration;
using Rijkshuisstijl.VersionManager.Models;

#endregion

namespace Rijkshuisstijl.VersionManager
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable(typeof(VersionManagerRecord).Name, table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("ContentItemId")
                .Column<int>("ContentItemVersionId")
                .Column<string>("Description"));
            return 1;
        }
    }
}