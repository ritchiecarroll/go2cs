// nugetMapFixture_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"errors"
	"fmt"
	"hash/crc32"
	"net/http"
	"net/http/httptest"
	"net/url"
	"os"
	"path/filepath"
	"strings"
	"sync"
	"testing"
)

// Every -nuget-map test is hermetic. For the whole test binary the mapping client refuses every request
// and the cache root refuses to resolve, so a test that forgets to inject a fixture fails by name instead
// of reaching nugetgo.net or writing into the real user cache directory. A test that needs either
// installs a fixture with newNuGetMapFixture, which also restores these defaults when it ends.
func init() {
	nugetMapHTTPClient = &http.Client{Transport: refusingTransport{}}
	nugetMapCacheRoot = func() (string, error) {
		return "", errors.New("test did not inject a -nuget-map cache directory")
	}
}

// refusingTransport fails every request, naming the URL.
type refusingTransport struct{}

func (refusingTransport) RoundTrip(req *http.Request) (*http.Response, error) {
	return nil, fmt.Errorf("hermetic test: no request may leave the process (%s)", req.URL)
}

// guardTransport forwards only to the fixture's own host and fails anything else by name, so even a
// mistyped URL in a test cannot reach the network -- above all not nugetgo.net.
type guardTransport struct {
	host  string
	inner http.RoundTripper
}

func (g guardTransport) RoundTrip(req *http.Request) (*http.Response, error) {
	if req.URL.Host != g.host {
		return nil, fmt.Errorf("hermetic test: request to %s refused (only the fixture %s is reachable)", req.URL, g.host)
	}

	return g.inner.RoundTrip(req)
}

// nugetMapFixture is a local HTTPS server serving mapping files by path, counting requests per path
// and honouring If-None-Match against a per-path ETag.
type nugetMapFixture struct {
	server   *httptest.Server
	mu       sync.Mutex
	files    map[string]string
	requests map[string]int
	notMod   map[string]int
	sawINM   map[string]int
	cacheDir string
}

// newNuGetMapFixture starts the fixture, points the registry URL at its /registry path, injects its
// client (behind guardTransport) and a t.TempDir() cache, and restores everything on cleanup.
func newNuGetMapFixture(t *testing.T) *nugetMapFixture {
	t.Helper()

	f := &nugetMapFixture{
		files:    make(map[string]string),
		requests: make(map[string]int),
		notMod:   make(map[string]int),
		sawINM:   make(map[string]int),
		cacheDir: t.TempDir(),
	}

	f.server = httptest.NewTLSServer(http.HandlerFunc(f.serve))

	host := strings.TrimPrefix(f.server.URL, "https://")
	savedClient, savedRoot, savedRegistry := nugetMapHTTPClient, nugetMapCacheRoot, nugetMapRegistryURL

	client := f.server.Client()
	client.Transport = guardTransport{host: host, inner: client.Transport}
	nugetMapHTTPClient = client
	nugetMapCacheRoot = func() (string, error) { return f.cacheDir, nil }
	nugetMapRegistryURL = f.url("/registry")

	t.Cleanup(func() {
		f.server.Close()
		nugetMapHTTPClient, nugetMapCacheRoot, nugetMapRegistryURL = savedClient, savedRoot, savedRegistry
	})

	return f
}

func (f *nugetMapFixture) url(path string) string {
	return f.server.URL + path
}

func (f *nugetMapFixture) set(path, body string) {
	f.mu.Lock()
	defer f.mu.Unlock()
	f.files[path] = body
}

// count is how many requests reached path, including conditional ones answered 304.
func (f *nugetMapFixture) count(path string) int {
	f.mu.Lock()
	defer f.mu.Unlock()
	return f.requests[path]
}

func (f *nugetMapFixture) serve(w http.ResponseWriter, r *http.Request) {
	f.mu.Lock()
	defer f.mu.Unlock()

	f.requests[r.URL.Path]++

	if strings.HasPrefix(r.URL.Path, "/redirect-to-http") {
		target := url.URL{Scheme: "http", Host: r.Host, Path: "/registry"}
		http.Redirect(w, r, target.String(), http.StatusFound)
		return
	}

	body, ok := f.files[r.URL.Path]

	if !ok {
		http.NotFound(w, r)
		return
	}

	etag := fmt.Sprintf("\"%08x\"", crc32.ChecksumIEEE([]byte(body)))

	if inm := r.Header.Get("If-None-Match"); inm != "" {
		f.sawINM[r.URL.Path]++

		if inm == etag {
			f.notMod[r.URL.Path]++
			w.WriteHeader(http.StatusNotModified)
			return
		}
	}

	w.Header().Set("ETag", etag)
	w.Header().Set("Content-Type", "text/plain")
	_, _ = w.Write([]byte(body))
}

// mapRow renders one schema-v1 row.
func mapRow(module, id, status string) string {
	return strings.Join([]string{module, id, status, "https://example.com/" + id, "2026-10-02", "someone"}, "\t") + "\n"
}

// writeMapFile writes a local mapping file and returns its path.
func writeMapFile(t *testing.T, name, body string) string {
	t.Helper()
	path := filepath.Join(t.TempDir(), name)

	if err := os.WriteFile(path, []byte(body), 0o644); err != nil {
		t.Fatal(err)
	}

	return path
}

// decisionFor finds a module's decision.
func decisionFor(t *testing.T, decisions []nugetMapDecision, module string) nugetMapDecision {
	t.Helper()

	for _, d := range decisions {
		if d.module == module {
			return d
		}
	}

	t.Fatalf("no decision for %s in %+v", module, decisions)
	return nugetMapDecision{}
}
