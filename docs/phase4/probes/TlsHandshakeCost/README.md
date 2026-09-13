# TlsHandshakeCost — how long a TLS handshake costs, converted vs Go

Measures handshake cost with **no `WriteTimeout` set** and keep-alives disabled, so every iteration
pays a full handshake. Written to answer whether net/http's `/h2` write-deadline divergence is a
performance gap or a deadline-semantics fault; see MAILBOX 2026-08-29.

Measured on G-LAPTOP (2026-08-29), three runs per side:

| | mean | worst |
|---|---|---|
| Go | 2 ms | 3 ms |
| converted C# | ~691–705 ms | ~1078–1130 ms |

Run both sides:

```
go run .
go2cs -go2cspath <repo>/src <this-dir>
dotnet build -c Debug -p:go2csPath=<repo>/src/ -p:UseSharedCompilation=false
./bin/Debug/net10.0/TlsHandshakeCost.exe
```

Quote nothing from a single run: take three per side and check they agree first.
