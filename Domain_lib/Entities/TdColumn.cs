using System;
using System.Collections.Generic;

namespace Domain_lib.Entities;

public partial class TdColumn
{
    public long Keyid { get; set; }

    public string ColumnName { get; set; } = null!;

    public int ColumnOrder { get; set; }

    public long ProjectId { get; set; }

    public int StatusId { get; set; }

    public DateTime Created { get; set; }

    public virtual TdProject Project { get; set; } = null!;

    public virtual TrStatus Status { get; set; } = null!;

    public virtual ICollection<TdCard> TdCards { get; set; } = new List<TdCard>();
}
