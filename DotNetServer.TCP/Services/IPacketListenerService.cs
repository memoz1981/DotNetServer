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

        _dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Tcp);
        _dataSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
        _dataSocket.Bind(new IPEndPoint(ipAddressToListen, portToListen));
        

        byte[] buffer = new byte[65535];

        while (!cancellationToken.IsCancellationRequested)
        {
            int bytesRead = await _dataSocket.ReceiveAsync(buffer);

            var received = string.Join(',', buffer.Take(bytesRead).Select(x => x.ToString("X2")));

            var bufferData = new BufferData(buffer, 0, bytesRead);

            yield return bufferData; 
        }
    }

    public void Dispose() => _dataSocket?.Dispose();

    public async Task SendAsync(byte[] dataToSend, IPEndPoint endPoint)
    {
        var bytesToSend = string.Join(',', dataToSend.Select(x => x.ToString("X2")));
        await _dataSocket.SendToAsync(dataToSend, endPoint);
    }

}
