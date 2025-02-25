using System;
using System.Collections.Generic;

namespace Edunext.Models;

public partial class ClassEnrollment
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public int UserId { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Classroom Class { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
