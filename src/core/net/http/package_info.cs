// go2cs code converter defines `global using` statements here for imported type
// aliases as package references are encountered via `import' statements. Exported
// type aliases that need a `global using` declaration will be loaded from the
// referenced package by parsing its 'package_info.cs' source file and reading its
// defined `GoTypeAlias` attributes.

// Package name separator "dot" used in imported type aliases is extended Unicode
// character '\uA4F8' which is a valid character in a C# identifier name. This is
// used to simulate Go's package level type aliases since C# does not yet support
// importing type aliases at a namespace level.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.net.http_package;

// For encountered type alias declarations, e.g., `type Table = map[string]int`,
// go2cs code converter will generate a `global using` statement for the alias in
// the converted source, e.g.: `global using Table = go.map<go.@string, nint>;`.
// Although scope of `global using` is available to all files in the project, all
// converted Go code for the project targets the same package, so `global using`
// statements will effectively have package level scope.

// Additionally, `GoTypeAlias` attributes will be generated here for exported type
// aliases. This allows the type alias to be imported and used from other packages
// when referenced.

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b42617365436f6e74657874282920636f6e746578742e436f6e746578747d", "http2ConfigureServer_baseContexter")]
[assembly: GoDynamicTypeLift("696e746572666163657b43616e63656c52657175657374282a6e65742f687474702e52657175657374297d", "setRequestCancel_canceler")]
[assembly: GoDynamicTypeLift("696e746572666163657b436c6f736549646c65436f6e6e656374696f6e7328297d", "CloseIdleConnections_closeIdler")]
[assembly: GoDynamicTypeLift("696e746572666163657b436c6f736557726974652829206572726f727d", "CloseWrite_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b456e61626c6546756c6c4475706c65782829206572726f727d", "EnableFullDuplex_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b466c7573684572726f722829206572726f727d", "Flush_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b497348545450324e6f436163686564436f6e6e4572726f7228297d", "http2isNoCachedConnError_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b53657452656164446561646c696e652874696d652e54696d6529206572726f727d", "SetReadDeadline_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b5365745772697465446561646c696e652874696d652e54696d6529206572726f727d", "SetWriteDeadline_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b556e656e637279707465644e6574436f6e6e2829206e65742e436f6e6e7d", "http2unencryptedNetConnFromTLSConn_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b646f4b656570416c69766573282920626f6f6c7d", "http2h1ServerKeepAlivesDisabled_I")]
[assembly: GoDynamicTypeLift("696e746572666163657b72657175657374546f6f4c6172676528297d", "Read_requestTooLarger")]
[assembly: GoDynamicTypeLift("6e65742f687474702e49", "http2h1ServerKeepAlivesDisabled_I")]
[assembly: GoDynamicTypeLift("6e65742f687474702e62617365436f6e746578746572", "http2ConfigureServer_baseContexter")]
[assembly: GoDynamicTypeLift("6e65742f687474702e63616e63656c6572", "setRequestCancel_canceler")]
[assembly: GoDynamicTypeLift("6e65742f687474702e636c6f736549646c6572", "CloseIdleConnections_closeIdler")]
[assembly: GoDynamicTypeLift("6e65742f687474702e72657175657374546f6f4c6172676572", "Read_requestTooLarger")]
[assembly: GoDynamicTypeLift("7374727563747b696f2e5265616465723b20696f2e577269746572546f7d", "rᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<ClientConn, RoundTripper>(Pointer = true)]
[assembly: GoImplement<Dir, FileSystem>(Pointer = true)]
[assembly: GoImplement<Dir, FileSystem>]
[assembly: GoImplement<File, io_package.ReadSeeker>]
[assembly: GoImplement<HandlerFunc, Handler>]
[assembly: GoImplement<MaxBytesError, error>(Pointer = true)]
[assembly: GoImplement<ProtocolError, error>(Pointer = true)]
[assembly: GoImplement<ResponseController, Hijacker>(Pointer = true)]
[assembly: GoImplement<ResponseWriter, io_package.Writer>]
[assembly: GoImplement<ServeMux, Handler>(Pointer = true)]
[assembly: GoImplement<Transport, RoundTripper>(Pointer = true)]
[assembly: GoImplement<Write_r1, io_package.Closer>(Promoted = true)]
[assembly: GoImplement<Write_r1, io_package.ReadCloser>]
[assembly: GoImplement<Write_r1, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<body, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<bodyEOFSignal, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<bodyEOFSignal, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bodyLocked, io_package.Reader>]
[assembly: GoImplement<bufioFlushWriter, io_package.Writer>]
[assembly: GoImplement<bufio_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bufio_package.Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<byteReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<cancelTimerBody, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<checkConnErrorWriter, io_package.Writer>]
[assembly: GoImplement<chunkWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<connReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<countingWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<dirEntryDirs, anyDirs>]
[assembly: GoImplement<eofReader, go.compress.flate_package.Reader>]
[assembly: GoImplement<errorReader, io_package.Reader>]
[assembly: GoImplement<exactSig, sniffSig>(Pointer = true)]
[assembly: GoImplement<expectContinueReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<fakeLocker, sync_package.Locker>]
[assembly: GoImplement<fileHandler, Handler>(Pointer = true)]
[assembly: GoImplement<fileInfoDirs, anyDirs>]
[assembly: GoImplement<fileTransport, RoundTripper>]
[assembly: GoImplement<finishAsyncByteRead, io_package.Reader>]
[assembly: GoImplement<globalOptionsHandler, Handler>]
[assembly: GoImplement<go.net.http.internal_package.FlushAfterChunkWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<gzipReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<htmlSig, sniffSig>]
[assembly: GoImplement<http1ClientConn, RoundTripper>]
[assembly: GoImplement<http1ClientConn, genericClientConn>]
[assembly: GoImplement<http2ConnectionError, error>]
[assembly: GoImplement<http2ContinuationFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2ContinuationFrame, http2headersOrContinuation>(Pointer = true)]
[assembly: GoImplement<http2DataFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2GoAwayError, error>]
[assembly: GoImplement<http2GoAwayFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2HeadersFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2HeadersFrame, http2headersOrContinuation>(Pointer = true)]
[assembly: GoImplement<http2MetaHeadersFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2PingFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2PriorityFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2PushPromiseFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2RSTStreamFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2SettingsFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2StreamError, error>]
[assembly: GoImplement<http2StreamError, http2writeFramer>]
[assembly: GoImplement<http2Transport, RoundTripper>(Pointer = true)]
[assembly: GoImplement<http2Transport, h2Transport>(Pointer = true)]
[assembly: GoImplement<http2UnknownFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2WindowUpdateFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2bufferedWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<http2bufferedWriterTimeoutWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<http2chunkWriter, io_package.Writer>]
[assembly: GoImplement<http2clientConnPool, http2ClientConnPool>(Pointer = true)]
[assembly: GoImplement<http2clientConnPool, http2clientConnPoolIdleCloser>(Pointer = true)]
[assembly: GoImplement<http2connError, error>]
[assembly: GoImplement<http2dataBuffer, http2pipeBuffer>(Pointer = true)]
[assembly: GoImplement<http2duplicatePseudoHeaderError, error>]
[assembly: GoImplement<http2eofReader, go.compress.flate_package.Reader>]
[assembly: GoImplement<http2erringRoundTripper, RoundTripper>]
[assembly: GoImplement<http2flushFrameWriter, http2writeFramer>]
[assembly: GoImplement<http2goAwayFlowError, error>]
[assembly: GoImplement<http2gzipReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<http2handlerPanicRST, http2writeFramer>]
[assembly: GoImplement<http2headerFieldNameError, error>]
[assembly: GoImplement<http2headerFieldValueError, error>]
[assembly: GoImplement<http2httpError, error>(Pointer = true)]
[assembly: GoImplement<http2missingBody, io_package.ReadCloser>]
[assembly: GoImplement<http2netHTTPClientConn, RoundTripper>]
[assembly: GoImplement<http2noBodyReader, io_package.ReadCloser>]
[assembly: GoImplement<http2noCachedConnError, error>]
[assembly: GoImplement<http2noDialClientConnPool, http2clientConnPoolIdleCloser>]
[assembly: GoImplement<http2noDialH2RoundTripper, RoundTripper>]
[assembly: GoImplement<http2priorityWriteSchedulerRFC7540, http2WriteScheduler>(Pointer = true)]
[assembly: GoImplement<http2priorityWriteSchedulerRFC9218, http2WriteScheduler>(Pointer = true)]
[assembly: GoImplement<http2pseudoHeaderError, error>]
[assembly: GoImplement<http2randomWriteScheduler, http2WriteScheduler>(Pointer = true)]
[assembly: GoImplement<http2requestBody, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<http2responseWriter, CloseNotifier>(Pointer = true)]
[assembly: GoImplement<http2responseWriter, Flusher>(Pointer = true)]
[assembly: GoImplement<http2responseWriter, Pusher>(Pointer = true)]
[assembly: GoImplement<http2responseWriter, ResponseWriter>(Pointer = true)]
[assembly: GoImplement<http2responseWriter, http2stringWriter>(Pointer = true)]
[assembly: GoImplement<http2roundRobinWriteScheduler, http2WriteScheduler>(Pointer = true)]
[assembly: GoImplement<http2serverConn, http2writeContext>(Pointer = true)]
[assembly: GoImplement<http2sortPriorityNodeSiblingsRFC7540, sort_package.Interface>]
[assembly: GoImplement<http2sorter, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<http2stickyErrWriter, io_package.Writer>]
[assembly: GoImplement<http2transportResponseBody, io_package.ReadCloser>]
[assembly: GoImplement<http2unencryptedTransport, RoundTripper>(Pointer = true)]
[assembly: GoImplement<http2write100ContinueHeadersFrame, http2writeFramer>]
[assembly: GoImplement<http2writeData, http2writeFramer>(Pointer = true)]
[assembly: GoImplement<http2writeGoAway, http2writeFramer>(Pointer = true)]
[assembly: GoImplement<http2writePing, http2writeFramer>(Pointer = true)]
[assembly: GoImplement<http2writePingAck, http2writeFramer>]
[assembly: GoImplement<http2writePushPromise, http2writeFramer>(Pointer = true)]
[assembly: GoImplement<http2writeResHeaders, http2writeFramer>(Pointer = true)]
[assembly: GoImplement<http2writeSettings, http2writeFramer>]
[assembly: GoImplement<http2writeSettingsAck, http2writeFramer>]
[assembly: GoImplement<http2writeWindowUpdate, http2writeFramer>]
[assembly: GoImplement<initALPNRequest, Handler>]
[assembly: GoImplement<ioFS, FileSystem>]
[assembly: GoImplement<ioFile, File>]
[assembly: GoImplement<io_package.ReadWriteCloser, io_package.ReadCloser>]
[assembly: GoImplement<loggingConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<maskedSig, sniffSig>(Pointer = true)]
[assembly: GoImplement<maxBytesReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<mp4Sig, sniffSig>]
[assembly: GoImplement<net_package.Conn, io_package.ReadWriteCloser>]
[assembly: GoImplement<net_package.Conn, io_package.ReadWriter>]
[assembly: GoImplement<net_package.Conn, io_package.Reader>]
[assembly: GoImplement<net_package.Conn, io_package.Writer>]
[assembly: GoImplement<net_package.TCPConn, closeWriter>(Pointer = true)]
[assembly: GoImplement<noBody, io_package.ReadCloser>]
[assembly: GoImplement<noBody, io_package.WriterTo>]
[assembly: GoImplement<noopHandler, Handler>(Pointer = true)]
[assembly: GoImplement<noopHandler, Handler>]
[assembly: GoImplement<nothingWrittenError, error>(Promoted = true)]
[assembly: GoImplement<nothingWrittenError, error>]
[assembly: GoImplement<onceCloseListener, net_package.Listener>(Pointer = true)]
[assembly: GoImplement<os_package.File, File>(Pointer = true)]
[assembly: GoImplement<persistConn, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<persistConnWriter, io_package.ReaderFrom>(Pointer = true)]
[assembly: GoImplement<persistConnWriter, io_package.Writer>]
[assembly: GoImplement<populateResponse, ResponseWriter>(Pointer = true)]
[assembly: GoImplement<readTrackingBody, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<readWriteCloserBody, io_package.ReadWriteCloser>(Pointer = true)]
[assembly: GoImplement<redirectHandler, Handler>(Pointer = true)]
[assembly: GoImplement<requestBodyReadError, error>(Promoted = true)]
[assembly: GoImplement<requestBodyReadError, error>]
[assembly: GoImplement<response, ResponseWriter>(Pointer = true)]
[assembly: GoImplement<response, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<rᴛ1, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<rᴛ1, io_package.Reader>]
[assembly: GoImplement<rᴛ1, io_package.WriterTo>(Promoted = true)]
[assembly: GoImplement<serverHandler, Handler>]
[assembly: GoImplement<socksAddr, net_package.Addr>(Pointer = true)]
[assembly: GoImplement<socksConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<socksConn, net_package.Conn>(Promoted = true)]
[assembly: GoImplement<statusError, error>]
[assembly: GoImplement<stringWriter, io_package.StringWriter>]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<textSig, sniffSig>]
[assembly: GoImplement<timeoutError, error>(Pointer = true)]
[assembly: GoImplement<timeoutHandler, Handler>(Pointer = true)]
[assembly: GoImplement<timeoutWriter, Pusher>(Pointer = true)]
[assembly: GoImplement<timeoutWriter, ResponseWriter>(Pointer = true)]
[assembly: GoImplement<tlsHandshakeTimeoutError, error>]
[assembly: GoImplement<transportReadFromServerError, error>]
[assembly: GoImplement<unencryptedHTTP2Request, Handler>]
[assembly: GoImplement<unencryptedNetConnInTLSConn, net_package.Conn>(Promoted = true)]
[assembly: GoImplement<unencryptedNetConnInTLSConn, net_package.Conn>]
[assembly: GoImplement<unsupportedTEError, error>(Pointer = true)]
[assembly: GoImplement<writerOnly, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<writerOnly, io_package.Writer>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(ClientConn))]
[assembly: GoFieldAccess(typeof(Cookie))]
[assembly: GoFieldAccess(typeof(CrossOriginProtection))]
[assembly: GoFieldAccess(typeof(Request))]
[assembly: GoFieldAccess(typeof(Response))]
[assembly: GoFieldAccess(typeof(ServeMux))]
[assembly: GoFieldAccess(typeof(Server))]
[assembly: GoFieldAccess(typeof(Transport))]
[assembly: GoFieldAccess(typeof(body))]
[assembly: GoFieldAccess(typeof(bodyEOFSignal))]
[assembly: GoFieldAccess(typeof(conn))]
[assembly: GoFieldAccess(typeof(connReader))]
[assembly: GoFieldAccess(typeof(expectContinueReader))]
[assembly: GoFieldAccess(typeof(go.crypto.tls_package.Config))]
[assembly: GoFieldAccess(typeof(go.crypto.tls_package.ConnectionState))]
[assembly: GoFieldAccess(typeof(go.mime.multipart_package.FileHeader))]
[assembly: GoFieldAccess(typeof(go.mime.multipart_package.Form))]
[assembly: GoFieldAccess(typeof(go.net.http.httptrace_package.ClientTrace))]
[assembly: GoFieldAccess(typeof(go.net.textproto_package.Reader))]
[assembly: GoFieldAccess(typeof(go.net.url_package.Error))]
[assembly: GoFieldAccess(typeof(go.net.url_package.URL))]
[assembly: GoFieldAccess(typeof(headerSorter))]
[assembly: GoFieldAccess(typeof(http2ClientConn))]
[assembly: GoFieldAccess(typeof(http2DataFrame))]
[assembly: GoFieldAccess(typeof(http2Framer))]
[assembly: GoFieldAccess(typeof(http2GoAwayFrame))]
[assembly: GoFieldAccess(typeof(http2HeadersFrame))]
[assembly: GoFieldAccess(typeof(http2MetaHeadersFrame))]
[assembly: GoFieldAccess(typeof(http2PingFrame))]
[assembly: GoFieldAccess(typeof(http2PushPromiseFrame))]
[assembly: GoFieldAccess(typeof(http2RSTStreamFrame))]
[assembly: GoFieldAccess(typeof(http2Server))]
[assembly: GoFieldAccess(typeof(http2Transport))]
[assembly: GoFieldAccess(typeof(http2WindowUpdateFrame))]
[assembly: GoFieldAccess(typeof(http2addConnCall))]
[assembly: GoFieldAccess(typeof(http2clientConnPool))]
[assembly: GoFieldAccess(typeof(http2clientStream))]
[assembly: GoFieldAccess(typeof(http2connOutflow))]
[assembly: GoFieldAccess(typeof(http2dialCall))]
[assembly: GoFieldAccess(typeof(http2frameCache))]
[assembly: GoFieldAccess(typeof(http2pipe))]
[assembly: GoFieldAccess(typeof(http2priorityNodeRFC7540))]
[assembly: GoFieldAccess(typeof(http2priorityWriteSchedulerRFC7540))]
[assembly: GoFieldAccess(typeof(http2requestBody))]
[assembly: GoFieldAccess(typeof(http2responseWriterState))]
[assembly: GoFieldAccess(typeof(http2serverConn))]
[assembly: GoFieldAccess(typeof(http2serverInternalState))]
[assembly: GoFieldAccess(typeof(http2startPushRequest))]
[assembly: GoFieldAccess(typeof(http2stream))]
[assembly: GoFieldAccess(typeof(http2transportTestHooks))]
[assembly: GoFieldAccess(typeof(http2writeData))]
[assembly: GoFieldAccess(typeof(http2writePushPromise))]
[assembly: GoFieldAccess(typeof(http2writeQueue))]
[assembly: GoFieldAccess(typeof(http2writeResHeaders))]
[assembly: GoFieldAccess(typeof(io_package.LimitedReader))]
[assembly: GoFieldAccess(typeof(net_package.OpError))]
[assembly: GoFieldAccess(typeof(onceCloseListener))]
[assembly: GoFieldAccess(typeof(os_package.SyscallError))]
[assembly: GoFieldAccess(typeof(pattern))]
[assembly: GoFieldAccess(typeof(persistConn))]
[assembly: GoFieldAccess(typeof(populateResponse))]
[assembly: GoFieldAccess(typeof(readTrackingBody))]
[assembly: GoFieldAccess(typeof(readWriteCloserBody))]
[assembly: GoFieldAccess(typeof(response))]
[assembly: GoFieldAccess(typeof(routingNode))]
[assembly: GoFieldAccess(typeof(serveMux121))]
[assembly: GoFieldAccess(typeof(socksAddr))]
[assembly: GoFieldAccess(typeof(socksDialer))]
[assembly: GoFieldAccess(typeof(socksUsernamePassword))]
[assembly: GoFieldAccess(typeof(time_package.Timer))]
[assembly: GoFieldAccess(typeof(timeoutWriter))]
[assembly: GoFieldAccess(typeof(transferReader))]
[assembly: GoFieldAccess(typeof(transferWriter))]
[assembly: GoFieldAccess(typeof(transportRequest))]
[assembly: GoFieldAccess(typeof(wantConn))]

// The go statements whose goroutine the converter proved cannot recurse, by the file, line and column of
// their goǃ call: go2cs-gen starts each on golib's bounded stack reservation rather than the Go-scale default.
[assembly: GoBoundedGoroutine("h2_bundle.cs", 7764, 9, 10)]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("net/http/client.go", "client.cs", "AG7aAQAiTAAJCoKUgqaC7oKUyOKCgoKUgoKmgoKUgoKmpoKClKaCgpTYAAsI8pSCgqaCgqaCgsqCgoLugoKmgoKCgoKmgpSUgoKCgpS4gqaUgpQAChaClJSC3KyygoKUAAIQ0pSClMQABxCClAAIGAAMAoKUgpSmgqaCgqSUgoKmgpSkhoS4gqKCgriClMLEgrSCtMgAAhDSggACOAAZAgAINAAZAoKClMbcuKKCgpQACArylIKC3ILGgoKUyrbqooKUgpQABFAAJQIAEArygoCkgoLKlAAJGJSClIKClJTcpoKCuJSCgoKUgriCpoIACBKCgoKClILugoKmpoKUyoIAAhDSgpSUyoKCuIKCgpSCgpSmkoKClKamAAsO5riCgoK4AA4YgoKCgoKCpoKCgoKCpoLcgoKCpu6mggALDIKClAACJgAQAgAKIgAQAoKClIIABCYAEAIACCIADgIABCIADgIACB4ADgKCgpQAChTyiAARHLKCgpSClIKUpoKCgsYADRyCgq7CgsqCyoKUpqKCgpQ=", "236-241:1;372-372:1;384-393:2;396-401:3;406-417:4;599-599:1;622-638:2;774-830:1", "", "709=urlErrorOp/1/1/1,New/1/1/2;737=urlErrorOp/1/1/1", "", "703:25,704:17")]
[assembly: GoPositionMap("net/http/clientconn.go", "clientconn.cs", "AGXiAQAYApTWppKClIKmgpQADh6CgoK4AA0cgoKCgtyCkqaCgpSCpoKCgpT64oLcsgAKBoKClIKmgIK2gIKkgpSClAAEFgAMAoKCgpTcsqyyAAIgABECgtrigqKo8oKCgpSClIKUgpSqgoKClIKCgtaCgoLu3IKCgoKCAAUkAA4CgoKCAAwSooKmgoKmgoKClILmgoKmgqSkuKKCkoKU1qKCkoKUAAkGooKSgpTWlILWooKSgtaktA==", "345-350:1", "", "172=JoinHostPort/1/1/2", "465:29")]
[assembly: GoPositionMap("net/http/clone.go", "clone.cs", "AA4uAAkCgrgAAhgACwKClIKCgoKUAAIYAAsCgpSmgoKCgoKUlJQAAhgACwKClIKCggACHgAMAoKClA==", "", "", "72=Clone/1/1/1")]
[assembly: GoPositionMap("net/http/cookie.go", "cookie.cs", "ABYoACZYgoKCgrqCgoKUgoKClJSssoKUgoKUgqKCooKUgpSSgpSU6qKCgpSCooKUgoKUkoKU3IKCgqaCgoKUgoKCpqSCgoKUpLS0tKS0grSCtIK0goKUgpSCtIKCgoKCgqaCtIK0gqSUAAYS4oKCyoKUgoKCpqyyggAdEOKCmKKCgoKClIKClILKgoKUgpSmgoKClIKCgoKUgpSClNi0tKSClAAOCLKClIKUgpSCgqaCgoK4goKmgoKmAAgWAAgCgoLcgoKUgqaCopSCgoKCgpSigoKUgpSSgpSmqJKClIKUqKSssoKUgqamlIKCgoKClLaCxsaClMaClIKUpJSCpqaUggAGIgAOAoKClKaC6qKmgqaigoKClIKClIKUgoKCpgACGAAMBIKClIKCpg==")]
[assembly: GoPositionMap("net/http/csrf.go", "csrf.cs", "ACxYkgAEGAAOAoKClIKUgpSClIKCgpSCAAwKtAADHgANAqaCgoKUgoK4AAIUAAkCgoKUAAgKwram+qSClKaCpqbupoKUyIK8AAgCgpS4goKUAAUS4oKCgoKUgpQ=", "207-217:1")]
[assembly: GoPositionMap("net/http/filetransport.go", "filetransport.cs", "ABI+AA0CAAIiAA4Cxu6CkoKUxoKCAAsYABEggoKUgpSmgoKUlIKUpoKmgoKUlIKmgoKUgoKU", "61-64:1")]
[assembly: GoPositionMap("net/http/fs.go", "fs.cs", "ADNksoKmgoKClIKClIKm3LiigoKUgoKUgoKUgoKClAAnRIKC+IKCAAsEiLKCgoKClIKCpoKCgpSkgoKCgoKCgsqClMqcwpSC3IKUgpSmgqYAAkQAHwKCgoKUgoKUlO64ABEM4oKCgqa4goKCgoSSgoKCgoKmgoKmgoKClJSCuIKCgtbKgpSCtIKmypQADBqCgoKUgoK0gpSCgoKCkrKygoKClIKClIKCpoLIABo2gpSUgrSqsoKCgpSCuIKC2KSmqqKqogAQGoKCgpSCgoKUgoKUgpSCgpSClKbmgoKClIKCyoKClOaCgoKUgoKCgpSCgpSClIKClIKUlOaCgpSCgpSCgriCgpTmgoKUgoKUgoKClMqClIKClIKUppaS5oKCuNyCgoKCgpTq1IKClIKClJSCgpSCxoKCyIKClAAKCNLKgoKmgoKCgpSkgoKCgqamgoKCgpSCgpSCgpSCuIKUgoK4goKCkoKCgtyCgoKUgoK4kqIACg7SgpSClIKm6qKClIIABjAAFwLcgpSCAAIuABYC3IKUpoKClIKCpqYADxyCgpSUgoKoxoKCtIK0goKClKaCgoKUpoKCgpSCgoKCgpSUlIKUgqbMsgACIAANAgACGAAJAsaigoKClAALEIKmggAJELiCgoKCgoK4AAoKooKUgoKUgoKCgoKUgpSSgoKUgoKUgoLcgpSCgpSClIKUgoKUpoKUgpSUgoKUgpSmlJSU/IKCysKCgoKClIKCpqKClA==", "161-161:1;247-257:1;376-394:1;761-761:1;896-898:1", "", "1163=contentRange/1/1/1", "429:25,810:21,854:29")]
[assembly: GoPositionMap("net/http/h2_bundle.go", "h2_bundle.cs", "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goKCgoKCysaigpSUlAAOFoKCgoKUgoKUgpSCgoKClAAPIsKCkJKCguaU5pSCAA8g4oKClJSmooKCuIKCgrjUgoKUgoKC2KTCgoKCgoKUgqbugpSmuIKmgoLcgpSCgqaCgoKmlIKkgoKCguSCgpS0tKaCpOiClIKCgsa0lLSUgqSCtLSCpLS01rTEAAgOgoLcgoKCAAsKooKCgpSCpoKCpoKmpoKm/oKCgoKCtpSUlJSCggAMDLrygpSCpIKCgoKUpoKStKSCgqYACQjswoKCgsqClILEtAAJEqS0toKClAACFPKCtMgABBgACgKWABQogoKC3JS0poKUyIKCpoKmlKzSgoKmgoKUAAgK1raCgoKCgoK4goLcgoKUgoKCgriCgpSCAAgOuMKCgpSClIKmlIKCgpQACxjKtLamgsbalAACGgAMAoKClIKCgoKClIKC3JSCgoKUgoKClIKUgqaCgoKUlAACFAAJAoKAABkqgqaigoKClJSCgoKmooKmooKCgr7SgoKCgoKUggAIEqaCgpSCgriUgqSCpIKCpoKCtIKUlAALCKKmgoKU7oSCgpSUppSkpKSkpKSkyLSC+LKCgpSCuJTclIIACAaygpSCypTclIKUlMaCtoLmspSC3JSCgpSmsoKClIKClIKUgpSUgoKCgpSCpoKUgoKUpoKCggAIBrKCgoK4lJS4lICCyIKCpqKCgpSClKaClIK0tLSktAALEIK25qKC7pQADAaygpSCggAKFtwACRSClJSUlJSCuIKClJS4lJSCpoKCgriClIIACBKCgpSClKaygoKUlKaCyJKqwoKUgqaClKqigpTOoqa8ogAVDrKC3ILcgqbcgpQABxCClJSCAAgSgpTupoKClJSCgpSmgoKUgoKUgpSClIKCAAkUgoKmprKCgoKCgoKUuILcggAOBrKCgoKUgoKmgpSCgoK4lKaCAAgGgsqUprKClIKmooKCpoLugoKCgqaCgoKUlIKmABgGwpQACBKCpoKCgpQAChamgoKClIKUgqaCgpSCgoKClKaUuKaylIKCpoKCpsoADRyCpqKCgoKCgoKCggASGtKCgoKCgpSClO6mooKCgoKCgpSCgpSUgpSCgpSCgrqygoKCgriUgpSCgsqCgoKClJSUgqIACASKwoLK0oKCypSAyqSCxIKkpLaosgAQILKCggALDrKCgqa6kqjCgoKCgpSClIKUABUkgoKCgoK4prKCgpSClIKClIIADx6CggAcPLKCppTGlIKCgqassoKUgpSCABEUAAkCgqaCpoKCgpKCgoKUpoKClKaCgoKUgpSmggAHEIKCgoK4ggAIEoKUgqaClILKgoKUgriC3JQAFk4AFQKCgpSCgqaCgoLYgoKmgpSigpSmgoKClKamgoKmgpSigpSmgoKClKamlKaCpoKCgpSCgtyCgqT6poKCgpSCgoKCgoKigqaCpoKCgpSClMgACxaC2IKCgpQACAaCgqamlJSCgoKCgqbupoKCgtiCgoKCgpQABBYACAKmguiSgoKUgpSClIKUpoKUuIKCgoKC2oKWABcEooKCuIKmgriClIKUgoK4goKUgoKUgpSClIKmgoLcjKaC3IKmAAcQxKTYxKSkggAQGLLKlIK4goLcgriCpoLugoKUgu6C7pSmgoKmAA4eooKClIKClKKC3AALFrKCgqaCgpSmgoL+soKIlAAKBqKClIKClIKClIK0gsSUgoKUggCQAcICgoKCgoKmgpSClKaCzsKCrLIACAaigriCgIKkgpSClIKUooKAgoKCyqSClJSClKiSgoKClJQACA6CxqKCpqKClACJAbYCooKUpsKCgqKkooKClIKmlLjCgoKSgoK0pKKClIKCkoIADRSygpSCggAPFtT6soKmAA0agsqigoKClIKCgqaCpoKUAAoIwuiCxqaCgoKCgpSCgoKCgoCUgoKUgoKCgrSCtILYAAoWgpSUgoKUvrKC2oKCgoKe4oK4gsqCgoKUgoLcgqamgoKUgpSUlAAIBqKClIKClIKClKaCgoKUgpSClOaCgoKCpoKClIKClIKUqqKmgoKUpqKmsoIAFCqCgpSCpoK4yoKCgoKUgoKUgoKUgoKmuIKClIKmgoKCgoKCgriAgoK2ksbCpoKSgoKCgpS0puKCgqKk4oKUgqaClIKUgoK4lLi4xq4ACgKCgtwACAKCgoKUggAkSuKCgoKUlIKCABci0oKC1qKClILKAAcQpgAIEoKmpoIAAhzigpSClKqipoKCqJKClKrqAAIS4qbCgpKCoqiigoKUgriigoKClIKClJSClKbSgoL2xrKCpoKCsoKCgoKCgpSClLSSgrSCpJSCgoLI0oKCgoKClKaClICCpICCttaigoKCgoKClIKUgrKCgoKCopIACAiqwoKC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", "2063-2063:1;2069-2075:2;3226-3276:1;3278-3278:2;3356-3363:1;4308-4308:1;4359-4381:2;4382-4384:3;4388-4400:4;4909-4909:1;5191-5201:1;5547-5547:1;6478-6505:1;6629-6636:1;6906-6920:1;6932-6946:1;6995-6998:1;7274-7319:1;7378-7380:1;7639-7655:1;7659-7661:2;7663-7670:3;7860-7863:1;7890-7893:1;8570-8584:1;8637-8643:1;8745-8754:1;8756-8781:2;8783-8804:3;9018-9020:1;9609-9611:1;9829-9831:1;9915-9917:2;10311-10347:1;10438-10449:1;10684-10688:1;11947-11969:1", "", "1742=Uint32/1/1/4;2102=Sprintf/1/1/1;2108=Sprintf/1/1/1;2355=Uint16/1/1/1,Uint32/1/1/2;2527=Uint32/1/2/2,Uint32/2/2/3;4769=RemoteAddr/1/1/5,String/1/1/5,http2newBufferedWriter/1/1/6,handler/1/1/7,http2newGoroutineLock/1/1/21;5336=maxHeaderListSize/1/1/3;6964=PseudoValue/1/5/1,PseudoValue/2/5/2,PseudoValue/3/5/3,PseudoValue/4/5/4,PseudoValue/5/5/5;7051=WithContext/1/1/13;7839=hasTrailers/1/1/4;8009=http2asciiEqualFold/2/6/1,http2asciiEqualFold/3/6/2,http2asciiEqualFold/4/6/3,http2asciiEqualFold/5/6/4,http2asciiEqualFold/6/6/5;8024=http2cloneHeader/1/1/4,getErrChan/1/1/5;8129=RequestURI/1/1/4,http2cloneHeader/1/1/5;9106=Now/1/1/18;9449=tooIdleLocked/1/1/5;9806=http2actualContentLength/1/1/6,ContextClientTrace/1/1/7;10220=http2actualContentLength/1/1/7;11072=New/1/1/3;11166=StatusText/1/1/5;12963=Bytes/1/1/2;13613=get/1/1/1;13668=get/1/1/1", "4659:23,5327:21,5330:21,5370:25,5376:25,5383:21,5615:21,8490:27,8497:27,8515:23,9211:21,9514:21,9680:25,10116:29,10856:21", "7161:64,7162:13,12275:21,12276:13")]
[assembly: GoPositionMap("net/http/h2_error.go", "h2_error.cs", "AA0agoKCgpSCgoKClIKCgoKmgoKU")]
[assembly: GoPositionMap("net/http/header.go", "header.cs", "AB48wgACENIAAhLiAAIQ0qiSgpSqooKssqiSpoKokoK4goKUgoKCpoKUgoKU1gAFEuKCgoKmpgAIDoIADhru4oKClIKCgqaCgqyypqKCgpSCgoLKlKKCgoKCgqaCpoKCpoIABBTgnMKClIKU7oKmgqaClIKmpoI=", "179-179:1")]
[assembly: GoPositionMap("net/http/http.go", "http.cs", "ACdSgJaAloCWgJaAloCUgoKUuKKCgpSClIKUAA8cABEguJCYooKUqKSokoKCgqamooKCgpSmgpSCgoKCgpSCgqaClO74goLIgg==")]
[assembly: GoPositionMap("net/http/mapping.go", "mapping.cs", "ABwykoKUgoKClJS+8oKUgoKUgoKmqsKClIKCgriCgg==")]
[assembly: GoPositionMap("net/http/pattern.go", "pattern.cs", "ACZKlIIAMFgAFQKClIKCgriygpSCgpSClJSClIKClIKCgoKmuIKmgpSCgpSClIKClIKClIKmgpSClIKCgpSClIKClIKUgpSClIKm5oKCpoKCpqaCgpSUABI2AA8CuJSCpoKUgpSCAAIUAAkCgpSUlIKUlJSClKrmgqiigoKCgtyCyoKUgpTIkoKUgpSClIKUgpSUlIKClKaClAACFgAIAqSkpIKUpKSkpMbMoqSkpLqygoKCgpSClIL8gpSClKiSgriCgoK8ooKSgoKUpoKCgpSqopSSgoKClIKWyoKCgpSmlIKCpoLKgpSClIK4gpSmpoKClA==", "89-93:1", "", "473=commonPath/1/1/4,differencePath/1/2/4,differencePath/2/2/4", "", "117:21,118:13")]
[assembly: GoPositionMap("net/http/request.go", "request.cs", "ACxSAAsUlpIAChTKuLjKysqWpgDeAfoDAAsCgpQAAhgACQKClIKCggACFgAIAoKUgoKCgoKCgIKCgqSCgqaAgoKCpIKqouqSqJKqooKU2LzCgpSClAAGEgAIAoKClAAHGAAIAvwADBLCgpSClIIACgaCgoKUgpSCgpSCgpSqosiSgpQACy4ADgIAAhLi6gANCgAJAoKCksqCgoKUggAHEIKCgpSUgoLuAAwagpTulIKCgpSCgqaCAAUUwoKCpoKCuIKClILKgoKUgoKCgoKUgsqCgpSCgqaCgqaCgoK4goKmgriCgoKCpoKUgoKCuICCgtqCgoKClKaClAAPEAAJFIKUqqKClIKClIKClOyypKSUgpSClIKUgoKUgoKUpgANHKiSAAY0ABcCuJSClIKUgoKUgoKmggAKFoKUgoKSgsaCgpKCxoKCkoIAFCKCgrgACAyygoKUAAgeABAClIKUgoKUgoKClAAEHAALAsjSgoKClNqCgoKClKaiggACEuKCgqaCAAwcABECgpSWkoKUgoK4goKClIKUgoIACxiCgqaCppS4goKUgoIACRSCgqaUlIKCppTclAAGGgAKAoKUAA0QlAAZFrKClILcgpSUgoKCpoIAABKIlIKmgsaCggALCLKCgpSmgpSClIKCgoKUgoKClJSCgpSCggAMFAAEKgAUAoKCgpSCpoKCgpSCgoKCgqaClIKUpgAGFgAKAoKUgqaUgqaCgqaCgqaClIKUppQAAhwADQKClIKUAAIQ8oKUgpSqwoKUgoKCpoKCgqauwoKUqqKClIKUvMaClIKCgoKUpgAIBoIACAaCgpTmgoKUpoKClAAIBoKCpMqCpqqigpSClAAEFPKklAAMCqI=", "585-589:1;592-599:2;928-931:1;935-938:2;942-945:3;963-963:4;1090-1094:1", "", "1791=Get/2/2/1,EqualFold/1/1/1", "", "637:25,638:17,644:21,645:13,1216:21,1217:13")]
[assembly: GoPositionMap("net/http/response.go", "response.cs", "ABkwAFzKAZLq/MKCgpSClAAKEuKCyoKCgpSUgoKUgpSCgpSCgpSCuIKCgpSUlJSCgqYACBTygoLuogAPKAARBIKCgoLKpoK4goKEkoKClKaUggAJGoK4goKUgoK4goLKgoKCyoK4goK4poKCAAMW8oKqosqi6qI=", "", "", "310=NewReader/1/1/4,MultiReader/1/1/4;403=HeaderValuesContainsToken/1/1/1")]
[assembly: GoPositionMap("net/http/responsecontroller.go", "responsecontroller.cs", "ABRMABECAAwQkoKClKSCpMTOooKClKTEAAwU0oKClKTEAAwW4oKClKTEAAweAAoCgoKUpMTuog==")]
[assembly: GoPositionMap("net/http/roundtrip.go", "roundtrip.cs", "AAsoAAwS8oKU")]
[assembly: GoPositionMap("net/http/routing_index.go", "routing_index.cs", "ACI+ooKUgpSCgoKUAAQcABIOgoKUgoKCpsqClMqe4oKCgoKUgoKCgoKCgriCgoLKgpQ=", "64-75:1")]
[assembly: GoPositionMap("net/http/routing_tree.go", "routing_tree.cs", "AClY1JSUrNKCgpSCgoKUgoKCgpS8ooKUgqrCgoKUlIKUgoKqwoKUgq7iuIKm7NKClICUpJSCuK7igtyCgpSmyoCCAAgQgoLKpoKUlM7CgpSCgoKUAAgKwoKUgoK4goKUkoKU", "233-238:1")]
[assembly: GoPositionMap("net/http/servemux121.go", "servemux121.cs", "ABgyvKKCggASJOKClIKUgpSCpoKUgoKCpoK0xIKCloKmgoKCyJKClKjWuIKmyoK4gIK2goKCpqoACAKCpoKUgpSClNq0goLKgoKmrvKCgoKClIKCrLKUgoK4goKUgoK4", "82-84:1")]
[assembly: GoPositionMap("net/http/server.go", "server.cs", "ADNW7tzKALQB7gLcAC940oKC2OKClJSCgpSCgqaClIIAIUiCtrKClJSUgoKCgqaCgpSClKaCgpSmgoKUgpSCgrgARpwBgqaCpoKCABYoooKCgoKUpoKClIKmrLKClJQACAqigoKCvMKCggAMGAAJAoLKgoLugoKCgriCpoKCgoKmgoIAERCSuIKUACA8ooKCuKCk0oKCoqTSgoKClIKUgoKipKKCgoIAFzCmgpSCgoKm0oKCgpSCgoKUoqSCggABGAAKAoKUgoK4woKCgpSCgoKClJSCgpSCgpSClIKCgoKUgoKUgoKClIKUggANFqSCpoKClKaClKSUpoKCgoK4poKCpoKCgoKCgqamooKCAAoSgoKUpoIAAhDSgsqClIKmABMYwoKUgoKCgoKClJSCgpSmooIADRYADwbygqaCgoCCpICCgsiClIKUgoKClKaCpoKUgoKClIKUgoKUgoK4lIKCgoKCppQADRyClIKCAAgKooK4hLjGgriUggAPHAAKFoL8ooKCgoKCgoKUgqYACAaigoKClIKCgpSUpu6CpoKClKaClIKmgoKClIIAEiLeggADEPKCgoKUgoKClIKCgoKCABwaAAgCgpSUgriC7oKCgpSCkoKClIKUgpSUpoKCgoKUgqaCgqaCAA8ggoLKgoKCypSCgoKUgqaCABMogIK2lAAQIoKk6oKUlMa0pMS2goKmAAgKgoL8yIKCgriClLiCgoKmgriCpqaUgqaClIKCgpSClILcgoKCuIKClO6CuIKUgtyGgriClLiCgoLKooKClIKClJKCAAgS4oKUlIKCgoKm3KKClAACSgAiAqaCqLKCgoKUppSmgpSClIKmgoKUgpS4opSCpoKCgpS4lIK8orimlMqCpoKmpoKCpoKmooKUgoKClMaCpoKmgqaCupKCABUoAAMQ4oKCABYuzLKklAAHEKKClLSkgpSCgoKUgriCgsygAAoS/JzCgpSAgqSAgqQAHBDSgpSCgoKCgoKClIKClIKCgpSCuIKCgoKClIiygoKClJSCpoKClIKCgoK4gpTugoKCuIKCpIKClICCtoKCgqaCpoKClJSClIKUntKCgu64gqamgoKUgoLKgoKUgpSCgqaUgpQACRSCgoKCgoKUgoKCgpSUgpTKpoCClP6CpoCClAAWHpSigpSClAANFILGggAJDqKCgpSCgoKClIKUgpSCgoKCgsYADhqCgqoACQKClIKCpoKCyIKCgpTW0oKCgpSCgoKm1tKCgoKUgoK0xIKUtMTMooKUlKS0AAwWkgAMHgAMAgAIEriCgoKogJiQAAEQ4oKUgoKCgoKCgoKCgpQABCIADgLcgoKCuJSCpoKCuIKCgpS4ypSCgpSmgoK4AAgUggAKEoIABBLiAIABhAKSyMqSgpSClKaUgpSmqKSClIKClAAELgAWAoKUgu7ygoKClLiCgri4gpiigoKUlIKClIKmuIKCkoKmlAACEgANAoKU3JSCgsqmAAg6ABwCgtyCyoLuyOaCgoKUgpQACwrCgoKUgpSCgpSUAAIWAAgCgpQAAxLSgpTcooKUvKKClNiCggALCNKClIKUgqaCgsqCgpSmgpSAkoKCppSkgoIABxwACwKCAAIeAAwCggBymgIADgKCgoLcgoKUgoKUABJAABgClIKCgqSClIKUlIKClKaCkoKClLSkAAgU8oKCqvKCgoKCuIKUpoKUgpTWgoKCgqYAKlYACRCCAAwkAAsCgoKUgqbGAAEWAAkCgoKCgoKCgpQADBoACgKClIKClIKClAAIDqLulIIACBLqAAMYAA8CgqaCgpSAgraClJSCgoKCuJSCgoKCgpSCgpSUgpSCgpSUgoKCgqaCgoIAByQAEgaAgraClIKCgoKCgriCpoKCyoLKgqaCgoKUyvyCgpKkgpS0gpSklIKUgpQABBoADwKCgoKUgoKUgpSClNbSgoKClIKUtKSCgpSmgoKUpoKmgq7CgoKUpsqigpTe4oKClAADFvKCAAIQ0oIACCQAEwKClIKCpoKCpqTc0oIAAhYACgKCpqKC2LrSgpSCgpSCgpS4lIIABBwACwIACBIAEBiCgpSm0oKCgoKkgoLKgtKSgqaCopLEtIKSgoKClIK0gpKkgoK0guYADxy2koKUppTSgoKClIKU1oKUlKSCgsaCyNKCgqIAFhSigqYADQqigtyCABAisJSigoKUgpSClAAgFpaigoKCAAoMsoKCgqaygoKCpqKCgoIADRSygoKClMaigoKClJSqwqSUqJKCgoI=", "987-989:1;1273-1285:1;1913-1932:1;2181-2186:1;2358-2372:1;2725-2728:1;2934-2941:1;3182-3191:1;3346-3357:1;3561-3575:1;3851-3859:1;3852-3856:1.1;4088-4092:1", "", "1169=wantsHttp10KeepAlive/1/1/11,wantsClose/1/1/12;1724=get/1/1/1,hasToken/1/1/1,isProtocolSwitchResponse/1/1/2;4676=Sprintf/1/1/1", "2251:21,3614:21,3901:21,4260:21,4392:25", "1112:29,1113:21,2175:64,2176:13,4409:29,4410:21")]
[assembly: GoPositionMap("net/http/sniff.go", "sniff.cs", "AA8q4oK4gqaCgriqopSUqqKUlAAfEgA7kAKCgpQADBK4gpSClIKUgoKCpgAMCoKCgpSCgoKUgriClMaCAAoIpoKUgoKUgpSClJSCpgANCpSCmqY=")]
[assembly: GoPositionMap("net/http/socks_bundle.go", "socks_bundle.cs", "ABQuggAPBuKCgpSAgoKkgoKCooKCpqK0grTagoKClIKClIKCpoKmgpSClIKClIKCuIKCgoKCgoKClKaClIKClIKCpoKUgpSClIKUgoKUgrSCtIKUtJSClJSClIKUlIIACAaCgoKUgoKUgpQADQyClKSkACEUgpSkpKSkpKSkpKQAIzzUooKUgoKUAAwYwoKUABxEAAwCgoKUgoKUgoKClIKUgoKUgoKCgpQAAhIACAKCgpSCgpSCgoKUAAISAAgCgoKUgoKClJSCgpSCgpQACAaC1pTGlKbCgpKClIKCgpSClKbKogAWIqKUpIKUgoKCgqaAgqSAgqSClIKUlA==", "39-44:1;45-53:2", "", "", "", "60:25,61:17")]
[assembly: GoPositionMap("net/http/status.go", "status.cs", "AMkBogGilKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSk")]
[assembly: GoPositionMap("net/http/transfer.go", "transfer.cs", "ACI+/IIACxCCgpSClIKCABcuoqaClIKUgoKCgoKCgoKCAAgSgqa0goKUgoKCgoKCgoLIgoKCpoKUgoKCyoKmAAQqABYGgpSClLiC3AACJAARAoKCgoKCgpSClIK0gpSCgoKClJSCAAgOuOiCxoKClIKUgqaClIKClKYAFgaigoKUgu6CgIKkgIKkgpSCgpSCyoKCooKklJSCpoKUgsqo4oKCgoKUggAHEIKCgoKUgoKClIKCgpSUgoKUgoKUgqaCgoK4griUgoK4lN4ACgKClIKClAAFENKAgqSAgoKkABUkgsqilKSklKiCgpaCpqSUqLKmgpSCgoKCgoKCxoKCgqaCxMiCuIKmgoKUgoKUpriCgtyUlOyUgpTGtMaUptqUgoKClIK0goKClIK2qICWgAAIDoLKooIACgiSgoKUpoLugpSCpoLM4oKmyoKCgsqClLiCgoLKgpSClJQADyCCgriCppQABxC47LKCpoKCgqaCpgAICJKCgpQABxCUlIKCgpKCpIKCtqaClIKUABYwtOKCgoKU6MKClJSClIKCyoKUuIIACRSCgoK4gqbIgpaipoKClIKmpvSUgoKClIKUggAKFoKmgoKClJSUtKTGgoKClMyygpSm0oKCgpSCAAwSppSmgoKUgvykgtbSgoLa8oKC1tKCgqIADBCCgpTG+rKClLiCgoKUlIKClAAKErKClIKCgpSClMbCnqKUpL6ytJSClIKUAAoOsoKCgoKm", "210-219:1;341-348:1;787-797:1", "", "", "", "406:21,407:13")]
[assembly: GoPositionMap("net/http/transport.go", "transport.cs", "ADZcAPEBmgSCgpSmgoKUpoKClKiyggAWLoKUgoKUgoKUgoKClJQAEBqCxvjCgoKCAAcQgoKCgoLKlJSCgpSClIKCgpQABxCCgoKUAAgSpoKClIKCuIIAEBakAAQmABICqqIAESiCgpSmooKClMrCypSs4oKUgsaCgoKUgqa4AA4IAAgCgoKUgoKUgoKUgoKUgoK4goK4gpSCgpSCgoKmgoKUgoKUgoIACximgu6UkoK4gqSC6oKCgoLugoKCpoKUlJSC3JSCuIKClKaAgqSAgqSAuKSUpoKCAAoKgrT6AAkQgoKmooKUzuKClIKCrvKClIKUgpSCgpSCgszS7pSUlAAHEJSmlJSUppS4lNgAAxgADwKCgoKClIKClIKirOKCgoKCgoKCgoKmgoKCpoKCugAJDoKCgoKUgoKUgoIAAhIACAKCgoKCAAweooKUqJKCxsKCgoKUgsqigpSCgoKU2oKCgoKCgoLcAA8elILGgoK4goKUAAIQAAoCgpSClIKUgoKCgpSC1qSmgsqC7oKCgqaCgoKCAAcQgoKmgpSUgriClIKUgoKUgoKmgoKCgoLcgoKUpoLcAAkCgqaCuJSUrLKCuICCgoKCyoK4pNyClIKC3IKmlIKUlILagpSCgoKC6PKClIKC2LKClIKCgoLYgoLGgoK4goKCtgAPCoKCgoKUlIKCgpSUACFC4oKU2OKClNjigpSClIKUgpSClNrCgoKCgqaUgoLKggAYLJKokqiSgoKmlIKCgqiSgpSClKrCgoKClIK6koKCgpS8ooKUggAJCLKClJSClK4ACgKCgoKCAAcQlAAIEpKCyoK42IK4goKUlLikgoKU6qSCgpT8AAgClIKUgoKmgoKUgoKmgpSCgoKiqMKCgqKCgoKivPKCgoKCpoKCgriUgrSo8oKmgoKCpu6CgoKCgoKCpoK4lILKgpS0yuSCgpSClIKCgoKAgpK2soKUgoKUlICCgqaUgpSkgoKUgoIACA4ADwTiAAoWgoKUlJSCgoKClICmgpSAgoKClKSCgpS2goKUgoKCgpSC3NiCgoCCpoK4tIKCxoKAgoLogoKCgoKCgqaUgpSAgoKkAAkUgqSCqtKCgoK4gqKStIKC2IKCpoKCgoK4goKCgpTIgoLKmoKCgoKUgoKClKaCgoKUgpSUpoKCgpSUuIKUgoIAFBqygoKs4oKCxgAeOoKCgoKCgqYABxKSgpSokoKUqqKCgpQADhaEkoKUADNqgqaygpSClIKClIKosoKCgqrygoLYsoKCgqbSgoKCoqrygoKSlJSCogACFAALAoIACRTKgriCgoKCppSmgoKmlIKClJT8AAsEwoKCgoKCuIKCgoKClJSClNyCpoKClIKCgpSCgoKClJSClIKClIKmgoKm5pSUlIKClIKClAAHEKbcmoKm5qbKgqaC9oKCqIKCgoKCgqa4goKCgoKCpubcxIqCxoK0toK0pIKClIKCgoKUppSU3rKClIKUzPKCgriCgoKClIKCgpSClJSCgoKCAAgSlJSUgpQADBqClLiCrLKClLKCpMSkpAAKCqKCgpQAFxyygoKUgoKUlAAKBoKClAALEILGwoKCtIKCggAHEJSClIKCpoKCgoLGABQe4sgADBiCkrSkADhogoKCtMiCuJSokoKSgpSCtoLEpKQADgjylIKCgoKClJSClIKClILuggAMIIKmgoKmhqaClMqCgpSCAAcQgoKUgpSClKaCgoKCguSClIKClICCgpSCktbaxoKUpIKUgqSk2sYAEhKigtzCgoIAAhAACgKCgqKkgoKUgoKmgoKUpsaCpKSkpLiigoKUqLKCgpQAFi6CtOKCgoKClIKmgoKCgoKUlNbSgoKClIKClILYkoKUgoIAFSKCtKaSgoKClKi2ooLK8oKCgpSCgoKmgoLY8oKCgpS0qPKCgoKClKKkwoKClJTmopQACQqCwgALDoIAAx4ADQKClAAJEpKCgpSCgpSmgoKCgoKokoKCupI=", "514-516:1;667-671:1;911-915:1;930-935:1;972-974:1;1540-1544:1;1628-1633:1;1728-1730:1;1732-1741:2;1782-1788:1;1856-1858:2;1899-1909:3;2293-2299:1;2301-2314:2;2417-2422:3;2423-2434:4;2590-2602:1;2898-2909:1", "", "55=defaultTransportDialContext/1/1/2;323=Clone/1/1/16;1735=key/1/1/2;1764=isReused/1/1/2;2050=key/1/1/2;2126=Username/1/1/1;2257=UnencryptedHTTP2/1/1/2,HTTP1/1/1/3;2771=wroteRequest/1/1/2,Invoke/1/1/3;2850=wroteRequest/1/1/3,Invoke/1/1/4;3354=Get/1/2/1,Get/2/2/2;3376=wantsClose/1/1/1,isProtocolSwitchHeader/1/1/2", "695:33,2084:39,2189:25,2201:33,2697:21,3001:25,3108:21,3192:25,3381:21,3440:37", "705:21,706:13,1745:21,1746:13,2670:21,2671:13")]
[assembly: GoPositionMap("net/http/transport_default_other.go", "transport_default_other.cs", "AAscgg==")]
// </GoSourcePositionMaps>

namespace go.net;

[GoPackage("http")]
public static partial class http_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("closeIdler")] internal partial interface CloseIdleConnections_closeIdler {}
    internal partial interface CloseWrite_type {}
    internal partial interface EnableFullDuplex_type {}
    internal partial interface Flush_type {}
    [GoLocalName("requestTooLarger")] internal partial interface Read_requestTooLarger {}
    internal partial interface SetReadDeadline_type {}
    internal partial interface SetWriteDeadline_type {}
    internal partial interface anyDirs {}
    internal partial interface closeWriter {}
    internal partial interface connectionStater {}
    internal partial interface erringRoundTripper {}
    internal partial interface genericClientConn {}
    internal partial interface h2Transport {}
    [GoLocalName("baseContexter")] internal partial interface http2ConfigureServer_baseContexter {}
    internal partial interface http2Frame {}
    internal partial interface http2clientConnPoolIdleCloser {}
    internal partial interface http2connectionStater {}
    [GoLocalName("I")] internal partial interface http2h1ServerKeepAlivesDisabled_I {}
    internal partial interface http2headersEnder {}
    internal partial interface http2headersOrContinuation {}
    internal partial interface http2isNoCachedConnError_type {}
    internal partial interface http2pipeBuffer {}
    internal partial interface http2streamEnder {}
    internal partial interface http2stringWriter {}
    internal partial interface http2unencryptedNetConnFromTLSConn_type {}
    internal partial interface http2writeContext {}
    internal partial interface http2writeFramer {}
    internal partial interface newClientConner {}
    internal partial interface rwUnwrapper {}
    [GoLocalName("canceler")] internal partial interface setRequestCancel_canceler {}
    internal partial interface sniffSig {}
    internal partial struct Write_r1 {}
    internal partial struct body {}
    internal partial struct bodyEOFSignal {}
    internal partial struct bodyLocked {}
    internal partial struct bufioFlushWriter {}
    internal partial struct byteReader {}
    internal partial struct cancelTimerBody {}
    internal partial struct checkConnErrorWriter {}
    internal partial struct chunkWriter {}
    internal partial struct condResult {}
    internal partial struct conn {}
    internal partial struct connLRU {}
    internal partial struct connOrError {}
    [GoValueClone("byteBuf")] internal partial struct connReader {}
    internal partial struct connectMethod {}
    internal partial struct connectMethodKey {}
    internal partial struct countingWriter {}
    internal partial struct dirEntryDirs {}
    internal partial struct entry<K, V> {}
    internal partial struct eofReader {}
    internal partial struct errorReader {}
    internal partial struct exactSig {}
    internal partial struct expectContinueReader {}
    internal partial struct extraHeader {}
    internal partial struct fakeLocker {}
    internal partial struct fileHandler {}
    internal partial struct fileInfoDirs {}
    internal partial struct fileTransport {}
    internal partial struct finishAsyncByteRead {}
    internal partial struct globalOptionsHandler {}
    internal partial struct gzipReader {}
    internal partial struct headerSorter {}
    internal partial struct htmlSig {}
    internal partial struct http1ClientConn {}
    internal partial struct http2ConnectionError {}
    internal partial struct http2ContinuationFrame {}
    internal partial struct http2DataFrame {}
    internal partial struct http2FrameHeader {}
    [GoValueClone("headerBuf")] internal partial struct http2Framer {}
    internal partial struct http2GoAwayError {}
    internal partial struct http2GoAwayFrame {}
    internal partial struct http2HeadersFrame {}
    internal partial struct http2HeadersFrameParam {}
    internal partial struct http2MetaHeadersFrame {}
    [GoValueClone("Data")] internal partial struct http2PingFrame {}
    internal partial struct http2PriorityFrame {}
    internal partial struct http2PriorityWriteSchedulerConfig {}
    internal partial struct http2PushPromiseFrame {}
    internal partial struct http2PushPromiseParam {}
    internal partial struct http2RSTStreamFrame {}
    internal partial struct http2RoundTripOpt {}
    internal partial struct http2ServeConnOpts {}
    internal partial struct http2Server {}
    internal partial struct http2Setting {}
    internal partial struct http2SettingsFrame {}
    internal partial struct http2StreamError {}
    internal partial struct http2Transport {}
    internal partial struct http2UnknownFrame {}
    internal partial struct http2WindowUpdateFrame {}
    internal partial struct http2addConnCall {}
    internal partial struct http2bodyReadMsg {}
    internal partial struct http2bufferedWriter {}
    internal partial struct http2bufferedWriterTimeoutWriter {}
    internal partial struct http2chunkWriter {}
    internal partial struct http2clientConnIdleState {}
    internal partial struct http2clientConnPool {}
    internal partial struct http2clientConnReadLoop {}
    internal partial struct http2clientStream {}
    internal partial struct http2closeWaiter {}
    internal partial struct http2connError {}
    internal partial struct http2connOutflow {}
    internal partial struct http2dataBuffer {}
    internal partial struct http2dialCall {}
    internal partial struct http2duplicatePseudoHeaderError {}
    internal partial struct http2eofReader {}
    internal partial struct http2erringRoundTripper {}
    internal partial struct http2errorReader {}
    internal partial struct http2flushFrameWriter {}
    internal partial struct http2frameCache {}
    internal partial struct http2frameWriteResult {}
    internal partial struct http2goAwayFlowError {}
    internal partial struct http2goroutineLock {}
    internal partial struct http2gzipReader {}
    internal partial struct http2handlerPanicRST {}
    internal partial struct http2headerFieldNameError {}
    internal partial struct http2headerFieldValueError {}
    internal partial struct http2http2Config {}
    internal partial struct http2httpError {}
    internal partial struct http2incomparable {}
    internal partial struct http2inflow {}
    internal partial struct http2missingBody {}
    internal partial struct http2netHTTPClientConn {}
    internal partial struct http2noBodyReader {}
    internal partial struct http2noCachedConnError {}
    internal partial struct http2noDialClientConnPool {}
    internal partial struct http2noDialH2RoundTripper {}
    internal partial struct http2outflow {}
    internal partial struct http2pipe {}
    internal partial struct http2priorityNodeRFC7540 {}
    internal partial struct http2priorityNodeStateRFC7540 {}
    internal partial struct http2priorityWriteSchedulerRFC7540 {}
    [GoValueClone("heads")] internal partial struct http2priorityWriteSchedulerRFC9218 {}
    internal partial struct http2pseudoHeaderError {}
    internal partial struct http2randomWriteScheduler {}
    internal partial struct http2readFrameResult {}
    internal partial struct http2requestBody {}
    internal partial struct http2resAndError {}
    internal partial struct http2responseWriter {}
    internal partial struct http2responseWriterState {}
    internal partial struct http2roundRobinWriteScheduler {}
    [GoValueClone("sentPingData")] internal partial struct http2serverConn {}
    internal partial struct http2serverInternalState {}
    internal partial struct http2serverMessage {}
    internal partial struct http2sortPriorityNodeSiblingsRFC7540 {}
    internal partial struct http2sorter {}
    internal partial struct http2startPushRequest {}
    internal partial struct http2stickyErrWriter {}
    internal partial struct http2stream {}
    internal partial struct http2streamMetadata {}
    internal partial struct http2streamState {}
    internal partial struct http2transportResponseBody {}
    internal partial struct http2transportTestHooks {}
    internal partial struct http2unencryptedTransport {}
    internal partial struct http2unstartedHandler {}
    internal partial struct http2write100ContinueHeadersFrame {}
    internal partial struct http2writeData {}
    internal partial struct http2writeGoAway {}
    [GoValueClone("data")] internal partial struct http2writePing {}
    internal partial struct http2writePingAck {}
    internal partial struct http2writePushPromise {}
    internal partial struct http2writeQueue {}
    internal partial struct http2writeQueuePool {}
    internal partial struct http2writeResHeaders {}
    internal partial struct http2writeSettings {}
    internal partial struct http2writeSettingsAck {}
    internal partial struct http2writeWindowUpdate {}
    internal partial struct httpRange {}
    internal partial struct incomparable {}
    internal partial struct initALPNRequest {}
    internal partial struct ioFS {}
    internal partial struct ioFile {}
    internal partial struct keyValues {}
    internal partial struct loggingConn {}
    internal partial struct mapping<K, V> {}
    internal partial struct maskedSig {}
    internal partial struct maxBytesReader {}
    internal partial struct mp4Sig {}
    internal partial struct muxEntry {}
    internal partial struct noopHandler {}
    internal partial struct nothingWrittenError {}
    internal partial struct onceCloseListener {}
    internal partial struct pattern {}
    internal partial struct persistConn {}
    internal partial struct persistConnWriter {}
    internal partial struct populateResponse {}
    internal partial struct readTrackingBody {}
    internal partial struct readWriteCloserBody {}
    internal partial struct redirectHandler {}
    internal partial struct relationship {}
    internal partial struct requestAndChan {}
    internal partial struct requestBodyReadError {}
    [GoValueClone("dateBuf", "clenBuf", "statusBuf")] internal partial struct response {}
    internal partial struct responseAndError {}
    internal partial struct routingIndex {}
    internal partial struct routingIndexKey {}
    internal partial struct routingNode {}
    internal partial struct rᴛ1 {}
    internal partial struct segment {}
    internal partial struct serveMux121 {}
    internal partial struct serverHandler {}
    internal partial struct socksAddr {}
    internal partial struct socksCommand {}
    internal partial struct socksConn {}
    internal partial struct socksDialer {}
    internal partial struct socksReply {}
    internal partial struct socksUsernamePassword {}
    internal partial struct statusError {}
    internal partial struct stringWriter {}
    internal partial struct tLogKey {}
    internal partial struct textSig {}
    internal partial struct timeoutError {}
    internal partial struct timeoutHandler {}
    internal partial struct timeoutWriter {}
    internal partial struct tlsHandshakeTimeoutError {}
    internal partial struct transferReader {}
    internal partial struct transferWriter {}
    internal partial struct transportReadFromServerError {}
    internal partial struct transportRequest {}
    internal partial struct unencryptedHTTP2Request {}
    internal partial struct unencryptedNetConnInTLSConn {}
    internal partial struct unsupportedTEError {}
    internal partial struct wantConn {}
    internal partial struct wantConnQueue {}
    internal partial struct writeRequest {}
    internal partial struct writerOnly {}
    public partial interface CloseNotifier {}
    public partial interface CookieJar {}
    public partial interface File {}
    public partial interface FileSystem {}
    public partial interface Flusher {}
    public partial interface Handler {}
    public partial interface Hijacker {}
    public partial interface Pusher {}
    public partial interface ResponseWriter {}
    public partial interface RoundTripper {}
    public partial interface http2ClientConnPool {}
    public partial interface http2WriteScheduler {}
    public partial struct Client {}
    public partial struct ClientConn {}
    public partial struct ConnState {}
    public partial struct Cookie {}
    public partial struct CrossOriginProtection {}
    public partial struct Dir {}
    public partial struct HTTP2Config {}
    public partial struct Header {}
    public partial struct MaxBytesError {}
    public partial struct ProtocolError {}
    public partial struct Protocols {}
    public partial struct PushOptions {}
    public partial struct Request {}
    public partial struct Response {}
    public partial struct ResponseController {}
    public partial struct SameSite {}
    public partial struct ServeMux {}
    public partial struct Server {}
    public partial struct Transport {}
    public partial struct contextKey {}
    public partial struct http2ClientConn {}
    public partial struct http2ClientConnState {}
    public partial struct http2ErrCode {}
    public partial struct http2Flags {}
    public partial struct http2FrameType {}
    public partial struct http2FrameWriteRequest {}
    public partial struct http2OpenStreamOptions {}
    public partial struct http2PriorityParam {}
    public partial struct http2SettingID {}
    public partial struct noBody {}
    public partial struct readResult {}
    public partial struct socksAuthMethod {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸflate() => builtin.initPackage(typeof(compress.flate_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸgzip() => builtin.initPackage(typeof(compress.gzip_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸtls() => builtin.initPackage(typeof(crypto.tls_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbase64() => builtin.initPackage(typeof(encoding.base64_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(go.@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸmime() => builtin.initPackage(typeof(mime_package));
    [GoInit] internal static void initᴛᴛimportꓸmimeꓸmultipart() => builtin.initPackage(typeof(go.mime.multipart_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸhttptrace() => builtin.initPackage(typeof(go.net.http.httptrace_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸinternal() => builtin.initPackage(typeof(go.net.http.internal_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸinternalꓸascii() => builtin.initPackage(typeof(go.net.http.@internal.ascii_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸinternalꓸhttpcommon() => builtin.initPackage(typeof(go.net.http.@internal.httpcommon_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸtextproto() => builtin.initPackage(typeof(go.net.textproto_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(go.net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸnetꓸhttp2ꓸhpack() => builtin.initPackage(typeof(vendor.golang.org.x.net.http2.hpack_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸnetꓸhttpꓸhttpguts() => builtin.initPackage(typeof(vendor.golang.org.x.net.http.httpguts_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸnetꓸhttpꓸhttpproxy() => builtin.initPackage(typeof(vendor.golang.org.x.net.http.httpproxy_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸnetꓸidna() => builtin.initPackage(typeof(vendor.golang.org.x.net.idna_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(http_package.Client)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛClient;
[GoMethodHost(typeof(http_package.ClientConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛClientConn;
[GoMethodHost(typeof(http_package.ConnState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛConnState;
[GoMethodHost(typeof(http_package.Cookie)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛCookie;
[GoMethodHost(typeof(http_package.CrossOriginProtection)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛCrossOriginProtection;
[GoMethodHost(typeof(http_package.Dir)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛDir;
[GoMethodHost(typeof(http_package.HandlerFunc)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛHandlerFunc;
[GoMethodHost(typeof(http_package.Header)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛHeader;
[GoMethodHost(typeof(http_package.MaxBytesError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛMaxBytesError;
[GoMethodHost(typeof(http_package.ProtocolError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛProtocolError;
[GoMethodHost(typeof(http_package.Protocols)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛProtocols;
[GoMethodHost(typeof(http_package.Request)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛRequest;
[GoMethodHost(typeof(http_package.Response)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛResponse;
[GoMethodHost(typeof(http_package.ResponseController)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛResponseController;
[GoMethodHost(typeof(http_package.ServeMux)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛServeMux;
[GoMethodHost(typeof(http_package.Server)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛServer;
[GoMethodHost(typeof(http_package.Transport)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛTransport;
[GoMethodHost(typeof(http_package.body)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛbody;
[GoMethodHost(typeof(http_package.bodyEOFSignal)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛbodyEOFSignal;
[GoMethodHost(typeof(http_package.bodyLocked)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛbodyLocked;
[GoMethodHost(typeof(http_package.bufioFlushWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛbufioFlushWriter;
[GoMethodHost(typeof(http_package.byteReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛbyteReader;
[GoMethodHost(typeof(http_package.cancelTimerBody)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛcancelTimerBody;
[GoMethodHost(typeof(http_package.checkConnErrorWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛcheckConnErrorWriter;
[GoMethodHost(typeof(http_package.chunkWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛchunkWriter;
[GoMethodHost(typeof(http_package.conn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛconn;
[GoMethodHost(typeof(http_package.connLRU)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛconnLRU;
[GoMethodHost(typeof(http_package.connReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛconnReader;
[GoMethodHost(typeof(http_package.connectMethod)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛconnectMethod;
[GoMethodHost(typeof(http_package.connectMethodKey)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛconnectMethodKey;
[GoMethodHost(typeof(http_package.contextKey)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛcontextKey;
[GoMethodHost(typeof(http_package.countingWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛcountingWriter;
[GoMethodHost(typeof(http_package.dirEntryDirs)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛdirEntryDirs;
[GoMethodHost(typeof(http_package.eofReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛeofReader;
[GoMethodHost(typeof(http_package.errorReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛerrorReader;
[GoMethodHost(typeof(http_package.exactSig)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛexactSig;
[GoMethodHost(typeof(http_package.expectContinueReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛexpectContinueReader;
[GoMethodHost(typeof(http_package.extraHeader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛextraHeader;
[GoMethodHost(typeof(http_package.fakeLocker)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛfakeLocker;
[GoMethodHost(typeof(http_package.fileHandler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛfileHandler;
[GoMethodHost(typeof(http_package.fileInfoDirs)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛfileInfoDirs;
[GoMethodHost(typeof(http_package.fileTransport)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛfileTransport;
[GoMethodHost(typeof(http_package.finishAsyncByteRead)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛfinishAsyncByteRead;
[GoMethodHost(typeof(http_package.globalOptionsHandler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛglobalOptionsHandler;
[GoMethodHost(typeof(http_package.gzipReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛgzipReader;
[GoMethodHost(typeof(http_package.htmlSig)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhtmlSig;
[GoMethodHost(typeof(http_package.http1ClientConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp1ClientConn;
[GoMethodHost(typeof(http_package.http2ClientConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2ClientConn;
[GoMethodHost(typeof(http_package.http2ConnectionError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2ConnectionError;
[GoMethodHost(typeof(http_package.http2ContinuationFrame)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2ContinuationFrame;
[GoMethodHost(typeof(http_package.http2DataFrame)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2DataFrame;
[GoMethodHost(typeof(http_package.http2ErrCode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2ErrCode;
[GoMethodHost(typeof(http_package.http2Flags)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2Flags;
[GoMethodHost(typeof(http_package.http2FrameHeader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2FrameHeader;
[GoMethodHost(typeof(http_package.http2FrameType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2FrameType;
[GoMethodHost(typeof(http_package.http2FrameWriteRequest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2FrameWriteRequest;
[GoMethodHost(typeof(http_package.http2Framer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2Framer;
[GoMethodHost(typeof(http_package.http2GoAwayError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2GoAwayError;
[GoMethodHost(typeof(http_package.http2GoAwayFrame)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2GoAwayFrame;
[GoMethodHost(typeof(http_package.http2HeadersFrame)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2HeadersFrame;
[GoMethodHost(typeof(http_package.http2MetaHeadersFrame)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2MetaHeadersFrame;
[GoMethodHost(typeof(http_package.http2PingFrame)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2PingFrame;
[GoMethodHost(typeof(http_package.http2PriorityParam)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2PriorityParam;
[GoMethodHost(typeof(http_package.http2PushPromiseFrame)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2PushPromiseFrame;
[GoMethodHost(typeof(http_package.http2ServeConnOpts)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2ServeConnOpts;
[GoMethodHost(typeof(http_package.http2Server)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2Server;
[GoMethodHost(typeof(http_package.http2Setting)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2Setting;
[GoMethodHost(typeof(http_package.http2SettingID)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2SettingID;
[GoMethodHost(typeof(http_package.http2SettingsFrame)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2SettingsFrame;
[GoMethodHost(typeof(http_package.http2StreamError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2StreamError;
[GoMethodHost(typeof(http_package.http2Transport)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2Transport;
[GoMethodHost(typeof(http_package.http2UnknownFrame)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2UnknownFrame;
[GoMethodHost(typeof(http_package.http2addConnCall)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2addConnCall;
[GoMethodHost(typeof(http_package.http2bufferedWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2bufferedWriter;
[GoMethodHost(typeof(http_package.http2bufferedWriterTimeoutWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2bufferedWriterTimeoutWriter;
[GoMethodHost(typeof(http_package.http2chunkWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2chunkWriter;
[GoMethodHost(typeof(http_package.http2clientConnPool)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2clientConnPool;
[GoMethodHost(typeof(http_package.http2clientConnReadLoop)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2clientConnReadLoop;
[GoMethodHost(typeof(http_package.http2clientStream)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2clientStream;
[GoMethodHost(typeof(http_package.http2closeWaiter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2closeWaiter;
[GoMethodHost(typeof(http_package.http2connError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2connError;
[GoMethodHost(typeof(http_package.http2connOutflow)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2connOutflow;
[GoMethodHost(typeof(http_package.http2dataBuffer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2dataBuffer;
[GoMethodHost(typeof(http_package.http2dialCall)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2dialCall;
[GoMethodHost(typeof(http_package.http2duplicatePseudoHeaderError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2duplicatePseudoHeaderError;
[GoMethodHost(typeof(http_package.http2eofReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2eofReader;
[GoMethodHost(typeof(http_package.http2erringRoundTripper)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2erringRoundTripper;
[GoMethodHost(typeof(http_package.http2errorReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2errorReader;
[GoMethodHost(typeof(http_package.http2flushFrameWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2flushFrameWriter;
[GoMethodHost(typeof(http_package.http2frameCache)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2frameCache;
[GoMethodHost(typeof(http_package.http2goAwayFlowError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2goAwayFlowError;
[GoMethodHost(typeof(http_package.http2goroutineLock)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2goroutineLock;
[GoMethodHost(typeof(http_package.http2gzipReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2gzipReader;
[GoMethodHost(typeof(http_package.http2handlerPanicRST)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2handlerPanicRST;
[GoMethodHost(typeof(http_package.http2headerFieldNameError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2headerFieldNameError;
[GoMethodHost(typeof(http_package.http2headerFieldValueError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2headerFieldValueError;
[GoMethodHost(typeof(http_package.http2httpError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2httpError;
[GoMethodHost(typeof(http_package.http2inflow)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2inflow;
[GoMethodHost(typeof(http_package.http2missingBody)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2missingBody;
[GoMethodHost(typeof(http_package.http2netHTTPClientConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2netHTTPClientConn;
[GoMethodHost(typeof(http_package.http2noBodyReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2noBodyReader;
[GoMethodHost(typeof(http_package.http2noCachedConnError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2noCachedConnError;
[GoMethodHost(typeof(http_package.http2noDialClientConnPool)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2noDialClientConnPool;
[GoMethodHost(typeof(http_package.http2noDialH2RoundTripper)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2noDialH2RoundTripper;
[GoMethodHost(typeof(http_package.http2outflow)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2outflow;
[GoMethodHost(typeof(http_package.http2pipe)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2pipe;
[GoMethodHost(typeof(http_package.http2priorityNodeRFC7540)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2priorityNodeRFC7540;
[GoMethodHost(typeof(http_package.http2priorityWriteSchedulerRFC7540)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2priorityWriteSchedulerRFC7540;
[GoMethodHost(typeof(http_package.http2priorityWriteSchedulerRFC9218)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2priorityWriteSchedulerRFC9218;
[GoMethodHost(typeof(http_package.http2pseudoHeaderError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2pseudoHeaderError;
[GoMethodHost(typeof(http_package.http2randomWriteScheduler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2randomWriteScheduler;
[GoMethodHost(typeof(http_package.http2requestBody)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2requestBody;
[GoMethodHost(typeof(http_package.http2responseWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2responseWriter;
[GoMethodHost(typeof(http_package.http2responseWriterState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2responseWriterState;
[GoMethodHost(typeof(http_package.http2roundRobinWriteScheduler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2roundRobinWriteScheduler;
[GoMethodHost(typeof(http_package.http2serverConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2serverConn;
[GoMethodHost(typeof(http_package.http2serverInternalState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2serverInternalState;
[GoMethodHost(typeof(http_package.http2sortPriorityNodeSiblingsRFC7540)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2sortPriorityNodeSiblingsRFC7540;
[GoMethodHost(typeof(http_package.http2sorter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2sorter;
[GoMethodHost(typeof(http_package.http2stickyErrWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2stickyErrWriter;
[GoMethodHost(typeof(http_package.http2stream)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2stream;
[GoMethodHost(typeof(http_package.http2streamState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2streamState;
[GoMethodHost(typeof(http_package.http2transportResponseBody)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2transportResponseBody;
[GoMethodHost(typeof(http_package.http2unencryptedTransport)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2unencryptedTransport;
[GoMethodHost(typeof(http_package.http2write100ContinueHeadersFrame)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2write100ContinueHeadersFrame;
[GoMethodHost(typeof(http_package.http2writeData)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2writeData;
[GoMethodHost(typeof(http_package.http2writeGoAway)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2writeGoAway;
[GoMethodHost(typeof(http_package.http2writePing)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2writePing;
[GoMethodHost(typeof(http_package.http2writePingAck)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2writePingAck;
[GoMethodHost(typeof(http_package.http2writePushPromise)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2writePushPromise;
[GoMethodHost(typeof(http_package.http2writeQueue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2writeQueue;
[GoMethodHost(typeof(http_package.http2writeQueuePool)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2writeQueuePool;
[GoMethodHost(typeof(http_package.http2writeResHeaders)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2writeResHeaders;
[GoMethodHost(typeof(http_package.http2writeSettings)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2writeSettings;
[GoMethodHost(typeof(http_package.http2writeSettingsAck)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2writeSettingsAck;
[GoMethodHost(typeof(http_package.http2writeWindowUpdate)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttp2writeWindowUpdate;
[GoMethodHost(typeof(http_package.httpRange)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛhttpRange;
[GoMethodHost(typeof(http_package.initALPNRequest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛinitALPNRequest;
[GoMethodHost(typeof(http_package.ioFS)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛioFS;
[GoMethodHost(typeof(http_package.ioFile)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛioFile;
[GoMethodHost(typeof(http_package.loggingConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛloggingConn;
[GoMethodHost(typeof(http_package.mapping<,>)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛmapping;
[GoMethodHost(typeof(http_package.maskedSig)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛmaskedSig;
[GoMethodHost(typeof(http_package.maxBytesReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛmaxBytesReader;
[GoMethodHost(typeof(http_package.mp4Sig)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛmp4Sig;
[GoMethodHost(typeof(http_package.noBody)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛnoBody;
[GoMethodHost(typeof(http_package.noopHandler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛnoopHandler;
[GoMethodHost(typeof(http_package.nothingWrittenError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛnothingWrittenError;
[GoMethodHost(typeof(http_package.onceCloseListener)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛonceCloseListener;
[GoMethodHost(typeof(http_package.pattern)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛpattern;
[GoMethodHost(typeof(http_package.persistConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛpersistConn;
[GoMethodHost(typeof(http_package.persistConnWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛpersistConnWriter;
[GoMethodHost(typeof(http_package.populateResponse)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛpopulateResponse;
[GoMethodHost(typeof(http_package.readTrackingBody)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛreadTrackingBody;
[GoMethodHost(typeof(http_package.readWriteCloserBody)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛreadWriteCloserBody;
[GoMethodHost(typeof(http_package.redirectHandler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛredirectHandler;
[GoMethodHost(typeof(http_package.response)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛresponse;
[GoMethodHost(typeof(http_package.routingIndex)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛroutingIndex;
[GoMethodHost(typeof(http_package.routingNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛroutingNode;
[GoMethodHost(typeof(http_package.serveMux121)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛserveMux121;
[GoMethodHost(typeof(http_package.serverHandler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛserverHandler;
[GoMethodHost(typeof(http_package.socksAddr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛsocksAddr;
[GoMethodHost(typeof(http_package.socksCommand)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛsocksCommand;
[GoMethodHost(typeof(http_package.socksConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛsocksConn;
[GoMethodHost(typeof(http_package.socksDialer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛsocksDialer;
[GoMethodHost(typeof(http_package.socksReply)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛsocksReply;
[GoMethodHost(typeof(http_package.socksUsernamePassword)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛsocksUsernamePassword;
[GoMethodHost(typeof(http_package.statusError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛstatusError;
[GoMethodHost(typeof(http_package.stringWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛstringWriter;
[GoMethodHost(typeof(http_package.textSig)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛtextSig;
[GoMethodHost(typeof(http_package.timeoutError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛtimeoutError;
[GoMethodHost(typeof(http_package.timeoutHandler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛtimeoutHandler;
[GoMethodHost(typeof(http_package.timeoutWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛtimeoutWriter;
[GoMethodHost(typeof(http_package.tlsHandshakeTimeoutError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛtlsHandshakeTimeoutError;
[GoMethodHost(typeof(http_package.transferReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛtransferReader;
[GoMethodHost(typeof(http_package.transferWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛtransferWriter;
[GoMethodHost(typeof(http_package.transportReadFromServerError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛtransportReadFromServerError;
[GoMethodHost(typeof(http_package.transportRequest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛtransportRequest;
[GoMethodHost(typeof(http_package.unencryptedHTTP2Request)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛunencryptedHTTP2Request;
[GoMethodHost(typeof(http_package.unencryptedNetConnInTLSConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛunencryptedNetConnInTLSConn;
[GoMethodHost(typeof(http_package.unsupportedTEError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛunsupportedTEError;
[GoMethodHost(typeof(http_package.wantConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛwantConn;
[GoMethodHost(typeof(http_package.wantConnQueue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httpᴛwantConnQueue;
// </MethodHolders>
