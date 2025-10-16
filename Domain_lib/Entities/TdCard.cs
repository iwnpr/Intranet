using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain_lib.Entities;

public partial class TdCard
{
    public long Keyid { get; set; }

    [Required(ErrorMessage = "Поле обязательно к заполнению")]
    public string CardName { get; set; } = null!;

    public string? Description { get; set; }

    public long ColumnId { get; set; }

    public long GitId { get; set; }

    public long GitIid { get; set; }

    public long? AssignedId { get; set; }

    public long AuthorId { get; set; }

    public DateTime? Duedate { get; set; }

    public int? Priority { get; set; }

    public int OrderNum { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public long? ClosedBy { get; set; }

    public DateTime Created { get; set; }

    public virtual TdUser? Assigned { get; set; }

    public virtual TdUser Author { get; set; } = null!;

    public virtual TdUser? ClosedByNavigation { get; set; }

    public virtual TdColumn Column { get; set; } = null!;

    public virtual TrPriority? PriorityNavigation { get; set; }

    public virtual ICollection<TdComment> TdComments { get; set; } = new List<TdComment>();
}
