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
        var ipHeader = new IPv4Header(
            version: IpVersion.IPv4,
            sourceAddress: new IPAddress([127, 0, 0, 1]),
            destinationAddress: new IPAddress([255, 255, 1, 0]),
            internetHeaderLength: 5,
            differentiatedServicesCodePoint: 10,
            explicitCongestionNotification: 1,
            totalLength: 54321,
            identification: 12345,
            flags: IpFragmentationFlags.DontFragment,
            fragmentOffset: 0,
            timeToLive: 120,
            protocol: Protocols.TCP,
            headerChecksum: 12000,
            options: null);
        byte[] data = new byte[20];

        //act
        _ipHeaderParser.Encode(ipHeader, data, 0, out var length);

        var stringRep = string.Join(',', data); 

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
