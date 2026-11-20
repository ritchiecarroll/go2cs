# sqlite-cs demo/file, windows/amd64, converted at go2cs 18960ca538 (C2 seat (3)+(A))

For i9: the windows RUN leg of C2's seat (3)+(A). Converted on P2's linux box, which cannot run the windows flavour.

- Source: sqlite-cs cf739c54a5, demo/file (module sqlitecs/demo/file, modernc.org/sqlite v1.62.0,
  modernc.org/libc v1.80.1), Go 1.26.11, GOFLAGS=-mod=mod.
- Converter: go2cs built from claude/c2-recurse-importer-seed at 18960ca538 with go1.26.11.
- Command: go2cs -recurse -platforms windows/amd64 [flag] -go2cspath <go2cs clone>/src <demo/file> <out>
  - windows-arm1-noflag: no -inline-arrays (the seat's claim: the importer seed derives libc#largeInteger).
  - windows-arm2-flag: -inline-arrays=modernc.org/libc#largeInteger (the control).
- The two trees are byte-identical (377 files each); see the two .files.sha256 manifests, which are equal.
  Manifest sha256 (sha256 of the manifest file): 717e5f385bf104c37fc5158717a90b5d3f0b5e245388f4777830b21608021f9d
- One edit after conversion, the same in both: Directory.Build.props go2csPath held this box's local path; it now
  reads GO2CS_CLONE_AT_18960ca538/src/. Set a go2csPath environment variable, or pass -p:go2csPath=..., to the src
  folder (with trailing separator) of a go2cs checkout at 18960ca538.

Build and run on windows (the sqlite-cs README recipe), once per arm:

    dotnet build <arm>/src/sqlitecs/demo/file/sqlitecs.demo.file.csproj -c Release -p:go2csPath=<clone>\src\
    dotnet <arm>/.artifacts/bin/<...>/Release/net10.0/<program>.dll > converted.txt

Compare converted.txt with the native Go program's output on the same box (go run . in demo/file).
Expected (C2): both arms build, and each prints exactly what native Go prints, WAL pass included.
