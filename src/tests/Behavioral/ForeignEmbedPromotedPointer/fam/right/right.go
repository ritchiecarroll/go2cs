// Package right is a sibling of package left: both sit one namespace below the module's.
package right

// Whoer is satisfied by left's struct through promotion.
type Whoer interface {
	Who() string
}

// T is embedded by value in package left.
type T struct{}

// Who has a pointer receiver.
func (*T) Who() string { return "right.T" }
