// c0012: CLASS A census. For each -tests project, the converted packages it references today against three
// candidate reference sets:
//
//	opt1  the whole import closure of what the test assembly compiles (production recompiled + both test variants);
//	opt2  the API closure: start from today's references plus the test assembly's own declarations, and add every
//	      converted package named by the signature of a member a package in the set exposes (exported funcs,
//	      every method in the method sets of exported types, exported fields, interface methods and embeds,
//	      exported vars), to a fixpoint;
//	opt2f opt2 with methods, funcs and fields kept only when the test assembly's sources select that NAME.
//
// Usage: c0012 -goos windows -root <dir with src/core/**.tests.csproj> -converted <prodprojs.txt> [-module dir -refs a,b]
package main

import (
	"bufio"
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

var explainTarget string

// refThis, when loaded, limits the use-site model's extension candidates to the methods go2cs emits as
// `this ref T` extensions, keyed "pkg.Type.Method".
var refThis map[string]map[string]string

var ptrOnly bool
var why = map[string]string{}

var refPattern = regexp.MustCompile(`ProjectReference Include="\$\(go2csPath\)core/([^"]+)/[^/"]+\.csproj"`)

func main() {
	goos := flag.String("goos", "windows", "GOOS the committed projects were generated for")
	root := flag.String("root", "", "directory holding src/core/**/*.tests.csproj")
	convertedList := flag.String("converted", "", "file listing src/core/**/<pkg>.csproj (non-test)")
	moduleDir := flag.String("module", "", "census one module directory instead (positive control)")
	moduleRefs := flag.String("refs", "", "with -module: comma-separated import paths the test project references")
	moduleConverted := flag.String("modconverted", "", "with -module: comma-separated module packages that convert")
	outRoot := flag.String("outroot", "", "a -tests -recurse output root (src/, pkg/): census its test projects")
	explain := flag.String("explain", "", "print why each opt2f package is added for this target")
	refThisFile := flag.String("refthis", "", "TSV of emitted ref-this extensions: package, visibility, receiver, method")
	flag.BoolVar(&ptrOnly, "ptronly", false, "use-site candidates: pointer-receiver methods only (the converter-side rule)")
	flag.Parse()
	explainTarget = *explain
	if *refThisFile != "" {
		refThis = map[string]map[string]string{}
		data, err := os.ReadFile(*refThisFile)
		if err != nil {
			fmt.Fprintln(os.Stderr, err)
			os.Exit(2)
		}
		for _, line := range strings.Split(strings.TrimSpace(string(data)), "\n") {
			f := strings.Split(line, "\t")
			if len(f) != 4 {
				continue
			}
			recv := strings.TrimPrefix(strings.SplitN(f[2], "<", 2)[0], "Δ")
			key := f[0] + "." + recv + "." + strings.TrimPrefix(f[3], "Δ")
			if refThis[key] == nil {
				refThis[key] = map[string]string{}
			}
			refThis[key]["vis"] = f[1]
		}
	}

	converted := map[string]bool{}
	if f, err := os.Open(*convertedList); err == nil {
		sc := bufio.NewScanner(f)
		for sc.Scan() {
			dir := filepath.ToSlash(filepath.Dir(sc.Text()))
			converted[strings.TrimPrefix(dir, "src/core/")] = true
		}
		f.Close()
	} else {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(2)
	}
	delete(converted, "golib")

	type target struct {
		path     string
		refs     map[string]bool
		refModel bool            // the test project references the production project instead of recompiling it
		compiles map[string]bool // the Go files whose conversions the project compiles (nil: every file)
	}

	var targets []target
	dir := ""

	if *outRoot != "" {
		importOf := func(dir string) string {
			for _, base := range []string{"src", "pkg"} {
				if rel, err := filepath.Rel(filepath.Join(*outRoot, base), dir); err == nil && !strings.HasPrefix(rel, "..") {
					return filepath.ToSlash(rel)
				}
			}
			return ""
		}
		relRef := regexp.MustCompile(`ProjectReference Include="([^$"][^"]*\.csproj)"`)
		_ = filepath.WalkDir(*outRoot, func(p string, d os.DirEntry, err error) error {
			if err != nil || d.IsDir() || !strings.HasSuffix(p, ".csproj") {
				return err
			}
			if !strings.HasSuffix(p, ".tests.csproj") {
				converted[importOf(filepath.Dir(p))] = true
				return nil
			}
			data, _ := os.ReadFile(p)
			refs := map[string]bool{}
			for _, m := range refPattern.FindAllStringSubmatch(string(data), -1) {
				if m[1] != "golib" {
					refs[m[1]] = true
				}
			}
			refModel := false
			for _, m := range relRef.FindAllStringSubmatch(string(data), -1) {
				target := filepath.Dir(filepath.Join(filepath.Dir(p), filepath.FromSlash(strings.ReplaceAll(m[1], "\\", "/"))))
				if target == filepath.Dir(p) {
					refModel = true
					continue
				}
				if ip := importOf(target); ip != "" {
					refs[ip] = true
				}
			}
			compiles := map[string]bool{}
			for _, m := range regexp.MustCompile(`<Compile Include="([^"/\\]+)\.cs"`).FindAllStringSubmatch(string(data), -1) {
				compiles[m[1]+".go"] = true
			}
			targets = append(targets, target{path: importOf(filepath.Dir(p)), refs: refs, refModel: refModel, compiles: compiles})
			return nil
		})
	} else if *moduleDir != "" {
		dir = *moduleDir
		refs := map[string]bool{}
		for _, r := range strings.Split(*moduleRefs, ",") {
			if r != "" {
				refs[r] = true
			}
		}
		for _, m := range strings.Split(*moduleConverted, ",") {
			if m != "" {
				converted[m] = true
			}
		}
		targets = append(targets, target{path: flag.Arg(0), refs: refs, refModel: true})
	} else {
		_ = filepath.WalkDir(filepath.Join(*root, "src", "core"), func(p string, d os.DirEntry, err error) error {
			if err != nil || d.IsDir() || !strings.HasSuffix(p, ".tests.csproj") {
				return err
			}
			rel, _ := filepath.Rel(filepath.Join(*root, "src", "core"), filepath.Dir(p))
			data, _ := os.ReadFile(p)
			refs := map[string]bool{}
			for _, m := range refPattern.FindAllStringSubmatch(string(data), -1) {
				if m[1] != "golib" {
					refs[m[1]] = true
				}
			}
			refModel := regexp.MustCompile(`ProjectReference Include="[^$"][^"]*\.csproj"`).Match(data)
			compiles := map[string]bool{}
			for _, m := range regexp.MustCompile(`<Compile Include="([^"/\\]+)\.cs"`).FindAllStringSubmatch(string(data), -1) {
				compiles[m[1]+".go"] = true
			}
			targets = append(targets, target{path: filepath.ToSlash(rel), refs: refs, refModel: refModel, compiles: compiles})
			return nil
		})
	}

	sort.Slice(targets, func(i, j int) bool { return targets[i].path < targets[j].path })

	patterns := make([]string, len(targets))
	for i, t := range targets {
		patterns[i] = t.path
	}

	cfg := &packages.Config{
		Mode: packages.NeedName | packages.NeedImports | packages.NeedDeps | packages.NeedTypes |
			packages.NeedSyntax | packages.NeedTypesInfo | packages.NeedForTest,
		Tests:      true,
		Dir:        dir,
		Env:        append(os.Environ(), "GOOS="+*goos, "GOARCH=amd64", "CGO_ENABLED=0", "GOFLAGS=-tags=purego,math_big_pure_go"),
		BuildFlags: nil,
	}

	roots, err := packages.Load(cfg, patterns...)
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(1)
	}

	// Every package of the whole graph by import path; a test variant does not replace the plain package.
	byPath := map[string]*packages.Package{}
	packages.Visit(roots, nil, func(p *packages.Package) {
		if p.ID == p.PkgPath && byPath[p.PkgPath] == nil {
			byPath[p.PkgPath] = p
		}
	})

	fmt.Printf("target\tnow\topt1_add\topt2_add\topt2f_add\topt2_list\topt2f_list\tuse_add\tuse_list\tmodel\n")

	var tot [5]int
	var changed [4]int

	for _, t := range targets {
		var compiled []*packages.Package
		hasInternal := false
		for _, r := range roots {
			if r.ForTest == t.path && (r.ID != r.PkgPath) && !strings.HasSuffix(r.PkgPath, ".test") {
				compiled = append(compiled, r)
				if r.PkgPath == t.path {
					hasInternal = true
				}
			}
		}
		if !hasInternal {
			if p := byPath[t.path]; p != nil {
				compiled = append(compiled, p)
			}
		}
		if len(compiled) == 0 {
			fmt.Printf("%s\tNOT LOADED\n", t.path)
			continue
		}

		names := map[string]bool{}
		closure := map[string]bool{}
		for _, c := range compiled {
			for _, f := range c.Syntax {
				ast.Inspect(f, func(n ast.Node) bool {
					if s, ok := n.(*ast.SelectorExpr); ok {
						names[s.Sel.Name] = true
					}
					return true
				})
			}
			seen := map[string]bool{}
			var walk func(p *packages.Package)
			walk = func(p *packages.Package) {
				for _, imp := range p.Imports {
					if seen[imp.ID] {
						continue
					}
					seen[imp.ID] = true
					closure[imp.PkgPath] = true
					walk(imp)
				}
			}
			walk(c)
		}

		opt1 := 0
		for p := range closure {
			if converted[p] && !t.refs[p] && p != t.path {
				opt1++
			}
		}

		apiClosure := func(filter bool) map[string]bool {
			set := map[string]bool{}
			for r := range t.refs {
				set[r] = true
			}
			var queue []*types.Package
			done := map[*types.Package]bool{}
			// The test assembly's own declarations, every one (it is one assembly).
			for _, c := range compiled {
				queue = append(queue, c.Types)
			}
			for r := range t.refs {
				if p := byPath[r]; p != nil && p.Types != nil {
					queue = append(queue, p.Types)
				}
			}
			own := map[*types.Package]bool{}
			for _, c := range compiled {
				own[c.Types] = true
			}
			for len(queue) > 0 {
				pkg := queue[0]
				queue = queue[1:]
				if pkg == nil || done[pkg] {
					continue
				}
				done[pkg] = true
				why = map[string]string{}
				for _, named := range surfacePackages(pkg, own[pkg], filter, names) {
					path := named.Path()
					if !converted[path] || path == t.path || set[path] {
						continue
					}
					set[path] = true
					if filter && t.path == explainTarget {
						fmt.Fprintf(os.Stderr, "EXPLAIN %s: + %s  via %s.%s\n", t.path, path, pkg.Path(), why[path])
					}
					if p := byPath[path]; p != nil && p.Types != nil {
						queue = append(queue, p.Types)
					} else {
						queue = append(queue, named)
					}
				}
			}
			return set
		}

		diff := func(set map[string]bool) []string {
			var out []string
			for p := range set {
				if !t.refs[p] {
					out = append(out, p)
				}
			}
			sort.Strings(out)
			return out
		}

		a2 := diff(apiClosure(false))
		a2f := diff(apiClosure(true))
		u := diff(useSite(t.path, t.refs, t.refModel, t.compiles, compiled, byPath, converted))
		model := "recompile"
		if t.refModel {
			model = "reference"
		}

		fmt.Printf("%s\t%d\t%d\t%d\t%d\t%s\t%s\t%d\t%s\t%s\n", t.path, len(t.refs), opt1, len(a2), len(a2f), strings.Join(a2, ","), strings.Join(a2f, ","), len(u), strings.Join(u, ","), model)
		tot[0] += len(t.refs)
		tot[1] += opt1
		tot[2] += len(a2)
		tot[3] += len(a2f)
		tot[4] += len(u)
		for i, n := range []int{opt1, len(a2), len(a2f), len(u)} {
			if n > 0 {
				changed[i]++
			}
		}
	}

	fmt.Printf("TOTAL\tnow=%d\topt1=%d\topt2=%d\topt2f=%d\tuse=%d\n", tot[0], tot[1], tot[2], tot[3], tot[4])
	fmt.Printf("CHANGED projects (of %d)\topt1=%d\topt2=%d\topt2f=%d\tuse=%d\n", len(targets), changed[0], changed[1], changed[2], changed[3])
}

// surfacePackages returns the packages named by the types in pkg's surface: exported members, or every member when
// all is set (the test assembly's own code). With filter, funcs, methods and fields count only when names holds the
// member's name.
func surfacePackages(pkg *types.Package, all bool, filter bool, names map[string]bool) []*types.Package {
	found := map[*types.Package]bool{}
	seen := map[types.Type]bool{}
	var visit func(t types.Type)

	keep := func(name string) bool { return !filter || names[name] }
	member := ""

	addSig := func(sig *types.Signature) {
		for i := 0; i < sig.Params().Len(); i++ {
			visit(sig.Params().At(i).Type())
		}
		for i := 0; i < sig.Results().Len(); i++ {
			visit(sig.Results().At(i).Type())
		}
		if r := sig.Recv(); r != nil {
			visit(r.Type())
		}
	}

	visit = func(t types.Type) {
		if t == nil || seen[t] {
			return
		}
		seen[t] = true
		switch t := t.(type) {
		case *types.Named:
			if t.Obj().Pkg() != nil {
				found[t.Obj().Pkg()] = true
				if _, ok := why[t.Obj().Pkg().Path()]; !ok {
					why[t.Obj().Pkg().Path()] = member + " (" + t.Obj().Name() + ")"
				}
			}
			if args := t.TypeArgs(); args != nil {
				for i := 0; i < args.Len(); i++ {
					visit(args.At(i))
				}
			}
		case *types.Alias:
			if t.Obj().Pkg() != nil {
				found[t.Obj().Pkg()] = true
			}
			visit(types.Unalias(t))
		case *types.Pointer:
			visit(t.Elem())
		case *types.Slice:
			visit(t.Elem())
		case *types.Array:
			visit(t.Elem())
		case *types.Map:
			visit(t.Key())
			visit(t.Elem())
		case *types.Chan:
			visit(t.Elem())
		case *types.Signature:
			addSig(t)
		case *types.Tuple:
			for i := 0; i < t.Len(); i++ {
				visit(t.At(i).Type())
			}
		case *types.Struct:
			for i := 0; i < t.NumFields(); i++ {
				if f := t.Field(i); f.Exported() || all {
					if f.Embedded() || keep(f.Name()) {
						visit(f.Type())
					}
				}
			}
		case *types.Interface:
			for i := 0; i < t.NumEmbeddeds(); i++ {
				visit(t.EmbeddedType(i))
			}
			for i := 0; i < t.NumExplicitMethods(); i++ {
				if m := t.ExplicitMethod(i); keep(m.Name()) {
					visit(m.Type())
				}
			}
		case *types.TypeParam:
			visit(t.Constraint())
		case *types.Union:
			for i := 0; i < t.Len(); i++ {
				visit(t.Term(i).Type())
			}
		}
	}

	scope := pkg.Scope()
	for _, name := range scope.Names() {
		obj := scope.Lookup(name)
		if !obj.Exported() && !all {
			continue
		}
		member = name
		switch obj := obj.(type) {
		case *types.Func:
			if keep(obj.Name()) {
				visit(obj.Type())
			}
		case *types.Var:
			visit(obj.Type())
		case *types.Const:
			visit(obj.Type())
		case *types.TypeName:
			visit(obj.Type().Underlying())
			if obj.IsAlias() {
				visit(obj.Type())
			}
			for _, recv := range []types.Type{obj.Type(), types.NewPointer(obj.Type())} {
				ms := types.NewMethodSet(recv)
				for i := 0; i < ms.Len(); i++ {
					m := ms.At(i).Obj()
					if (m.Exported() || all) && keep(m.Name()) {
						member = name + "." + m.Name()
						visit(m.Type())
					}
				}
			}
		}
	}

	out := make([]*types.Package, 0, len(found))
	for p := range found {
		if p != pkg {
			out = append(out, p)
		}
	}
	sort.Slice(out, func(i, j int) bool { return out[i].Path() < out[j].Path() })
	return out
}

// useSite is the use-site model: the packages C# must load to bind the test assembly's own code. It is every package
// named by the type of an expression the compiled test sources hold, plus, for each method the sources call on a
// concrete (non-interface) receiver, the signature packages of EVERY method of that name that a type in a visible
// package declares -- the extension-method candidates C# examines -- to a fixpoint.
func useSite(pkgPath string, refs map[string]bool, refModel bool, compiles map[string]bool, compiled []*packages.Package,
	byPath map[string]*packages.Package, converted map[string]bool) map[string]bool {
	needed := map[string]bool{}
	methodNames := map[string]bool{}
	var queue []*types.Package
	reason := ""

	add := func(p *types.Package) {
		if p == nil || needed[p.Path()] {
			return
		}
		needed[p.Path()] = true
		if explainTarget == pkgPath && converted[p.Path()] && !refs[p.Path()] {
			fmt.Fprintf(os.Stderr, "USE %s: + %s via %s\n", pkgPath, p.Path(), reason)
		}
		if bp := byPath[p.Path()]; bp != nil && bp.Types != nil {
			queue = append(queue, bp.Types)
		} else {
			queue = append(queue, p)
		}
	}

	var addType func(t types.Type, seen map[types.Type]bool)
	addType = func(t types.Type, seen map[types.Type]bool) {
		if t == nil || seen[t] {
			return
		}
		seen[t] = true
		switch t := t.(type) {
		case *types.Named:
			add(t.Obj().Pkg())
			if args := t.TypeArgs(); args != nil {
				for i := 0; i < args.Len(); i++ {
					addType(args.At(i), seen)
				}
			}
		case *types.Alias:
			add(t.Obj().Pkg())
			addType(types.Unalias(t), seen)
		case *types.Pointer:
			addType(t.Elem(), seen)
		case *types.Slice:
			addType(t.Elem(), seen)
		case *types.Array:
			addType(t.Elem(), seen)
		case *types.Map:
			addType(t.Key(), seen)
			addType(t.Elem(), seen)
		case *types.Chan:
			addType(t.Elem(), seen)
		case *types.Signature:
			addType(t.Params(), seen)
			addType(t.Results(), seen)
		case *types.Tuple:
			for i := 0; i < t.Len(); i++ {
				addType(t.At(i).Type(), seen)
			}
		case *types.Struct:
			for i := 0; i < t.NumFields(); i++ {
				addType(t.Field(i).Type(), seen)
			}
		case *types.Interface:
			for i := 0; i < t.NumEmbeddeds(); i++ {
				addType(t.EmbeddedType(i), seen)
			}
			for i := 0; i < t.NumExplicitMethods(); i++ {
				addType(t.ExplicitMethod(i).Type(), seen)
			}
		case *types.TypeParam:
			addType(t.Constraint(), seen)
		}
	}

	for r := range refs {
		add(types.NewPackage(r, ""))
	}

	own := map[string]bool{pkgPath: true}

	for _, c := range compiled {
		own[c.Types.Path()] = true
		for _, f := range c.Syntax {
			name := c.Fset.File(f.Pos()).Name()
			if refModel && !strings.HasSuffix(name, "_test.go") {
				continue
			}
			if compiles != nil && !compiles[filepath.Base(name)] {
				continue
			}
			ast.Inspect(f, func(n ast.Node) bool {
				e, ok := n.(ast.Expr)
				if !ok {
					return true
				}
				if tv, ok := c.TypesInfo.Types[e]; ok {
					reason = "expr " + c.Fset.Position(e.Pos()).String() + " : " + tv.Type.String()
					addType(tv.Type, map[types.Type]bool{})
				}
				if sel, ok := e.(*ast.SelectorExpr); ok {
					if s := c.TypesInfo.Selections[sel]; s != nil && (s.Kind() == types.MethodVal || s.Kind() == types.MethodExpr) {
						if !types.IsInterface(s.Recv()) {
							methodNames[sel.Sel.Name] = true
						}
					}
				}
				return true
			})
		}
		queue = append(queue, c.Types)
	}

	done := map[*types.Package]bool{}
	for len(queue) > 0 {
		pkg := queue[0]
		queue = queue[1:]
		if pkg == nil || done[pkg] || pkg.Scope() == nil {
			continue
		}
		done[pkg] = true
		visibleAll := own[pkg.Path()]
		for _, name := range pkg.Scope().Names() {
			tn, ok := pkg.Scope().Lookup(name).(*types.TypeName)
			if !ok || tn.IsAlias() || types.IsInterface(tn.Type()) || (!tn.Exported() && !visibleAll) {
				continue
			}
			for _, recv := range []types.Type{tn.Type(), types.NewPointer(tn.Type())} {
				ms := types.NewMethodSet(recv)
				for i := 0; i < ms.Len(); i++ {
					m := ms.At(i).Obj()
					if ptrOnly {
						if _, isPtr := m.Type().(*types.Signature).Recv().Type().(*types.Pointer); !isPtr {
							continue
						}
					}
					if refThis != nil {
						entry := refThis[pkg.Path()+"."+name+"."+m.Name()]
						if entry == nil || (entry["vis"] != "public" && !visibleAll) {
							continue
						}
					}
					if methodNames[m.Name()] && (m.Exported() || visibleAll) {
						reason = "candidate " + pkg.Path() + "." + name + "." + m.Name()
						addType(m.Type(), map[types.Type]bool{})
					}
				}
			}
		}
	}

	out := map[string]bool{}
	for p := range needed {
		if converted[p] && p != pkgPath && !own[p] {
			out[p] = true
		}
	}
	return out
}
