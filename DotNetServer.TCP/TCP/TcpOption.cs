namespace DotNetServer.TCP.TCP;
public abstract class TcpOption
{
    protected TcpOption(TcpOptionsKind kind, int length)
    {
        Kind = kind;
        Length = length;
    }

    public TcpOptionsKind Kind { get; }
    public int Length { get; }
}

public class TcpOptionNone : TcpOption
{
    public TcpOptionNone() : base(TcpOptionsKind.EndOfOptionsList, 1) {}
}

public class TcpOptionNoOp : TcpOption
{
    public TcpOptionNoOp() : base(TcpOptionsKind.NoOp, 1) { }
}

public class TcpOptionMss : TcpOption
{
    public TcpOptionMss() : base(TcpOptionsKind.MaximumSegmentSize, 4)
    {
        MaximumSegmentSize = 4;
    }

    public ushort MaximumSegmentSize { get; }
}

public class TcpOptionWindowScale : TcpOption
{
    public TcpOptionWindowScale() : base(TcpOptionsKind.MaximumSegmentSize, 3)
    {
        WindowScale = 3;
    }

    public byte WindowScale { get; }
}
