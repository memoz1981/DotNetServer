using DotNetServer.TCP.IP;

namespace DotNetServer.TCP.TCP;
public interface ITcpHeaderParser
{
    TcpHeader Decode(byte[] data, int startIndex, out int length);

    void Encode(TcpHeader ipHeader, byte[] data, int startIndex, out int length);
}

public class TcpHeaderParser : ITcpHeaderParser
{
    public TcpHeader Decode(byte[] data, int startIndex, out int length)
    {

    }
    public void Encode(TcpHeader ipHeader, byte[] data, int startIndex, out int length) => throw new NotImplementedException();
}
