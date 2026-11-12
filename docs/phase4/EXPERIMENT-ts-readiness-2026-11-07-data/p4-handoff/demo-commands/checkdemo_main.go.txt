// checkdemo: the typescript-go-cs type-check demo. It is not part of TypeScript: it was added beside TypeScript's
// own packages only so it can import them, and is converted to C# with them.
//
// For each .ts file named on the command line it builds a one-file program in memory (the file as /main.ts, a
// tsconfig with "strict": true, the bundled lib files), type-checks it single-threaded, and prints every config,
// global, syntactic and semantic diagnostic with its message chain, so the native and the converted run can be
// byte-compared.
//
//	checkdemo <file.ts>...
package main

import (
	"context"
	"fmt"
	"os"
	"strings"

	"github.com/microsoft/TypeScript/tsc/internal/ast"
	"github.com/microsoft/TypeScript/tsc/internal/bundled"
	"github.com/microsoft/TypeScript/tsc/internal/compiler"
	"github.com/microsoft/TypeScript/tsc/internal/core"
	"github.com/microsoft/TypeScript/tsc/internal/tsoptions"
	"github.com/microsoft/TypeScript/tsc/internal/vfs/vfstest"
)

const tsconfig = `{ "compilerOptions": { "strict": true }, "files": ["main.ts"] }`

func report(kind string, diags []*ast.Diagnostic) {
	for _, d := range diags {
		file := ""
		if d.File() != nil {
			file = d.File().FileName()
		}
		fmt.Printf("%s TS%d %s [%d,+%d] %s\n", kind, d.Code(), file, d.Pos(), d.Len(), d.String())
		var chain func(ds []*ast.Diagnostic, depth int)
		chain = func(ds []*ast.Diagnostic, depth int) {
			for _, c := range ds {
				fmt.Printf("%s  TS%d %s\n", strings.Repeat("  ", depth), c.Code(), c.String())
				chain(c.MessageChain(), depth+1)
			}
		}
		chain(d.MessageChain(), 1)
	}
}

func main() {
	ctx := context.Background()
	for _, name := range os.Args[1:] {
		text, err := os.ReadFile(name)
		if err != nil {
			fmt.Println("read error:", err)
			os.Exit(1)
		}
		fmt.Printf("file %s (%d bytes)\n", name[strings.LastIndexAny(name, `/\`)+1:], len(text))
		fs := bundled.WrapFS(vfstest.FromMap(map[string]string{"/main.ts": string(text), "/tsconfig.json": tsconfig}, true))
		host := compiler.NewCompilerHost("/", fs, bundled.LibPath(), nil, nil, nil)
		parsed, errs := tsoptions.GetParsedCommandLineOfConfigFile("/tsconfig.json", &core.CompilerOptions{}, nil, host, nil)
		report("config", errs)
		p := compiler.NewProgram(compiler.ProgramOptions{Config: parsed, Host: host, SingleThreaded: core.TSTrue})
		report("options", p.GetConfigFileParsingDiagnostics())
		file := p.GetSourceFile("/main.ts")
		report("syntax", p.GetSyntacticDiagnostics(ctx, file))
		report("global", p.GetGlobalDiagnostics(ctx))
		report("semantic", p.GetSemanticDiagnostics(ctx, file))
		fmt.Printf("files %d\n", len(p.GetSourceFiles()))
	}
}
