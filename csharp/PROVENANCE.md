# Provenance of the converted C#

Converted by the go2cs 1.26.11.1 release from this repository's `demo/` and `demo/file/` at commit
`cf739c54a50ec04ebcc779b142be7e642903f87d`, with Go 1.26.11.

- **Converter**: go2cs built from the 1.26.11.1 release record commit
  `5dd5cafd5b60b9e2d454cec5ac1a7c147b4a838c` (`go build -trimpath`, `CGO_ENABLED=0`, Go 1.26.11).
  `go2cs -version` reads `go2cs.release=1.26.11.1`; the executable's SHA-256 is
  `d4b782576d8d2a4614e68cb82ffa1ac4c6fea4eb6659f5286e7f56aa29b2eb00`.
- **Command**, once per demo and once per target, from inside the demo's folder:

  ```
  go2cs -recurse=nuget -inline-arrays=modernc.org/libc#largeInteger -platforms <os>/amd64 <demo> <out>
  ```

- **Environment**: `GO2CSPATH` pointed at an empty folder, so the converter read only the standard-library metadata
  it carries. `GOTOOLCHAIN=local`, `CGO_ENABLED=0`.
- **Standard library**: referenced as the `go.*` 1.26.11.1 packages on nuget.org (the `GoStdLibVersion` default in
  each tree's `Directory.Build.props` floats to the newest 1.26 release); no local feed and no go2cs clone.

| Folder | Target | Demo |
|---|---|---|
| `windows-amd64/demo` | windows/amd64 | `demo/` |
| `windows-amd64/demo-file` | windows/amd64 | `demo/file/` |
| `linux-amd64/demo` | linux/amd64 | `demo/` |
| `linux-amd64/demo-file` | linux/amd64 | `demo/file/` |

Build output (`.artifacts/`) is not included. Build a demo with
`dotnet build <folder>/src/sqlitecs/demo[/file]/sqlitecs.demo[.file].csproj -c Release`.
