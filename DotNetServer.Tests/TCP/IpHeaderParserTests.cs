using System.Net;
using DotNetServer.TCP.IP;
using Shouldly;
using Xunit;

namespace DotNetServer.Tests.TCP;

public class IpHeaderParserTests
{
    private readonly IPv4HeaderParser _ipHeaderParser;

    public IpHeaderParserTests()
    {
        _ipHeaderParser = new(); 
    }

    [Fact]
    public void ShouldBeAbleTo_EncodeAndDecode_Successfully()
    {
        //arrange
        var ipHeader = new IPv4Header(IpVersion.IPv4, new IPAddress([127, 0, 0, 1]),
            new IPAddress([255, 255, 1, 0]), 200, 110, 220, 54321, 1234567, IpFragmentationFlags.DontFragment,
            0, 120, Protocols.TCP, 12000, null);
        byte[] data = new byte[20];

        //act
        _ipHeaderParser.Encode(ipHeader, data, 0, out var length);
        var parsedHeader = _ipHeaderParser.Decode(data, out var lengthParsed);

        //assert
        length.ShouldBe(20);
        lengthParsed.ShouldBe(20);

        var parsedHeaderIpv4 = parsedHeader.ShouldBeOfType<IPv4Header>();

        parsedHeaderIpv4.Version.ShouldBe(ipHeader.Version);
        parsedHeaderIpv4.SourceAddress.ShouldBe(ipHeader.SourceAddress);
        parsedHeaderIpv4.DestinationAddress.ShouldBe(ipHeader.DestinationAddress);
        parsedHeaderIpv4.InternetHeaderLength.ShouldBe(ipHeader.InternetHeaderLength);
        parsedHeaderIpv4.DifferentiatedServicesCodePoint.ShouldBe(ipHeader.DifferentiatedServicesCodePoint);
        parsedHeaderIpv4.ExplicitCongestionNotification.ShouldBe(ipHeader.ExplicitCongestionNotification);
        parsedHeaderIpv4.TotalLength.ShouldBe(ipHeader.TotalLength);
        parsedHeaderIpv4.Identification.ShouldBe(ipHeader.Identification);
        parsedHeaderIpv4.Flags.ShouldBe(ipHeader.Flags);
        parsedHeaderIpv4.FragmentOffset.ShouldBe(ipHeader.FragmentOffset);
        parsedHeaderIpv4.TimeToLive.ShouldBe(ipHeader.TimeToLive);
        parsedHeaderIpv4.Protocol.ShouldBe(ipHeader.Protocol);
        parsedHeaderIpv4.HeaderChecksum.ShouldBe(ipHeader.HeaderChecksum);
        parsedHeaderIpv4.Options.ShouldBe(ipHeader.Options);
    }

}
