// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Sockets;
using Microsoft.AspNetCore.Connections.Features;

namespace Cube.QuickSocket.AspNetCore.Transport.Internal;

internal sealed partial class SocketConnection : IConnectionSocketFeature
{
    public Socket Socket => _socket;

    private void InitializeFeatures()
    {
        _currentIConnectionSocketFeature = this;
    }
}
