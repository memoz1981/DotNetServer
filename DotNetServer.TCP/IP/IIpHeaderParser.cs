namespace DotNetServer.TCP.IP;
public interface IIpHeaderParser
{
    IpHeader Decode(byte[] data);

    void Encode(IpHeader ipHeader, byte[] data, int startIndex, out int lastIndex); 
}

public class IpHeaderParser : IIpHeaderParser
{
    public IpHeader Decode(byte[] data) => throw new NotImplementedException();

    public void Encode(IpHeader ipHeader, byte[] data, int startIndex, out int lastIndex)
        => throw new NotImplementedException();
}
