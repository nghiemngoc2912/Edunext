using System;
using System.Collections.Generic;

namespace Edunext.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public int QuestionId { get; set; }

    public int UserId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? UpdatedAt { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
