using System;
using System.Collections.Generic;

namespace Edunext.Models;

public partial class Assignment
{
    public int Id { get; set; }

    public int ClassSlotId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly DueDate { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();

    public virtual ClassSlotContent ClassSlot { get; set; } = null!;
}
