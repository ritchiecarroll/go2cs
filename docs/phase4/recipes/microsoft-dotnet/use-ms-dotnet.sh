#!/usr/bin/env bash
# Build a private .NET root that runs MICROSOFT's 10.0.12 runtime on a box whose installed .NET is Canonical's
# (/usr/lib/dotnet, RID ubuntu.*-x64, libcoreclr linking the system libunwind) and whose egress blocks
# builds.dotnet.microsoft.com. The runtime comes from nuget.org, signature-verified. Idempotent.
#   eval "$(bash use-ms-dotnet.sh)"      # prints the two exports; run once per shell
set -euo pipefail
VER=10.0.12
SRC=${DOTNET_SYSTEM_ROOT:-/usr/lib/dotnet}
DST=${DOTNET_MS_ROOT:-$HOME/.dotnet-ms}
PKG=microsoft.netcore.app.runtime.linux-x64
WORK=$DST.work
if [ ! -f "$DST/.ms-runtime-$VER" ]; then
  [ -x "$SRC/dotnet" ] || { echo "no .NET at $SRC" >&2; exit 2; }
  [ -d "$SRC/shared/Microsoft.NETCore.App/$VER" ] || { echo "$SRC has no Microsoft.NETCore.App $VER to replace" >&2; exit 2; }
  rm -rf "$WORK" && mkdir -p "$WORK"
  curl -sSf --max-time 600 -o "$WORK/rt.nupkg" "https://api.nuget.org/v3-flatcontainer/$PKG/$VER/$PKG.$VER.nupkg" >&2
  "$SRC/dotnet" nuget verify "$WORK/rt.nupkg" --all >"$WORK/verify.txt" 2>&1 || { cat "$WORK/verify.txt" >&2; echo "nuget signature verification FAILED" >&2; exit 3; }
  grep -q 'CN=Microsoft Corporation' "$WORK/verify.txt" || { echo "the package is not signed by Microsoft" >&2; exit 3; }
  (cd "$WORK" && mkdir pack && cd pack && unzip -q ../rt.nupkg)
  P=$WORK/pack/runtimes/linux-x64
  rm -rf "$DST" && cp -a "$SRC" "$DST"                  # SDK, packs, templates and the dotnet muxer stay as installed
  F=$DST/shared/Microsoft.NETCore.App/$VER
  cp "$F/.version" "$WORK/version" 2>/dev/null || true
  rm -rf "$F" && mkdir -p "$F"
  cp "$P"/lib/net10.0/* "$F"/ && cp "$P"/native/* "$F"/  # Microsoft's runtime: libcoreclr, libclrjit, libhostpolicy, managed libraries
  rm -f "$F/libhostfxr.so"
  [ -f "$WORK/version" ] && cp "$WORK/version" "$F/.version"
  cp "$P/native/libhostfxr.so" "$DST/host/fxr/$VER/libhostfxr.so"   # Microsoft's hostfxr too; only the muxer binary is the distro's
  sha256sum "$P/native/libcoreclr.so" | cut -d' ' -f1 > "$DST/.ms-runtime-$VER"
  rm -rf "$WORK"
fi
echo "export DOTNET_ROOT=$DST"
echo "export PATH=$DST:\$PATH"
