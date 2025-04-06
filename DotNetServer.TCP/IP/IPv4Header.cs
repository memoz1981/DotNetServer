namespace DotNetServer.TCP.IP;
public sealed class IPv4Header : IpHeader
{
    public IPv4Header() 
    {
        
    }


    // 4 bits - number of 32 bit words in IP Header
    // (value 5 -> IP header length 20 bytes)
    public byte InternetHeaderLength { get; }

    // calculated IP Header length
    public int HeaderLength { get => InternetHeaderLength * 4; }

    // 6 bits - real time streaming uses this
    public byte DifferentiatedServicesCodePoint { get; }

    // 2 bits - optional feature
    public byte ExplicitCongestionNotification { get; }

    // packet size in bytes
    // minimum 20 bytes - maximum 65535 bytes
    public int TotalLength { get; }

    // used to identify fragmented packets of a single IP datagram
    public int Identification { get; }

    // fragmentation flags
    public IpFragmentationFlags Flags { get; }

    // offset of a particular fragment relative to original IP datagram
    public int FragmentOffset { get; }

    // determines a datagrams lifetime
    public byte TimeToLive { get; }

    // protocol
    public Protocols Protocol { get; }

    // calculated value - for received packets read, for sent calculate
    public int HeaderChecksum { get; }

    // For now will be kept as raw data
    public byte[] Options { get; }
}
