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
        ushort checksum,
        ushort urgentPointer)
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
        Options = new();
    }

    public void AddOption(TcpOption optionToAdd) => Options.Add(optionToAdd);

    public int SourcePort { get; }
    public int DestinationPort { get; }
    public uint SequenceNumber { get; }
    public uint AcknowledgementNumber { get; }
    public byte DataOffset { get; }
    public int TcpHeaderLength { get => DataOffset * 4; }
    public TcpHeaderFlags Flags { get; }
    public int Window { get; }
    public ushort Checksum { get; }
    public ushort UrgentPointer { get; }
    //placeholder property to sort out options - for now skipping. 
    public int OptionsLength { get => TcpHeaderLength - 20; }
    public List<TcpOption> Options { get; }

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
        foreach (var option in Options)
            builder.Append(option.ToString());

        builder.AppendLine("------- TCP Header End --------");

        return builder.ToString();
    }
}
