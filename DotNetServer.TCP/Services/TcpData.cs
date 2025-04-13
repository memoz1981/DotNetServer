using DotNetServer.TCP.IP;
using DotNetServer.TCP.TCP;

namespace DotNetServer.TCP.Services;

public record struct TcpData(IpHeader IpHeader, TcpHeader TcpHeader, BufferData Data, int? DataIndexStart);
