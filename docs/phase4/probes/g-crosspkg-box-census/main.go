// xpbox: the census for DESIGN-crosspkg-exported-box (go.* 1.26.10.2's headline, option (b): box every exported
// package-level value variable). For one GOOS (amd64, -tags purego, as go2cs -stdlib loads), every EXPORTED
// package-level var of every std package, classified:
//
//	kind      pointer | interface | value (everything else: struct, array, slice, map, func, chan, basic)
//	boxed     the committed corpus already declares its box (`Ꮡ<Name> = new `) in the emitted file of its Go file
//	handown   that emitted file is a whole-file hand-own (`[module: GoManualConversion]` line)
//	linkname  its file carries a //go:linkname directive naming it
//	emitted   the emitted file was found (a control: a miss means the census cannot read the var's emission)
//
// The emitted file of <dir>/<stem>.go is <core>/<pkg>/<goos>/<stem>.cs when the L3 folder holds it, else
// <core>/<pkg>/<stem>.cs; a `<stem>_impl.cs` companion is read with it.
//
// Usage: xpbox -core <src/core> -goos <goos> [-tests] > out.tsv   (summary on stderr)
package main

import (
	"flag"
	"fmt"
	"go/ast"
	"go/types"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

// Both spellings the corpus carries: `[module: GoManualConversion]` and `[module: go.GoManualConversion]`.
var handOwnLine = regexp.MustCompile(`(?m)^\[module: *(go\.)?GoManualConversion\b`)

func main() {
	core := flag.String("core", "", "src/core of the tree whose emission is read")
	goos := flag.String("goos", "windows", "target GOOS (GOARCH is amd64)")
	tests := flag.Bool("tests", false, "census the vars declared in _test.go files instead (the -tests leg)")
	importers := flag.Bool("importers", false, "census the IMPORTER sites instead: every use of another package's exported non-pointer var")
	flag.Parse()

	if *importers {
		importerCensus(*goos)
		return
	}

	cfg := &packages.Config{
		Mode:       packages.NeedName | packages.NeedFiles | packages.NeedTypes | packages.NeedSyntax,
		Env:        append(os.Environ(), "GOOS="+*goos, "GOARCH=amd64", "CGO_ENABLED=0"),
		BuildFlags: []string{"-tags=purego"},
		Tests:      *tests,
	}

	pkgs, err := packages.Load(cfg, "std")

	if err != nil {
		fmt.Fprintln(os.Stderr, "load:", err)
		os.Exit(2)
	}

	type row struct{ pkg, name, kind, file, emitted, handown, boxed, linkname, autobox string }
	var rows []row
	seen := map[string]bool{}
	fileText := map[string]string{}

	read := func(path string) (string, bool) {
		if t, ok := fileText[path]; ok {
			return t, t != "\x00"
		}

		b, err := os.ReadFile(path)

		if err != nil {
			fileText[path] = "\x00"
			return "", false
		}

		fileText[path] = string(b)
		return string(b), true
	}

	for _, p := range pkgs {
		path := p.PkgPath

		if strings.HasPrefix(path, "cmd/") || strings.HasPrefix(path, "vendor/") || strings.Contains(path, "/vendor/") || p.Types == nil {
			continue
		}

		// With -tests, only the in-package test variant ("p [p.test]") and the external test package carry test files.
		if *tests && !strings.Contains(p.ID, ".test]") && !strings.HasSuffix(path, "_test") {
			continue
		}

		for _, file := range p.Syntax {
			goFile := p.Fset.Position(file.Pos()).Filename
			isTest := strings.HasSuffix(goFile, "_test.go")

			if isTest != *tests {
				continue
			}

			linknamed := map[string]bool{}

			for _, cg := range file.Comments {
				for _, c := range cg.List {
					if f := strings.Fields(c.Text); len(f) >= 2 && f[0] == "//go:linkname" {
						linknamed[f[1]] = true
					}
				}
			}

			stem := strings.TrimSuffix(filepath.Base(goFile), ".go")
			pkgDir := filepath.Join(*core, filepath.FromSlash(strings.TrimSuffix(path, "_test")))
			candidates := []string{filepath.Join(pkgDir, *goos, stem+".cs"), filepath.Join(pkgDir, stem+".cs")}

			for _, decl := range file.Decls {
				gen, ok := decl.(*ast.GenDecl)

				if !ok {
					continue
				}

				for _, spec := range gen.Specs {
					vs, ok := spec.(*ast.ValueSpec)

					if !ok {
						continue
					}

					for _, id := range vs.Names {
						obj, ok := p.Types.Scope().Lookup(id.Name).(*types.Var)

						if !ok || !obj.Exported() || obj.Pos() != id.Pos() {
							continue
						}

						key := path + "." + id.Name

						if seen[key] {
							continue
						}

						seen[key] = true

						kind := "value"

						switch u := types.Unalias(obj.Type()).Underlying().(type) {
						case *types.Pointer:
							kind = "pointer"
						case *types.Interface:
							kind = "interface"
						case *types.Signature:
							kind = "value:func"
						case *types.Slice:
							kind = "value:slice"
						case *types.Map:
							kind = "value:map"
						case *types.Struct:
							kind = "value:struct"
						case *types.Array:
							kind = "value:array"
						case *types.Chan:
							kind = "value:chan"
						case *types.Basic:
							kind = "value:basic:" + u.Name()
						}

						r := row{pkg: path, name: id.Name, kind: kind, file: filepath.Base(goFile), emitted: "no", handown: "no", boxed: "no", linkname: "no", autobox: "-"}

						if linknamed[id.Name] {
							r.linkname = "yes"
						}

						for _, cs := range candidates {
							text, ok := read(cs)

							if !ok {
								continue
							}

							r.emitted = strings.TrimPrefix(filepath.ToSlash(cs), filepath.ToSlash(*core)+"/")

							if handOwnLine.MatchString(text) {
								r.handown = "yes"
								r.autobox = "no"

								// the converter's own view: the re-emitted twin the hand-own displaces
								if auto, ok := read(cs + ".auto"); ok && strings.Contains(auto, "Ꮡ"+id.Name+" = new ") {
									r.autobox = "yes"
								}
							}

							impl, _ := read(strings.TrimSuffix(cs, ".cs") + "_impl.cs")

							if strings.Contains(text, "Ꮡ"+id.Name+" = new ") || strings.Contains(impl, "Ꮡ"+id.Name+" = new ") {
								r.boxed = "yes"
							}

							break
						}

						rows = append(rows, r)
					}
				}
			}
		}
	}

	sort.Slice(rows, func(i, j int) bool {
		if rows[i].pkg != rows[j].pkg {
			return rows[i].pkg < rows[j].pkg
		}
		return rows[i].name < rows[j].name
	})

	fmt.Println("goos\tpkg\tname\tkind\tfile\temitted\thandown\tboxed\tlinkname\tautobox")

	count := map[string]int{}

	for _, r := range rows {
		fmt.Printf("%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\n", *goos, r.pkg, r.name, r.kind, r.file, r.emitted, r.handown, r.boxed, r.linkname, r.autobox)
		count["all"]++
		count["kind="+r.kind]++

		if r.emitted == "no" {
			count["emitted=no"]++
			continue
		}

		if r.boxed == "yes" {
			count["boxed-today kind="+r.kind]++
			continue
		}

		count["to-box kind="+r.kind]++

		if r.handown == "yes" {
			count["to-box handown kind="+r.kind]++
		}

		if r.linkname == "yes" {
			count["to-box linkname kind="+r.kind]++
		}
	}

	keys := make([]string, 0, len(count))

	for k := range count {
		keys = append(keys, k)
	}

	sort.Strings(keys)

	for _, k := range keys {
		fmt.Fprintf(os.Stderr, "%s tests=%v %-40s %d\n", *goos, *tests, k, count[k])
	}
}
