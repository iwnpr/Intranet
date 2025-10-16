using System;
using System.Collections.Generic;

namespace Domain_lib.Entities;

public partial class TdProject
{
    public long Keyid { get; set; }

    public string ProjectName { get; set; } = null!;

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }

    public long? GroupId { get; set; }

    public long? GitId { get; set; }

    public long? OwnerId { get; set; }

    public virtual TdUser? Owner { get; set; }

    public virtual ICollection<TdColumn> TdColumns { get; set; } = new List<TdColumn>();
}
