using System;
using System.Collections.Generic;

namespace Edunext.Models;

public partial class AssignmentSubmission
{
    public int Id { get; set; }

    public int AssignmentId { get; set; }

    public int UserId { get; set; }

    public DateTime? SubmissionDate { get; set; }

    public string? FileLink { get; set; }

    public decimal? Grade { get; set; }

    public string? Feedback { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Assignment Assignment { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
