using System;
using System.Collections.Generic;

namespace Edunext.Models;

public partial class Question
{
    public int Id { get; set; }

    public int ClassSlotId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime FromTime { get; set; }

    public DateTime ToTime { get; set; }

    public int? Status { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ClassSlotContent ClassSlot { get; set; } = null!;

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
