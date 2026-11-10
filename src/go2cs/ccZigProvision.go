// ccZigProvision.go - Gbtc
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
	"path/filepath"
	"runtime"
	"strings"
	"time"
)

// THE PINNED ZIG (seat Z, step 4 of ccToolchain.go's resolution order; docs/phase4/DESIGN-cgo-zig-provision.md §4-5)
//
// One zig release per converter release, one archive per host, each with the SHA-256 ziglang.org's download index
// states. The archive comes from ziglang.org's community mirrors in random order and then ziglang.org itself, as
// ziglang.org asks of automated downloads. EVERY source's bytes are checked against the pinned hash, and against the
// archive's signature, before anything is unpacked. A mismatch deletes the file and tries the next source, so a hostile
// mirror can fail a download but cannot substitute an archive. The install is unpacked into a staging directory and
// renamed into place whole, as the Go toolchain's is (toolchainProvision.go), so a crashed install leaves nothing a
// later run would mistake for one.

const (
	zigDownloadBase    = "https://ziglang.org/download"
	zigMirrorListURL   = "https://ziglang.org/download/community-mirrors.txt"
	zigInstallStampSHA = ".go2cs-sha256" // written into the install before it is renamed into place
)

// pinnedZigVersion is the release step 4 installs. pinnedZigArchives is generated from ziglang.org's download index,
// as toolchainPins.go is from go.dev's; until that generator lands (DESIGN-cgo-zig-provision.md §4, §6 Q4) it is empty,
// so this go2cs has no pin for any host and step 4 refuses by name.
const pinnedZigVersion = "0.17.0"

var pinnedZigArchives = map[string]zigArchivePin{}

type zigArchivePin struct {
	file   string
	sha256 string
}

// zigSource is one place the pinned archive can be fetched from.
type zigSource struct {
	name string // shown in messages: "ziglang.org" or the mirror's base URL
	url  string // the archive; its signature is url + ".minisig"
}

type zigProvisioner struct {
	version   string // the pinned release, "0.17.0"
	goos      string
	goarch    string
	pin       zigArchivePin
	cacheRoot string // <user cache>/go2cs/toolchains
	mirrorURL string // the community mirror list
	client    *http.Client
	progress  io.Writer
	// sources lists where to fetch from, in order; verifySignature checks the downloaded archive against the
	// signature fetched beside it; unpack writes the archive's top-level directory into dest.
	sources         func() []zigSource
	verifySignature func(archive, signature string) error
	unpack          func(archive, dest string) error
}

// newZigProvisioner returns this host's pinned zig, or nil when this go2cs carries no pin for it.
func newZigProvisioner() *zigProvisioner {
	pin, ok := pinnedZigArchives[runtime.GOOS+"-"+runtime.GOARCH]
	if !ok {
		return nil
	}

	cacheDir, err := os.UserCacheDir()
	if err != nil {
		cacheDir = ""
	}

	z := &zigProvisioner{
		version:   pinnedZigVersion,
		goos:      runtime.GOOS,
		goarch:    runtime.GOARCH,
		pin:       pin,
		cacheRoot: filepath.Join(cacheDir, "go2cs", "toolchains"),
		mirrorURL: zigMirrorListURL,
		client:    &http.Client{Timeout: toolchainDownloadTimeout},
		progress:  os.Stderr,
	}

	z.sources = z.defaultSources
	z.verifySignature = verifyZigSignature
	z.unpack = z.unpackArchive

	return z
}

func (z *zigProvisioner) platform() string { return z.goos + "/" + z.goarch }

// root is the install directory: one per release and host, so two pins never share one.
func (z *zigProvisioner) root() string {
	return filepath.Join(z.cacheRoot, fmt.Sprintf("zig-%s-%s-%s", z.version, z.goos, z.goarch))
}

func (z *zigProvisioner) exe() string {
	return filepath.Join(z.root(), "zig"+exeSuffix(z.goos))
}

// choice is the CC value and the environment a load through this install carries. zig's caches go under go2cs's own
// cache, beside the install, so a conversion never writes into the user's home cache.
func (z *zigProvisioner) choice() (string, []string, error) {
	exe, ok := quoteCCWord(z.exe())
	if !ok {
		return "", nil, fmt.Errorf("the pinned zig's path %s holds both quote characters, which a CC value cannot carry; set -cc or CC", z.exe())
	}

	caches := filepath.Join(z.cacheRoot, "zig-cache")

	return exe + " cc", []string{
		"ZIG_GLOBAL_CACHE_DIR=" + filepath.Join(caches, "global"),
		"ZIG_LOCAL_CACHE_DIR=" + filepath.Join(caches, "local"),
	}, nil
}

// cached reports a whole install of this pin: the stamp written before the rename names the pinned hash.
func (z *zigProvisioner) cached() (string, []string, bool) {
	stamp, err := os.ReadFile(filepath.Join(z.root(), zigInstallStampSHA))
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
func (z *zigProvisioner) install() (string, []string, error) {
	if _, _, err := z.choice(); err != nil {
		return "", nil, err
	}

	var failures []string

	for _, source := range z.sources() {
		if z.progress != nil {
			fmt.Fprintf(z.progress, "go2cs: downloading the pinned C toolchain, zig %s, from %s\n", z.version, source.name)
		}

		err := z.installFromSource(source)
		if err == nil {
			return z.choice()
		}

		failures = append(failures, fmt.Sprintf("%s: %v", source.name, err))
	}

	if len(failures) == 0 {
		return "", nil, fmt.Errorf("go2cs has no source to download zig %s from", z.version)
	}

	return "", nil, fmt.Errorf("go2cs could not install the pinned C toolchain, zig %s (%s); every source failed:\n  %s\n"+
		"install it from a copied archive with `go2cs toolchain install -cc -from FILE`, or set CC or -cc",
		z.version, z.pin.file, strings.Join(failures, "\n  "))
}

func (z *zigProvisioner) installFromSource(source zigSource) error {
	archive, err := z.download(source.url)
	if err != nil {
		return err
	}

	defer os.Remove(archive)

	signature, err := z.download(source.url + ".minisig")
	if err != nil {
		return fmt.Errorf("its signature: %w", err)
	}

	defer os.Remove(signature)

	return z.installFile(archive, signature)
}

// installFile verifies an archive and its signature, then unpacks and installs it. Nothing is unpacked from an archive
// that fails either check.
func (z *zigProvisioner) installFile(archive, signature string) error {
	if err := verifySHA256(archive, z.pin.sha256); err != nil {
		return err
	}

	if err := z.verifySignature(archive, signature); err != nil {
		return err
	}

	staging, err := os.MkdirTemp(z.cacheRoot, ".unpack-")
	if err != nil {
		return err
	}

	defer os.RemoveAll(staging)

	if err := z.unpack(archive, staging); err != nil {
		return err
	}

	if _, err := os.Stat(filepath.Join(staging, "zig"+exeSuffix(z.goos))); err != nil {
		return fmt.Errorf("%s holds no zig%s at its top level", z.pin.file, exeSuffix(z.goos))
	}

	if err := os.WriteFile(filepath.Join(staging, zigInstallStampSHA), []byte(z.pin.sha256+"\n"), 0o644); err != nil {
		return err
	}

	return z.renameIntoPlace(staging)
}

// renameIntoPlace moves staging to root, retrying what toolchainProvision.go retries; another go2cs that finished
// first leaves a whole install, which is kept.
func (z *zigProvisioner) renameIntoPlace(staging string) error {
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

	return fmt.Errorf("go2cs could not move the unpacked zig %s from %s to %s: %w", z.version, staging, z.root(), err)
}

// download fetches url into a temporary file in the cache root, which it creates, and returns its name.
func (z *zigProvisioner) download(url string) (string, error) {
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

// defaultSources is the community mirror list in random order, then ziglang.org. A mirror list that cannot be read
// leaves ziglang.org alone.
func (z *zigProvisioner) defaultSources() []zigSource {
	mirrors := z.readMirrorList(z.mirrorURL)
	rand.Shuffle(len(mirrors), func(i, j int) { mirrors[i], mirrors[j] = mirrors[j], mirrors[i] })

	sources := make([]zigSource, 0, len(mirrors)+1)

	for _, mirror := range mirrors {
		sources = append(sources, zigSource{name: mirror, url: mirror + "/" + z.pin.file})
	}

	return append(sources, zigSource{name: "ziglang.org", url: fmt.Sprintf("%s/%s/%s", zigDownloadBase, z.version, z.pin.file)})
}

// readMirrorList reads one base URL per line; blank lines and '#' comments are skipped, and only https is kept.
func (z *zigProvisioner) readMirrorList(url string) []string {
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
func (z *zigProvisioner) unpackArchive(archive, dest string) error {
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

// unpackTarXz and verifyZigSignature are held for the second reader's answers (DESIGN-cgo-zig-provision.md §6 Q3, and
// minisign's prehashed form, which needs BLAKE2b). Until then each refuses by name, so no pinned install can pass a
// check that does not exist yet.
func unpackTarXz(archive, dest, prefix string) error {
	return errors.New("go2cs cannot unpack a .tar.xz archive yet: its reader is held for a design answer")
}

func verifyZigSignature(archive, signature string) error {
	return errors.New("go2cs cannot check zig's minisign signature yet: the verifier is held for a design answer")
}
