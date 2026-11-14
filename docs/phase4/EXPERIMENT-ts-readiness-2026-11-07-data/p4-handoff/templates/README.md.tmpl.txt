# typescript-go-cs

> [!IMPORTANT]
> **This is a proof of what the [go2cs](https://github.com/ritchiecarroll/go2cs) transpiler can do. It is NOT a
> replacement for the TypeScript compiler.** Do not use it to compile TypeScript. Use
> [TypeScript](https://github.com/microsoft/TypeScript) itself.

> [!WARNING]
> **Not affiliated with or endorsed by Microsoft.** TypeScript is a trademark of Microsoft Corporation. This is an
> unofficial C# conversion of part of TypeScript's Go source, made with go2cs and published by the go2cs project.

## What it shows

go2cs converted TypeScript's own Go scanner and parser, at
[microsoft/TypeScript `{{TS_SHORT}}`](https://github.com/microsoft/TypeScript/tree/{{TS_COMMIT}}/tsc), to C#.
{{HEADLINE}} These results were measured on {{PLATFORM}} on {{DATE}}:

- **The demo:** its output is {{DEMO_VERDICT}} with the native build's ({{DEMO_LINES}} lines, SHA-256
  `{{DEMO_SHA_GO}}`).
- **The parser corpus:** {{CORPUS_SENTENCE}}. These are the inputs TypeScript's own parser fuzz test runs, covering
  {{NODES}} syntax nodes and {{DIAGS}} parse diagnostics on the native side.

[PROOF.md](PROOF.md) has both demo outputs, the corpus harness and every figure above.

{{SCOPE}}

## Beyond parsing

{{BEYOND}}

## What's next

Type-checking and JavaScript emit are the next layers. They are expected after go2cs moves to Go 1.27.

[![Go source](https://img.shields.io/badge/Go_source-microsoft%2FTypeScript_@{{TS_SHORT}}-00ADD8?logo=go)](https://github.com/microsoft/TypeScript/tree/{{TS_COMMIT}}/tsc) [![C# Source](https://img.shields.io/badge/C%23_Source-converted-512BD4?logo=dotnet)](src/github.com/microsoft/TypeScript/tsc)\
[![Parser corpus](https://img.shields.io/badge/Parser_corpus-{{N}}_of_{{UNITS}}_match-{{BADGE_COLOR}}?logo=go)](PROOF.md) [![go2cs](https://img.shields.io/badge/go2cs-@{{GOSTDLIB}}-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-{{GOSTDLIB}})

## What is here

| Path | What it is |
|:--|:--|
| `src/github.com/microsoft/TypeScript/tsc/internal/…` | The TypeScript packages the parser needs (`parser`, `scanner`, `ast`, `core` and their imports), converted by go2cs. Do not edit them: change the Go and convert again. |
| `src/github.com/microsoft/TypeScript/tsc/cmd/parsedemo/` | The demo, converted. Its Go source is `go/cmd/parsedemo/main.go`. |
| `pkg/…` | The third-party Go modules the parser imports, converted, each with its own license at the module root. |
| `go/` | The Go sources this project added: the parse demo, the type-check demo and the corpus extractor. They are not part of TypeScript. |
| `samples/`, `proof/` | The demo inputs, the harness (`compare.py`) and its measured outputs. |
| `LICENSE`, `NOTICE.txt` | TypeScript's Apache-2.0 license and its notices, verbatim. |
| `MODIFICATIONS.md` | What was changed and what was added, as the license requires. |

## Building and running

Building needs the .NET 10 SDK. The restore takes the go2cs standard library (`go.*`) from nuget.org.

```
dotnet build src/github.com/microsoft/TypeScript/tsc/cmd/parsedemo/parsedemo.csproj -c Release -p:GoStdLibVersion={{GOSTDLIB}}
dotnet run --project src/github.com/microsoft/TypeScript/tsc/cmd/parsedemo/parsedemo.csproj -c Release -p:GoStdLibVersion={{GOSTDLIB}} -- samples/good.ts samples/bad.ts
```

The demo prints each file's parse diagnostics, then every syntax node's kind with its start and end, in tree order.
[PROOF.md](PROOF.md) shows how to run the same comparison over the parser corpus.

## Provenance

- **Converted:** with `go2cs -recurse=nuget` at go2cs commit `{{CONVERTER}}`.
- **Built:** against go.* `{{GOSTDLIB}}` with the .NET SDK {{SDK}}.
- **Compared:** against {{GOVERSION}} building the same Go sources.

## License

The conversion is a derivative of TypeScript's source and carries its license:

- [LICENSE](LICENSE) is Apache-2.0, verbatim.
- [NOTICE.txt](NOTICE.txt) is verbatim.
- [MODIFICATIONS.md](MODIFICATIONS.md) states the changes.
- Each module under `pkg/` carries its own license.
