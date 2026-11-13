// mfdemo: a go2cs demo for typescript-go-cs (added beside TypeScript only to import its packages).
// A tsc-like driver over the compiler package: each project directory named on
// the command line is loaded into memory as /proj, its tsconfig.json is parsed,
// the program is built (bundled libs), every diagnostic tsc would report is
// printed, and the program is emitted with every output printed verbatim, so
// the native and the converted run can be byte-compared.
package main

import (
	"context"
	"fmt"
	"io/fs"
	"os"
	"path/filepath"
	"sort"
	"sync"

	"github.com/microsoft/TypeScript/tsc/internal/ast"
	"github.com/microsoft/TypeScript/tsc/internal/bundled"
	"github.com/microsoft/TypeScript/tsc/internal/compiler"
	"github.com/microsoft/TypeScript/tsc/internal/tsoptions"
	"github.com/microsoft/TypeScript/tsc/internal/vfs"
	"github.com/microsoft/TypeScript/tsc/internal/vfs/vfstest"
)

type sys struct{ fs vfs.FS }

func (s *sys) FS() vfs.FS                  { return s.fs }
func (s *sys) GetCurrentDirectory() string { return "/proj" }

type output struct{ name, text string }

func printDiag(d *ast.Diagnostic) {
	file := "<global>"
	if d.File() != nil {
		file = d.File().FileName()
	}
	fmt.Printf("diag %s TS%d [%d,+%d] %s\n", file, d.Code(), d.Pos(), d.Len(), d.String())
}

func main() {
	for _, dir := range os.Args[1:] {
		files := map[string]string{}
		err := filepath.WalkDir(dir, func(p string, e fs.DirEntry, err error) error {
			if err != nil || e.IsDir() {
				return err
			}
			b, err := os.ReadFile(p)
			if err != nil {
				return err
			}
			rel, _ := filepath.Rel(dir, p)
			files["/proj/"+filepath.ToSlash(rel)] = string(b)
			return nil
		})
		if err != nil {
			fmt.Println("read error:", err)
			os.Exit(1)
		}
		fmt.Printf("project %s (%d files)\n", filepath.Base(dir), len(files))
		s := &sys{bundled.WrapFS(vfstest.FromMap(files, true))}
		config, errs := tsoptions.GetParsedCommandLineOfConfigFile("/proj/tsconfig.json", nil, nil, s, nil)
		for _, d := range errs {
			printDiag(d)
		}
		if config == nil {
			continue
		}
		host := compiler.NewCompilerHost("/proj", s.fs, bundled.LibPath(), nil, nil, nil)
		program := compiler.NewProgram(compiler.ProgramOptions{Config: config, Host: host})
		ctx := context.Background()
		names := []string{}
		for _, f := range program.SourceFiles() {
			if !bundled.IsBundled(f.FileName()) {
				names = append(names, f.FileName())
			}
		}
		fmt.Printf("files %d (%d bundled)\n", len(program.SourceFiles()), len(program.SourceFiles())-len(names))
		for _, n := range names {
			fmt.Printf("source %s\n", n)
		}
		diags := compiler.SortAndDeduplicateDiagnostics(compiler.GetDiagnosticsOfAnyProgram(ctx, program, nil, false,
			program.GetBindDiagnostics, program.GetSemanticDiagnostics))
		fmt.Printf("diagnostics %d\n", len(diags))
		for _, d := range diags {
			printDiag(d)
		}
		var outs []output
		var mu sync.Mutex
		res := program.Emit(ctx, compiler.EmitOptions{
			WriteFile: func(fileName string, text string, _ *compiler.WriteFileData) error {
				mu.Lock()
				outs = append(outs, output{fileName, text})
				mu.Unlock()
				return nil
			},
		})
		sort.Slice(outs, func(i, j int) bool { return outs[i].name < outs[j].name })
		fmt.Printf("skipped %v, emit diagnostics %d, outputs %d\n", res.EmitSkipped, len(res.Diagnostics), len(outs))
		for _, d := range res.Diagnostics {
			printDiag(d)
		}
		for _, o := range outs {
			fmt.Printf("emit %s (%d bytes)\n%s\n--- end %s\n", o.name, len(o.text), o.text, o.name)
		}
	}
}
