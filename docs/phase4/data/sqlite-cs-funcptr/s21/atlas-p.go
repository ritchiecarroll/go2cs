package atlasprobe

import (
	_ "golang.org/x/crypto/chacha20poly1305"
	_ "golang.org/x/crypto/blake2b"
	_ "golang.org/x/crypto/sha3"
	_ "golang.org/x/crypto/argon2"
	_ "golang.org/x/crypto/curve25519"
	_ "github.com/klauspost/compress/zstd"
	_ "github.com/klauspost/compress/s2"
)
