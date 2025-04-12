namespace DotNetServer.TCP.Extensions;
public static class BigEndianExtensions
{
    public static void WriteUInt32BigEndian(this uint value, byte[] data, ref int index)
    {
        data[index++] = (byte)((value >> 24) & 0xFF);
        data[index++] = (byte)((value >> 16) & 0xFF);
        data[index++] = (byte)((value >> 8) & 0xFF);
        data[index++] = (byte)(value & 0xFF);
    }

    public static void WriteUShortBigEndian(this ushort value, byte[] data, ref int index)
    {
        data[index++] = (byte)((value >> 8) & 0xFF);
        data[index++] = (byte)(value & 0xFF);
    }

    public static uint ReadUIntFromBigEndianArray(this byte[] data, int startIndex)
    {
        return (uint)(data[startIndex++] << 24 | data[startIndex++] << 16 | data[startIndex++] << 8 | data[startIndex++]); 
    }
}
