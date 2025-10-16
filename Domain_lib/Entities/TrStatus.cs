namespace Domain_lib.Entities;

public partial class TrStatus
{
    public int Keyid { get; set; }

    public string StatusName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<TdColumn> TdColumns { get; set; } = new List<TdColumn>();

    public virtual ICollection<TdFile> TdFiles { get; set; } = new List<TdFile>();

    public virtual ICollection<TdUser> TdUsers { get; set; } = new List<TdUser>();
}
