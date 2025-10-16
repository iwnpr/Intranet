using System;
using System.Collections.Generic;

namespace Domain_lib.Entities;

public partial class TdComment
{
    public long Keyid { get; set; }

    public string Context { get; set; } = null!;

    public string CommentText { get; set; } = null!;

    public bool IsSystem { get; set; }

    public bool Deleted { get; set; }

    public long? GitId { get; set; }

    public long UserId { get; set; }

    public long Card { get; set; }

    public DateTime Created { get; set; }

    public virtual TdCard CardNavigation { get; set; } = null!;

    public virtual ICollection<TdFile> TdFiles { get; set; } = new List<TdFile>();

    public virtual TdUser User { get; set; } = null!;
}
