// Probe (not a seat): name resolution through the platform resolver for the loopback name.
// On darwin this is net's cgoLookupHostIP over libc getaddrinfo; no external network.
package main

import (
	"fmt"
	"net"
	"sort"
)

func main() {
	ips, err := net.LookupIP("localhost")
	if err != nil {
		fmt.Printf("LookupIP error: %T %v\n", err, err)
	} else {
		var s []string
		for _, ip := range ips {
			s = append(s, ip.String())
		}
		sort.Strings(s)
		fmt.Printf("LookupIP: %v\n", s)
	}

	hosts, err := net.LookupHost("localhost")
	if err != nil {
		fmt.Printf("LookupHost error: %T %v\n", err, err)
	} else {
		sort.Strings(hosts)
		fmt.Printf("LookupHost: %v\n", hosts)
	}
}
