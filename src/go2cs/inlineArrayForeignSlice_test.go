// inlineArrayForeignSlice_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"slices"
	"strconv"
	"strings"
	"testing"
)

// THE SLICE FACE OF A FOREIGN INLINE ARRAY (COORD 2026-11-15, Part 2 of the go-winio GUID wall, ruled (a)).
//
// An array field held inline by ANOTHER package's admission (win.GUID's Data4 here, x/sys/windows' GUID in go-winio)
// cannot be kept out by the consumer: the declaring package has already laid it out as a go.arrays member. go-winio's
// pkg/guid slices it four ways (copy into, copy from, [:2] and [2:]), and each was CS1503 (a Range handed to an
// array8<byte>) in the Windows build. Go means a slice that ALIASES the struct's own bytes, so that is what is emitted:
// the field's address, pinned through its box, as unsafe.Slice spells it. A LOCAL sliced face is unchanged (it keeps its
// type out). The fixture asserts the values Go prints, aliasing both ways included, and a value receiver that is sliced
// is sliced as Go's COPY, never the caller's struct.
var inlineForeignSliceModule = map[string]string{
	"win/win.go": `package win

import "unsafe"

type GUID struct {
	Data1 uint32
	Data2 uint16
	Data3 uint16
	Data4 [8]byte
}

func Register(g *GUID) uintptr { return uintptr(unsafe.Pointer(g)) }
`,
	"guid/guid.go": `package guid

import (
	"fmt"

	"example.com/foreignslice/win"
)

type GUID win.GUID

// go-winio's fromArray: copy INTO a local's inline array.
func FromArray(b [16]byte) GUID {
	var g GUID
	g.Data1 = uint32(b[0])
	copy(g.Data4[:], b[8:16])
	return g
}

// go-winio's toArray: copy FROM a value receiver's inline array.
func (g GUID) ToArray() [16]byte {
	b := [16]byte{}
	b[0] = byte(g.Data1)
	copy(b[8:16], g.Data4[:])
	return b
}

// go-winio's String: [:2] and [2:] of a value receiver.
func (g GUID) String() string {
	return fmt.Sprintf("%08x-%04x-%012x", g.Data1, g.Data4[:2], g.Data4[2:])
}

// A value receiver sliced and written: Go slices the receiver's COPY, so the caller's struct is untouched, and the
// slice outlives the call.
func (g GUID) Scribble() []byte {
	s := g.Data4[:]
	s[0] = 0xEE
	return s
}

// A pointer receiver sliced: the caller's own storage.
func (g *GUID) Tail() []byte { return g.Data4[2:] }

// A local sliced: writes through the slice reach the field and writes to the field reach the slice.
func Alias() (byte, byte, int, int) {
	var g GUID
	s := g.Data4[:2]
	s[0] = 7
	g.Data4[1] = 8
	return g.Data4[0], s[1], len(s), cap(s)
}
`,
	"main.go": `package main

import (
	"fmt"
	"runtime"

	"example.com/foreignslice/guid"
	"example.com/foreignslice/win"
)

func main() {
	var b [16]byte
	for i := range b {
		b[i] = byte(i + 1)
	}

	g := guid.FromArray(b)
	fmt.Println(g.Data4)
	fmt.Println(g.ToArray())
	fmt.Println(g.String())

	s := g.Scribble()
	fmt.Println(s[0], g.Data4[0])

	t := g.Tail()
	t[0] = 0xAA
	fmt.Println(g.Data4[2], g.String())

	fmt.Println(guid.Alias())

	// The root slices win.GUID's field directly, three-index form included.
	var w win.GUID
	ws := w.Data4[3:5:6]
	ws[1] = 5
	fmt.Println(w.Data4[4], len(ws), cap(ws))

	// The slices outlive a collection: their storage is the struct's, held by the slice.
	runtime.GC()
	fmt.Println(s[7], t[5], ws[0])
}
`,
}

// The module converts, builds and prints what go prints. GO2CS_INLINE_BUILD=1 runs it (it builds golib too). At the
// emission that rendered x.f[lo:hi] as a C# Range over the member, the build fails CS1503 at every slice site.
func TestInlineArrayForeignSliceAliasesAsGoDoes(t *testing.T) {
	dotnet := requireInlineBuild(t)
	want := goRunInlineModule(t, "foreignslice", inlineForeignSliceModule)
	out := convertInlineModule(t, "foreignslice", inlineForeignSliceModule)

	if built, err := buildConvertedInlineModule(t, dotnet, out); err != nil {
		t.Fatalf("a module slicing a foreign inline array does not build: %v\n%s", err, built)
	}

	got := runConvertedInlineModule(t, dotnet, out)
	t.Logf("READING go prints:\n%s\nREADING go2cs prints:\n%s", want, got)

	if got != want || want == "" {
		t.Fatalf("the two runs differ, or printed nothing:\n   go:\n%s\ngo2cs:\n%s", want, got)
	}
}

// goRunInlineModule runs the module with go and returns what it prints, line endings normalized.
func goRunInlineModule(t *testing.T, name string, files map[string]string) string {
	t.Helper()

	dir := t.TempDir()
	writeModuleFile(t, filepath.Join(dir, "go.mod"), "module example.com/"+name+"\n\ngo 1.23\n")

	for file, source := range files {
		writeModuleFile(t, filepath.Join(dir, file), source)
	}

	goRun := exec.Command("go", "run", ".")
	goRun.Dir = dir
	goRun.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=0")
	output, err := goRun.CombinedOutput()

	if err != nil {
		t.Fatalf("go did not run the module: %v\n%s", err, output)
	}

	return strings.TrimSpace(strings.ReplaceAll(string(output), "\r\n", "\n"))
}

// recurseBuiltProgram is the program a build of project left. A -recurse conversion redirects every project's
// BaseOutputPath under its output root's .artifacts/bin (generateRecurseBuildFiles), so the program is found there, by
// the project's AssemblyName and its runtimeconfig, not assumed beside the project.
func recurseBuiltProgram(t *testing.T, out, project string) string {
	t.Helper()

	contents, err := os.ReadFile(project)

	if err != nil {
		t.Fatal(err)
	}

	name := declaredAssemblyName(string(contents))
	root := filepath.Dir(filepath.Dir(filepath.Dir(out))) // <root>/src/example.com/<name>
	var found []string

	_ = filepath.WalkDir(filepath.Join(root, ".artifacts", "bin"), func(path string, entry os.DirEntry, err error) error {
		if err == nil && !entry.IsDir() && entry.Name() == name+".runtimeconfig.json" {
			found = append(found, filepath.Join(filepath.Dir(path), name+".dll"))
		}

		return nil
	})

	if len(found) != 1 {
		t.Fatalf("the build left %d programs named %s under %s, want 1: %v", len(found), name, filepath.Join(root, ".artifacts", "bin"), found)
	}

	return found[0]
}

// runConvertedInlineModule runs the program a build of the one project under out left, with args.
func runConvertedInlineModule(t *testing.T, dotnet, out string, args ...string) string {
	t.Helper()

	projects, err := filepath.Glob(filepath.Join(out, "*.csproj"))

	if err != nil || len(projects) != 1 {
		t.Fatalf("the conversion wrote %d project files, want 1 (%v)", len(projects), err)
	}

	output, err := exec.Command(dotnet, append([]string{recurseBuiltProgram(t, out, projects[0])}, args...)...).CombinedOutput()

	if err != nil {
		t.Fatalf("the converted program did not run: %v\n%s", err, output)
	}

	return strings.TrimSpace(strings.ReplaceAll(string(output), "\r\n", "\n"))
}

// THE COST OF THE PIN (COORD 2026-11-15, note (1)). A slice of a foreign inline array pins the box that holds the
// struct, for as long as the box lives, and the pin is released by the box's finalizer. A value receiver that is sliced
// is boxed per call, so a loop over such a method pins one fresh box per iteration. The bar was posted BEFORE the first
// run, against an unpinned control that reads the same bytes through a local array, at N = 1,000,000 calls, read after
// GC.Collect, WaitForPendingFinalizers and GC.Collect:
//
//	B1 retained heap   pinned <= control * 1.10 + 256 KiB
//	B2 fragmentation   pinned <= control + 1 MiB
//	B3 pinned objects  pinned <= control + 16
//	B4 peak heap       pinned <= control * 2 + 1 MiB (sampled every millisecond during the run)
//
// Two instrument corrections were made after the first run and before any change to the code under test, and both
// readings were reported: the program drains with runtime.GC() before it exits (collect, wait for finalizers, collect),
// because finalizers do not run once the process is exiting, so the exit reading held every pin still queued for its
// box's finalizer; and the peak samples GC.GetTotalMemory, because the last collection's heap size read 0 for a control
// that never collected. Elapsed time and collection counts are reported, with no bar. The leak arm keeps every slice reachable, so its pins
// can never be released: it must FAIL B1 and B3, or the bar cannot fail and proves nothing.
var inlineSliceGCModule = map[string]string{
	"win/win.go": inlineForeignSliceModule["win/win.go"],
	"guid/guid.go": strings.ReplaceAll(inlineForeignSliceModule["guid/guid.go"], "example.com/foreignslice/", "example.com/slicegc/") + `
// Head slices a value receiver: one boxed copy, pinned, per call.
func (g GUID) Head() []byte { return g.Data4[:2] }

// ToArrayElems returns the same [16]byte as ToArray with no slice of Data4: control3's array return.
func (g GUID) ToArrayElems() [16]byte {
	b := [16]byte{}
	b[0] = byte(g.Data1)
	for j := 0; j < 8; j++ {
		b[8+j] = g.Data4[j]
	}
	return b
}

// HeadCopy reads the same bytes through a local array, which no admission holds inline: nothing is pinned.
func (g GUID) HeadCopy() []byte {
	var a [8]byte
	for i := range a {
		a[i] = g.Data4[i]
	}
	return a[:2]
}
`,
	"main.go": `package main

import (
	"fmt"
	"os"
	"runtime"
	"strconv"

	"example.com/slicegc/guid"
)

var kept [][]byte

func main() {
	n, _ := strconv.Atoi(os.Args[2])
	b := [16]byte{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16}
	g := guid.FromArray(b)
	sum := 0

	switch os.Args[1] {
	case "pinned":
		for i := 0; i < n; i++ {
			sum += int(g.Head()[1])
		}
	case "control":
		for i := 0; i < n; i++ {
			sum += int(g.HeadCopy()[1])
		}
	case "leak":
		for i := 0; i < n; i++ {
			s := g.Head()
			kept = append(kept, s)
			sum += int(s[1])
		}
	case "copyin":
		for i := 0; i < n; i++ {
			h := guid.FromArray(b)
			sum += int(h.Data4[1])
		}
	case "copyout":
		for i := 0; i < n; i++ {
			a := g.ToArray()
			sum += int(a[9])
		}
	case "control3":
		for i := 0; i < n; i++ {
			a := g.ToArrayElems()
			sum += int(a[9])
		}
	case "control2":
		for i := 0; i < n; i++ {
			var h guid.GUID
			for j := 0; j < 8; j++ {
				h.Data4[j] = b[8+j]
			}
			var a [16]byte
			for j := 0; j < 8; j++ {
				a[8+j] = h.Data4[j]
			}
			sum += int(a[9])
		}
	}

	// Go's own drain, inside the program's lifetime: the harness reads at process exit, when finalizers no
	// longer run, so a pin waiting on its box's finalizer would read as retained.
	runtime.GC()
	fmt.Println("SUM", sum)
}
`,
}

// inlineSliceGCHarness is compiled into the converted program: it samples the GC while main runs and reports, at exit,
// what a full collection leaves.
const inlineSliceGCHarness = `namespace go2cs_slicegc_harness
{
    internal static class GcHarness
    {
        private static long s_peakHeap;
        private static volatile bool s_stop;

        [global::System.Runtime.CompilerServices.ModuleInitializer]
        internal static void Start()
        {
            global::System.Diagnostics.Stopwatch clock = global::System.Diagnostics.Stopwatch.StartNew();
            int gen0 = global::System.GC.CollectionCount(0), gen1 = global::System.GC.CollectionCount(1), gen2 = global::System.GC.CollectionCount(2);
            global::System.Threading.Thread sampler = new(() =>
            {
                while (!s_stop)
                {
                    long heap = global::System.GC.GetTotalMemory(false);
                    if (heap > s_peakHeap) s_peakHeap = heap;
                    global::System.Threading.Thread.Sleep(1);
                }
            }) { IsBackground = true };
            sampler.Start();

            global::System.AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                s_stop = true;
                long ms = clock.ElapsedMilliseconds;
                int c0 = global::System.GC.CollectionCount(0) - gen0, c1 = global::System.GC.CollectionCount(1) - gen1, c2 = global::System.GC.CollectionCount(2) - gen2;
                global::System.GC.Collect();
                global::System.GC.WaitForPendingFinalizers();
                global::System.GC.Collect();
                global::System.GCMemoryInfo info = global::System.GC.GetGCMemoryInfo();
                global::System.Console.WriteLine($"GC heap={info.HeapSizeBytes} frag={info.FragmentedBytes} pinned={info.PinnedObjectsCount} peak={s_peakHeap} ms={ms} gen0={c0} gen1={c1} gen2={c2}");
            };
        }
    }
}
`

type sliceGCReading struct{ heap, frag, pinned, peak int64 }

func readSliceGC(t *testing.T, mode, output string) sliceGCReading {
	t.Helper()

	m := regexp.MustCompile(`GC heap=(\d+) frag=(\d+) pinned=(\d+) peak=(\d+) .*`).FindStringSubmatch(output)

	if m == nil {
		t.Fatalf("%s: the harness printed no GC line:\n%s", mode, output)
	}

	t.Logf("READING %s: %s", mode, m[0])

	var r sliceGCReading

	for i, field := range []*int64{&r.heap, &r.frag, &r.pinned, &r.peak} {
		*field, _ = strconv.ParseInt(m[i+1], 10, 64)
	}

	return r
}

// sliceGCBars returns each bar the pinned reading breaks against the control, by name.
func sliceGCBars(pinned, control sliceGCReading) []string {
	var broken []string

	if float64(pinned.heap) > float64(control.heap)*1.10+256<<10 {
		broken = append(broken, "B1")
	}

	if pinned.frag > control.frag+1<<20 {
		broken = append(broken, "B2")
	}

	if pinned.pinned > control.pinned+16 {
		broken = append(broken, "B3")
	}

	if float64(pinned.peak) > float64(control.peak)*2+1<<20 {
		broken = append(broken, "B4")
	}

	return broken
}

func TestInlineArrayForeignSlicePinIsReleased(t *testing.T) {
	dotnet := requireInlineBuild(t)
	out := convertInlineModule(t, "slicegc", inlineSliceGCModule)
	writeModuleFile(t, filepath.Join(out, "GcHarness.cs"), inlineSliceGCHarness)

	if built, err := buildConvertedInlineModule(t, dotnet, out); err != nil {
		t.Fatalf("the GC module does not build: %v\n%s", err, built)
	}

	const n = "1000000"
	control := readSliceGC(t, "control", runConvertedInlineModule(t, dotnet, out, "control", n))
	pinned := readSliceGC(t, "pinned", runConvertedInlineModule(t, dotnet, out, "pinned", n))
	leak := readSliceGC(t, "leak", runConvertedInlineModule(t, dotnet, out, "leak", n))

	if broken := sliceGCBars(leak, control); !slices.Contains(broken, "B1") || !slices.Contains(broken, "B3") {
		t.Fatalf("the leak arm keeps every pin and breaks %v, want B1 and B3: the bars cannot fail", broken)
	}

	// COORD's ruling (a), 2026-11-15: an ESCAPING slice keeps its pin, and B3 (the pins are released) is its gate. B1,
	// B2 and B4 break by margins the BOARD discloses (2026-11-15, C2): one finalizable pin per boxed copy, released
	// after two collections. They are read and reported here, not gated.
	broken := sliceGCBars(pinned, control)

	if slices.Contains(broken, "B3") {
		t.Errorf("slicing a value receiver's inline array 1,000,000 times leaves its pins held after a drain (B3); breaks %v", broken)
	}

	t.Logf("READING disclosed (ruling (a)): breaks %v against the unpinned control", broken)
}

// THE COPY SHAPES (COORD 2026-11-15, ruling (b)): copy into or out of x.f[lo:hi] reads or writes the inline storage over
// a reference for the copy's duration, so it takes no pin and no box, as Go's own copy lets nothing escape. Its bar,
// posted before the first run, N = 1,000,000:
//
//	C1 retained heap   copy <= control * 1.10 + 256 KiB
//	C2 fragmentation   copy <= control + 1 MiB
//	C3 pinned objects  copy <= control + 16
//	C4 peak heap       copy <= control * 1.25 + 1 MiB
//
// The control was control2 (the same 8 bytes moved element-wise both ways, returning nothing) for both shapes. Against
// it copyout MISSED C4 (peak 105.6 MB against 41.5 MB) on the first run: ToArray returns its [16]byte by copy, and
// control2 returns nothing. control3, the same array return built element-wise with no slice of Data4, read the same
// 105.6 MB, so COORD ruled (02:58Z) that control3 is copyout's control, with the bar's numbers unchanged. copyin keeps
// control2, which shares its shape.
func TestInlineArrayForeignSliceCopyTakesNoPin(t *testing.T) {
	dotnet := requireInlineBuild(t)
	out := convertInlineModule(t, "slicegc", inlineSliceGCModule)
	guidCs := strings.ReplaceAll(readGenerated(t, filepath.Join(out, "guid", "guid.cs")), "\r\n", "\n")

	for _, want := range []string{"copy(ref g.Data4, 0, 8, b[8..16]);", "copy(b[8..16], ref g.Data4, 0, 8);"} {
		if !strings.Contains(guidCs, want) {
			t.Errorf("want the pin-free copy %q:\n%s", want, guidCs)
		}
	}

	writeModuleFile(t, filepath.Join(out, "GcHarness.cs"), inlineSliceGCHarness)

	if built, err := buildConvertedInlineModule(t, dotnet, out); err != nil {
		t.Fatalf("the GC module does not build: %v\n%s", err, built)
	}

	const n = "1000000"
	controls := map[string]sliceGCReading{}

	for _, mode := range []string{"control2", "control3"} {
		controls[mode] = readSliceGC(t, mode, runConvertedInlineModule(t, dotnet, out, mode, n))
	}

	for mode, controlMode := range map[string]string{"copyin": "control2", "copyout": "control3"} {
		control := controls[controlMode]
		reading := readSliceGC(t, mode, runConvertedInlineModule(t, dotnet, out, mode, n))
		var broken []string

		if float64(reading.heap) > float64(control.heap)*1.10+256<<10 {
			broken = append(broken, "C1")
		}

		if reading.frag > control.frag+1<<20 {
			broken = append(broken, "C2")
		}

		if reading.pinned > control.pinned+16 {
			broken = append(broken, "C3")
		}

		if float64(reading.peak) > float64(control.peak)*1.25+1<<20 {
			broken = append(broken, "C4")
		}

		if len(broken) > 0 {
			t.Errorf("%s breaks %v against %s", mode, broken, controlMode)
		}
	}
}
