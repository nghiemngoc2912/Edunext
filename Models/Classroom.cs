using System;
using System.Collections.Generic;

namespace Edunext.Models;

public partial class Classroom
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public int SemesterId { get; set; }

    public int TeacherId { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsDeleted { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<ClassEnrollment> ClassEnrollments { get; set; } = new List<ClassEnrollment>();

    public virtual ICollection<ClassSlotContent> ClassSlotContents { get; set; } = new List<ClassSlotContent>();

    public virtual Course Course { get; set; } = null!;

    public virtual Semester Semester { get; set; } = null!;

    public virtual User Teacher { get; set; } = null!;
}
