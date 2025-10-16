using System;
using System.Collections.Generic;

namespace Domain_lib.Entities;

public partial class TeHistory
{
    public long Keyid { get; set; }

    public long UserId { get; set; }

    public int EventId { get; set; }

    public string? Description { get; set; }

    public long History { get; set; }

    public DateTime EventDateTime { get; set; }

    public virtual TrEvent Event { get; set; } = null!;

    public virtual TdHistory HistoryNavigation { get; set; } = null!;

    public virtual TdUser User { get; set; } = null!;
}
