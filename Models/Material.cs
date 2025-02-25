using System;
using System.Collections.Generic;

namespace Edunext.Models;

public partial class Material
{
    public int MaterialId { get; set; }

    public int ClassId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string FileLink { get; set; } = null!;

    public DateTime? UploadedAt { get; set; }

    public virtual Classroom Class { get; set; } = null!;
}
