namespace SpiralGrid.Models
{
    public class SpiralGridViewModel
    {
        public int[,] Grid { get; set; }

        public List<List<int>> IntersectedNumbers { get; set; } = [];

        public List<int> Targets { get; set; } = [];
    }
}
