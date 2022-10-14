using DotNetGraph.Core;

namespace DotNetGraph.Attributes
{
    public class DotPositionAttribute : IDotAttribute
    {
        public DotPosition Position { get; set; }
        
        public DotPositionAttribute(DotPosition position = default)
        {
            Position = position;
        }
        
        public DotPositionAttribute(int x, int y, int z)
        {
            Position = new DotPosition(x, y, z);
        }
        
        public static implicit operator DotPositionAttribute(DotPosition position)
        {
            return new DotPositionAttribute(position);
        }
    }
}