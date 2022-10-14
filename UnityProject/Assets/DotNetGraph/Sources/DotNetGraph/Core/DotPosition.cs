namespace DotNetGraph.Core
{
    public class DotPosition
    {
        public int X { get; set; }

        public int Y { get; set; }
        
        public int Z { get; set; }

        public DotPosition()
        {
        }

        public DotPosition(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = y;
        }
    }
}