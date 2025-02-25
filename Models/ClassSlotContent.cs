using System;
using System.Collections.Generic;

namespace Edunext.Models;

public partial class ClassSlotContent
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public int SlotId { get; set; }

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual Classroom Class { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual Slot Slot { get; set; } = null!;
}
