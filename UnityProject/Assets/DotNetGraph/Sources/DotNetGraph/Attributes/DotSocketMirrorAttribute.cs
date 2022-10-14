using DotNetGraph.Core;

namespace DotNetGraph.Attributes
{
    public class DotSocketMirrorAttribute : IDotAttribute
    {
        public int SocketIndex { get; set; }
        
        public DotSocketMirrorAttribute(int socketIndex = default)
        {
            SocketIndex = socketIndex;
        }
        
        public static implicit operator DotSocketMirrorAttribute(int? socketIndex)
        {
            return socketIndex.HasValue ? new DotSocketMirrorAttribute(socketIndex.Value) : null;
        }
    }
}