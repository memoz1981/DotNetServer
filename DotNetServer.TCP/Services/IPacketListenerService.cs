using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace DotNetServer.TCP.Services;

/// <summary>
/// Service to send/receive byte array data (length is important to specify, especially while listening - as buffer will be used)
/// </summary>
public interface IPacketListenerService : IDisposable
{
    IAsyncEnumerable<BufferData> Subscribe(IPAddress ipAddressToListen,
        int portToListen, CancellationToken cancellationToken);
    Task SendAsync(byte[] dataToSend, IPEndPoint endPoint); 
}

public class PacketListenerService : IPacketListenerService
{
    private Socket _dataSocket = null;

    public async IAsyncEnumerable<BufferData> Subscribe(IPAddress ipAddressToListen, int portToListen,
        [EnumeratorCancellation]CancellationToken cancellationToken)
    {
        if (_dataSocket is not null)
            throw new InvalidOperationException("Already listening to another stream...");

        _dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
        _dataSocket.Bind(new IPEndPoint(ipAddressToListen, portToListen));
        _dataSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

        byte[] buffer = new byte[65535];

        while (!cancellationToken.IsCancellationRequested)
        {
            int bytesRead = await _dataSocket.ReceiveAsync(buffer);

            var bufferData = new BufferData(buffer, 0, bytesRead);

            yield return bufferData; 
        }
    }

    public void Dispose() => _dataSocket?.Dispose();

    public async Task SendAsync(byte[] dataToSend, IPEndPoint endPoint)
        => await _dataSocket.SendToAsync(dataToSend, endPoint); 
}
