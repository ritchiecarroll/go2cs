// typelib supplies named types of ANOTHER package, each implementing the importing package's interface: five by
// value and one by pointer.
package typelib

// Count is a named int.
type Count int

// Name implements the importing package's interface by value.
func (c Count) Name() string { return "count" }

// Label is a named string.
type Label string

// Name implements the importing package's interface by value.
func (l Label) Name() string { return "label" }

// List is a named slice.
type List []int

// Name implements the importing package's interface by value.
func (l List) Name() string { return "list" }

// Dict is a named map.
type Dict map[string]int

// Name implements the importing package's interface by value.
func (d Dict) Name() string { return "dict" }

// Mark is a struct.
type Mark struct {
	Tag string
}

// Name implements the importing package's interface by value.
func (m Mark) Name() string { return "mark" }

// Box is a struct that implements the interface by pointer.
type Box struct {
	Tag string
}

// Name implements the importing package's interface by pointer.
func (b *Box) Name() string { return "box" }

// NewList constructs a List.
func NewList() List { return List{3, 1, 2} }

// NewDict constructs a Dict of one entry.
func NewDict() Dict { return Dict{"a": 1} }

// NewMark constructs a Mark.
func NewMark() Mark { return Mark{Tag: "x"} }

// NewBox constructs a Box, by pointer.
func NewBox() *Box { return &Box{Tag: "y"} }
