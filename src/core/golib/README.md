# go.lib — go2cs core runtime library

go.lib is the hand-written C# runtime that implements Go language semantics for code converted by
[go2cs](https://github.com/ritchiecarroll/go2cs): slices, maps, channels, Go strings, arrays, the heap box that
stands for a Go pointer, nil, the builtin functions (append, len, make, panic, recover and the rest), and the Go
numeric type aliases. [Consuming converted Go from C#](https://go2cs.net/ConsumingGoFromCSharp.html) shows how each
of them looks from C#.

Every go2cs-converted package (go.fmt, go.strings, …) depends on go.lib. It is pulled in
automatically when you install any converted package — you normally do not reference it directly.

For a Native AOT or trimmed publish, go.lib sets TrimMode to partial unless your project sets a trim mode itself:
go.lib reads the layout of Go structs by reflection, and a full trim removes the field metadata it needs.

## License

MIT. See the [go2cs repository](https://github.com/ritchiecarroll/go2cs).

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
