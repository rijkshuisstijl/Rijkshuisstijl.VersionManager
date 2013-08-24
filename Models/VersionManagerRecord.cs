namespace Rijkshuisstijl.VersionManager.Models
{
    public class VersionManagerRecord
    {
        public virtual int Id { get; set; }
        public virtual int ContentItemId { get; set; }
        public virtual int ContentItemVersionId { get; set; }
        public virtual string Description { get; set; }
    }
}