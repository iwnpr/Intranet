namespace Domain_lib.Entities;

public partial class TrPriority
{
    public int Keyid { get; set; }

    public string PriorityName { get; set; } = null!;

    public virtual ICollection<TdCard> TdCards { get; set; } = new List<TdCard>();
}
