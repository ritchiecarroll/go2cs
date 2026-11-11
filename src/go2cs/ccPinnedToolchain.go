// ccPinnedToolchain.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"context"
	"errors"
	"fmt"
	"io"
	"math/rand/v2"
	"net/http"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"strings"
	"time"
)

// THE PINNED C TOOLCHAIN (seat Z, step 4 of ccToolchain.go's resolution order; docs/phase4/DESIGN-cgo-zig-provision.md)
//
// The default is PER OS (owner ruling, PLAN-cgo-interop §5.1): zig on linux and darwin, llvm-mingw on windows. Under
// zig, windows links libc functions such as cos and strlen statically from zig's bundled mingw libraries, which leaves
// no DLL to P/Invoke; llvm-mingw's UCRT build resolves them to the system DLLs, as Go's own mingw build does. Either is
// selectable explicitly (-cc pinned:zig, -cc pinned:llvm-mingw) wherever this go2cs carries a pin for it.
//
// One release of each per converter release, one archive per host, each with a SHA-256 built into the converter
// (ccToolchainPins.go):
//   - zig: from ziglang.org's download index. The archive comes from ziglang.org's community mirrors in random order
//     and then ziglang.org itself, as ziglang.org asks of automated downloads.
//   - llvm-mingw: from the GitHub release's asset digest. Its publisher signs nothing, so the pin is that SHA-256 alone,
//     and the archive comes from the release itself.
// EVERY source's bytes are checked against the pinned hash before anything is unpacked. A mismatch deletes the file and
// tries the next source, so a hostile source can fail a download but cannot substitute an archive. The install is
// unpacked into a staging directory and renamed into place whole, as the Go toolchain's is (toolchainProvision.go), so a
// crashed install leaves nothing a later run would mistake for one.

const (
	zigDownloadBase       = "https://ziglang.org/download"
	zigMirrorListURL      = "https://ziglang.org/download/community-mirrors.txt"
	llvmMingwDownloadBase = "https://github.com/mstorsjo/llvm-mingw/releases/download"
	ccInstallStampSHA     = ".go2cs-sha256" // written into the install before it is renamed into place
	ccToolchainZig        = "zig"
	ccToolchainLLVMMingw  = "llvm-mingw"
)

// ccSource is one place the pinned archive can be fetched from.
type ccSource struct {
	name string // shown in messages: "ziglang.org" or the mirror's base URL
	url  string // the archive
}

type pinnedProvisioner struct {
	name      string // ccToolchainZig or ccToolchainLLVMMingw
	version   string // the pinned release, "0.17.0" or "20261020"
	exeRel    string // the compiler under the install root, slash-separated
	ccArgs    string // what follows the compiler in CC: " cc" for zig, nothing for llvm-mingw
	goos      string
	goarch    string
	pin       ccArchivePin
	cacheRoot string // <user cache>/go2cs/toolchains
	mirrorURL string // the community mirror list
	client    *http.Client
	progress  io.Writer
	// sources lists where to fetch from, in order; unpack writes the archive's top-level directory into dest; and
	// lookPath finds the system tools a .tar.xz unpack needs (exec.LookPath).
	sources  func() []ccSource
	unpack   func(archive, dest string) error
	lookPath func(string) (string, error)
}

// defaultCCToolchain is the pinned toolchain a host OS takes when -cc names none: llvm-mingw on windows, zig elsewhere.
func defaultCCToolchain(goos string) string {
	if goos == "windows" {
		return ccToolchainLLVMMingw
	}

	return ccToolchainZig
}

// llvmMingwCompiler is the clang driver an llvm-mingw install runs for a Go architecture, targeting windows.
func llvmMingwCompiler(goarch string) string {
	if goarch == "arm64" {
		return "bin/aarch64-w64-mingw32-clang.exe"
	}

	return "bin/x86_64-w64-mingw32-clang.exe"
}

// newPinnedProvisioner returns the named pinned toolchain for this host (the host OS's default for ""), or nil when
// this go2cs carries no pin of it for the host.
func newPinnedProvisioner(name string) *pinnedProvisioner {
	cacheDir, err := os.UserCacheDir()
	if err != nil {
		cacheDir = ""
	}

	z := newPinnedProvisionerFor(name, runtime.GOOS, runtime.GOARCH, filepath.Join(cacheDir, "go2cs", "toolchains"))
	if z == nil {
		return nil
	}

	z.client = &http.Client{Timeout: toolchainDownloadTimeout}
	z.progress = os.Stderr

	return z
}

// newPinnedProvisionerFor describes the named pinned toolchain for a platform, or returns nil when no pin exists.
func newPinnedProvisionerFor(name, goos, goarch, cacheRoot string) *pinnedProvisioner {
	if name == "" {
		name = defaultCCToolchain(goos)
	}

	z := &pinnedProvisioner{name: name, goos: goos, goarch: goarch, cacheRoot: cacheRoot, mirrorURL: zigMirrorListURL}

	var ok bool

	switch name {
	case ccToolchainZig:
		z.version, z.exeRel, z.ccArgs = pinnedZigVersion, "zig"+exeSuffix(goos), " cc"
		z.pin, ok = pinnedZigArchives[goos+"-"+goarch]
	case ccToolchainLLVMMingw:
		z.version, z.exeRel = pinnedLLVMMingwVersion, llvmMingwCompiler(goarch)
		z.pin, ok = pinnedLLVMMingwArchives[goos+"-"+goarch]
	}

	if !ok {
		return nil
	}

	z.sources = z.defaultSources
	z.unpack = z.unpackArchive
	z.lookPath = exec.LookPath

	return z
}

func (z *pinnedProvisioner) platform() string { return z.goos + "/" + z.goarch }

// root is the install directory: one per toolchain, release and host, so two pins never share one.
func (z *pinnedProvisioner) root() string {
	return filepath.Join(z.cacheRoot, fmt.Sprintf("%s-%s-%s-%s", z.name, z.version, z.goos, z.goarch))
}

func (z *pinnedProvisioner) exe() string {
	return filepath.Join(z.root(), filepath.FromSlash(z.exeRel))
}

// label names the toolchain in messages: "zig 0.17.0", "llvm-mingw 20261020".
func (z *pinnedProvisioner) label() string { return z.name + " " + z.version }

// choice is the CC value and the environment a load through this install carries. zig's caches go under go2cs's own
// cache, beside the install, so a conversion never writes into the user's home cache; llvm-mingw needs nothing.
func (z *pinnedProvisioner) choice() (string, []string, error) {
	exe, ok := quoteCCWord(z.exe())
	if !ok {
		return "", nil, fmt.Errorf("the pinned %s's path %s holds both quote characters, which a CC value cannot carry; set -cc or CC", z.name, z.exe())
	}

	if z.name != ccToolchainZig {
		return exe + z.ccArgs, nil, nil
	}

	caches := filepath.Join(z.cacheRoot, "zig-cache")

	return exe + z.ccArgs, []string{
		"ZIG_GLOBAL_CACHE_DIR=" + filepath.Join(caches, "global"),
		"ZIG_LOCAL_CACHE_DIR=" + filepath.Join(caches, "local"),
	}, nil
}

// cached reports a whole install of this pin: the stamp written before the rename names the pinned hash.
func (z *pinnedProvisioner) cached() (string, []string, bool) {
	stamp, err := os.ReadFile(filepath.Join(z.root(), ccInstallStampSHA))
	if err != nil || strings.TrimSpace(string(stamp)) != z.pin.sha256 {
		return "", nil, false
	}

	if _, err := os.Stat(z.exe()); err != nil {
		return "", nil, false
	}

	cc, env, err := z.choice()

	return cc, env, err == nil
}

// install downloads the pinned archive from the first source whose bytes verify, and installs it.
func (z *pinnedProvisioner) install() (string, []string, error) {
	if _, _, err := z.choice(); err != nil {
		return "", nil, err
	}

	if err := z.checkUnpackTools(); err != nil {
		return "", nil, err
	}

	var failures []string

	for _, source := range z.sources() {
		if z.progress != nil {
			fmt.Fprintf(z.progress, "go2cs: downloading the pinned C toolchain, %s, from %s\n", z.label(), source.name)
		}

		err := z.installFromSource(source)
		if err == nil {
			return z.choice()
		}

		failures = append(failures, fmt.Sprintf("%s: %v", source.name, err))
	}

	if len(failures) == 0 {
		return "", nil, fmt.Errorf("go2cs has no source to download %s from", z.label())
	}

	return "", nil, fmt.Errorf("go2cs could not install the pinned C toolchain, %s (%s); every source failed:\n  %s\n"+
		"install it from a copied archive with `go2cs toolchain install -cc -from FILE`, or set CC or -cc",
		z.label(), z.pin.file, strings.Join(failures, "\n  "))
}

// installCommand is `go2cs toolchain install -cc [-from <file>]`: a named archive is checked against the pin even when
// the pin is already installed, so a wrong file is never reported fine, and nothing is downloaded for it; without one,
// the pin is downloaded unless it is already cached.
func (z *pinnedProvisioner) installCommand(from string) error {
	if from == "" {
		if _, _, ok := z.cached(); ok {
			return nil
		}

		_, _, err := z.install()

		return err
	}

	if err := z.checkUnpackTools(); err != nil {
		return err
	}

	if err := os.MkdirAll(z.cacheRoot, 0o755); err != nil {
		return err
	}

	if err := z.installFile(from); err != nil {
		return fmt.Errorf("%s is not the pinned %s (%s): %w", from, z.label(), z.pin.file, err)
	}

	return nil
}

func (z *pinnedProvisioner) installFromSource(source ccSource) error {
	archive, err := z.download(source.url)
	if err != nil {
		return err
	}

	defer os.Remove(archive)

	return z.installFile(archive)
}

// installFile verifies an archive against the pinned hash, then unpacks and installs it. Nothing is unpacked from an
// archive that fails the check.
func (z *pinnedProvisioner) installFile(archive string) error {
	if err := verifySHA256(archive, z.pin.sha256); err != nil {
		return err
	}

	staging, err := os.MkdirTemp(z.cacheRoot, ".cc-unpack-")
	if err != nil {
		return err
	}

	defer os.RemoveAll(staging)

	if err := z.unpack(archive, staging); err != nil {
		return err
	}

	if _, err := os.Stat(filepath.Join(staging, filepath.FromSlash(z.exeRel))); err != nil {
		return fmt.Errorf("%s holds no %s under its top-level directory", z.pin.file, z.exeRel)
	}

	if err := os.WriteFile(filepath.Join(staging, ccInstallStampSHA), []byte(z.pin.sha256+"\n"), 0o644); err != nil {
		return err
	}

	return z.renameIntoPlace(staging)
}

// renameIntoPlace moves staging to root, retrying what toolchainProvision.go retries; another go2cs that finished
// first leaves a whole install, which is kept.
func (z *pinnedProvisioner) renameIntoPlace(staging string) error {
	err := os.Rename(staging, z.root())

	for _, pause := range installRenameBackoff {
		if err == nil || !transientRenameError(err) {
			break
		}

		time.Sleep(pause)
		err = os.Rename(staging, z.root())
	}

	if err == nil {
		return nil
	}

	if _, _, ok := z.cached(); ok {
		return nil
	}

	return fmt.Errorf("go2cs could not move the unpacked %s from %s to %s: %w", z.label(), staging, z.root(), err)
}

// download fetches url into a temporary file in the cache root, which it creates, and returns its name.
func (z *pinnedProvisioner) download(url string) (string, error) {
	ctx, cancel := context.WithTimeout(context.Background(), toolchainDownloadTimeout)
	defer cancel()

	request, err := http.NewRequestWithContext(ctx, http.MethodGet, url, nil)
	if err != nil {
		return "", err
	}

	response, err := z.client.Do(request)
	if err != nil {
		return "", err
	}

	defer response.Body.Close()

	if response.StatusCode != http.StatusOK {
		return "", fmt.Errorf("GET %s: %s", url, response.Status)
	}

	if err := os.MkdirAll(z.cacheRoot, 0o755); err != nil {
		return "", err
	}

	out, err := os.CreateTemp(z.cacheRoot, ".download-")
	if err != nil {
		return "", err
	}

	_, copyErr := io.Copy(out, response.Body)
	closeErr := out.Close()

	if copyErr != nil || closeErr != nil {
		os.Remove(out.Name())
		return "", errors.Join(copyErr, closeErr)
	}

	return out.Name(), nil
}

// defaultSources is, for zig, the community mirror list in random order, then ziglang.org (a mirror list that cannot
// be read leaves ziglang.org alone); for llvm-mingw, the GitHub release, which has no mirrors.
func (z *pinnedProvisioner) defaultSources() []ccSource {
	if z.name == ccToolchainLLVMMingw {
		return []ccSource{{name: "github.com/mstorsjo/llvm-mingw", url: fmt.Sprintf("%s/%s/%s", llvmMingwDownloadBase, z.version, z.pin.file)}}
	}

	mirrors := z.readMirrorList(z.mirrorURL)
	rand.Shuffle(len(mirrors), func(i, j int) { mirrors[i], mirrors[j] = mirrors[j], mirrors[i] })

	sources := make([]ccSource, 0, len(mirrors)+1)

	for _, mirror := range mirrors {
		sources = append(sources, ccSource{name: mirror, url: mirror + "/" + z.pin.file})
	}

	return append(sources, ccSource{name: "ziglang.org", url: fmt.Sprintf("%s/%s/%s", zigDownloadBase, z.version, z.pin.file)})
}

// readMirrorList reads one base URL per line; blank lines and '#' comments are skipped, and only https is kept.
func (z *pinnedProvisioner) readMirrorList(url string) []string {
	file, err := z.download(url)
	if err != nil {
		return nil
	}

	defer os.Remove(file)

	data, err := os.ReadFile(file)
	if err != nil {
		return nil
	}

	var mirrors []string

	for _, line := range strings.Split(string(data), "\n") {
		line = strings.TrimSpace(line)

		if strings.HasPrefix(line, "https://") {
			mirrors = append(mirrors, strings.TrimRight(line, "/"))
		}
	}

	return mirrors
}

// unpackArchive unpacks the archive's single top-level directory, which is its file name without the extension.
func (z *pinnedProvisioner) unpackArchive(archive, dest string) error {
	switch {
	case strings.HasSuffix(z.pin.file, ".zip"):
		return unpackZip(archive, dest, strings.TrimSuffix(z.pin.file, ".zip")+"/")
	case strings.HasSuffix(z.pin.file, ".tar.xz"):
		return unpackTarXz(archive, dest, strings.TrimSuffix(z.pin.file, ".tar.xz")+"/")
	}

	return fmt.Errorf("go2cs cannot unpack %s: not a .zip or .tar.xz", z.pin.file)
}

// quoteCCWord quotes one word of a CC value the way cmd/go splits CC (cmd/internal/quoted.Split): a word with no
// space or quote stays bare, and otherwise takes whichever quote it does not contain. A word holding both cannot be
// carried, as quoted.Join refuses it too.
func quoteCCWord(word string) (string, bool) {
	hasSingle, hasDouble := strings.Contains(word, "'"), strings.Contains(word, `"`)

	switch {
	case !hasSingle && !hasDouble && !strings.ContainsAny(word, " \t\n\r"):
		return word, true
	case !hasSingle:
		return "'" + word + "'", true
	case !hasDouble:
		return `"` + word + `"`, true
	}

	return "", false
}

// checkUnpackTools refuses BEFORE any download when this host cannot unpack the pinned archive. A .zip unpacks in
// process. A .tar.xz goes through the system tar (§6 Q3): macOS's bsdtar reads xz itself, but GNU tar execs an `xz`
// binary, which slim images (Debian-slim, Alpine) lack, and zig's linux archives ship as .tar.xz only.
func (z *pinnedProvisioner) checkUnpackTools() error {
	if !strings.HasSuffix(z.pin.file, ".tar.xz") {
		return nil
	}

	if _, err := z.lookPath("tar"); err != nil {
		return errors.New("the pinned zig needs tar to unpack: install tar, or pass -cc / CC, or " +
			"`go2cs toolchain install -cc -from FILE` with an archive unpacked elsewhere")
	}

	if z.goos == "linux" {
		if _, err := z.lookPath("xz"); err != nil {
			return errors.New("the pinned zig needs xz to unpack: install xz-utils, or pass -cc / CC, or " +
				"`go2cs toolchain install -cc -from FILE` with an archive unpacked elsewhere")
		}
	}

	return nil
}

// unpackTarXz unpacks a verified .tar.xz into dest through the system tar, dropping the archive's single top-level
// directory (prefix) as unpackZip does. The archive's bytes are the pinned ones, so its names are zig's own.
func unpackTarXz(archive, dest, prefix string) error {
	out, err := exec.Command("tar", "-xf", archive, "-C", dest, "--strip-components=1", strings.TrimSuffix(prefix, "/")).CombinedOutput()
	if err != nil {
		return fmt.Errorf("tar could not unpack the pinned zig: %w: %s", err, strings.TrimSpace(string(out)))
	}

	return nil
}
