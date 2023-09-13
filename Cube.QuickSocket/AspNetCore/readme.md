
## Why this?

- Nuget package `Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets` has been deprecated and no longer maintained.
- The `Kestrel` is one of the best scaffold, but it's too heavy for tiny project with custom protocol, 
  and it doesn't provide a unified coding pattern(the server & the client).  


List of source code:

## Buffers.MemoryPool
https://github.com/dotnet/aspnetcore/tree/v7.0.10/src/Shared/Buffers.MemoryPool

## Kestrel.Transport.Sockets.Internal
https://github.com/dotnet/aspnetcore/tree/v7.0.10/src/Servers/Kestrel/Transport.Sockets/src/Internal

## DuplexPipe
https://github.com/dotnet/aspnetcore/blob/v7.0.10/src/Shared/ServerInfrastructure/DuplexPipe.cs

## shared
https://github.com/dotnet/aspnetcore/tree/v7.0.10/src/Servers/Kestrel/shared

## ServerInfrastructure
https://github.com/dotnet/aspnetcore/tree/v7.0.10/src/Shared/ServerInfrastructure
