using System;
using System.Collections.Generic;

namespace Domain_lib.Entities;

public partial class TdUser
{
    public long Keyid { get; set; }

    public string UserName { get; set; } = null!;

    public string Login { get; set; } = null!;

    public string? Email { get; set; }

    public long? GitId { get; set; }

    public int StatusId { get; set; }

    public bool IsSystem { get; set; }

    public DateTime Created { get; set; }

    public virtual TrStatus Status { get; set; } = null!;

    public virtual ICollection<TdCard> TdCardAssigneds { get; set; } = new List<TdCard>();

    public virtual ICollection<TdCard> TdCardAuthors { get; set; } = new List<TdCard>();

    public virtual ICollection<TdCard> TdCardClosedByNavigations { get; set; } = new List<TdCard>();

    public virtual ICollection<TdComment> TdComments { get; set; } = new List<TdComment>();

    public virtual ICollection<TdFile> TdFiles { get; set; } = new List<TdFile>();

    public virtual ICollection<TdGroup> TdGroups { get; set; } = new List<TdGroup>();

    public virtual ICollection<TdHistory> TdHistories { get; set; } = new List<TdHistory>();

    public virtual ICollection<TdProject> TdProjects { get; set; } = new List<TdProject>();
}
