using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace DotNetServer.TCP.Services;

/// <summary>
/// Service to send/receive byte array data (length is important to specify, especially while listening - as buffer will be used)
/// </summary>
public interface IPacketListenerService
{
    IAsyncEnumerable<BufferData> Subscribe(IPAddress ipAddressToListen,
        int portToListen, CancellationToken cancellationToken);
    Task SendAsync(byte[] dataToSend, IPAddress destinationIpAddress, int destinationPort); 
}

public class PacketListenerService : IPacketListenerService
{
    public async IAsyncEnumerable<BufferData> Subscribe(IPAddress ipAddressToListen, int portToListen,
        [EnumeratorCancellation]CancellationToken cancellationToken)
    {
        using var rawSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
        rawSocket.Bind(new IPEndPoint(ipAddressToListen, portToListen));
        rawSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
        byte[] buffer = new byte[65535];

        while (!cancellationToken.IsCancellationRequested)
        {
            int bytesRead = await rawSocket.ReceiveAsync(buffer);

            var bufferData = new BufferData(buffer, bytesRead);

            yield return bufferData; 
        }
    }

    public Task SendAsync(byte[] dataToSend, IPAddress destinationIpAddress, int destinationPort) => throw new NotImplementedException();
}
