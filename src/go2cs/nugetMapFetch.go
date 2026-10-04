// nugetMapFetch.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"crypto/sha256"
	"encoding/hex"
	"errors"
	"fmt"
	"io"
	"net/http"
	"os"
	"path/filepath"
	"strings"
	"time"
)

// Reading the -nuget-map sources: a local file by its path, or an https:// URL through a small cache.
//
// This is the converter's first HTTP client. It is reached only from runNuGetMapResolution, which runs
// only under -recurse=nuget with a -nuget-map source named. It fetches https:// only (a redirect included),
// caps every source at nugetMapMaxBytes, and keeps the last good copy of each URL under the user cache
// directory, revalidated with its ETag on every run. When a URL cannot be fetched, the cached copy is used
// with a loud warning; with no cached copy, a source the USER named fails the run by name, while the
// default registry warns and answers nothing, so every module it would have answered converts locally --
// the plan's fallback for everything.

// nugetMapRegistryURL is the official registry, appended last unless -nuget-map-only. A variable so a
// test can point it at a local fixture; nothing else assigns it.
var nugetMapRegistryURL = "https://nugetgo.net/v1/mappings.txt"

// nugetMapMaxBytes caps one mapping source. A variable only so a test can lower it.
var nugetMapMaxBytes int64 = 8 << 20

// nugetMapHTTPClient fetches every URL source. A variable so a test can inject a fixture's client; the
// redirect policy below is applied on top of whichever client is in use.
var nugetMapHTTPClient = &http.Client{Timeout: 30 * time.Second}

// errNuGetMapInsecureRedirect refuses a redirect that would leave https://.
var errNuGetMapInsecureRedirect = errors.New("a redirect away from https:// is refused (-nuget-map fetches https:// only)")

// nugetMapCheckRedirect is the fetch's redirect policy: https:// only, at most five hops.
func nugetMapCheckRedirect(req *http.Request, via []*http.Request) error {
	if req.URL.Scheme != "https" {
		return fmt.Errorf("%w: %s", errNuGetMapInsecureRedirect, req.URL)
	}

	if len(via) >= 5 {
		return errors.New("stopped after 5 redirects")
	}

	return nil
}

// nugetMapCacheRoot is where fetched sources are cached. A variable so a test can inject t.TempDir().
var nugetMapCacheRoot = func() (string, error) {
	dir, err := os.UserCacheDir()

	if err != nil {
		return "", err
	}

	return filepath.Join(dir, "go2cs", "nuget-map"), nil
}

// nugetMapSourceList is the ordered list of sources a run reads: the -nuget-map sources as listed, then
// the registry unless -nuget-map-only. -nuget-map off reads none.
func nugetMapSourceList(o nugetMapOptions) []string {
	if o.off {
		return nil
	}

	list := append([]string(nil), o.sources...)

	if !o.only {
		list = append(list, nugetMapRegistryURL)
	}

	return list
}

// loadNuGetMapSource reads and parses one source, returning any warnings about how it was read.
func loadNuGetMapSource(src string, refresh bool) (nugetMapSource, []string, error) {
	if !strings.Contains(src, "://") {
		data, err := readNuGetMapFile(src)

		if err != nil {
			return nugetMapSource{}, nil, err
		}

		return parseNuGetMap(src, data)
	}

	data, warning, err := fetchNuGetMapURL(src, refresh)

	if err != nil {
		if src != nugetMapRegistryURL {
			return nugetMapSource{}, nil, err
		}

		return nugetMapSource{name: src, rows: map[string]nugetMapRow{}}, []string{fmt.Sprintf("%v; every module the registry would answer stays unmapped (converted locally)", err)}, nil
	}

	var warnings []string

	if warning != "" {
		warnings = append(warnings, warning)
	}

	parsed, parseWarnings, err := parseNuGetMap(src, data)
	return parsed, append(warnings, parseWarnings...), err
}

// readNuGetMapFile reads a local mapping file under the size cap.
func readNuGetMapFile(path string) ([]byte, error) {
	file, err := os.Open(path)

	if err != nil {
		return nil, fmt.Errorf("-nuget-map %s: %w", path, err)
	}

	defer file.Close()

	data, err := io.ReadAll(io.LimitReader(file, nugetMapMaxBytes+1))

	if err != nil {
		return nil, fmt.Errorf("-nuget-map %s: %w", path, err)
	}

	if int64(len(data)) > nugetMapMaxBytes {
		return nil, fmt.Errorf("-nuget-map %s: larger than the %d-byte cap on a mapping source", path, nugetMapMaxBytes)
	}

	return data, nil
}

// fetchNuGetMapURL returns a URL source's body. Without refresh, the cached copy's ETag is sent and a 304
// serves the cached body; a fetch that fails falls back to the cached copy with a warning. With refresh,
// the fetch is unconditional and never falls back.
func fetchNuGetMapURL(src string, refresh bool) ([]byte, string, error) {
	cacheBody, cacheTag, cacheErr := nugetMapCachePaths(src)
	cached, haveCache := []byte(nil), false

	if cacheErr == nil && !refresh {
		if data, err := os.ReadFile(cacheBody); err == nil {
			cached, haveCache = data, true
		}
	}

	fallback := func(failure error) ([]byte, string, error) {
		if haveCache {
			fetched := "an earlier run"

			if info, err := os.Stat(cacheBody); err == nil {
				fetched = info.ModTime().UTC().Format(time.RFC3339)
			}

			return cached, fmt.Sprintf("-nuget-map %s could not be fetched (%v); using the cached copy from %s", src, failure, fetched), nil
		}

		return nil, "", fmt.Errorf("-nuget-map %s: %w", src, failure)
	}

	req, err := http.NewRequest(http.MethodGet, src, nil)

	if err != nil {
		return nil, "", fmt.Errorf("-nuget-map %s: %w", src, err)
	}

	if haveCache {
		if tag, err := os.ReadFile(cacheTag); err == nil && len(tag) > 0 {
			req.Header.Set("If-None-Match", string(tag))
		}
	}

	client := *nugetMapHTTPClient
	client.CheckRedirect = nugetMapCheckRedirect
	resp, err := client.Do(req)

	if err != nil {
		if errors.Is(err, errNuGetMapInsecureRedirect) {
			return nil, "", fmt.Errorf("-nuget-map %s: %w", src, err)
		}

		return fallback(err)
	}

	defer resp.Body.Close()

	if resp.StatusCode == http.StatusNotModified && haveCache {
		return cached, "", nil
	}

	if resp.StatusCode != http.StatusOK {
		return fallback(fmt.Errorf("HTTP %s", resp.Status))
	}

	if resp.ContentLength > nugetMapMaxBytes {
		return nil, "", fmt.Errorf("-nuget-map %s: larger than the %d-byte cap on a mapping source", src, nugetMapMaxBytes)
	}

	data, err := io.ReadAll(io.LimitReader(resp.Body, nugetMapMaxBytes+1))

	if err != nil {
		return fallback(err)
	}

	if int64(len(data)) > nugetMapMaxBytes {
		return nil, "", fmt.Errorf("-nuget-map %s: larger than the %d-byte cap on a mapping source", src, nugetMapMaxBytes)
	}

	warning := ""

	if cacheErr == nil {
		if err := writeNuGetMapCache(cacheBody, cacheTag, data, resp.Header.Get("ETag")); err != nil {
			warning = fmt.Sprintf("-nuget-map %s: the cache could not be written (%v); the next run fetches it again", src, err)
		}
	}

	return data, warning, nil
}

// nugetMapCachePaths names a URL's cached body and ETag: one pair per URL, keyed by its SHA-256.
func nugetMapCachePaths(src string) (string, string, error) {
	root, err := nugetMapCacheRoot()

	if err != nil {
		return "", "", err
	}

	sum := sha256.Sum256([]byte(src))
	key := hex.EncodeToString(sum[:16])

	return filepath.Join(root, key+".txt"), filepath.Join(root, key+".etag"), nil
}

// writeNuGetMapCache stores a fetched body and its ETag, the body through a temporary file and a rename
// so a reader never sees a half-written copy.
func writeNuGetMapCache(bodyPath, tagPath string, data []byte, etag string) error {
	if err := os.MkdirAll(filepath.Dir(bodyPath), 0o755); err != nil {
		return err
	}

	temp := bodyPath + ".tmp"

	if err := os.WriteFile(temp, data, 0o644); err != nil {
		return err
	}

	if err := os.Rename(temp, bodyPath); err != nil {
		return err
	}

	if etag == "" {
		_ = os.Remove(tagPath)
		return nil
	}

	return os.WriteFile(tagPath, []byte(etag), 0o644)
}
