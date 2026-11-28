// The client's cipher-suite order, as a TLS server sees it in GetConfigForClient. crypto/tls lists AES-GCM ahead of
// ChaCha20-Poly1305 only when it detects AES-GCM hardware (hasAESGCMHardwareSupport: on arm64, internal/cpu's
// ARM64.HasAES and HasPMULL), so the order is that detection made visible to a user program. The handshake is
// abandoned as soon as the hello is read: nothing here needs a certificate.
package main

import (
	"crypto/tls"
	"errors"
	"fmt"
	"net"
)

var errHelloRead = errors.New("hello read")

func main() {
	clientEnd, serverEnd := net.Pipe()
	seen := make(chan []uint16, 1)

	server := tls.Server(serverEnd, &tls.Config{
		GetConfigForClient: func(hello *tls.ClientHelloInfo) (*tls.Config, error) {
			seen <- hello.CipherSuites
			return nil, errHelloRead
		},
	})

	serverDone := make(chan error, 1)
	go func() {
		serverDone <- server.Handshake()
		serverEnd.Close()
	}()

	clientDone := make(chan error, 1)
	go func() {
		clientDone <- tls.Client(clientEnd, &tls.Config{ServerName: "example.test"}).Handshake()
		clientEnd.Close()
	}()

	suites := <-seen

	firstAES, firstChaCha := -1, -1
	for i, id := range suites {
		name := tls.CipherSuiteName(id)
		fmt.Printf("%2d %s\n", i, name)

		switch id {
		case tls.TLS_AES_128_GCM_SHA256, tls.TLS_AES_256_GCM_SHA384,
			tls.TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256, tls.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
			tls.TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384, tls.TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384:
			if firstAES < 0 {
				firstAES = i
			}
		case tls.TLS_CHACHA20_POLY1305_SHA256,
			tls.TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256, tls.TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256:
			if firstChaCha < 0 {
				firstChaCha = i
			}
		}
	}

	fmt.Println("suites offered:", len(suites))
	fmt.Println("AES-GCM preferred (hardware AES-GCM detected):", firstAES >= 0 && firstAES < firstChaCha)

	serverErr := <-serverDone
	fmt.Println("server stopped at the hello:", errors.Is(serverErr, errHelloRead))
	fmt.Println("client handshake failed:", <-clientDone != nil)
}
