namespace Domain_lib.Entities;

public partial class TdRole
{
    public long Keyid { get; set; }

    public string SystemName { get; set; } = null!;

    public string? DisplayName { get; set; }

    public virtual ICollection<TdUser> TdUsers { get; set; } = new List<TdUser>();
}
