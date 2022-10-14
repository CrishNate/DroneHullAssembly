using DotNetGraph.Core;

namespace DotNetGraph.Attributes
{
    public class DotRotationAttribute : IDotAttribute
    {
        public DotPosition Rotation { get; set; }
        
        public DotRotationAttribute(DotPosition rotation = default)
        {
            Rotation = rotation;
        }
        
        public DotRotationAttribute(int x, int y, int z)
        {
            Rotation = new DotPosition(x, y, z);
        }
        
        public static implicit operator DotRotationAttribute(DotPosition rotation)
        {
            return new DotRotationAttribute(rotation);
        }
    }
}