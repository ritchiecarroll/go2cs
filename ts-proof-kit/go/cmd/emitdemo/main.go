// emitdemo: a go2cs demo for typescript-go-cs (added beside TypeScript only to import its packages).
// Each .ts file named on the command line is compiled as /main.ts in its own
// program (bundled libs, declaration on), emitted (JavaScript and .d.ts), and
// every output is printed verbatim with the emit diagnostics, so the native and
// the converted run can be byte-compared.
package main

import (
	"context"
	"fmt"
	"os"
	"sort"
	"strings"

	"github.com/microsoft/TypeScript/tsc/internal/bundled"
	"github.com/microsoft/TypeScript/tsc/internal/compiler"
	"github.com/microsoft/TypeScript/tsc/internal/core"
	"github.com/microsoft/TypeScript/tsc/internal/tsoptions"
	"github.com/microsoft/TypeScript/tsc/internal/tspath"
	"github.com/microsoft/TypeScript/tsc/internal/vfs/vfstest"
)

type output struct{ name, text string }

func main() {
	for _, name := range os.Args[1:] {
		text, err := os.ReadFile(name)
		if err != nil {
			fmt.Println("read error:", err)
			os.Exit(1)
		}
		base := name[strings.LastIndexAny(name, `/\`)+1:]
		fmt.Printf("file %s (%d bytes)\n", base, len(text))
		fs := bundled.WrapFS(vfstest.FromMap(map[string]string{"/main.ts": string(text)}, true))
		opts := &core.CompilerOptions{Declaration: core.TSTrue, NewLine: core.NewLineKindLF}
		config := tsoptions.NewParsedCommandLine(opts, []string{"/main.ts"}, nil,
			tspath.ComparePathsOptions{UseCaseSensitiveFileNames: true, CurrentDirectory: "/"})
		host := compiler.NewCompilerHost("/", fs, bundled.LibPath(), nil, nil, nil)
		program := compiler.NewProgram(compiler.ProgramOptions{Config: config, Host: host, SingleThreaded: core.TSTrue})
		var outs []output
		res := program.Emit(context.Background(), compiler.EmitOptions{
			WriteFile: func(fileName string, text string, _ *compiler.WriteFileData) error {
				outs = append(outs, output{fileName, text})
				return nil
			},
		})
		sort.Slice(outs, func(i, j int) bool { return outs[i].name < outs[j].name })
		fmt.Printf("skipped %v, diagnostics %d, outputs %d\n", res.EmitSkipped, len(res.Diagnostics), len(outs))
		for _, d := range res.Diagnostics {
			fmt.Printf("diag TS%d [%d,+%d] %s\n", d.Code(), d.Pos(), d.Len(), d.String())
		}
		for _, o := range outs {
			fmt.Printf("emit %s (%d bytes)\n%s\n--- end %s\n", o.name, len(o.text), o.text, o.name)
		}
	}
}
