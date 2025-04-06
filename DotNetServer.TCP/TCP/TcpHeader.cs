namespace DotNetServer.TCP.TCP;
public class TcpHeader
{
    public int SourcePort { get; }
    public int DestinationPort { get; }
    public int SequenceNumber { get; }
    public int AcknowledgementNumber { get; }
    public byte DataOffset { get; }
    public int TcpHeaderLength { get => DataOffset * 4; }
    public TcpHeaderFlags Flags { get; }
    public int Window { get; }
    public int Checksum { get; }
    public int UrgentPointer { get; }
    //placeholder property to sort out options - for now skipping. 
    public int OptionsLength { get; }

}
