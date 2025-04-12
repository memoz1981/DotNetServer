using System.Net;

namespace DotNetServer.TCP.Services;

/// <summary>
/// Service to send/receive byte array data (length is important to specify, especially while listening - as buffer will be used)
/// </summary>
public interface IPacketListenerService
{
    IAsyncEnumerable<(byte[] data, int length)> Subscribe(IPAddress ipAddressToListen, int portToListen, CancellationToken cancellationToken);
    Task SendAsync(byte[] dataToSend, int length, IPAddress destinationIpAddress, int destinationPort); 
}
