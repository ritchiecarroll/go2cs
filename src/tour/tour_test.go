package main

import (
	"io"
	"net/http"
	"net/url"
	"os"
	"regexp"
	"strings"
	"testing"
)

// The Tour install is pinned in three places; they move together with the corpus's Go release.
func TestTourInstallPinsAgree(t *testing.T) {
	pin := tourModule[strings.Index(tourModule, "@")+1:]
	if pin == "latest" {
		t.Fatalf("tourModule is %q: @latest follows x/website onto a newer Go than the converter's", tourModule)
	}
	for file, pattern := range map[string]string{
		"scripts/start.ps1": `(?m)^\$TourVersion = "([^"]+)"`,
		"scripts/start.sh":  `(?m)^tour_version=(\S+)`,
	} {
		text, err := os.ReadFile(file)
		if err != nil {
			t.Fatal(err)
		}
		match := regexp.MustCompile(pattern).FindSubmatch(text)
		if match == nil {
			t.Fatalf("%s: no tour version variable found", file)
		}
		if got := string(match[1]); got != pin {
			t.Errorf("%s pins %q, tourModule pins %q", file, got, pin)
		}
		if strings.Contains(string(text), "tour@latest") {
			t.Errorf("%s still installs tour@latest", file)
		}
	}
}

func TestModifyResponseDefaultsTourToDarkWithSyntax(t *testing.T) {
	body := `<!doctype html><html data-theme="auto"><head></head><body>Tour</body></html>`
	response := &http.Response{
		Header: http.Header{"Content-Type": []string{"text/html; charset=utf-8"}},
		Body:   io.NopCloser(strings.NewReader(body)),
	}

	proxy := newTourProxy("127.0.0.1:3999", t.TempDir())
	if err := proxy.modifyResponse(response); err != nil {
		t.Fatal(err)
	}
	updated, err := io.ReadAll(response.Body)
	if err != nil {
		t.Fatal(err)
	}
	text := string(updated)
	for _, want := range []string{
		`data-theme="dark"`,
		`localStorage.getItem("syntax") === null`,
		`localStorage.setItem("syntax", "true")`,
		`href="/bridge.css"`,
		`src="/bridge.js"`,
	} {
		if !strings.Contains(text, want) {
			t.Errorf("modified Tour response missing %q:\n%s", want, text)
		}
	}
}

func TestNewSolutionsLink(t *testing.T) {
	if got := newSolutionsLink(defaultSolutionsURL, defaultSolutionsText); got.URL != defaultSolutionsURL || got.Text != defaultSolutionsText {
		t.Fatalf("default link = %+v", got)
	}
	if got := newSolutionsLink("https://example.com/answers", "  "); got.Text != defaultSolutionsText {
		t.Fatalf("blank text = %+v, want the default text", got)
	}
	for _, url := range []string{"", "   ", "javascript:alert(1)", "example.com/answers"} {
		if got := newSolutionsLink(url, defaultSolutionsText); got != (solutionsLink{}) {
			t.Errorf("newSolutionsLink(%q) = %+v, want no link", url, got)
		}
	}
}

func TestModifyResponseAddsGoVersionToTourScript(t *testing.T) {
	body := "code += '-- go.mod --\\n' +\n                    'module example\\n' +\n                    'require golang.org/x/tour v0.1.0\\n' +"
	response := &http.Response{
		Header:  http.Header{"Content-Type": []string{"application/javascript"}},
		Body:    io.NopCloser(strings.NewReader(body)),
		Request: &http.Request{URL: &url.URL{Path: "/tour/script.js"}},
	}

	proxy := newTourProxy("127.0.0.1:3999", t.TempDir())
	if err := proxy.modifyResponse(response); err != nil {
		t.Fatal(err)
	}
	updated, err := io.ReadAll(response.Body)
	if err != nil {
		t.Fatal(err)
	}
	text := string(updated)
	if !strings.Contains(text, "'module example\\n' +\n                    'go 1.23.1\\n' +") {
		t.Fatalf("modified Tour script missing Go language version:\n%s", text)
	}
	if got := response.Header.Get("Cache-Control"); got != "no-store" {
		t.Errorf("Cache-Control = %q, want no-store", got)
	}
}
