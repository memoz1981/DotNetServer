using System.Net;
using System.Net.Sockets;

namespace DotNetServer.TCP;
public class TcpListener : ITcpListener
{
    private const AddressFamily _addressFamily = AddressFamily.InterNetwork;
    private const SocketType _socketType = SocketType.Stream;
    private const ProtocolType _protocolType = ProtocolType.Tcp;
    private readonly Socket _listener = null; 
    public IPAddress Host { get; init; }
    public int Port { get; init; }
    public TcpListener() {}
    public TcpListener(IPAddress host, int port) => (Host, Port) = (host, port);
    public void Dispose() => _listener?.Dispose();

    public Task Initialize() => throw new NotImplementedException();

    public Task<Socket> AcceptAsync() => throw new NotImplementedException();
}
