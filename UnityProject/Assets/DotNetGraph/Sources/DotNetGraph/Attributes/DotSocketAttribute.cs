using DotNetGraph.Core;

namespace DotNetGraph.Attributes
{
    public class DotSocketAttribute : IDotAttribute
    {
        public int SocketIndex { get; set; }
        
        public DotSocketAttribute(int socketIndex = default)
        {
            SocketIndex = socketIndex;
        }
        
        public static implicit operator DotSocketAttribute(int? socketIndex)
        {
            return socketIndex.HasValue ? new DotSocketAttribute(socketIndex.Value) : null;
        }
    }
}