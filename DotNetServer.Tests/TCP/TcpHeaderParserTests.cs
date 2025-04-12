using DotNetServer.TCP.TCP;
using Shouldly;
using Xunit;

namespace DotNetServer.Tests.TCP;
public class TcpHeaderParserTests
{
    [Fact]
    public void ShouldBeAbleTo_EncodeAndDecode_Successfully()
    {
        //arrange
        var header = new TcpHeader(
            sourcePort: 80,
            destinationPort: 443,
            sequenceNumber: 10101,
            acknowledgementNumber: 10102,
            dataOffset: 14,//20 bytes (5 words) header + 36 bytes (9 words) options
            flags: TcpHeaderFlags.SYN | TcpHeaderFlags.ACK | TcpHeaderFlags.FIN,
            window: 20202,
            checksum: 9876543,
            urgentPointer: 1234567);

        //add options
        header.AddOption(new TcpOptionNone()); // 1 byte (actually this option should go to end, but this is test only
        header.AddOption(new TcpOptionNoOp()); //1 byte
        header.AddOption(new TcpOptionMss(9856)); //4 bytes
        header.AddOption(new TcpOptionWindowScale(234)); // 3 bytes
        header.AddOption(new TcpOptionsSackPermitted()); // 2 bytes
        header.AddOption(new TcpOptionsSack([(5678123, 1234876)])); //10 bytes
        header.AddOption(new TcpOptionsTimestamp(1357246, 2468135)); //10 bytes
        header.AddOption(new TcpOptionUserTimeout(12567)); //4 bytes
        header.AddOption(new TcpOptionNone()); //to sum to 36 bytes (additional length of 9)
        var parser = new TcpHeaderParser();
        byte[] data = new byte[56];

        //act - encode
        parser.Encode(header, data, 0, out var length);
        var stringRep = string.Join(',', data); 

        //act - decode back
        var headerDecoded = parser.Decode(data, 0, out var lengthDecoded);

        //assert
        headerDecoded.ShouldNotBeNull();
        length.ShouldBe(56);
        lengthDecoded.ShouldBe(56); 
    }
}
