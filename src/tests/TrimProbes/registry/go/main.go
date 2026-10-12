// Trim-stage registry probe (the per-OS marker's one lost type, array<runtime.MemStats_BySize>). Two parts:
//  1. expvar publishes runtime.MemStats as JSON. Marshal only READS the array (Index), which needs no registered
//     operations, so this part is the realistic consumer's control: it should match Go on every tree.
//  2. A reflect operation that CREATES a box over the array -- reflect.New and Value.Addr -- which is where golib
//     consults the escape registry. The array's type is found by reflection from the MemStats value, so this package
//     never names the array type itself (it would then register it on its own and hide the gap).
// Under Native AOT with a full trim, part 2 needs the array's operations registered by some package.
// Prints only shape, never a value that varies.
package main

import (
	"encoding/json"
	"expvar"
	"fmt"
	"reflect"
	"runtime"
)

func main() {
	v := expvar.Get("memstats")
	if v == nil {
		fmt.Println("memstats: not published")
		return
	}

	var m map[string]any
	if err := json.Unmarshal([]byte(v.String()), &m); err != nil {
		fmt.Println("memstats: unmarshal:", err)
		return
	}

	bySize, ok := m["BySize"].([]any)
	fmt.Println("BySize is an array:", ok)
	fmt.Println("BySize entries:", len(bySize))

	if len(bySize) > 0 {
		entry, _ := bySize[0].(map[string]any)
		_, size := entry["Size"]
		_, mallocs := entry["Mallocs"]
		_, frees := entry["Frees"]
		fmt.Println("entry fields:", len(entry), size, mallocs, frees)
	}

	_, numGC := m["NumGC"]
	fmt.Println("NumGC present:", numGC)

	var ms runtime.MemStats
	field, found := reflect.TypeOf(ms).FieldByName("BySize")
	fmt.Println("BySize field found:", found, field.Type.Kind(), field.Type.Len())

	created := reflect.New(field.Type)
	fmt.Println("reflect.New:", created.Kind(), created.Elem().Kind(), created.Elem().Len(), created.Elem().Index(0).Kind())

	addr := reflect.ValueOf(&ms).Elem().FieldByName("BySize").Addr()
	fmt.Println("Addr:", addr.Kind(), addr.Elem().Len())
}
