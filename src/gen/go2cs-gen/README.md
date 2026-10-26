# go.gen — go2cs source generator

go.gen is the Roslyn source generator / analyzer used by
[go2cs](https://github.com/ritchiecarroll/go2cs) to emit the compile-time C# glue that makes
converted Go code behave like Go:

- interface satisfaction (duck-typed GoImplement wiring),
- pointer-receiver method overloads,
- implicit type-alias conversions,
- struct embedding / promotion,
- lazily built package variables,
- field access through pointers reinterpreted from unsafe.Pointer,
- throwing stubs for Go functions with no Go body (assembly or cgo),
- Go-string boxing for string literals passed to any parameters,
- the records Go reflection reads (channel directions, struct member facts, container operations).

Two analyzers ship with the generator and fail the build when a C# string literal binds a C# type
where Go has a string, or when a stack-append buffer is passed as anything but a plain local.

Install go.gen when you convert your **own** Go code to C# with go2cs — the generator must run at
compile time so the generated members are available. The pre-converted Go standard-library packages
(go.fmt, go.strings, …) already have this glue baked into their compiled assemblies, so consuming
those packages does **not** require go.gen.

This is a development-time dependency (analyzer); it ships no runtime assembly.

## License

MIT. See the [go2cs repository](https://github.com/ritchiecarroll/go2cs).

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
