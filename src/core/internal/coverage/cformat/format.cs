// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

// This package provides apis for producing human-readable summaries
// of coverage data (e.g. a coverage percentage for a given package or
// set of packages) and for writing data in the legacy test format
// emitted by "go test -coverprofile=<outfile>".
//
// The model for using these apis is to create a Formatter object,
// then make a series of calls to SetPackage and AddUnit passing in
// data read from coverage meta-data and counter-data files. E.g.
//
//		myformatter := cformat.NewFormatter()
//		...
//		for each package P in meta-data file: {
//			myformatter.SetPackage(P)
//			for each function F in P: {
//				for each coverable unit U in F: {
//					myformatter.AddUnit(U)
//				}
//			}
//		}
//		myformatter.EmitPercent(os.Stdout, nil, "", true, true)
//		myformatter.EmitTextual(nil, somefile)
//
// These apis are linked into tests that are built with "-cover", and
// called at the end of test execution to produce text output or
// emit coverage percentages.
using cmp = cmp_package;
using fmt = fmt_package;
using coverage = go.@internal.coverage_package;
using cmerge = go.@internal.coverage.cmerge_package;
using io = io_package;
using maps = maps_package;
using slices = slices_package;
using sort = sort_package;
using strings = strings_package;
using tabwriter = text.tabwriter_package;
using go.@internal;
using go.@internal.coverage;
using iter = iter_package;
using text;

partial class cformat_package {

partial struct Formatter {
    // Maps import path to package state.
    internal map<@string, ж<pstate>> pm;
    // Records current package being visited.
    internal @string pkg;
    // Pointer to current package state.
    internal ж<pstate> p;
    // Counter mode.
    internal coverage.CounterMode cm;
}

// pstate records package-level coverage data state:
// - a table of functions (file/fname/literal)
// - a map recording the index/ID of each func encountered so far
// - a table storing execution count for the coverable units in each func
partial struct pstate {
    // slice of unique functions
    internal slice<fnfile> funcs;
    // maps function to index in slice above (index acts as function ID)
    internal map<fnfile, uint32> funcTable;
    // A table storing coverage counts for each coverable unit.
    internal map<extcu, uint32> unitTable;
}

// extcu encapsulates a coverable unit within some function.
partial struct extcu {
    internal uint32 fnfid; // index into p.funcs slice
    public partial ref go.@internal.coverage_package.CoverableUnit CoverableUnit { get; }
}

// fnfile is a function-name/file-name tuple.
partial struct fnfile {
    internal @string @file;
    internal @string fname;
    internal bool lit;
}

public static ж<Formatter> NewFormatter(coverage.CounterMode cm) {
    return Ꮡ(new Formatter(
        pm: new map<@string, ж<pstate>>(),
        cm: cm
    ));
}

// SetPackage tells the formatter that we're about to visit the
// coverage data for the package with the specified import path.
// Note that it's OK to call SetPackage more than once with the
// same import path; counter data values will be accumulated.
public static void SetPackage(this ref Formatter fm, @string importpath) {
    if (importpath == fm.pkg) {
        return;
    }
    fm.pkg = importpath;
    var (ps, ok) = fm.pm[importpath, ꟷ];
    if (!ok) {
        ps = @new<pstate>();
        fm.pm[importpath] = ps;
        ps.Value.unitTable = new map<extcu, uint32>();
        ps.Value.funcTable = new map<fnfile, uint32>();
    }
    fm.p = ps;
}

// AddUnit passes info on a single coverable unit (file, funcname,
// literal flag, range of lines, and counter value) to the formatter.
// Counter values will be accumulated where appropriate.
public static void AddUnit(this ref Formatter fm, @string @file, @string fname, bool isfnlit, coverage.CoverableUnit unit, uint32 count) {
    if (fm.p == nil) {
        throw panic("AddUnit invoked before SetPackage");
    }
    var fkey = new fnfile(@file: @file, fname: fname, lit: isfnlit);
    var (idx, ok) = (~fm.p).funcTable[fkey, ꟷ];
    if (!ok) {
        idx = (uint32)len((~fm.p).funcs);
        fm.p.Value.funcs = append((~fm.p).funcs, fkey);
        fm.p.Value.funcTable[fkey] = idx;
    }
    var ukey = new extcu(fnfid: idx, CoverableUnit: unit);
    var pcount = (~fm.p).unitTable[ukey];
    uint32 result = default!;
    if (fm.cm == coverage.CtrModeSet){
        if (count != 0 || pcount != 0) {
            result = 1;
        }
    } else {
        // Use saturating arithmetic.
        (result, _) = cmerge.SaturatingAdd(pcount, count);
    }
    fm.p.Value.unitTable[ukey] = result;
}

// sortUnits sorts a slice of extcu objects in a package according to
// source position information (e.g. file and line). Note that we don't
// include function name as part of the sorting criteria, the thinking
// being that is better to provide things in the original source order.
internal static void sortUnits(this ж<pstate> Ꮡp, slice<extcu> units) {
    slices.SortFunc(units, (extcu ui, extcu uj) => {
        @string ifile = Ꮡp.Value.funcs[ui.fnfid].@file;
        @string jfile = Ꮡp.Value.funcs[uj.fnfid].@file;
        {
            nint r = strings.Compare(ifile, jfile); if (r != 0) {
                return r;
            }
        }
        // NB: not taking function literal flag into account here (no
        // need, since other fields are guaranteed to be distinct).
        {
            nint r = cmp.Compare(ui.StLine, uj.StLine); if (r != 0) {
                return r;
            }
        }
        {
            nint r = cmp.Compare(ui.EnLine, uj.EnLine); if (r != 0) {
                return r;
            }
        }
        {
            nint r = cmp.Compare(ui.StCol, uj.StCol); if (r != 0) {
                return r;
            }
        }
        {
            nint r = cmp.Compare(ui.EnCol, uj.EnCol); if (r != 0) {
                return r;
            }
        }
        return cmp.Compare(ui.NxStmts, uj.NxStmts);
    });
}

// EmitTextual writes the accumulated coverage data for 'pkgs' in the legacy
// cmd/cover text format to the writer 'w'; if pkgs is empty, text output
// is emitted for all packages recorded.  We sort the data items by
// importpath, source file, and line number before emitting (this sorting
// is not explicitly mandated by the format, but seems like a good idea
// for repeatable/deterministic dumps).
public static error EmitTextual(this ref Formatter fm, slice<@string> pkgs, io.Writer w) {
    if (fm.cm == coverage.CtrModeInvalid) {
        throw panic("internal error, counter mode unset");
    }
    if (len(pkgs) == 0) {
        pkgs = new slice<@string>(0, len(fm.pm));
        foreach (var (importpath, _) in fm.pm) {
            pkgs = append(pkgs, importpath);
        }
    }
    {
        var (_, err) = fmt.Fprintf(w, "mode: %s\n"u8, fm.cm.String()); if (err != default!) {
            return err;
        }
    }
    sort.Strings(pkgs);
    foreach (var (_, importpath) in pkgs) {
        var p = fm.pm[importpath];
        if (p == nil) {
            continue;
        }
        var units = new slice<extcu>(0, () => new(nil), len((~p).unitTable));
        foreach (var (u, _) in (~p).unitTable) {
            units = append(units, u);
        }
        p.sortUnits(units);
        foreach (var (_, u) in units) {
            var count = (~p).unitTable[u];
            @string @file = (~p).funcs[u.fnfid].@file;
            {
                var (_, err) = fmt.Fprintf(w, "%s:%d.%d,%d.%d %d %d\n"u8,
                    @file, u.StLine, u.StCol,
                    u.EnLine, u.EnCol, u.NxStmts, count); if (err != default!) {
                    return err;
                }
            }
        }
    }
    return default!;
}

// EmitPercent writes out a "percentage covered" string to the writer
// 'w', selecting the set of packages in 'pkgs' and suffixing the
// printed string with 'inpkgs'.
public static error EmitPercent(this ref Formatter fm, io.Writer w, slice<@string> pkgs, @string inpkgs, bool noteEmpty, bool aggregate) {
    if (len(pkgs) == 0) {
        pkgs = new slice<@string>(0, len(fm.pm));
        foreach (var (importpath, _) in fm.pm) {
            pkgs = append(pkgs, importpath);
        }
    }
    error rep(uint64 cov, uint64 tot) {
        if (tot != 0){
            {
                var (_, err) = fmt.Fprintf(w, "coverage: %.1f%% of statements%s\n"u8,
                    100.0D * (float64)cov / (float64)tot, inpkgs); if (err != default!) {
                    return err;
                }
            }
        } else 
        if (noteEmpty) {
            {
                var (_, err) = fmt.Fprintf(w, "coverage: [no statements]\n"u8); if (err != default!) {
                    return err;
                }
            }
        }
        return default!;
    }
    slices.Sort<slice<@string>, @string>(pkgs);
    uint64 totalStmts = default!;
    uint64 coveredStmts = default!;
    foreach (var (_, importpath) in pkgs) {
        var p = fm.pm[importpath];
        if (p == nil) {
            continue;
        }
        if (!aggregate) {
            (totalStmts, coveredStmts) = (0, 0);
        }
        foreach (var (unit, count) in (~p).unitTable) {
            var nx = (uint64)unit.NxStmts;
            totalStmts += nx;
            if (count != 0) {
                coveredStmts += nx;
            }
        }
        if (!aggregate) {
            {
                var (_, err) = fmt.Fprintf(w, "\t%s\t\t"u8, importpath); if (err != default!) {
                    return err;
                }
            }
            {
                var err = rep(coveredStmts, totalStmts); if (err != default!) {
                    return err;
                }
            }
        }
    }
    if (aggregate) {
        {
            var err = rep(coveredStmts, totalStmts); if (err != default!) {
                return err;
            }
        }
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object totalˢ = (@string)"total"u8;
private static readonly object statementsˢ = (@string)"(statements)"u8;

// EmitFuncs writes out a function-level summary to the writer 'w'. A
// note on handling function literals: although we collect coverage
// data for unnamed literals, it probably does not make sense to
// include them in the function summary since there isn't any good way
// to name them (this is also consistent with the legacy cmd/cover
// implementation). We do want to include their counts in the overall
// summary however.
public static error EmitFuncs(this ж<Formatter> Ꮡfm, io.Writer w) {
    GoFrame ᒐ = default;
    try {
        ref var fm = ref Ꮡfm.DerefOrNull();

        if (fm.cm == coverage.CtrModeInvalid) {
            throw panic("internal error, counter mode unset");
        }
        float64 perc(uint64 covered, uint64 total) {
            if (total == 0) {
                total = 1;
            }
            return 100.0D * (float64)covered / (float64)total;
        }
        var tabber = tabwriter.NewWriter(w, 1, 8, 1, (rune)'\t', 0);
        var tabberʗ1 = tabber;
        defer(() => tabberʗ1.Flush(), ref ᒐ);
        var allStmts = (uint64)0;
        var covStmts = (uint64)0;
        // Emit functions for each package, sorted by import path.
        foreach (var (_, importpath) in slices.Sorted(maps.Keys<map<@string, ж<pstate>>, @string, ж<pstate>>(fm.pm))) {
            var p = fm.pm[importpath];
            if (len((~p).unitTable) == 0) {
                continue;
            }
            var units = new slice<extcu>(0, () => new(nil), len((~p).unitTable));
            foreach (var (u, _) in (~p).unitTable) {
                units = append(units, u);
            }
            // Within a package, sort the units, then walk through the
            // sorted array. Each time we hit a new function, emit the
            // summary entry for the previous function, then make one last
            // emit call at the end of the loop.
            p.sortUnits(units);
            @string fname = ""u8;
            @string ffile = ""u8;
            var flit = false;
            uint32 fline = default!;
            uint64 cstmts = default!;
            uint64 tstmts = default!;
            var pʗ1 = p;
            void captureFuncStart(extcu u) {
                fname = (~pʗ1).funcs[u.fnfid].fname;
                ffile = (~pʗ1).funcs[u.fnfid].@file;
                flit = (~pʗ1).funcs[u.fnfid].lit;
                fline = u.StLine;
            }
            var captureFuncStartʗ1 = captureFuncStart;
            var percʗ1 = perc;
            var tabberʗ2 = tabber;
            error emitFunc(extcu u) {
                // Don't emit entries for function literals (see discussion
                // in function header comment above).
                if (!flit) {
                    {
                        var (_, err) = fmt.Fprintf(new tabwriter_WriterжWriter(tabberʗ2), "%s:%d:\t%s\t%.1f%%\n"u8,
                            ffile, fline, fname, percʗ1(cstmts, tstmts)); if (err != default!) {
                            return err;
                        }
                    }
                }
                captureFuncStartʗ1(u);
                allStmts += tstmts;
                covStmts += cstmts;
                tstmts = 0;
                cstmts = 0;
                return default!;
            }
            foreach (var (k, u) in units) {
                if (k == 0){
                    captureFuncStart(u);
                } else {
                    if (fname != (~p).funcs[u.fnfid].fname) {
                        // New function; emit entry for previous one.
                        {
                            var err = emitFunc(u); if (err != default!) {
                                return err;
                            }
                        }
                    }
                }
                tstmts += (uint64)u.NxStmts;
                var count = (~p).unitTable[u];
                if (count != 0) {
                    cstmts += (uint64)u.NxStmts;
                }
            }
            {
                var err = emitFunc(new extcu(nil)); if (err != default!) {
                    return err;
                }
            }
        }
        {
            var (_, err) = fmt.Fprintf(new tabwriter_WriterжWriter(tabber), "%s\t%s\t%.1f%%\n"u8,
                totalˢ, statementsˢ, perc(covStmts, allStmts)); if (err != default!) {
                return err;
            }
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end cformat_package
