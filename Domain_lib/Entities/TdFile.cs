using System;
using System.Collections.Generic;

namespace Domain_lib.Entities;

public partial class TdFile
{
    public long Keyid { get; set; }

    public string FileName { get; set; } = null!;

    public string? FilePath { get; set; }

    public string? FileExtension { get; set; }

    public string Url { get; set; } = null!;

    public string InnerPath { get; set; } = null!;

    public int StatusId { get; set; }

    public long UserId { get; set; }

    public long? ProjectId { get; set; }

    public long? CommentId { get; set; }

    public DateTime Created { get; set; }

    public virtual TdComment? Comment { get; set; }

    public virtual TrStatus Status { get; set; } = null!;

    public virtual TdUser User { get; set; } = null!;
}
