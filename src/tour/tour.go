package main

import (
	"bytes"
	"context"
	"fmt"
	"io"
	"net/http"
	"net/http/httputil"
	"net/url"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"sync"
	"time"
)

const (
	// Pinned, not @latest: see $TourVersion in scripts/start.ps1. Keep the three in step.
	tourModule    = "golang.org/x/website/tour@v0.0.0-20261021155534-52d470bcf26e"
	tourGoVersion = "1.23.1"
)

type tourProxy struct {
	address   string
	repoRoot  string
	target    *url.URL
	proxy     *httputil.ReverseProxy
	process   *exec.Cmd
	ready     bool
	readyLock sync.RWMutex
}

func newTourProxy(address, repoRoot string) *tourProxy {
	target, _ := url.Parse("http://" + address)
	t := &tourProxy{address: address, repoRoot: repoRoot, target: target}
	proxy := httputil.NewSingleHostReverseProxy(target)
	originalDirector := proxy.Director
	proxy.Director = func(request *http.Request) {
		originalDirector(request)
		request.Header.Del("Accept-Encoding")
		if request.URL.Path == "/tour/script.js" {
			request.Header.Del("If-Modified-Since")
			request.Header.Del("If-None-Match")
		}
		if request.Header.Get("Upgrade") != "" {
			// The Tour websocket enforces a same-origin handshake. The browser
			// quite correctly sends the wrapper's origin, so translate it to the
			// private upstream origin while forwarding the upgrade.
			request.Header.Set("Origin", target.Scheme+"://"+target.Host)
		}
	}
	proxy.ModifyResponse = t.modifyResponse
	proxy.ErrorHandler = t.proxyError
	t.proxy = proxy
	return t
}

func (t *tourProxy) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	if !t.available() {
		t.proxyError(w, r, fmt.Errorf("the official Tour process is not running"))
		return
	}
	t.proxy.ServeHTTP(w, r)
}

func (t *tourProxy) start(ctx context.Context) error {
	if t.check() {
		t.setAvailable(true)
		return nil
	}

	binary, err := findTourBinary()
	if err != nil {
		return fmt.Errorf("%w; run scripts/start.ps1 once to install it", err)
	}

	command := newCommand(ctx, binary, "-http="+t.address, "-openbrowser=false")
	command.Dir = t.repoRoot
	hideCommandWindow(command)
	var tourLog bytes.Buffer
	command.Stdout = &tourLog
	command.Stderr = &tourLog
	if err := command.Start(); err != nil {
		return err
	}
	t.process = command

	deadline := time.Now().Add(30 * time.Second)
	for time.Now().Before(deadline) {
		if t.check() {
			t.setAvailable(true)
			return nil
		}
		if command.ProcessState != nil && command.ProcessState.Exited() {
			break
		}
		time.Sleep(150 * time.Millisecond)
	}

	_ = command.Process.Kill()
	return fmt.Errorf("Tour did not become ready: %s", strings.TrimSpace(tourLog.String()))
}

func (t *tourProxy) close() {
	if t.process != nil && t.process.Process != nil {
		_ = t.process.Process.Kill()
		_, _ = t.process.Process.Wait()
	}
}

func (t *tourProxy) available() bool {
	t.readyLock.RLock()
	defer t.readyLock.RUnlock()
	return t.ready
}

func (t *tourProxy) setAvailable(value bool) {
	t.readyLock.Lock()
	t.ready = value
	t.readyLock.Unlock()
}

func (t *tourProxy) check() bool {
	client := http.Client{Timeout: 750 * time.Millisecond}
	response, err := client.Get(t.target.String() + "/tour/welcome/1")
	if err != nil {
		return false
	}
	_ = response.Body.Close()
	return response.StatusCode >= 200 && response.StatusCode < 500
}

func findTourBinary() (string, error) {
	if configured := strings.TrimSpace(os.Getenv("GO_TOUR_BIN")); configured != "" {
		if _, err := os.Stat(configured); err == nil {
			return configured, nil
		}
	}
	if found, err := exec.LookPath("tour"); err == nil {
		return found, nil
	}

	command := newCommand(context.Background(), "go", "env", "GOPATH")
	output, err := command.Output()
	if err == nil {
		name := "tour"
		if filepath.Separator == '\\' {
			name += ".exe"
		}
		candidate := filepath.Join(strings.TrimSpace(string(output)), "bin", name)
		if _, err := os.Stat(candidate); err == nil {
			return candidate, nil
		}
	}
	return "", fmt.Errorf("Tour of Go is not installed (%s)", tourModule)
}

func (t *tourProxy) modifyResponse(response *http.Response) error {
	response.Header.Del("X-Frame-Options")
	if location := response.Header.Get("Location"); location != "" {
		response.Header.Set("Location", strings.ReplaceAll(location, t.target.String(), ""))
	}
	if response.Request != nil && response.Request.URL.Path == "/tour/script.js" {
		return addTourGoVersion(response)
	}
	if !strings.Contains(response.Header.Get("Content-Type"), "text/html") {
		return nil
	}

	body, err := io.ReadAll(response.Body)
	if err != nil {
		return err
	}
	_ = response.Body.Close()
	html := string(body)
	// Dark is the Tour of go2cs default. The Tour's cookie bootstrap still
	// overrides this when the user has explicitly selected light or auto.
	html = strings.Replace(html, `data-theme="auto"`, `data-theme="dark"`, 1)
	html = strings.Replace(html, "</head>", `<script>
try { if (localStorage.getItem("syntax") === null) localStorage.setItem("syntax", "true"); } catch (_) {}
</script><link rel="stylesheet" href="/bridge.css"></head>`, 1)
	html = strings.Replace(html, "</body>", `<script src="/bridge.js"></script></body>`, 1)
	body = []byte(html)
	response.Body = io.NopCloser(bytes.NewReader(body))
	response.ContentLength = int64(len(body))
	response.Header.Set("Content-Length", fmt.Sprint(len(body)))
	return nil
}

func addTourGoVersion(response *http.Response) error {
	body, err := io.ReadAll(response.Body)
	if err != nil {
		return err
	}
	_ = response.Body.Close()

	const moduleLine = `'module example\n' +`
	versionLine := moduleLine + "\n                    'go " + tourGoVersion + `\n' +`
	body = bytes.Replace(body, []byte(moduleLine), []byte(versionLine), 1)
	response.Body = io.NopCloser(bytes.NewReader(body))
	response.ContentLength = int64(len(body))
	response.Header.Set("Content-Length", fmt.Sprint(len(body)))
	response.Header.Set("Cache-Control", "no-store")
	response.Header.Del("ETag")
	response.Header.Del("Last-Modified")
	return nil
}

func (t *tourProxy) proxyError(w http.ResponseWriter, _ *http.Request, err error) {
	w.Header().Set("Content-Type", "text/html; charset=utf-8")
	w.WriteHeader(http.StatusBadGateway)
	fmt.Fprintf(w, `<!doctype html><html><body style="font:16px system-ui;padding:3rem;background:#071926;color:#d9f5ff">
<h1>The official Tour is offline</h1><p>%s</p><p>Run <code>scripts/start.ps1</code> from <code>src/tour</code>, then reload this page.</p>
</body></html>`, htmlEscape(err.Error()))
}

func htmlEscape(value string) string {
	replacer := strings.NewReplacer("&", "&amp;", "<", "&lt;", ">", "&gt;", `"`, "&#34;", "'", "&#39;")
	return replacer.Replace(value)
}
