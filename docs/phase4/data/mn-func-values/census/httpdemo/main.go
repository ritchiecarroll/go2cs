// httpdemo: the census's control program. Three handlers reach net/http's HandlerFunc.ServeHTTP: a literal made in
// main, a named function converted in main, and a literal made by a middleware factory that never calls it.
package main

import (
	"fmt"
	"net/http"
	"time"
)

func logging(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) { next.ServeHTTP(w, r) })
}

func slow(w http.ResponseWriter, r *http.Request) {
	time.Sleep(time.Millisecond)
	fmt.Fprintln(w, "slow")
}

func main() {
	mux := http.NewServeMux()
	mux.HandleFunc("/slow", slow)
	mux.HandleFunc("/lit", func(w http.ResponseWriter, r *http.Request) {
		<-time.After(time.Millisecond)
		fmt.Fprintln(w, "lit")
	})
	fmt.Println(http.ListenAndServe("127.0.0.1:0", logging(mux)))
}
