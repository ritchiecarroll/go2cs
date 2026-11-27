// Package pb has the shape protoc-gen-go-grpc generates: a service interface sealed by an UNEXPORTED
// marker method, and an exported struct that supplies the marker (and a default for every method) for
// a server to embed. Another package's server meets the interface only through the embedded struct.
package pb

// GreeterServer is sealed: only a type embedding UnimplementedGreeterServer can implement it.
type GreeterServer interface {
	SayHello(name string) string
	mustEmbedUnimplementedGreeterServer()
}

// UnimplementedGreeterServer must be embedded by every GreeterServer.
type UnimplementedGreeterServer struct{}

func (UnimplementedGreeterServer) SayHello(name string) string {
	return "unimplemented SayHello(" + name + ")"
}

func (UnimplementedGreeterServer) mustEmbedUnimplementedGreeterServer() {}

var servers []GreeterServer

// RegisterGreeterServer records a server; it calls the marker, as the sealed method is otherwise never called.
func RegisterGreeterServer(s GreeterServer) {
	s.mustEmbedUnimplementedGreeterServer()
	servers = append(servers, s)
}

// Serve sends name to every registered server.
func Serve(name string) []string {
	var replies []string

	for _, s := range servers {
		replies = append(replies, s.SayHello(name))
	}

	return replies
}
