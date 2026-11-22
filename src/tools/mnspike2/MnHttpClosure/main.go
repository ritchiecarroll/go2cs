// MnHttpClosure: stage (c)'s closure program. A loopback net/http server and its client in one process: the server's
// handlers park (a channel, a timer), the client parks on the network, and main waits for both. It ends on its own.
package main

import (
	"fmt"
	"io"
	"net"
	"net/http"
	"sort"
	"strings"
	"sync"
	"time"
)

type counter struct {
	mu   sync.Mutex
	hits map[string]int
}

func (c *counter) wrap(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		c.mu.Lock()
		c.hits[r.URL.Path]++
		c.mu.Unlock()
		next.ServeHTTP(w, r)
	})
}

func slow(w http.ResponseWriter, r *http.Request) {
	<-time.After(time.Millisecond)
	fmt.Fprintln(w, "slow")
}

func main() {
	ln, err := net.Listen("tcp", "127.0.0.1:0")
	if err != nil {
		fmt.Println("listen:", err)
		return
	}

	ready := make(chan string, 1)
	mux := http.NewServeMux()
	mux.HandleFunc("/slow", slow)
	mux.HandleFunc("/chan", func(w http.ResponseWriter, r *http.Request) {
		ready <- r.URL.Query().Get("v")
		fmt.Fprintln(w, "chan", <-ready)
	})

	c := &counter{hits: map[string]int{}}
	srv := &http.Server{Handler: c.wrap(mux)}
	done := make(chan error, 1)
	go func() { done <- srv.Serve(ln) }()

	base := "http://" + ln.Addr().String()
	var wg sync.WaitGroup
	results := make([]string, 6)

	for i := range results {
		wg.Add(1)
		go func(i int) {
			defer wg.Done()
			path := "/slow"
			if i%2 == 1 {
				path = fmt.Sprintf("/chan?v=%d", i)
			}
			resp, err := http.Get(base + path)
			if err != nil {
				results[i] = "error: " + err.Error()
				return
			}
			defer resp.Body.Close()
			body, _ := io.ReadAll(resp.Body)
			results[i] = fmt.Sprintf("%d %s", resp.StatusCode, strings.TrimSpace(string(body)))
		}(i)
	}

	wg.Wait()

	for i, r := range results {
		fmt.Println(i, r)
	}

	srv.Close()
	fmt.Println("serve:", <-done)

	paths := make([]string, 0, len(c.hits))
	for p := range c.hits {
		paths = append(paths, p)
	}
	sort.Strings(paths)
	for _, p := range paths {
		fmt.Println(p, c.hits[p])
	}
}
