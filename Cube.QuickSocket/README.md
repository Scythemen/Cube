# quick-socket

Yet another simple way to use socket by quick start.

```text
quick-socket pipeline

         socket          middleware    middleware    middleware     middleware    middleware
       connection         logging     flow-analyze   protocol-A     protocol-B       last
       ┌─────────┐       ┌────────┐    ┌────────┐    ┌────────┐     ┌────────┐    ┌────────┐
 input │         │ decode│        │    │        │    │        │     │        │    │        │
 ──────┼─►       ├───────┼─►      ├────┼──►     ├────┼─►      ├─────┼──►     ├────┼───►    │
       │         │       │        │    │        │    │        │     │        │    │        │
       │         │       │        │    │        │    │        │     │        │    │        │
output │         │ encode│        │    │        │    │        │     │        │    │        │
◄──────┤      ◄──┼───────┤     ◄──┼────┤    ◄───┼────┤    ◄───┼─────┤    ◄───┼────┤        │◄─┐
       │         │       │        │    │        │    │        │     │        │    │        │  │
       │ ▲    ◄──┼───────┼────────┼────┼────────┼────┼─2      │     │        │    │        │  │
       │ │       │       │        │    │        │    │        │     │        │    │        │  │
       └─┼───────┘       └────────┘    └────────┘    └─┬───┬──┘     └────────┘    └────────┘  │
         │                                            1│   │3                                 │
         └─────────────────────────────────────────────┘   └──────────────────────────────────┘


```

There are 3 ways to send the output by the extension methods of the `ConnectionContext` in class `ConnectionContextExtensions.cs`:  
- `Arrow-Line 1`, send directly to the socket connection.
- `Arrow-Line 2`, start from the specific encoder middleware(here is the middleware `protocol-A`),
  and pass the output through the encoder middlewares until to the socket.   
- `Arrow-Line 3`, start from the tail of pipeline, and pass the output through all of the encoder middlewares until to the socket.


