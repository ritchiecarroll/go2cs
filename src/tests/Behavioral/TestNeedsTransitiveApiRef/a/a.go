package a

// Version is a type that only package b's API names; b's test never imports this package.
type Version struct {
	Path    string
	Version string
}
