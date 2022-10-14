using DotNetGraph.Core;

namespace DotNetGraph.Attributes
{
    public class DotSocketToAttribute : IDotAttribute
    {
        public int SocketIndex { get; set; }
        
        public DotSocketToAttribute(int socketIndex = default)
        {
            SocketIndex = socketIndex;
        }
        
        public static implicit operator DotSocketToAttribute(int? socketIndex)
        {
            return socketIndex.HasValue ? new DotSocketToAttribute(socketIndex.Value) : null;
        }
    }
}