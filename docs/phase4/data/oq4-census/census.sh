#!/bin/bash
# OQ4 census at a tree: every simple-name dependency on a converted assembly's name. Read-only (git grep on the
# commit, never the worktree). Output: one section per category in census-<sha>.txt; counts on stdout.
set -u
M=${1:-742a3dbb24}
R=${REPO:-.}            # the repository (any checkout holding the commit)
O=${OUT:-census-$M.txt}  # the detail file
: > $O
cat_() { # name, pathspec..., with the pattern in $PAT
  local name=$1; shift
  local hits
  hits=$(git -C $R grep -nE "$PAT" $M -- "$@" 2>/dev/null | sed "s#^$M:##")
  printf '===== %s  [%s]\n%s\n\n' "$name" "$PAT" "$hits" >> $O
  printf '%-44s %5d\n' "$name" "$(printf '%s' "$hits" | grep -c .)"
}
PAT='<AssemblyName>'; cat_ "A1 AssemblyName in templates/converter" 'src/go2cs/*.xml' 'src/go2cs/*.go' ':!src/go2cs/*_test.go'
PAT='PackageId'; cat_ "A2 PackageId derivation (converter)" 'src/go2cs/*.go' 'src/go2cs/*.xml' ':!src/go2cs/*_test.go'
PAT='InternalsVisibleTo'; cat_ "C1 friend grants: converter emission" 'src/go2cs/*.go' 'src/go2cs/*.xml' ':!src/go2cs/*_test.go'
PAT='InternalsVisibleTo'; cat_ "C2 friend grants: golib/hand-owned/gen" 'src/core/golib' 'src/core/unsafe' 'src/core/testing/*.cs' 'src/gen'
PAT='InternalsVisibleTo'; cat_ "C3 friend grants: converted csproj (by kind)" 'src/core/**/*.csproj'
PAT='Type\.GetType\("|Assembly\.Load\(|AssemblyName\(|GetName\(\)\.Name|AssemblyQualifiedName'; cat_ "E1 runtime name lookups (golib/runtime/testing/gen)" 'src/core/golib' 'src/core/runtime/*_impl.cs' 'src/core/runtime/**/*_impl.cs' 'src/core/testing' 'src/core/reflect' 'src/gen'
PAT=', (runtime|fmt|os|reflect|testing|sync|net|log|errors|io|strings|time|unsafe)[a-z.]*"'; cat_ "E2 assembly-qualified type strings" 'src/core/golib' 'src/core/**/*_impl.cs' 'src/core/testing' 'src/gen' 'src/go2cs/*.go'
PAT='\.dll'; cat_ "G1 .dll paths: converter" 'src/go2cs/*.go' ':!src/go2cs/*_test.go'
PAT='\.dll'; cat_ "G2 .dll paths: release/nugetgo tooling" 'src/*.ps1' 'src/tools' ':!src/tools/**/testdata/**'
PAT='\.dll'; cat_ "G3 .dll paths: test harnesses" 'src/tests/**/*.cs' 'src/tests/**/*.ps1' 'src/tests/**/*.targets' 'src/utilities'
PAT='lib/net10\.0|runtimes/'; cat_ "H1 package asset paths" 'src/*.ps1' 'src/tools' 'src/core/golib/*.targets' 'src/core/golib/*.props' 'src/core/Directory.Build.*' ':!src/tools/**/testdata/**'
PAT='\.dll|AssemblyName'; cat_ "K1 docs mentioning dll/assembly names" 'docs/*.md' 'README.md' 'src/core/golib/README.md'
echo "detail: $O"
