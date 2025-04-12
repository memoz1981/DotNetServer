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
            checksum: 5644,
            urgentPointer: 5645);

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
        var decodedHeader = parser.Decode(data, 0, out var lengthDecoded);

        //assert
        decodedHeader.ShouldNotBeNull();
        length.ShouldBe(56);
        lengthDecoded.ShouldBe(56);

        decodedHeader.SourcePort.ShouldBe(header.SourcePort);
        decodedHeader.DestinationPort.ShouldBe(header.DestinationPort);
        decodedHeader.SequenceNumber.ShouldBe(header.SequenceNumber);
        decodedHeader.AcknowledgementNumber.ShouldBe(header.AcknowledgementNumber);
        decodedHeader.DataOffset.ShouldBe(header.DataOffset);
        decodedHeader.Flags.ShouldBe(header.Flags);
        decodedHeader.Window.ShouldBe(header.Window);
        decodedHeader.Checksum.ShouldBe(header.Checksum);
        decodedHeader.UrgentPointer.ShouldBe(header.UrgentPointer);

        decodedHeader.Options.Count.ShouldBe(header.Options.Count);
        decodedHeader.Options[0].ShouldBeOfType<TcpOptionNone>();

        decodedHeader.Options[1].ShouldBeOfType<TcpOptionNoOp>();

        decodedHeader.Options[2].ShouldBeOfType<TcpOptionMss>();
        ((TcpOptionMss)decodedHeader.Options[2]).MaximumSegmentSize.ShouldBe((ushort)9856);

        decodedHeader.Options[3].ShouldBeOfType<TcpOptionWindowScale>();
        ((TcpOptionWindowScale)decodedHeader.Options[3]).WindowScale.ShouldBe((byte)234);

        decodedHeader.Options[4].ShouldBeOfType<TcpOptionsSackPermitted>();

        decodedHeader.Options[5].ShouldBeOfType<TcpOptionsSack>();
        var block = ((TcpOptionsSack)decodedHeader.Options[5]).Blocks;
        block.Count.ShouldBe(1);
        block.First().Item1.ShouldBe((uint)5678123);
        block.First().Item2.ShouldBe((uint)1234876); 

        decodedHeader.Options[6].ShouldBeOfType<TcpOptionsTimestamp>();
        ((TcpOptionsTimestamp)decodedHeader.Options[6]).TimestampValue.ShouldBe((uint)1357246);
        ((TcpOptionsTimestamp)decodedHeader.Options[6]).TimestampEchoReply.ShouldBe((uint)2468135);

        decodedHeader.Options[7].ShouldBeOfType<TcpOptionUserTimeout>();
        ((TcpOptionUserTimeout)decodedHeader.Options[7]).TimeoutInMs.ShouldBe((ushort)12567);

        decodedHeader.Options[8].ShouldBeOfType<TcpOptionNone>();
    }
}
