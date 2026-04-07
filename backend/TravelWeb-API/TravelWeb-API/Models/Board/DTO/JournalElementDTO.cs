using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TravelWeb_API.Models.Board.DTO
{
    public class JournalElementDTO
    {        
        public byte Page { get; set; }

        public double PosX { get; set; }

        public double PosY { get; set; }

        public double Rotation { get; set; }

        public int Zindex { get; set; }

        public byte? ElementType { get; set; }

        public string content { get; set; } = null!;

        public int Width { get; set; }
        public int Height { get; set; }

    }
}
