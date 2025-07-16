using DbData.Entities.Abstracts;

namespace DbData.Entities
{
    public class User : BaseEntity
    {
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool Admin { get; set; } = false;
        public ICollection<Device> Devices { get; set; }
    }
}
