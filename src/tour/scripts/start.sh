#!/usr/bin/env sh
set -eu

# The x/website revision the offline Tour is installed from: the newest commit whose go.mod says
# go 1.24.x (9e72c5721b9d, 2026-02-11; the next commit raises it to 1.25). Never @latest: that
# declares a newer Go than the converter's, so the install fails under GOTOOLCHAIN=local and
# silently downloads another toolchain under auto. This moves with the corpus's Go release (the
# Go 1.26 hop bumps it); keep it equal to start.ps1 and to tourModule in tour.go.
tour_version=v0.0.0-20260211181957-9e72c5721b9d

tour_root=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
repo_root=$(CDPATH= cd -- "$tour_root/../.." && pwd)

go version
dotnet --version

go_path=$(go env GOPATH)
tour_binary="$go_path/bin/tour"
if [ ! -x "$tour_binary" ]; then
  echo "Installing the official offline Tour of Go..."
  go install golang.org/x/website/tour@$tour_version
fi

export GO_TOUR_BIN="$tour_binary"
cd "$tour_root"
exec go run . -repo="$repo_root" "$@"
