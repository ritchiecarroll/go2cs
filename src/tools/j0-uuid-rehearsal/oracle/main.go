// The J0 oracle: google/uuid v1.6.0 run by Go itself. j0-consume.ps1 runs this beside the C# consumer
// (consumer/Program.cs), which prints the SAME keys in the SAME order from the converted package, and the two
// outputs are compared line by line.
//
// Deterministic keys (v3/v5 hashes, the namespace constants, Parse over every accepted spelling, the text and
// binary encodings, a parse error's text) must match BYTE FOR BYTE. Time- and randomness-based UUIDs (v1, v4,
// v6, v7) cannot, so their keys print a STRUCTURAL reading instead: version, variant, canonical length, and
// that the value survives a String/Parse round trip.
package main

import (
	"encoding/hex"
	"fmt"

	"github.com/google/uuid"
)

func show(key string, u uuid.UUID) {
	fmt.Printf("%s=%s version=%s variant=%s\n", key, u.String(), u.Version().String(), u.Variant().String())
}

func structural(key string, u uuid.UUID, err error) {
	if err != nil {
		fmt.Printf("%s=error %v\n", key, err)
		return
	}
	s := u.String()
	back, perr := uuid.Parse(s)
	fmt.Printf("%s=version=%s variant=%s len=%d roundtrip=%v\n", key, u.Version().String(), u.Variant().String(), len(s), perr == nil && back == u)
}

func main() {
	show("namespace.dns", uuid.NameSpaceDNS)
	show("namespace.url", uuid.NameSpaceURL)
	show("namespace.oid", uuid.NameSpaceOID)
	show("namespace.x500", uuid.NameSpaceX500)

	show("v3.dns.example.com", uuid.NewMD5(uuid.NameSpaceDNS, []byte("example.com")))
	show("v3.url.go2cs", uuid.NewMD5(uuid.NameSpaceURL, []byte("https://go2cs.net")))
	show("v5.dns.example.com", uuid.NewSHA1(uuid.NameSpaceDNS, []byte("example.com")))
	show("v5.url.go2cs", uuid.NewSHA1(uuid.NameSpaceURL, []byte("https://go2cs.net")))
	show("v5.oid.empty", uuid.NewSHA1(uuid.NameSpaceOID, []byte("")))
	show("v5.x500.unicode", uuid.NewSHA1(uuid.NameSpaceX500, []byte("CN=Grüße,O=go2cs")))

	for _, in := range []string{
		"f47ac10b-58cc-0372-8567-0e02b2c3d479",
		"urn:uuid:f47ac10b-58cc-4372-a567-0e02b2c3d479",
		"{f47ac10b-58cc-4372-a567-0e02b2c3d479}",
		"f47ac10b58cc4372a5670e02b2c3d479",
		"F47AC10B-58CC-4372-A567-0E02B2C3D479",
		"not-a-uuid",
		"f47ac10b-58cc-4372-a567-0e02b2c3d47",
		"f47ac10b-58cc-4372-a567+0e02b2c3d479",
	} {
		u, err := uuid.Parse(in)
		if err != nil {
			fmt.Printf("parse %q=error %v\n", in, err)
			continue
		}
		show(fmt.Sprintf("parse %q", in), u)
	}

	u := uuid.MustParse("f47ac10b-58cc-4372-a567-0e02b2c3d479")
	fmt.Printf("urn=%s\n", u.URN())
	text, _ := u.MarshalText()
	fmt.Printf("marshaltext=%s\n", text)
	var back uuid.UUID
	err := back.UnmarshalText(text)
	fmt.Printf("unmarshaltext=%s err=%v equal=%v\n", back.String(), err, back == u)
	bin, _ := u.MarshalBinary()
	fmt.Printf("marshalbinary=%s\n", hex.EncodeToString(bin))
	fmt.Printf("validate.good=%v\n", uuid.Validate("f47ac10b-58cc-4372-a567-0e02b2c3d479"))
	fmt.Printf("validate.bad=%v\n", uuid.Validate("f47ac10b-58cc-4372-a567-0e02b2c3d47x"))

	v1, err1 := uuid.NewUUID()
	structural("v1", v1, err1)
	structural("v4", uuid.New(), nil)
	v6, err6 := uuid.NewV6()
	structural("v6", v6, err6)
	v7, err7 := uuid.NewV7()
	structural("v7", v7, err7)
}
