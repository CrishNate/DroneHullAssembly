using DotNetGraph.Core;

namespace DotNetGraph.Attributes
{
    public class DotMirrorAttribute : IDotAttribute
    {
        public bool SocketMirror { get; set; }
        
        public DotMirrorAttribute(bool socketMirror = default)
        {
            SocketMirror = socketMirror;
        }
        
        public static implicit operator DotMirrorAttribute(bool? socketMirror)
        {
            return socketMirror.HasValue ? new DotMirrorAttribute(socketMirror.Value) : null;
        }
    }
}