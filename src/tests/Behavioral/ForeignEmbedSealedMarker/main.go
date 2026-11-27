// Program ForeignEmbedSealedMarker is the sealed-marker near miss of go2cs's GO2CS2016 refusal: a
// server meets another package's interface's UNEXPORTED marker method only through an embedded
// struct, the shape of every generated gRPC server. The marker returns nothing and the server
// declares no method of that name, so the converted adapter's stub is a no-op exactly as the Go
// marker is, and the conversion is not refused.
package main

import (
	"fmt"

	"ForeignEmbedSealedMarker/pb"
)

// server overrides SayHello on a pointer receiver.
type server struct {
	pb.UnimplementedGreeterServer
	greeting string
}

func (s *server) SayHello(name string) string { return s.greeting + ", " + name }

// quiet keeps the embedded default.
type quiet struct{ pb.UnimplementedGreeterServer }

func main() {
	pb.RegisterGreeterServer(&server{greeting: "hello"})
	pb.RegisterGreeterServer(quiet{})
	pb.RegisterGreeterServer(&quiet{})

	var s pb.GreeterServer = &server{greeting: "hi"}
	pb.RegisterGreeterServer(s)

	for _, reply := range pb.Serve("go") {
		fmt.Println(reply)
	}
}
