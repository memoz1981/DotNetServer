﻿using DotNetServer.TCP.IP;
using DotNetServer.TCP.TCP;

namespace DotNetServer.TCP.Services;

public record struct TcpData(IpHeader ipHeader, TcpHeader tcpHeader, BufferData data, int? DataIndexStart);
