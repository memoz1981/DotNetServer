using DotNetServer.TCP.IP;
using DotNetServer.TCP.TCP;

namespace DotNetServer.TCP.Services;

//for now this service assumed one packet per request, later will be extended...
// check if it needs to be disposable
public class TcpConnection
{
    public TcpConnection(
        ITcpConnectionManager connectionManager,
        TcpConnectionKey key,
        ushort? maximumSegmentSize,
        byte? windowScale,
        bool sackPermitted,
        List<(uint, uint)> sackBlocks,
        uint? timestampValue,
        uint? timestampEchoReply,
        ushort? timeoutInMs,
        TcpConnectionState state = TcpConnectionState.None)
    {
        _key = key;
        _state = state;
        _maximumSegmentSize = maximumSegmentSize;
        _windowScale = windowScale;
        _sackPermitted = sackPermitted;
        _sackBlocks = sackBlocks;
        _timestampValue = timestampValue;
        _timestampEchoReply = timestampEchoReply;
        _timeoutInMs = timeoutInMs;
        _connectionManager = connectionManager;
    }

    private readonly TcpConnectionKey _key;
    private TcpConnectionState _state;
    private ushort? _maximumSegmentSize;
    private byte? _windowScale;
    private bool? _sackPermitted;
    private List<(uint, uint)> _sackBlocks;
    private uint? _timestampValue;
    private uint? _timestampEchoReply;
    private ushort? _timeoutInMs;
    private readonly ITcpConnectionManager _connectionManager;

    /// <summary>
    /// Updates Tcp connection state and sets values
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public void Receive(TcpProcessingContext context)
    {
        switch  (context.TcpHeaderReceived.Flags, _state) 
        {
            case (TcpHeaderFlags.SYN, TcpConnectionState.None):
                _state = TcpConnectionState.SynReceived;
                break;

            case (TcpHeaderFlags.SYN, _):
                throw new InvalidOperationException("The connection state is already set...");

            case (TcpHeaderFlags.ACK, TcpConnectionState.SynAckSent):
                _state = TcpConnectionState.Established;
                break;

            default:
                throw new NotImplementedException("This section will be populated later..."); 
        };
    }

    /// <summary>
    /// Prepares packets to be sent back to client. 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public IEnumerable<TcpProcessingContext> Send(TcpProcessingContext context)
    {
        if (_state == TcpConnectionState.SynReceived)
        {
            var tcpHeader = BuildTcpHeader(context, 6500, context.TcpHeaderReceived.SequenceNumber + 1,
                TcpHeaderFlags.SYN | TcpHeaderFlags.ACK, 0); 
            var ipHeader = (IPv4Header)BuildIpHeader(context, tcpHeader.TcpHeaderLength + 20);

            var checkSumTcp = ChecksumCalculator.ComputeTcpChecksumSafe(ipHeader, tcpHeader, []);
            tcpHeader.SetCheckSum(checkSumTcp);

            var checkSumIp = ChecksumCalculator.CalculateChecksum(ipHeader);
            ipHeader.SetChecksum(checkSumIp);

            context.SetIpHeader(ipHeader);
            context.SetTcpHeader(tcpHeader);
        }

        yield return context; 
    }

    private IpHeader BuildIpHeader(TcpProcessingContext context, int totalLength, int? identification = null)
    {
        if (context.IpHeaderSent is not null)
            return context.IpHeaderSent;

        var version = context.IpHeaderReceived.Version;

        var ipv4 = (IPv4Header)context.IpHeaderReceived;

        return new IPv4Header(
            version: context.IpHeaderReceived.Version,
            sourceAddress: context.IpHeaderReceived.DestinationAddress,
            destinationAddress: context.IpHeaderReceived.SourceAddress,
            internetHeaderLength: 5,
            differentiatedServicesCodePoint: ipv4.DifferentiatedServicesCodePoint,
            explicitCongestionNotification: ipv4.ExplicitCongestionNotification,
            totalLength: totalLength,
            identification: identification ?? ipv4.Identification,
            flags: IpFragmentationFlags.DontFragment,
            fragmentOffset: 0,
            timeToLive: ipv4.TimeToLive,
            protocol: ipv4.Protocol,
            headerChecksum: 0,
            options: []);
    }

    private TcpHeader BuildTcpHeader(TcpProcessingContext context, uint sequenceNumber, uint ackNumber,
        TcpHeaderFlags flags, byte dataOffset)
    {
        if (context.TcpHeaderSent is not null)
            return context.TcpHeaderSent;

        var optionsLength = (byte)context.TcpHeaderReceived.Options.Sum(op => (byte)op.Length);

        var tcpHeader = new TcpHeader(
            sourcePort: context.TcpHeaderReceived.DestinationPort,
            destinationPort: context.TcpHeaderReceived.SourcePort,
            sequenceNumber: sequenceNumber,
            acknowledgementNumber: ackNumber,
            dataOffset: (byte)((optionsLength + 20)/4),
            flags: flags,
            window: context.TcpHeaderReceived.Window,
            checksum: 0,
            urgentPointer: 0);

        //for now just add same options as original header...
        foreach (var option in context.TcpHeaderReceived.Options)
            tcpHeader.AddOption(option);

        return tcpHeader; 
    }

    public bool IsConnectionEstablished() => _state == TcpConnectionState.Established; 
}
