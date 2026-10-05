// Package targetlib holds a type whose methods take a parameter named `target`, as testify's
// assert.Assertions does (ErrorAs, ErrorIs), for a struct in ANOTHER package to embed.
package targetlib

// Finder is embedded by the main package.
type Finder struct {
	Name string
}

// Match reports the receiver's name joined to the target, by value.
func (f Finder) Match(target string) string {
	return f.Name + "=>" + target
}

// Retarget rewrites the receiver through a pointer receiver.
func (f *Finder) Retarget(target string) {
	f.Name = target
}
