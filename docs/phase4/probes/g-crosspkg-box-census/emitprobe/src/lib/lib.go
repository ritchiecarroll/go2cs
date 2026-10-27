package lib

import "sync"

type Counter struct{ n int }

func (c *Counter) Inc()      { c.n++ }
func (c Counter) Value() int { return c.n }

type Holder struct {
	WG sync.WaitGroup
	N  int
}

var N int           // plain value var
var V Counter       // pointer-receiver methods, not box-only
var S Holder        // struct with a box-only field type
var Arr [3]int      // array
var RW sync.RWMutex // box-only methods: boxed today by the owner arm
var Table = []int{1, 2, 3}

func ReadN() int { return N }
