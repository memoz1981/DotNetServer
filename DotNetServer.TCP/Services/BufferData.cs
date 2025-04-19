namespace DotNetServer.TCP.Services;

public record struct BufferData(byte[] Data, int DataStartIndex, int Length); 
