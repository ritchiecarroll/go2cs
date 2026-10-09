#!/usr/bin/env bash
# rehearse.sh -- reproduces the arms of docs/phase4/REHEARSAL-nugetgo-hashset-2026-10-09.md (linux, bash).
# LOCAL ONLY: nothing is pushed or published. Every restore uses a private NUGET_PACKAGES under $NG.
#   R  = a go2cs checkout at the tree under test (the record used 56f0f1f254)
#   NG = an empty work folder outside the checkout
# Needs Go 1.24.13 (GOTOOLCHAIN=local), the .NET 10 SDK and pwsh on PATH.
set -euo pipefail
: "${R:?set R to the go2cs checkout}" "${NG:?set NG to an empty work folder}"
export GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
export GOROOT="$(go env GOROOT)"
MOD=github.com/ritchiecarroll/hashset VER=v1.0.0
ID=nugetgo.github.com.ritchiecarroll.hashset PV=1.0.0-local.1
mkdir -p "$NG/bin"
(cd "$R/src/go2cs" && go build -o "$NG/bin/go2cs" .)
go mod download "$MOD@$VER"
SRC="$(go env GOMODCACHE)/$MOD@$VER"

# Step 1: convert with -recurse, run the Go tests both sides (validation pages under $NG/s1/validation).
"$NG/bin/go2cs" -recurse -tests -test-action all -go2cspath "$R/src" "$SRC" "$NG/s1"

# Step 2: the pack's input is a -recurse=nuget root.
"$NG/bin/go2cs" -recurse=nuget -go2cspath "$R/src" "$SRC" "$NG/s2"
VAL="$NG/s1/validation/$MOD"
pack() { # $1 = -Feed
  rm -rf "$NG/packscratch"
  pwsh -NoProfile -File "$R/src/tools/nugetgo/nugetgo-pack.ps1" -ModulePath "$MOD" -GoVersion "$VER" -RecurseRoot "$NG/s2" \
    -ValidationDir "$VAL" -ClosureVersion 1.24.13.4 -Feed "$1" -OutDir "$NG/feed" -Scratch "$NG/packscratch" \
    -RepositoryUrl https://github.com/ritchiecarroll/hashset-cs -Upstream 'The go2cs Authors' -UpstreamPublishes \
    -RehearsalSuffix local.1
}
# Arm 2A: the published closure (expected RED on a tree past the last published release).
pack https://api.nuget.org/v3/index.json || echo "arm 2A: pack failed (exit $?)"
# Arm 2B: go.lib + go.gen packed from THIS tree under an unreal version, in a local folder feed.
(cd "$R/src" && NUGET_PACKAGES="$NG/np-b" dotnet pack core/golib/golib.csproj -c Release -o "$NG/localfeed" -p:GoBuildNumber=900 \
  && NUGET_PACKAGES="$NG/np-b" dotnet pack gen/go2cs-gen/go2cs-gen.csproj -c Release -o "$NG/localfeed" -p:GoBuildNumber=900)
pack "$NG/localfeed"
(cd "$R/src/tools/nugetgo" && pwsh -NoProfile -File ./Test-NugetgoIdentity.ps1 && pwsh -NoProfile -File ./Test-NugetgoSelfDescription.ps1)

# Step 3: a sample app importing hashset, and row #1 in a local mappings file.
mkdir -p "$NG/app"
cp "$R/docs/phase4/rehearsal-nugetgo-hashset/main.go.txt" "$NG/app/main.go"
printf 'module example.com/hashsetdemo\n\ngo 1.24\n\nrequire %s %s\n' "$MOD" "$VER" > "$NG/app/go.mod"
(cd "$NG/app" && go mod tidy && go run . > "$NG/app.go.out")
printf '# module-path\tnuget-id\tstatus\tsource-repo\tregistered\tcontact\n%s\t%s\tcanonical\thttps://github.com/ritchiecarroll/hashset-cs\t2026-10-09\towner\n' \
  "$MOD" "$ID" > "$NG/mappings.txt"
export XDG_CACHE_HOME="$NG/xdg"
# 3a: the converter can select only from api.nuget.org, so the package is not found and hashset converts locally.
"$NG/bin/go2cs" -recurse=nuget -nuget-map "$NG/mappings.txt" -nuget-map-only -go2cspath "$R/src" "$NG/app" "$NG/a1"
# 3b: a hand-seeded lock + the nupkg in the converter's cache take the lock-first path.
NUPKG="$NG/feed/$ID.$PV.nupkg"
mkdir -p "$XDG_CACHE_HOME/go2cs/nuget-map/packages/$ID/$PV" "$NG/a2"
cp "$NUPKG" "$XDG_CACHE_HOME/go2cs/nuget-map/packages/$ID/$PV/"
HASH="sha512-$(openssl dgst -sha512 -binary "$NUPKG" | base64 -w0)"
printf '# module\tversion\tnuget-id\tstatus\tlayer\tpackage-version\tcontent-hash\n%s\t%s\t%s\tcanonical\t%s\t%s\t%s\n' \
  "$MOD" "$VER" "$ID" "$NG/mappings.txt" "$PV" "$HASH" > "$NG/a2/go2cs.nuget.lock"
"$NG/bin/go2cs" -recurse=nuget -nuget-map "$NG/mappings.txt" -nuget-map-only -go2cspath "$R/src" "$NG/app" "$NG/a2"
run_arm() { # $1 = arm name, $2.. = extra local feeds
  local arm=$1; shift
  { echo '<?xml version="1.0" encoding="utf-8"?>'; echo '<configuration><packageSources><clear />'
    echo '<add key="nuget.org" value="https://api.nuget.org/v3/index.json" />'
    echo "<add key=\"rehearsal\" value=\"$NG/feed\" />"
    local n=0; for f in "$@"; do n=$((n+1)); echo "<add key=\"local$n\" value=\"$f\" />"; done
    echo '</packageSources></configuration>'; } > "$NG/a2/nuget.config"
  rm -rf "$NG/a2/.artifacts"
  (cd "$NG/a2/src/example.com/hashsetdemo" && NUGET_PACKAGES="$NG/np-app$arm" dotnet build -c Release \
    && NUGET_PACKAGES="$NG/np-app$arm" dotnet run -c Release --no-build > "$NG/a2-run$arm.out" 2>&1) || true
  diff "$NG/app.go.out" "$NG/a2-run$arm.out" && echo "arm 3$arm: IDENTICAL to go run" || echo "arm 3$arm: differs"
}
run_arm A                    # go.* 1.24.13.4 from nuget.org
run_arm B "$NG/localfeed"    # go.lib/go.gen from this tree
