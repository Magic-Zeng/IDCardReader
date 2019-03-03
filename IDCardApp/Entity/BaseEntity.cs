using System.ComponentModel.DataAnnotations;

namespace IDCardApp.Entity
{
    public class BaseEntity
    {
        [Key]
        public string PkId { get; set; }

        public BaseEntity NewId()
        {
            this.PkId = System.Guid.NewGuid().ToString("N");
            return this;
        }
    }
}
