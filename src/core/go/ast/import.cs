// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using cmp = cmp_package;
using token = global::go.go.token_package;
using slices = slices_package;
using strconv = strconv_package;
using global::go.go;

partial class ast_package {

// SortImports sorts runs of consecutive import lines in import blocks in f.
// It also removes duplicate imports when it is possible to do so without data loss.
public static void SortImports(ж<token.FileSet> Ꮡfset, ж<File> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    foreach (var (_, d) in f.Decls) {
        var (dΔ1, ok) = d._<ж<GenDecl>>(ᐧ);
        if (!ok || (~dΔ1).Tok != token.IMPORT) {
            // Not an import declaration, so we're done.
            // Imports are always first.
            break;
        }
        if (!(~dΔ1).Lparen.IsValid()) {
            // Not a block: sorted by default.
            continue;
        }
        // Identify and sort runs of specs on successive lines.
        nint i = 0;
        var specs = (~dΔ1).Specs[..0];
        foreach (var (j, s) in (~dΔ1).Specs) {
            if (j > i && lineAt(Ꮡfset, s.Pos()) > 1 + lineAt(Ꮡfset, (~dΔ1).Specs[j - 1].End())) {
                // j begins a new run. End this one.
                specs = appendꓸꓸꓸ(specs, sortSpecs(Ꮡfset, ref (Ꮡf).DerefOrNull(), (~dΔ1).Specs.slice(i, j)));
                i = j;
            }
        }
        specs = appendꓸꓸꓸ(specs, sortSpecs(Ꮡfset, ref (Ꮡf).DerefOrNull(), (~dΔ1).Specs.slice(i)));
        dΔ1.Value.Specs = specs;
        // Deduping can leave a blank line before the rparen; clean that up.
        if (len((~dΔ1).Specs) > 0) {
            var lastSpec = (~dΔ1).Specs[len((~dΔ1).Specs) - 1];
            nint lastLine = lineAt(Ꮡfset, lastSpec.Pos());
            nint rParenLine = lineAt(Ꮡfset, (~dΔ1).Rparen);
            while (rParenLine > lastLine + 1) {
                rParenLine--;
                Ꮡfset.File((~dΔ1).Rparen).MergeLine(rParenLine);
            }
        }
    }
    // Make File.Imports order consistent.
    f.Imports = f.Imports[..0];
    foreach (var (_, decl) in f.Decls) {
        {
            var (declΔ1, ok) = decl._<ж<GenDecl>>(ᐧ); if (ok && (~declΔ1).Tok == token.IMPORT) {
                foreach (var (_, spec) in (~declΔ1).Specs) {
                    f.Imports = append(f.Imports, spec._<ж<ImportSpec>>());
                }
            }
        }
    }
}

internal static nint lineAt(ж<token.FileSet> Ꮡfset, tokenꓸPos pos) {
    return Ꮡfset.PositionFor(pos, false).Line;
}

internal static @string importPath(Spec s) {
    var (t, err) = strconv.Unquote((~(~s._<ж<ImportSpec>>()).Path).Value);
    if (err == default!) {
        return t;
    }
    return ""u8;
}

internal static @string importName(Spec s) {
    var n = s._<ж<ImportSpec>>().Value.Name;
    if (n == nil) {
        return ""u8;
    }
    return (~n).Name;
}

internal static @string importComment(Spec s) {
    var c = s._<ж<ImportSpec>>().Value.Comment;
    if (c == nil) {
        return ""u8;
    }
    return c.Text();
}

// collapse indicates whether prev may be removed, leaving only next.
internal static bool collapse(Spec prev, Spec next) {
    if (importPath(next) != importPath(prev) || importName(next) != importName(prev)) {
        return false;
    }
    return (~prev._<ж<ImportSpec>>()).Comment == nil;
}

partial struct posSpan {
    public tokenꓸPos Start;
    public tokenꓸPos End;
}

partial struct cgPos {
    internal bool left; // true if comment is to the left of the spec, false otherwise.
    internal ж<CommentGroup> cg;
}

internal static slice<Spec> sortSpecs(ж<token.FileSet> Ꮡfset, ref File f, slice<Spec> specs) {
    // Can't short-circuit here even if specs are already sorted,
    // since they might yet need deduplication.
    // A lone import, however, may be safely ignored.
    if (len(specs) <= 1) {
        return specs;
    }
    // Record positions for specs.
    var pos = new slice<posSpan>(len(specs));
    foreach (var (i, s) in specs) {
        pos[i] = new posSpan(s.Pos(), s.End());
    }
    // Identify comments in this range.
    tokenꓸPos begSpecs = pos[0].Start;
    tokenꓸPos endSpecs = pos[len(pos) - 1].End;
    tokenꓸPos beg = Ꮡfset.File(begSpecs).LineStart(lineAt(Ꮡfset, begSpecs));
    nint endLine = lineAt(Ꮡfset, endSpecs);
    var endFile = Ꮡfset.File(endSpecs);
    tokenꓸPos end = default!;
    if (endLine == endFile.LineCount()){
        end = endSpecs;
    } else {
        end = endFile.LineStart(endLine + 1); // beginning of next line
    }
    nint first = len(f.Comments);
    nint last = -1;
    foreach (var (i, g) in f.Comments) {
        if (g.End() >= end) {
            break;
        }
        // g.End() < end
        if (beg <= g.Pos()) {
            // comment is within the range [beg, end[ of import declarations
            if (i < first) {
                first = i;
            }
            if (i > last) {
                last = i;
            }
        }
    }
    slice<ж<CommentGroup>> comments = default!;
    if (last >= 0) {
        comments = f.Comments.slice(first, last + 1);
    }
    // Assign each comment to the import spec on the same line.
    var importComments = new map<ж<ImportSpec>, slice<cgPos>>{};
    nint specIndex = 0;
    foreach (var (_, g) in comments) {
        while (specIndex + 1 < len(specs) && pos[specIndex + 1].Start <= g.Pos()) {
            specIndex++;
        }
        bool left = default!;
        // A block comment can appear before the first import spec.
        if (specIndex == 0 && pos[specIndex].Start > g.Pos()){
            left = true;
        } else 
        if (specIndex + 1 < len(specs) && lineAt(Ꮡfset, // Or it can appear on the left of an import spec.
 pos[specIndex].Start) + 1 == lineAt(Ꮡfset, g.Pos())) {
            specIndex++;
            left = true;
        }
        var s = specs[specIndex]._<ж<ImportSpec>>();
        importComments[s] = append(importComments[s], new cgPos(left: left, cg: g));
    }
    // Sort the import specs by import path.
    // Remove duplicates, when possible without data loss.
    // Reassign the import paths to have the same position sequence.
    // Reassign each comment to the spec on the same line.
    // Sort the comments by new position.
    slices.SortFunc(specs, (Spec a, Spec b) => {
        @string ipath = importPath(a);
        @string jpath = importPath(b);
        nint r = cmp.Compare(ipath, jpath);
        if (r != 0) {
            return r;
        }
        @string iname = importName(a);
        @string jname = importName(b);
        r = cmp.Compare(iname, jname);
        if (r != 0) {
            return r;
        }
        return cmp.Compare(importComment(a), importComment(b));
    });
    // Dedup. Thanks to our sorting, we can just consider
    // adjacent pairs of imports.
    var deduped = specs[..0];
    foreach (var (i, s) in specs) {
        if (i == len(specs) - 1 || !collapse(s, specs[i + 1])){
            deduped = append(deduped, s);
        } else {
            tokenꓸPos p = s.Pos();
            Ꮡfset.File(p).MergeLine(lineAt(Ꮡfset, p));
        }
    }
    specs = deduped;
    // Fix up comment positions
    foreach (var (i, s) in specs) {
        var sΔ1 = s._<ж<ImportSpec>>();
        if ((~sΔ1).Name != nil) {
            sΔ1.Value.Name.Value.NamePos = pos[i].Start;
        }
        sΔ1.Value.Path.Value.ValuePos = pos[i].Start;
        sΔ1.Value.EndPos = pos[i].End;
        foreach (var (_, g) in importComments[sΔ1]) {
            foreach (var (_, c) in (~g.cg).List) {
                if (g.left){
                    c.Value.Slash = pos[i].Start - 1;
                } else {
                    // An import spec can have both block comment and a line comment
                    // to its right. In that case, both of them will have the same pos.
                    // But while formatting the AST, the line comment gets moved to
                    // after the block comment.
                    c.Value.Slash = pos[i].End;
                }
            }
        }
    }
    slices.SortFunc(comments, (ж<CommentGroup> a, ж<CommentGroup> b) => cmp.Compare(a.Pos(), b.Pos()));
    return specs;
}

} // end ast_package
