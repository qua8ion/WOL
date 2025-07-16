using DbData.Entities.Abstracts;

namespace DbData.Entities;

public class Device: BaseEntity
{
    public string IpV4 { get; set; } = null!;
    public string Mac { get; set; } = null!;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ICollection<User> AlowedUsers { get; set; }
}