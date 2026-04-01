using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.Board.DbSet;

public partial class JournalElement
{
    public int ElementId { get; set; }

    public int? ArticleId { get; set; }

    public byte Page { get; set; }

    public double PosX { get; set; }

    public double PosY { get; set; }

    public double Rotation { get; set; }

    public int Zindex { get; set; }

    public byte? ElementType { get; set; }

    public string? Photo { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }

    public Journal Journal { get; set; } = null!;
    
}
