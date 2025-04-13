namespace DotNetServer.TCP.Services;

public record struct HttpData(TcpData TcpData, byte[] Data); 
