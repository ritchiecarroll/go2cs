// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"archive/tar"
	"archive/zip"
	"compress/gzip"
	"context"
	"crypto/sha256"
	"encoding/hex"
	"errors"
	"fmt"
	"io"
	"io/fs"
	"net/http"
	"os"
	"os/exec"
	"path"
	"path/filepath"
	"runtime"
	"strings"
	"time"

	"golang.org/x/mod/sumdb/dirhash"
)

// The converter needs a Go toolchain at run time: go/packages shells out to `go list`, which needs the
// compiler, and the stack-append harvest compiles (docs/PLAN-dotnet-tool-installer.md §1). A user who
// installs go2cs as a dotnet tool has no Go, so a release build brings its own: on first use it
// downloads the exact Go release it was built with into a go2cs-owned cache, verifies it against hashes
// compiled into the binary (toolchainPins.go), and runs every child against it.
//
// The choice, in order:
//   1. GO2CS_GOROOT names an existing Go of the pinned release: use it.
//   2. GOROOT is set: the operator chose a toolchain; leave it alone (today's behavior).
//   3. The go on PATH is the pinned release, or this binary was built (without -trimpath) by a Go of
//      the pinned release that is still installed: use it, as main always has.
//   4. Otherwise use the cached pinned Go, downloading it first if it is not there.
// Steps 1 and 4 export GOROOT, prepend its bin to PATH and set GOTOOLCHAIN=local, so neither an older Go
// on PATH nor a module's toolchain line can swap in another release under the converter.
//
// A binary built with a Go other than the pinned one (a contributor's build during a hop) provisions
// nothing: its published release is its own build Go, and the pins describe another.

const (
	// goRootOverrideEnv names an existing Go install of the pinned release, for offline hosts and for
	// users who already have one somewhere other than PATH.
	goRootOverrideEnv = "GO2CS_GOROOT"

	toolchainDownloadTimeout = 30 * time.Minute
)

// toolchainSource is one place the pinned Go can be fetched from.
type toolchainSource struct {
	name   string // shown in messages: "go.dev" or the proxy URL
	url    string
	verify func(file string) error
	unpack func(file, dest string) error
}

// toolchainProvisioner holds what the decision reads, so tests can supply each piece.
type toolchainProvisioner struct {
	goVersion string // the pinned release, "go1.26.10"
	goos      string
	goarch    string
	cacheRoot string // <user cache>/go2cs/toolchains
	getenv    func(string) string
	setenv    func(string, string) error
	pathGo    func() string // GOVERSION of the go on PATH, "" when there is none
	buildRoot func() string // the GOROOT this binary was built with, "" for a -trimpath build
	sources   func() []toolchainSource
	client    *http.Client
	progress  io.Writer
}

// ensureGoToolchain is main's first step. It returns an error only when a Go is needed and cannot be
// provided; the message says what to do.
func ensureGoToolchain(args []string) error {
	if runtime.Version() != pinnedGoVersion {
		return nil
	}

	if _, ok := pinnedGoModules[runtime.GOOS+"-"+runtime.GOARCH]; !ok {
		return nil
	}

	cacheDir, err := os.UserCacheDir()
	if err != nil {
		cacheDir = ""
	}

	provisioner := &toolchainProvisioner{
		goVersion: pinnedGoVersion,
		goos:      runtime.GOOS,
		goarch:    runtime.GOARCH,
		cacheRoot: filepath.Join(cacheDir, "go2cs", "toolchains"),
		getenv:    os.Getenv,
		setenv:    os.Setenv,
		pathGo:    pathGoVersion,
		buildRoot: runtime.GOROOT,
		client:    &http.Client{Timeout: toolchainDownloadTimeout},
		progress:  os.Stderr,
	}

	provisioner.sources = provisioner.defaultSources

	return provisioner.ensure(!argsNeedNoToolchain(args))
}

// argsNeedNoToolchain reports a command line that only asks for the version or for help. Those answer
// without converting anything, so they must not start a download.
func argsNeedNoToolchain(args []string) bool {
	if len(args) == 0 {
		return true
	}

	for _, arg := range args {
		switch strings.TrimLeft(arg, "-") {
		case "version", "h", "help":
		default:
			return false
		}
	}

	return true
}

// ensure applies the choice above. With download false it uses a Go that is already present and never
// fetches one.
func (p *toolchainProvisioner) ensure(download bool) error {
	if root := p.getenv(goRootOverrideEnv); root != "" {
		if err := p.checkRelease(root); err != nil {
			return fmt.Errorf("%s: %w", goRootOverrideEnv, err)
		}

		return p.export(root)
	}

	if p.getenv("GOROOT") != "" {
		return nil
	}

	if p.pathGo() == p.goVersion {
		return nil
	}

	// main falls back to the build GOROOT when no go is on PATH: a contributor's build started with a
	// sparse environment (the behavioral runner's children) keeps converting with it.
	if root := p.buildRoot(); root != "" && p.checkRelease(root) == nil {
		return nil
	}

	root := p.cachedRoot()

	if p.checkRelease(root) == nil {
		return p.export(root)
	}

	if !download {
		return nil
	}

	if p.cacheRoot == "" || !filepath.IsAbs(p.cacheRoot) {
		return fmt.Errorf("go2cs needs %s and found no user cache directory to download it into; "+
			"install %s and set %s to its root", p.goVersion, p.goVersion, goRootOverrideEnv)
	}

	if err := p.install(root); err != nil {
		return err
	}

	return p.export(root)
}

// cachedRoot is where the pinned Go lives once installed: one directory per release and platform.
func (p *toolchainProvisioner) cachedRoot() string {
	return filepath.Join(p.cacheRoot, p.goVersion+"."+p.goos+"-"+p.goarch)
}

// checkRelease confirms root is a Go install of the pinned release, by its VERSION file, the same
// file `go version` reads.
func (p *toolchainProvisioner) checkRelease(root string) error {
	data, err := os.ReadFile(filepath.Join(root, "VERSION"))
	if err != nil {
		return fmt.Errorf("%s is not a Go installation (no VERSION file)", root)
	}

	release, _, _ := strings.Cut(string(data), "\n")

	if strings.TrimSpace(release) != p.goVersion {
		return fmt.Errorf("%s holds %s; this go2cs converts with %s", root, strings.TrimSpace(release), p.goVersion)
	}

	if _, err := os.Stat(filepath.Join(root, "bin", "go"+exeSuffix(p.goos))); err != nil {
		return fmt.Errorf("%s has no bin/go", root)
	}

	return nil
}

// export makes root the toolchain of this process and every child it starts.
func (p *toolchainProvisioner) export(root string) error {
	path := filepath.Join(root, "bin")

	if existing := p.getenv("PATH"); existing != "" {
		path += string(os.PathListSeparator) + existing
	}

	for _, setting := range [][2]string{{"GOROOT", root}, {"PATH", path}, {"GOTOOLCHAIN", "local"}} {
		if err := p.setenv(setting[0], setting[1]); err != nil {
			return err
		}
	}

	return nil
}

// install downloads, verifies and unpacks the pinned Go into root. It unpacks into a sibling
// directory and renames it into place, so a process that is interrupted, or that races another go2cs
// doing the same, never leaves a partial install where checkRelease would accept it.
func (p *toolchainProvisioner) install(root string) error {
	if err := os.MkdirAll(p.cacheRoot, 0o755); err != nil {
		return err
	}

	var failures []string

	for _, source := range p.sources() {
		err := p.installFrom(source, root)

		if err == nil {
			return nil
		}

		failures = append(failures, fmt.Sprintf("%s: %v", source.name, err))
	}

	return fmt.Errorf("go2cs could not download %s for %s/%s:\n  %s\n"+
		"Install %s yourself and set %s to its root, or set GOPROXY to a module proxy that serves golang.org/toolchain",
		p.goVersion, p.goos, p.goarch, strings.Join(failures, "\n  "), p.goVersion, goRootOverrideEnv)
}

func (p *toolchainProvisioner) installFrom(source toolchainSource, root string) error {
	fmt.Fprintf(p.progress, "go2cs: downloading %s (%s/%s) from %s into %s, once\n", p.goVersion, p.goos, p.goarch, source.name, root)

	file, err := p.download(source.url)
	if err != nil {
		return err
	}

	defer os.Remove(file)

	if err := source.verify(file); err != nil {
		return err
	}

	staging, err := os.MkdirTemp(p.cacheRoot, ".unpack-")
	if err != nil {
		return err
	}

	defer os.RemoveAll(staging)

	if err := source.unpack(file, staging); err != nil {
		return err
	}

	if err := p.checkRelease(staging); err != nil {
		return err
	}

	if err := os.Rename(staging, root); err != nil {
		// Another go2cs finished first: keep its install if it is whole.
		if p.checkRelease(root) == nil {
			return nil
		}

		return err
	}

	return nil
}

// download fetches url into a temporary file in the cache directory and returns its name.
func (p *toolchainProvisioner) download(url string) (string, error) {
	ctx, cancel := context.WithTimeout(context.Background(), toolchainDownloadTimeout)
	defer cancel()

	request, err := http.NewRequestWithContext(ctx, http.MethodGet, url, nil)
	if err != nil {
		return "", err
	}

	response, err := p.client.Do(request)
	if err != nil {
		return "", err
	}

	defer response.Body.Close()

	if response.StatusCode != http.StatusOK {
		return "", fmt.Errorf("GET %s: %s", url, response.Status)
	}

	out, err := os.CreateTemp(p.cacheRoot, ".download-")
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

// defaultSources lists the release archive first (go.dev, what a person downloading Go gets) and then
// each module proxy GOPROXY names, which serves the same release as the golang.org/toolchain module
// (what GOTOOLCHAIN downloads). A host that reaches only a proxy still gets the same bytes' release.
func (p *toolchainProvisioner) defaultSources() []toolchainSource {
	var sources []toolchainSource
	platform := p.goos + "-" + p.goarch

	if archive, ok := pinnedGoArchives[platform]; ok {
		sources = append(sources, toolchainSource{
			name:   "go.dev",
			url:    "https://go.dev/dl/" + archive.file,
			verify: func(file string) error { return verifySHA256(file, archive.sha256) },
			unpack: func(file, dest string) error {
				if strings.HasSuffix(archive.file, ".zip") {
					return unpackZip(file, dest, "go/")
				}

				return unpackTarGz(file, dest, "go/")
			},
		})
	}

	if sum, ok := pinnedGoModules[platform]; ok {
		module := toolchainModuleVersion(p.goVersion, platform)

		for _, proxy := range goProxyURLs(p.getenv("GOPROXY")) {
			sources = append(sources, toolchainSource{
				name:   proxy,
				url:    proxy + "/golang.org/toolchain/@v/" + module + ".zip",
				verify: func(file string) error { return verifyModuleZip(file, sum) },
				unpack: func(file, dest string) error {
					if err := unpackZip(file, dest, "golang.org/toolchain@"+module+"/"); err != nil {
						return err
					}

					return finishToolchainModule(dest, p.goos)
				},
			})
		}
	}

	return sources
}

// toolchainModuleVersion is the golang.org/toolchain version carrying a release: v0.0.1-go1.26.10.linux-amd64.
func toolchainModuleVersion(goVersion, platform string) string {
	return "v0.0.1-" + goVersion + "." + platform
}

// goProxyURLs reads the proxies a GOPROXY value lists, in order, dropping "direct" and "off" (a
// toolchain is never fetched from a VCS). Unset means the public proxy, as for the go command.
func goProxyURLs(value string) []string {
	if strings.TrimSpace(value) == "" {
		value = "https://proxy.golang.org"
	}

	var urls []string

	for _, entry := range strings.FieldsFunc(value, func(r rune) bool { return r == ',' || r == '|' }) {
		entry = strings.TrimRight(strings.TrimSpace(entry), "/")

		if strings.HasPrefix(entry, "https://") || strings.HasPrefix(entry, "http://") {
			urls = append(urls, entry)
		}
	}

	return urls
}

func verifySHA256(file, want string) error {
	f, err := os.Open(file)
	if err != nil {
		return err
	}

	defer f.Close()

	hash := sha256.New()

	if _, err := io.Copy(hash, f); err != nil {
		return err
	}

	if got := hex.EncodeToString(hash.Sum(nil)); got != want {
		return fmt.Errorf("SHA-256 %s does not match the pinned %s", got, want)
	}

	return nil
}

// verifyModuleZip checks a module zip against its go.sum hash, the h1: value sum.golang.org records.
func verifyModuleZip(file, want string) error {
	got, err := dirhash.HashZip(file, dirhash.Hash1)
	if err != nil {
		return err
	}

	if got != want {
		return fmt.Errorf("module hash %s does not match the pinned %s", got, want)
	}

	return nil
}

// safeJoin maps an archive entry name below prefix onto dest, refusing names that would land outside it.
func safeJoin(dest, name, prefix string) (string, bool) {
	rest, ok := strings.CutPrefix(name, prefix)
	if !ok || rest == "" {
		return "", false
	}

	clean := path.Clean(rest)

	if clean == "." || strings.HasPrefix(clean, "../") || clean == ".." || path.IsAbs(clean) || strings.Contains(clean, ":") {
		return "", false
	}

	return filepath.Join(dest, filepath.FromSlash(clean)), true
}

func unpackZip(file, dest, prefix string) error {
	reader, err := zip.OpenReader(file)
	if err != nil {
		return err
	}

	defer reader.Close()

	for _, entry := range reader.File {
		target, ok := safeJoin(dest, entry.Name, prefix)
		if !ok {
			continue
		}

		if entry.FileInfo().IsDir() {
			if err := os.MkdirAll(target, 0o755); err != nil {
				return err
			}

			continue
		}

		if err := writeEntry(target, entry.Mode(), entry.Open); err != nil {
			return err
		}
	}

	return nil
}

func unpackTarGz(file, dest, prefix string) error {
	f, err := os.Open(file)
	if err != nil {
		return err
	}

	defer f.Close()

	gz, err := gzip.NewReader(f)
	if err != nil {
		return err
	}

	reader := tar.NewReader(gz)

	for {
		header, err := reader.Next()

		if err == io.EOF {
			return nil
		}

		if err != nil {
			return err
		}

		target, ok := safeJoin(dest, header.Name, prefix)
		if !ok {
			continue
		}

		switch header.Typeflag {
		case tar.TypeDir:
			if err := os.MkdirAll(target, 0o755); err != nil {
				return err
			}
		case tar.TypeReg:
			open := func() (io.ReadCloser, error) { return io.NopCloser(reader), nil }

			if err := writeEntry(target, header.FileInfo().Mode(), open); err != nil {
				return err
			}
		}
	}
}

func writeEntry(target string, mode fs.FileMode, open func() (io.ReadCloser, error)) error {
	if err := os.MkdirAll(filepath.Dir(target), 0o755); err != nil {
		return err
	}

	in, err := open()
	if err != nil {
		return err
	}

	defer in.Close()

	out, err := os.OpenFile(target, os.O_CREATE|os.O_WRONLY|os.O_TRUNC, mode.Perm()|0o600)
	if err != nil {
		return err
	}

	_, copyErr := io.Copy(out, in)

	return errors.Join(copyErr, out.Close())
}

// finishToolchainModule does what the go command does on first use of a downloaded toolchain module
// (cmd/go/internal/toolchain/select.go): a module zip records no file modes, so the commands get their
// execute bits, and each _go.mod (stored so the module zip holds no nested modules) becomes go.mod.
func finishToolchainModule(root, goos string) error {
	if goos != "windows" {
		allowExec := func(dir, pattern string) error {
			return filepath.WalkDir(dir, func(path string, entry fs.DirEntry, err error) error {
				if errors.Is(err, fs.ErrNotExist) {
					return nil
				}

				if err != nil || entry.IsDir() {
					return err
				}

				if pattern != "" {
					if matched, _ := filepath.Match(pattern, entry.Name()); !matched {
						return nil
					}
				}

				info, err := entry.Info()
				if err != nil {
					return err
				}

				return os.Chmod(path, info.Mode()&0o777|0o111)
			})
		}

		for _, step := range [][2]string{{"pkg/tool", ""}, {"lib", "go_?*_?*_exec"}, {"bin", ""}} {
			if err := allowExec(filepath.Join(root, filepath.FromSlash(step[0])), step[1]); err != nil {
				return err
			}
		}
	}

	return filepath.WalkDir(filepath.Join(root, "src"), func(path string, entry fs.DirEntry, err error) error {
		if err != nil || entry.IsDir() || entry.Name() != "_go.mod" {
			return err
		}

		data, err := os.ReadFile(path)
		if err != nil {
			return err
		}

		return os.WriteFile(filepath.Join(filepath.Dir(path), "go.mod"), data, 0o644)
	})
}

// pathGoVersion is the GOVERSION of the go command on PATH, or "" when there is none. GOTOOLCHAIN=local
// keeps a module's toolchain line in the working directory from answering for another release.
func pathGoVersion() string {
	goCommand, err := exec.LookPath("go")
	if err != nil {
		return ""
	}

	cmd := exec.Command(goCommand, "env", "GOVERSION")
	cmd.Env = append(os.Environ(), "GOTOOLCHAIN=local")

	out, err := cmd.Output()
	if err != nil {
		return ""
	}

	return strings.TrimSpace(string(out))
}

func exeSuffix(goos string) string {
	if goos == "windows" {
		return ".exe"
	}

	return ""
}
