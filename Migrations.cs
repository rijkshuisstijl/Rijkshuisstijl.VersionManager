#region

using Orchard.Data.Migration;

#endregion

namespace Rijkshuisstijl.VersionManager
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable("VersionManagerRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("ContentItemId")
                .Column<int>("ContentItemVersionId")
                .Column<string>("Description"));
            return 1;
        }
    }
}