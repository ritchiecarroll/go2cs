package b

import "go2cs/TestNeedsTransitiveApiRef/a"

// Client is what b's test calls. Its Lookup takes two strings.
type Client struct{}

func (c *Client) Lookup(path, vers string) (lines []string, err error) {
	return []string{path + "@" + vers}, nil
}

// Server has a method of the SAME NAME and the SAME ARITY whose signature names a.Version, so a's
// assembly is part of b's API even though no call in b's test touches it.
type Server struct{}

func (s *Server) Lookup(path string, m a.Version) (int64, error) {
	return int64(len(path) + len(m.Path)), nil
}
