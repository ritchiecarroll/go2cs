# go.lib — go2cs core runtime library

`go.lib` is the hand-written C# runtime that implements Go language semantics for code converted by
[go2cs](https://github.com/ritchiecarroll/go2cs): slices (`slice<T>`), maps (`map<K,V>`),
channels (`channel<T>`), strings (`@string`), arrays (`array<T>`), the `builtin` functions
(`append`, `len`, `make`, `panic`, `recover`, …), the `ж<T>` heap box, `nil`, and the Go numeric
type aliases.

Every go2cs-converted package (`go.fmt`, `go.strings`, …) depends on `go.lib`. It is pulled in
automatically when you install any converted package — you normally do not reference it directly.

## License

MIT. See the [go2cs repository](https://github.com/ritchiecarroll/go2cs).

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
