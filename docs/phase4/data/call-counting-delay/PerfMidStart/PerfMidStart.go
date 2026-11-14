package main

import (
	"bytes"
	"encoding/json"
	"fmt"
	"net/url"
	"regexp"
	"strings"
	"text/template"
)

// Mid-size start-up (a scratch program for the call-counting-delay record, not a suite row): one pass through
// encoding/json, regexp, net/url and text/template, about a hundred packages, then exit. The harness reports the
// wall time; elapsed_ns is a constant so Go and C# outputs match exactly.

type item struct {
	Name  string   `json:"name"`
	Tags  []string `json:"tags"`
	Count int      `json:"count"`
}

func main() {
	var in item
	if err := json.Unmarshal([]byte(`{"name":"go2cs","tags":["a","b"],"count":3}`), &in); err != nil {
		panic(err)
	}

	out, _ := json.Marshal(in)
	re := regexp.MustCompile(`([a-z]+)([0-9]+)`)
	m := re.FindAllStringSubmatch("abc12 def345 x9", -1)
	u, _ := url.Parse("https://example.com/a/b?x=1&y=two#frag")
	var buf bytes.Buffer
	template.Must(template.New("t").Parse("{{.Name}}:{{range .Tags}}[{{.}}]{{end}}={{.Count}}")).Execute(&buf, in)

	sum := len(out) + len(m) + len(u.Query()) + len(buf.String()) + len(strings.Fields(buf.String()))
	fmt.Println("checksum:", sum, string(out), buf.String(), u.Host)
	fmt.Println("elapsed_ns:", 0)
}
