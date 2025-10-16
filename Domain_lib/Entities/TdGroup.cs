using System;
using System.Collections.Generic;

namespace Domain_lib.Entities;

public partial class TdGroup
{
    public long Keyid { get; set; }

    public string GroupName { get; set; } = null!;

    public long? GitId { get; set; }

    public long? OwnerId { get; set; }

    public DateTime Created { get; set; }

    public virtual TdUser? Owner { get; set; }
}
