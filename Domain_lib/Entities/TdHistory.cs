using System;
using System.Collections.Generic;

namespace Domain_lib.Entities;

public partial class TdHistory
{
    public long Keyid { get; set; }

    public DateTime Timestamp { get; set; }

    public string EntityType { get; set; } = null!;

    public long EntityId { get; set; }

    public string Action { get; set; } = null!;

    public string? Description { get; set; }

    public long UserId { get; set; }

    public string? OldState { get; set; }

    public bool IsSystem { get; set; }

    public virtual TdUser User { get; set; } = null!;
}
