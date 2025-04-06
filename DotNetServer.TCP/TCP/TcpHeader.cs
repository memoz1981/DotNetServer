using System.Text;

namespace DotNetServer.TCP.TCP;
public class TcpHeader
{
    public TcpHeader(
        int sourcePort,
        int destinationPort,
        uint sequenceNumber,
        uint acknowledgementNumber,
        byte dataOffset,
        TcpHeaderFlags flags,
        int window,
        int checksum,
        int urgentPointer)
    {
        SourcePort = sourcePort;
        DestinationPort = destinationPort;
        SequenceNumber = sequenceNumber;
        AcknowledgementNumber = acknowledgementNumber;
        DataOffset = dataOffset;
        Flags = flags;
        Window = window;
        Checksum = checksum;
        UrgentPointer = urgentPointer;
    }

    public int SourcePort { get; }
    public int DestinationPort { get; }
    public uint SequenceNumber { get; }
    public uint AcknowledgementNumber { get; }
    public byte DataOffset { get; }
    public int TcpHeaderLength { get => DataOffset * 4; }
    public TcpHeaderFlags Flags { get; }
    public int Window { get; }
    public int Checksum { get; }
    public int UrgentPointer { get; }
    //placeholder property to sort out options - for now skipping. 
    public int OptionsLength { get => TcpHeaderLength - 20; }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.AppendLine("------- TCP Header Start --------");
        builder.AppendLine($"SourcePort: {SourcePort}");
        builder.AppendLine($"DestinationPort: {DestinationPort}");
        builder.AppendLine($"SequenceNumber: {SequenceNumber}");
        builder.AppendLine($"AcknowledgementNumber: {AcknowledgementNumber}");
        builder.AppendLine($"DataOffset: {DataOffset}");
        builder.AppendLine($"TcpHeaderLength: {TcpHeaderLength}");
        builder.AppendLine($"Flags: {Flags}");
        builder.AppendLine($"Window: {Window}");
        builder.AppendLine($"Checksum: {Checksum}");
        builder.AppendLine($"UrgentPointer: {UrgentPointer}");
        builder.AppendLine($"OptionsLength: {OptionsLength}");
        builder.AppendLine("------- TCP Header End --------");

        return builder.ToString();
    }
}
