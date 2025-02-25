using System;
using System.Collections.Generic;

namespace Edunext.Models;

public partial class Slot
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string Name { get; set; } = null!;

    public int Order { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<ClassSlotContent> ClassSlotContents { get; set; } = new List<ClassSlotContent>();

    public virtual Course Course { get; set; } = null!;
}
