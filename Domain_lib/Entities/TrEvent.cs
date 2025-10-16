namespace Domain_lib.Entities;

public partial class TrEvent
{
    public int Keyid { get; set; }

    public string EventName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<TeHistory> TeHistories { get; set; } = new List<TeHistory>();
}
