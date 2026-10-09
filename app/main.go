package main

import (
	"fmt"

	"github.com/google/uuid"
)

func main() {
	fmt.Println(uuid.NewSHA1(uuid.NameSpaceDNS, []byte("go2cs.net")))
	fmt.Println(uuid.NewMD5(uuid.NameSpaceDNS, []byte("go2cs.net")))
	u, err := uuid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479")
	fmt.Println(u.Version(), u.Variant(), err)
}
