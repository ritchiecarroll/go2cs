// P521CurveBytes prints P-521 results that depend on every field multiply and square: ECDH public keys
// and shared secrets, ECDSA public keys from fixed private keys, verification of a fixed signature and
// of tampered copies, and crypto/elliptic point arithmetic. The converted field arithmetic must give
// Go's bytes exactly.
package main

import (
	"crypto/ecdh"
	"crypto/ecdsa"
	"crypto/elliptic"
	"crypto/sha512"
	"encoding/hex"
	"fmt"
	"math/big"
)

// key derives a 66-byte P-521 scalar below the group order from a label.
func key(label string) []byte {
	d := sha512.Sum512([]byte(label))
	raw := append([]byte{0x01}, d[:]...)
	return append(raw, 0x5a)
}

const fixedSig = "308187024127f6c8c09b10eee4ae0d773c088dc6b1503ee66d8b5e426c5cc703b53d31cbc2fcda3efc690f13304cb216d3f8adc1df4e5be19cd1a6d4706a93735982553bd8bb02420177558dbd35bde68d14e5505c488ce0450326714df7e6605000e34079a5e19bf261da154433259e5769e646b4b5aa543447658880a30915770eb5868295edf33a74"

func main() {
	curve := ecdh.P521()

	for i := 0; i < 4; i++ {
		a, err := curve.NewPrivateKey(key(fmt.Sprintf("go2cs P-521 ECDH a %d", i)))
		if err != nil {
			panic(err)
		}
		b, err := curve.NewPrivateKey(key(fmt.Sprintf("go2cs P-521 ECDH b %d", i)))
		if err != nil {
			panic(err)
		}
		ab, err := a.ECDH(b.PublicKey())
		if err != nil {
			panic(err)
		}
		ba, err := b.ECDH(a.PublicKey())
		if err != nil {
			panic(err)
		}
		fmt.Printf("ecdh %d pub %x\n", i, a.PublicKey().Bytes())
		fmt.Printf("ecdh %d shared %x agree %v\n", i, ab, string(ab) == string(ba))
	}

	priv, err := ecdsa.ParseRawPrivateKey(elliptic.P521(), key("go2cs P-521 behavioral key 0"))
	if err != nil {
		panic(err)
	}
	pub, err := priv.PublicKey.Bytes()
	if err != nil {
		panic(err)
	}
	fmt.Printf("ecdsa pub %x\n", pub)

	h := sha512.Sum512([]byte("go2cs P-521 behavioral message"))
	sig, _ := hex.DecodeString(fixedSig)
	fmt.Println("ecdsa fixed signature verifies:", ecdsa.VerifyASN1(&priv.PublicKey, h[:], sig))

	tampered := append([]byte(nil), sig...)
	tampered[len(tampered)-1] ^= 1
	fmt.Println("ecdsa tampered signature verifies:", ecdsa.VerifyASN1(&priv.PublicKey, h[:], tampered))

	h[0] ^= 1
	fmt.Println("ecdsa other message verifies:", ecdsa.VerifyASN1(&priv.PublicKey, h[:], sig))

	p := elliptic.P521()
	for i, k := range []int64{1, 2, 3, 0x1ff, 1 << 40} {
		x, y := p.ScalarBaseMult(big.NewInt(k).Bytes())
		x2, y2 := p.ScalarMult(x, y, key(fmt.Sprintf("go2cs P-521 scalar %d", i)))
		fmt.Printf("elliptic %d base %x %x\n", i, x, y)
		fmt.Printf("elliptic %d mult %x %x on curve %v\n", i, x2, y2, p.IsOnCurve(x2, y2))
	}
}
