// parsedemo: the typescript-go-cs demo. It is not part of TypeScript: it was added beside TypeScript's own
// packages only so it can import them, and is converted to C# with them.
//
// Parses each .ts file named on the command line and prints its parse
// diagnostics and a pre-order dump of node kinds with pos/end, so the native
// and the converted run can be byte-compared.
//
//	parsedemo <file.ts>...         the dump, for each file
//	parsedemo -corpus <dir> [from] one digest line per unit of <dir>/manifest.tsv (parsecorpus-extract's output),
//	                               optionally starting at unit index <from>
//	parsedemo -unit <dir> <index>  the dump of one corpus unit, parsed as the corpus mode parses it
package main

import (
	"bufio"
	"crypto/sha256"
	"fmt"
	"io"
	"os"
	"path/filepath"
	"strings"

	"github.com/microsoft/TypeScript/tsc/internal/ast"
	"github.com/microsoft/TypeScript/tsc/internal/core"
	"github.com/microsoft/TypeScript/tsc/internal/parser"
	"github.com/microsoft/TypeScript/tsc/internal/tspath"
)

// dump parses text as fileName and writes the diagnostics and the node dump; it returns the counts.
func dump(w io.Writer, fileName, text string, jsx, force bool) (diags, nodes int) {
	opts := ast.SourceFileParseOptions{FileName: fileName, Path: tspath.Path(fileName),
		ExternalModuleIndicatorOptions: ast.ExternalModuleIndicatorOptions{JSX: jsx, Force: force}}
	file := parser.ParseSourceFile(opts, text, core.GetScriptKindFromFileName(fileName))
	for _, d := range file.Diagnostics() {
		fmt.Fprintf(w, "diag TS%d [%d,+%d] %s\n", d.Code(), d.Pos(), d.Len(), d.String())
		diags++
	}
	var walk func(n *ast.Node, depth int) bool
	walk = func(n *ast.Node, depth int) bool {
		fmt.Fprintf(w, "%s%v [%d,%d]\n", strings.Repeat("  ", depth), n.Kind, n.Pos(), n.End())
		nodes++
		n.ForEachChild(func(c *ast.Node) bool { return walk(c, depth+1) })
		return false
	}
	walk(file.AsNode(), 0)
	return diags, nodes
}

// unit reads corpus unit idx; FuzzParser parses it as "/index" + its manifest extension.
func unit(dir, idx, ext string) (fileName, text string) {
	b, err := os.ReadFile(filepath.Join(dir, "units", idx+".txt"))
	if err != nil {
		fmt.Println("read error:", err)
		os.Exit(1)
	}
	return "/index" + ext, string(b)
}

// digest is one corpus line: the dump's SHA-256 and counts, or the panic, so one bad unit never stops the run.
func digest(fileName, text string, jsx, force bool) (line string) {
	defer func() {
		if r := recover(); r != nil {
			line = fmt.Sprintf("PANIC %v", r)
		}
	}()
	h := sha256.New()
	d, n := dump(h, fileName, text, jsx, force)
	return fmt.Sprintf("%x %d %d", h.Sum(nil), d, n)
}

// manifest is the rows of <dir>/manifest.tsv: index, extension, jsx, force, sha256, source, unit name.
func manifest(dir string) []string {
	man, err := os.ReadFile(filepath.Join(dir, "manifest.tsv"))
	if err != nil {
		fmt.Println("read error:", err)
		os.Exit(1)
	}
	return strings.Split(strings.TrimSuffix(string(man), "\n"), "\n")
}

func main() {
	args := os.Args[1:]
	switch {
	case (len(args) == 2 || len(args) == 3) && args[0] == "-corpus":
		rows := manifest(args[1])
		if len(args) == 3 { // resume after a fatal crash: start at this unit index
			for len(rows) > 0 && strings.Split(rows[0], "\t")[0] != args[2] {
				rows = rows[1:]
			}
		}
		out := bufio.NewWriter(os.Stdout)
		defer out.Flush()
		for _, row := range rows {
			f := strings.Split(row, "\t")
			fileName, text := unit(args[1], f[0], f[1])
			fmt.Fprintf(out, "%s %s\n", f[0], digest(fileName, text, f[2] == "true", f[3] == "true"))
		}
	case len(args) == 3 && args[0] == "-unit":
		for _, row := range manifest(args[1]) {
			if f := strings.Split(row, "\t"); f[0] == args[2] {
				fileName, text := unit(args[1], f[0], f[1])
				dump(os.Stdout, fileName, text, f[2] == "true", f[3] == "true")
			}
		}
	default:
		for _, name := range args {
			text, err := os.ReadFile(name)
			if err != nil {
				fmt.Println("read error:", err)
				os.Exit(1)
			}
			base := name[strings.LastIndexAny(name, `/\`)+1:]
			fmt.Printf("file %s (%d bytes)\n", base, len(text))
			dump(os.Stdout, "/"+base, string(text), false, false)
		}
	}
}
