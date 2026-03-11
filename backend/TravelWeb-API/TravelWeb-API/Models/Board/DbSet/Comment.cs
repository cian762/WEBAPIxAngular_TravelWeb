using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelWeb_API.Models.Board.DbSet;

public partial class Comment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CommentId { get; set; }

    public int ArticleId { get; set; }

    public string UserId { get; set; }

    public int? ParentId { get; set; }

    public string? Contents { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Article Article { get; set; } = null!;
}
