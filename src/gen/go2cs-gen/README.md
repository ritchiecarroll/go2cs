# go.gen — go2cs source generator

`go.gen` is the Roslyn source generator / analyzer used by
[go2cs](https://github.com/ritchiecarroll/go2cs) to emit the compile-time C# glue that makes
converted Go code behave like Go:

- interface satisfaction (duck-typed `GoImplement` wiring),
- pointer-receiver method overloads,
- implicit type-alias conversions,
- struct embedding / promotion,
- package `init` ordering.

Install `go.gen` when you convert your **own** Go code to C# with go2cs — the generator must run at
compile time so the generated members are available. The pre-converted Go standard-library packages
(`go.fmt`, `go.strings`, …) already have this glue baked into their compiled assemblies, so consuming
those packages does **not** require `go.gen`.

This is a development-time dependency (analyzer); it ships no runtime assembly.

## License

MIT. See the [go2cs repository](https://github.com/ritchiecarroll/go2cs).

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
