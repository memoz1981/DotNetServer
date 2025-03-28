using System.Net.Sockets;
namespace DotNetServer.TCP;

public interface ITcpListener : IDisposable
{
    Task Initialize();
    Task<Socket> AcceptAsync();
}
