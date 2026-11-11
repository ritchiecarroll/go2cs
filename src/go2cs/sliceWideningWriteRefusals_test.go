// sliceWideningWriteRefusals_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bytes"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

// The defect's own program (docs/phase4/DESIGN-slice-widening-write-refusal.md, section 1): Go prints cba, and the
// element-wise widening hands reverse a copy, so the converted program printed abc with no diagnostic.
const sliceWideningReverseRepro = `package main

import "fmt"

type Named interface{ Name() string }

type item struct{ n string }

func (i *item) Name() string { return i.n }

func reverse[N Named](ns []N) {
	for i, j := 0, len(ns)-1; i < j; i, j = i+1, j-1 {
		ns[i], ns[j] = ns[j], ns[i]
	}
}

func main() {
	all := []*item{{"a"}, {"b"}, {"c"}}
	reverse(all)
	for _, x := range all {
		fmt.Print(x.Name())
	}
	fmt.Println()
}
`

// TestSliceWideningWriteRefusedThroughTheCommandLine: the repro is refused by name, with a canonical GO2CS2006 line
// at the widened argument naming the write, and the run exits non-zero.
func TestSliceWideningWriteRefusedThroughTheCommandLine(t *testing.T) {
	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":   "module example/repro\n\ngo 1.24\n",
		"repro.go": sliceWideningReverseRepro,
	})

	stderr, err := runConverterOnFixture(t, dir, "CGO_ENABLED=0")

	if err == nil {
		t.Fatalf("reverse writes the widened slice; the conversion must be refused with a non-zero exit\nstderr:\n%s", stderr)
	}

	if !strings.Contains(stderr, "Refusing to convert: package example/repro") {
		t.Errorf("the human-readable refusal line is missing\nstderr:\n%s", stderr)
	}

	requireStderrLine(t, stderr, dir, "repro.go", "(19,10): error GO2CS2006: reverse(...) receives all as []N; reverse may write it: an element is assigned at repro.go:13")
}

// TestSliceWideningReadOnlyCalleeStillConverts: the same program with a callee that only ranges over the slice
// converts as it did before the gate.
func TestSliceWideningReadOnlyCalleeStillConverts(t *testing.T) {
	dir := t.TempDir()

	readOnly := strings.Replace(sliceWideningReverseRepro, "ns[i], ns[j] = ns[j], ns[i]", "_, _ = ns[i].Name(), ns[j].Name()", 1)

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":   "module example/repro\n\ngo 1.24\n",
		"repro.go": readOnly,
	})

	if stderr, err := runConverterOnFixture(t, dir, "CGO_ENABLED=0"); err != nil {
		t.Fatalf("a read-only callee must convert, got %v\nstderr:\n%s", err, stderr)
	} else if strings.Contains(stderr, "GO2CS2006") {
		t.Fatalf("a read-only callee was refused\nstderr:\n%s", stderr)
	}
}

// The prototype's 17 shapes (DESIGN-slice-widening-write-refusal.md, section 3), then one shape for each rule of that
// section the 17 leave uncovered. Every call site is on its own line, marked `shape:<name>`; copyIn's two arguments
// are two sites, one per parameter.
const sliceWideningShapeFixture = `package fx

import (
	"fmt"
	"slices"
	"sort"
)

type Named interface{ Name() string }

type item struct{ n string }

func (i *item) Name() string { return i.n }

func walk[N Named](ns []N) {
	for _, x := range ns {
		_ = x.Name()
	}
}

func reverse[N Named](ns []N) {
	for i, j := 0, len(ns)-1; i < j; i, j = i+1, j-1 {
		ns[i], ns[j] = ns[j], ns[i]
	}
}

func first[N Named](ns []N) string {
	if len(ns) == 0 || ns == nil {
		return ""
	}
	return ns[0].Name()
}

func viaHelperRead[N Named](ns []N)  { walk(ns) }
func viaHelperWrite[N Named](ns []N) { reverse(ns) }
func appends[N Named](ns []N, x N)   { ns = append(ns, x); _ = ns }
func copyIn[N Named](ns, src []N)    { copy(ns, src) }
func copyOut[N Named](ns []N)        { dst := make([]N, len(ns)); copy(dst, ns) }
func slicesReverse[N Named](ns []N)  { slices.Reverse(ns) }
func sortSlice[N Named](ns []N) {
	sort.Slice(ns, func(i, j int) bool { return ns[i].Name() < ns[j].Name() })
}
func closureWrite[N Named](ns []N, x N) { f := func() { ns[0] = x }; f() }
func aliasWrite[N Named](ns []N, x N)   { y := ns; y[0] = x }
func returns[N Named](ns []N) []N       { return ns }
func addr[N Named](ns []N) *N           { return &ns[0] }
func recursive[N Named](ns []N, d int) {
	if d > 0 {
		recursive(ns, d-1)
	}
}
func rangeSub[N Named](ns []N) {
	for _, x := range ns[1:] {
		_ = x
	}
}

func capNil[N Named](ns []N) bool    { return cap(ns) > 0 && nil != ns }
func zeroView[N Named](ns []N) []N   { return append(ns[:0:0], ns...) }
func spread[N Named](ns []N) []N     { var out []N; return append(out, ns...) }
func cloned[N Named](ns []N) []N     { return slices.Clone(ns) }
func lenOfReslice[N Named](ns []N) int { return len(ns[1:]) }
func selfReslice[N Named](ns []N) {
	for len(ns) > 0 {
		_ = ns[0].Name()
		ns = ns[1:]
	}
}
func rebind[N Named](ns []N, x N) int { ns = []N{x}; return len(ns) }
func clears[N Named](ns []N)      { clear(ns) }
func printed[N Named](ns []N)     { fmt.Println(ns) }
func funcValue[N Named](ns []N)   { g := walk[N]; g(ns) }
func sent[N Named](ns []N)        { ch := make(chan []N, 1); ch <- ns }
func keptReslice[N Named](ns []N) []N { return ns[1:] }

func sites(all, other []*item, one *item) {
	walk(all) // shape:walk
	reverse(all) // shape:reverse
	_ = first(all) // shape:first
	viaHelperRead(all) // shape:viaHelperRead
	viaHelperWrite(all) // shape:viaHelperWrite
	appends(all, one) // shape:appends
	copyIn(all, other) // shape:copyIn
	copyOut(all) // shape:copyOut
	slicesReverse(all) // shape:slicesReverse
	sortSlice(all) // shape:sortSlice
	closureWrite(all, one) // shape:closureWrite
	aliasWrite(all, one) // shape:aliasWrite
	_ = returns(all) // shape:returns
	_ = addr(all) // shape:addr
	recursive(all, 2) // shape:recursive
	rangeSub(all) // shape:rangeSub
	_ = capNil(all) // shape:capNil
	_ = zeroView(all) // shape:zeroView
	_ = spread(all) // shape:spread
	_ = cloned(all) // shape:cloned
	_ = lenOfReslice(all) // shape:lenOfReslice
	selfReslice(all) // shape:selfReslice
	_ = rebind(all, one) // shape:rebind
	clears(all) // shape:clears
	printed(all) // shape:printed
	funcValue(all) // shape:funcValue
	sent(all) // shape:sent
	_ = keptReslice(all) // shape:keptReslice
}
`

// sliceWideningShapes is each site's expected verdict: write is "" for a callee proven read-only, else a fragment of
// the write the refusal must name.
var sliceWideningShapes = []struct{ shape, arg, write string }{
	{"walk", "all", ""},
	{"reverse", "all", "an element is assigned at fx.go:"},
	{"first", "all", ""},
	{"viaHelperRead", "all", ""},
	{"viaHelperWrite", "all", "it is passed to reverse at fx.go:"},
	{"appends", "all", "it is append's destination at fx.go:"},
	{"copyIn", "all", "it is copy's destination at fx.go:"},
	{"copyIn", "other", ""},
	{"copyOut", "all", ""},
	{"slicesReverse", "all", "it is passed to slices.Reverse at fx.go:"},
	{"sortSlice", "all", "it is passed to sort.Slice at fx.go:"},
	{"closureWrite", "all", "an element is assigned at fx.go:"},
	{"aliasWrite", "all", "it is assigned to another variable at fx.go:"},
	{"returns", "all", "it is returned at fx.go:"},
	{"addr", "all", "an element's address is taken at fx.go:"},
	{"recursive", "all", ""},
	{"rangeSub", "all", ""},
	{"capNil", "all", ""},
	{"zeroView", "all", ""},
	{"spread", "all", ""},
	{"cloned", "all", ""},
	{"lenOfReslice", "all", ""},
	{"selfReslice", "all", ""},
	{"rebind", "all", ""},
	{"clears", "all", "it is passed to the builtin clear at fx.go:"},
	{"printed", "all", "it is passed in the variadic position of fmt.Println at fx.go:"},
	{"funcValue", "all", "it is passed to a call that is not a declared function at fx.go:"},
	{"sent", "all", "it is used where a read is not proven at fx.go:"},
	{"keptReslice", "all", "it is resliced and the reslice kept at fx.go:"},
}

// TestSliceWideningVerdictPerShape: every shape's site is examined, each verdict is the expected one, and a refused
// site names its write. Flipping any one row's verdict turns this red on that shape (safety floor 13).
func TestSliceWideningVerdictPerShape(t *testing.T) {
	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/fx\n\ngo 1.24\n",
		"fx.go":  sliceWideningShapeFixture,
	})

	lines := map[string]int{}

	for i, line := range strings.Split(sliceWideningShapeFixture, "\n") {
		if at := strings.Index(line, "// shape:"); at >= 0 {
			lines[strings.TrimSpace(line[at+len("// shape:"):])] = i + 1
		}
	}

	type siteKey struct {
		line int
		arg  string
	}

	sites := map[siteKey]string{}

	for _, site := range sliceWideningSites(loadProductionForDir(t, dir)) {
		sites[siteKey{site.position.Line, site.arg}] = site.text
	}

	if len(sites) != len(sliceWideningShapes) {
		t.Errorf("%d sites examined, want %d (one per shape row)", len(sites), len(sliceWideningShapes))
	}

	for _, row := range sliceWideningShapes {
		line, ok := lines[row.shape]

		if !ok {
			t.Errorf("shape %s: no fixture line is marked shape:%s", row.shape, row.shape)
			continue
		}

		text, examined := sites[siteKey{line, row.arg}]

		switch {
		case !examined:
			t.Errorf("shape %s (%s): the site was not examined", row.shape, row.arg)
		case row.write == "" && text != "":
			t.Errorf("shape %s (%s): refused, want read-only:\n%s", row.shape, row.arg, text)
		case row.write != "" && text == "":
			t.Errorf("shape %s (%s): read-only, want refused naming %q", row.shape, row.arg, row.write)
		case row.write != "" && !strings.Contains(text, row.write):
			t.Errorf("shape %s (%s): the refusal does not name %q:\n%s", row.shape, row.arg, row.write, text)
		}
	}
}

// The trigger-agreement fixture: read-only callees only, so the package converts, reached through every call form
// the converter treats differently (a local and a foreign callee, an explicit instantiation, a defer, a go statement,
// a closure).
var sliceWideningAgreementFixture = map[string]string{
	"go.mod": "module example/agree\n\ngo 1.24\n",
	"a/a.go": `package a

type Named interface{ Name() string }

func Walk[N Named](ns []N) {
	for _, x := range ns {
		_ = x.Name()
	}
}
`,
	"agree.go": `package agree

import "example/agree/a"

type item struct{ n string }

func (i *item) Name() string { return i.n }

func walk[N a.Named](ns []N) int {
	n := 0
	for range ns {
		n++
	}
	return n
}

func calls(all []*item) {
	_ = walk(all)
	a.Walk(all)
	_ = walk[*item](all)
	defer a.Walk(all)
	go a.Walk(all)
	func() { _ = walk(all) }()
}
`,
}

// TestSliceWideningGateExaminesEveryWidenedArgument: every argument the emitter widens element-wise is one the gate
// examined. The emitter's widenings are counted in the converted output; the gate's examined sites come from the
// same load. The emitter widens a subset of the trigger's sites (it also needs the pair's adapter), and on this
// fixture the two counts are equal.
func TestSliceWideningGateExaminesEveryWidenedArgument(t *testing.T) {
	dir := t.TempDir()
	writeModuleFiles(t, dir, sliceWideningAgreementFixture)

	examined := sliceWideningSites(loadProductionForDir(t, dir))

	for _, site := range examined {
		if site.text != "" {
			t.Fatalf("the agreement fixture must convert, but %s was refused: %s", site.position, site.text)
		}
	}

	out := t.TempDir()
	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	var stderr bytes.Buffer
	cmd := exec.Command(driverBinary(t), "-go2cspath", treeSrc, dir, out)
	cmd.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=0")
	cmd.Stdout, cmd.Stderr = &bytes.Buffer{}, &stderr

	if err := cmd.Run(); err != nil {
		t.Fatalf("conversion failed: %v\nstderr:\n%s", err, stderr.String())
	}

	emitted, err := os.ReadFile(filepath.Join(out, "agree.cs"))

	if err != nil {
		t.Fatal(err)
	}

	widened := strings.Count(string(emitted), "widen<")

	if widened == 0 || widened != len(examined) {
		t.Fatalf("the emitter widened %d argument(s) and the gate examined %d; every widened argument must be examined\n%s",
			widened, len(examined), emitted)
	}
}

// TestRecurseSkipsASliceWideningRefusal: under -recurse a package refused with GO2CS2006 is skipped like any other
// pre-emission refusal (recurseRefusal.go): it gets no project, the rest of the module converts, and the run ends
// nonzero naming it with its code.
func TestRecurseSkipsASliceWideningRefusal(t *testing.T) {
	module := t.TempDir()

	writeModuleFiles(t, module, map[string]string{
		"go.mod":             "module example.com/rw\n\ngo 1.24\n",
		"main.go":            "package main\n\nimport (\n\t\"example.com/rw/other\"\n\t\"example.com/rw/refused\"\n)\n\nfunc main() {\n\tother.Hello()\n\trefused.Run()\n}\n",
		"refused/refused.go": strings.Replace(strings.Replace(sliceWideningReverseRepro, "package main", "package refused", 1), "func main() {", "func Run() {", 1),
		"other/other.go":     "package other\n\nimport \"fmt\"\n\nfunc Hello() { fmt.Println(\"hello\") }\n",
	})

	out := t.TempDir()
	output, exit := runCLIExit(t, module, "-recurse", "-go2cspath", t.TempDir(), ".", out)

	if exit == 0 {
		t.Errorf("exit 0; want nonzero, since a package was refused\n%s", output)
	}

	root := filepath.Join(out, "src", "example.com", "rw")

	if !hasProject(t, filepath.Join(root, "other")) {
		t.Errorf("other/ has no project; want it converted even though refused/ was refused\n%s", output)
	}

	if hasProject(t, filepath.Join(root, "refused")) {
		t.Errorf("refused/ has a project; want none, since it was refused")
	}

	for _, want := range []string{"error GO2CS2006", "  example.com/rw/refused  GO2CS2006"} {
		if !strings.Contains(output, want) {
			t.Errorf("output does not contain %q\n%s", want, output)
		}
	}
}
