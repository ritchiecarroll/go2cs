// ChanElemDims is increment C's row: a channel VALUE's element array length. A channel's element is
// not a present value that can be measured (its buffer is not peekable), so the length must ride on
// the channel value the way its direction already does, stamped where the channel is made. Parked
// red after increment B by design; ChanOf's CONSTRUCTED route is covered in CanonicalTypeIdentity.
package main

import (
	"fmt"
	"reflect"
)

func main() {
	c := make(chan [3]int)

	// VALUE row -- RED BY BOUNDARY after increment B, and the reason travels with the assertion: a
	// channel value's element length is not measurable (its buffer is not peekable), so B does not
	// seed it. Increment C carries it on the channel value the way direction already rides
	// (channel<T>.m_direction, stamped where the channel is made). The cost C must justify is the
	// +8 B per header the value-cargo arm costs if generalized to slices -- DESIGN-descriptor-cargo.md
	// section 12.2. A stated expected-fail, not a mysterious one.
	fmt.Printf("value row [red by boundary until increment C: not measurable from a channel value] %%T=%T String()=%s Elem().Len()=%d\n",
		c, reflect.TypeOf(c).String(), reflect.TypeOf(c).Elem().Len())

	// CONSTRUCTED row -- fixed by increment B: ChanOf passes the element's cargo unshifted, so the
	// constructed descriptor's OWN properties hold. Its IDENTITY with TypeOf(c) does not, and cannot until
	// C seeds the value side: an identity row against a boundary side is a boundary row (section 12.4's
	// prediction was wrong on exactly this, and says so). That identity is C's row.
	ct := reflect.ChanOf(reflect.BothDir, reflect.ArrayOf(3, reflect.TypeOf(0)))
	name := ct.String()
	n := ct.Elem().Len()
	// Println, not Printf: a Printf FORMAT containing a comma inside parentheses is mangled by the converter
	// (the trigger the routed item names); Println with the same text converts.
	fmt.Println("constructed row: ChanOf(BothDir, ArrayOf(3,int)) String()="+name+" Elem().Len()=", n)
}
