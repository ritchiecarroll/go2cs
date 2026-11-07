# Data for DESIGN-extension-receiver-scope.md

One linux lane box (4 cores, SDK 10.0.403, go1.26.10), master `df7c09ab7b`, the crypto/tls `-tests` host
(`-bogo-mode -is-handshaker-supported`, tiering off).

| path | what it is |
|:--|:--|
| `bridges-census.go.txt`, `bridges-goroot-go1.26.10.txt` | the go/ast census of internal-test methods on production types, and its output (`PKG`, package, bridge methods, test-own methods, sites) |
| `hook-StartupHook.cs.txt` | the startup-hook instrument (never shipped): the cold walk and receiver homes (`P1L3_LIST`), the probe's keys (`P1L3_KEYS`), the scan seam's passes (`P1L3_SCAN`) |
| `probe-loaded-assemblies.txt` | the 242 assemblies the probe launch loads |
| `scan-parts-run{1,2,3}.tsv` | per assembly: host classes, extension methods, walk, GetMethods+IsDefined and GetParameters in µs, receivers homed here, elsewhere, or neither |
| `cross-assembly-receivers.tsv` | the 35 methods whose receiver's home is another assembly |
| `probe-keys.txt` | every key the probe asked the scan, and the number of assembly loads |
| `scan-passes/run{1..5}.tsv` | the scan seam's calls in 5 launches: µs since the hook, thread, assembly |
