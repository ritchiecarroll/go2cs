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
[assembly: GoPositionMap("net/http/clientconn.go", "clientconn.cs", "AGXiAQAYApTGppKClIKmgpQADh6CgoK4AA0cgoKCgtyCkqaCgpSCpoKCgpT64oLcsgAKBoKClIKmgIK2gIKkgpSClAAEFgAMAoKCgpTcsqyyAAIgABECgtrigqKo8oKCgpSClIKUgpSqgoKClIKCgtaCgoLu3IKCgoKCAAUkAA4CgoKCAAwSooKmgoKmgoKClILmgoKmgqSkuKKCkoKU1qKCkoKUAAkGooKSgpTWlILWooKSgtaktA==", "345-350:1", "", "171=JoinHostPort/1/1/2", "464:29")]
[assembly: GoPositionMap("net/http/clone.go", "clone.cs", "AA4uAAkCgrgAAhgACwKClIKCgoKUAAIYAAsCgpSmgoKCgoKUlJQAAhgACwKClIKCggACHgAMAoKClA==", "", "", "72=Clone/1/1/1")]
[assembly: GoPositionMap("net/http/cookie.go", "cookie.cs", "ABYoACZYgoKCgrqCgoKUgoKClJSssoKUgoKUgqKCooKUgpSSgpSU6qKCgpSCooKUgoKUkoKU3IKCgqaCgoKUgoKCppSCgoKUlLS0tKS0grSCtIK0goKUgpSCtIKCgoKCgqaCtIK0gqSUAAYS4oKCyoKUgoKCpqyyggAdEOKCmKKCgoKClIKClILKgoKUgpSmgoKClIKCgoKUgpSClNi0tKSClAAOCLKClIKUgpSCgqaCgoK4goKmgoKmAAgWAAgCgoLcgoKUgqaCopSCgoKCgpSigoKUgpSSgpSmqJKClIKUqKSssoKUgqamlIKCgoKClLaCxsaClMaClIKUpJSCpqaUggAGIgAOAoKClKaC6qKmgqaigoKClIKClIKUgoKCpgACGAAMBIKClIKCpg==")]
[assembly: GoPositionMap("net/http/csrf.go", "csrf.cs", "ACxYkgAEGAAOAoKClIKUgpSClIKCgpSCAAwKtAADHgANAqaCgoKUgoK4AAIUAAkCgoKUAAgKwram+qSClKaCpqbupoKUyIK8AAgCgpS4goKUAAUS4oKCgoKUgpQ=", "207-217:1")]
[assembly: GoPositionMap("net/http/filetransport.go", "filetransport.cs", "ABI+AA0CAAIiAA4Cxu6CkoKUxoKCAAsYABEggoKUgpSmgoKUlIKUpoKmgoKUlIKmgoKUgoKU", "61-64:1")]
[assembly: GoPositionMap("net/http/fs.go", "fs.cs", "ADNksoKmgoKClIKClIKm3LiigoKUgoKUgoKUgoKClAAnRIKC+IKCAAsEiLKCgoKClIKCpoKCgpSkgoKCgoKCgsqClMqcwpSC3IKUgpSmgqYAAkQAHwKCgoKUgoKUlO64ABEM4oKCgqa4goKCgoSSgoKCgoKmgoKmgoKClJSCuIKCgsbKgpSCtIKmypQADBqCgoKUgoK0gpSCgoKCkrKygoKClIKClIKCpoLIABo2gpSUgrSqsoKCgpSCuIKC2KSmqqKqogAQGoKCgpSCgoKUgoKUgpSCgpSClKbmgoKClIKCyoKClOaCgoKUgoKCgpSCgpSClIKClIKUlOaCgpSCgpSCgriCgpTmgoKUgoKUgoKClMqClIKClIKUppaS5oKCuNyCgoKCgpTq1IKClIKClJSCgpSCxoKCyIKClAAKCNLKgoKmgoKCgpSkgoKCgqamgoKCgpSCgpSCgpSCuIKUgoK4goKCkoKCgtyCgoKUgoK4kqIACg7SgpSClIKm6qKClIIABjAAFwLcgpSCAAIuABYC3IKUpoKClIKCpqYADxyCgpSUgoKoxoKCtIK0goKClKaCgoKUpoKCgpSCgoKCgpSUlIKUgqbMsgACIAANAgACGAAJAsaigoKClAALEIKmggAJELiCgoKCgoK4AAoKooKUgoKUgoKCgoKUgpSSgoKUgoKUgoLcgpSCgpSClIKUgoKUpoKUgpSUgoKUgpSmlJSU/IKCysKCgoKClIKCpqKClA==", "161-161:1;247-257:1;376-394:1;761-761:1;896-898:1", "", "1162=contentRange/1/1/1", "428:25,809:21,853:29")]
[assembly: GoPositionMap("net/http/h2_bundle.go", "h2_bundle.cs", "AEZ+4oKUgoKmqJKClKqigoKmqqKClADYAsYF8gABqgSkABcwggAPIIIACRDElIKCgoKUlIKCgriClIKCpoKClIKCgoKClIKClIIAFCSylJSCgpSCksiSgpSCgoKUlAAEFgAKAoKCgoKmgoKClLiCpJSCgpQADRSClIKCgpSClIKCyJKCgqaClIKUgqbSgoKCgoKUgoKUpqKk0oLugoLGxIKCgoLKgpQACxCCzuKUlLiUprgAIEaiAAwagoKqogALGIKCgqaClIKmooK4goKCgoKUlIKUlIKqpqKCpqKClIKUgpSClIKUgpSClIKUgpSClIKUgpSClIK4ggAMGgAIEIKUpKSkpLiClLS0tLS0ABEiuKKClIKCgoKCgoKUgoKCgoKCpqaCgpSokqiSgriCgpSCgoKCgpSmgoKCgqaCgoIAFi4AEiKCgpSmgoKUAAoOggAPHJSCxoKClAASFAARHIIACAqCAAgKggAICoIACAqCyIIAEiaSAAIU8oKUhqKClIKmlIKCqqKClILMsoKUgoIACxSCgqaCgoKUgqaCgoKClIIAFCaCgpSUlIKCgpSmgoKUgoKUgqqigpSCpoKUppSC6gAQJgAOGoKClAAKEJIAGzgAGD4ADBqCgpQAGTyQ1IKCgoKC5qKCgoKCgoKUgoKUgoKUuIKUpoKCuNiUggAICIKCqsKCgtaCgoKUAFLAAYKClKaUAAsYxoKClLiCpoKClKaigoKCppSCgoKClKaUlJSCAAkWsoKUAAoOgoKUyJIACRKSgpSClILOwoKUAAIU8uqYooKUAAQaAAoCgoKClIKClJSClK7igpSCgoKUlIKCgpSUgoKUgpQAAhoADAKCgpSuwoKssoKCgqaCgsqCuIKmlIKUyAAOFoKuwoIADAaC3IKUgpSCgoKCgoKmyoKUgqiCgoKWgqaCAAQQ0gACGAALAoKUqqKClIKClIKClMqCgpSClIKClIKCABgagu6ClAAHEIKUgpSUgoK4lMaCpoKCgoKmqqKC3JaSgoK4goKCgoKCuJSCgoKClJSqooKCgqYABBAACAKCgoKUruKCAA8YAAkEgoKClIKClIKCxrKCgpSCggAPIMKCAAoGgoKClIKClAAJEKKCgoKCAA0e0oLGggARFIKCgpSC7oKClIKUAAsU9IKUgoIADxyCgqaCpoKmggAQBqKmyoKUgoKCgqaCgoKCgpSCgoKCgqaCgpSCABtMAAoCgpSCgpSClIKUgpSCgpSCgoKUgpSClIKCAAsUACVQggAKBoKCgpSCgpSCggALHOKClIKUgoKClIKCABEUgoKClIKClM7igpSCggAPFIKCgpTGgoKmgs7igpSCgpSCggAOFoKCpoIADgaipu6CmKKCgoK4goKClJSUgpSCABZAAAkCgpSCgpSClIKClIKUgoKCqsKCgsaCgpSmgoKUACtaooKClIKmqqKCgqaqooKCpqaCkoKCpLS0yoKCuIKUxoKClJTs8oKUpqKilIKCgoKigpSUlIKCgqaCgriCgqaCgoKC3JSClIKCgoKClJSmtIKCAAkUgoK43IKCuKaAgraClICClMiClICCpIKCgpSUgIKCgpSkAA0GgoKClIKCgoKUgpSCxoKCgpSCgsaClLS0xqSmAA4cgoKUxoKClIK4goKUgtiUooKCgpSCgoKUgoKClNaUgsrCpIKmgpSCAAgOlIKCgoLGtNiCtoKClIKCgpS0tLSCgqSCgoKmlIKClJSClIKClKaUgqiSgpQADySCgoKUgoKClIIAGDQAFTIACRCCAA8agqiklILGgsaCxoK2ABAiAAsUgoKUAAQYAAkCgpSCgpSCpqaClKSUAAwewqiSqJIADhqCAA0c7IKClKaigoKClKaCgoKUgoKCggAICoLM0oKUgoKCgqaCyoKClKqilKSklAAKEpSUtNwACAyUlJzigoKUgoKmxoKCggAhPPKCgoKUoqTSgoKClNoACAKCgoKUgoKUgpSCgoKUgpQACQqCugAIAoKCgpSCgsqClAAGEMCaoJiQlPKClIKCgpSClJSCgoKUlIKippKCuNbK4oKCgpTa8oKCgoKUpgARIIKCgpaUgoIAYN4BooKUgoKmooKUgoKmooKUgoKU6uiCgpSmgoKClAAMEAAIAoKUgpS4goKUppSCgriCgrq2ggAKFpSClIKmgpSCgp7SiJTuksqSgoKClJSClJQAHUKigpSmooKUpqKCgpSCpgAEIgAQAoKUAAgG0oKkgoIAGDKCpoLugqaClNyCgoKUgoKUgoKClIKCAAoWgoKmAAsYAAoWgriCuICCgqSmgqaCgqaixLKCgoKUxoKUgoIASZIBgoKClKaCggAmSpSUlKKmgpSCAAcQgoKmgqassoK4ooK4ooKUAAUQwoKUqqKCpoLugoKCgoKCysaigpSUlAAOFoKCgoKUgoKUgpSCgoKClAAPIsKCkJKCguaU5pSCAA8g4oKClJSmooKCuIKCgrjUgoKUgoKC2KTCgoKCgoKUgqbugpSmuIKmgoLcgpSCgqaCgoKmlIKkgoKCguSCgpS0tKaCpOiClIKCgsa0lLSUgqSCtLSCpLS01rTEAAgOgoLcgoKCAAsKooKCgpSCpoKCpoKmpoKm/oKCgoKCtpSUlJSCggAMDLrygpSCpIKCgoKUpoKStKSCgqYACQjswoKCgsqClILEtAAJEqS0toKClAACFPKCtMgABBgACgKWABQogoKC3JS0poKUyIKCpoKmlKzSgoKmgoKUAAgK1raCgoKCgoK4goLcgoKUgoKCgriCgpSCAAgOuMKCgpSClIKmlIKCgpQACxjKtLamgsbalAACGgAMAoKClIKCgoKClIKC3JSCgoKUgoKClIKUgqaCgoKUlAACFAAJAoKAABkqgqaigoKClJSCgoKmooKmooKCgr7SgoKCgoKUggAIEqaCgpSCgriUgqSCpIKCpoKCtIKUlAALCKKmgoKU7oSCgpSUppSkpKSkpKSkyLSC+LKCgpSCuJTclIIACAaygpSCypTclIKUlMaCtoLmspSC3JSCgpSmsoKClIKClIKUgpSUgoKCgpSCpoKUgoKUpoKCggAIBrKCgoK4lJS4lICCyIKCpqKCgpSClKaClIK0tLSktAALEIK25qKC7pQADAaygpSCggAKFtwACRSClJSUlJSCuIKClJS4lJSCpoKCgriClIIACBKCgpSClKaygoKUlKaCyJKqwoKUgqaClKqigpTOoqa8ogAVDrKC3ILcgqbcgpQABxCClJSCAAgSgpTupoKClJSCgpSmgoKUgoKUgpSClIKCAAkUgoKmprKCgoKCgoKUuILcggAOBrKCgoKUgoKmgpSCgoK4lKaCAAgGgsqUprKClIKmooKCpoLugoKCgqaCgoKUlIKmABgGwpQACBKCpoKCgpQAChamgoKClIKUgqaCgpSCgoKClKaUuKaylIKCpoKCpsoADRyCpqKCgoKCgoKCggASGtKCgoKCgpSClO6mooKCgoKCgpSCgpSUgpSCgpSCgrqygoKCgriUgpSCgsqCgoKClJSUgqIACASKwoLK0oKCypSAyqSCxIKkpLaosgAQILKCggALDrKCgqa6kqjCgoKCgpSClIKUABUkgoKCgoK4prKCgpSClIKClIIADx6CggAcPLKCppTGlIKCgqassoKUgpSCABEUAAkCgqaCpoKCgpKCgoKUpoKClKaCgoKUgpSmggAHEIKCgoK4ggAIEoKUgqaClILKgoKUgriC3JQAFk4AFQKCgpSCgqaCgoLYgoKmgpSigpSmgoKClKamgoKmgpSigpSmgoKClKamlKaCpoKCgpSCgtyCgqT6poKCgpSCgoKCgoKigqaCpoKCgpSClMgACxaC2IKCgpQACAaCgqamlJSCgoKCgqbupoKCgtiCgoKCgpQABBYACAKmguiSgoKUgpSClIKUpoKUuIKCgoKC2oKWABcEooKCuIKmgriClIKUgoK4goKUgoKUgpSClIKmgoLcjKaC3IKmAAcQxKTYxKSkggAQGLLKlIK4goLcgriCpoLugoKUgu6C7pSmgoKmAA4eooKClIKClKKC3AALFrKCgqaCgpSmgoL+soKIlAAKBqKClIKClIKClIK0gsSUgoKUggCQAcICgoKCgoKmgpSClKaCzsKCrLIACAaigriCgIKkgpSClIKUooKAgoKCyqSClJSClKiSgoKClJQACA6CxqKCpqKClACJAbYCooKUpsKCgqKkooKClIKmlLjCgoKSgoK0pKKClIKCkoIADRSygpSCggAPFtT6soKmAA0agsqigoKClIKCgqaCpoKUAAoIwuiCxqaCgoKCgpSCgoKCgoCUgoKUgoKCgrSCtILYAAoWgpSUgoKUvrKC2oKCgoKe4oK4gsqCgoKUgoLcgqamgoKUgpSUlAAIBqKClIKClIKClKaCgoKUgpSClOaCgoKCpoKClIKClIKUqqKmgoKUpqKmsoIAFCqCgpSCpoK4yoKCgoKUgoKUgoKUgoKmuIKClIKmgoKCgoKCgriAgoK2ksbCpoKSgoKCgpS0puKCgqKk4oKUgqaClIKUgoK4lLi4xq4ACgKCgtwACAKCgoKUggAkSuKCgoKUlIKCABci0oKC1qKClILKAAcQpgAIEoKmpoIAAhzigpSClKqipoKCqJKClKrqAAIS4qbCgpKCoqiigoKUgriigoKClIKClJSClKbSgoL2xrKCpoKCsoKCgoKCgpSClLSSgrSCpJSCgoLI0oKCgoKClKaClICCpICCttaigoKCgoKClIKUgrKCgoKCopIACAiqwoKCgpSCgqyyggAICLKCgpTqtIKC3MyygpSClMbSgoKipIKCuIKmsoIADRyUpKLEpKTIooIACRSUgoK4gqamgoKCAA0cgpSmgtSk7KSCxoKCpIIABhCygsYACxIADQKCmKKC3IKUgsSkpILI9qSmgoKUgoKCgpSCgpSUgqaCgoKUAAcQgoKCpoKClIKCguS0tLS0pIKCgriCgoKmuJSCgoCCgpKC2oL0pKSCtKSkAAgKwoKUgrbEpKQAChKCkpSClKaCgoKC9qIAECbilJTugoKCgpSClIKCgoKUgqa4pOiCgoKCgoKCABMogoKmgoKUlJSmlIKUlIKmgoKCgqaCysKCppSCgpSCgpSCgoKk/pKCgoKClIKCgtyUpoLsppQAAxDSgoKClMqUgpQADxqCgpSCgoKClMbigoKUgoKUgoKmgoKCgIKClIK2goKCgoIAABDygoKUgoKmgoKCgpSkgrS4goKCgoKUgoKCgoLulJSCuLjcgoKCgoKmgpKCgoKC3IKUlIKU3gAKAoKCgpKCgpSClMSkpMaCgpSCgoSUgpSClAAJCpKUgoKCgqaCpoKCpriCpqaCgpQADBTCgoKCgoKCuNKCgoKClIKCgriUgoKClIKmogAMEtKCkoKCgoK0AA4UgtiCgpSCxsKCkpSC3IKCgsqClAAHEIKClIKCkqaCgqaC+raClKaCtAAOCKKCgpSCgoKUgoKUgoKUgoKUxqKCgoKCgpSCgoKUgpSCgoKUlIKCgpSClIKCgpSmlLS0tLS0tLTEpIKClAAICrKCuJSCypSCypSUgpSmgoKCpsqUlJSCgoKClAAUEgAJAoKmgoKUkoKmgoKC7oKCgoKCgpSCpoLKgoKUyoKClLiCAAkUgoKUgpSCgpSmgoIACAqCAA4egpSCgO6WgoKUgsiCpoKCpoKClJSmgoKUgoKCgoKUprKUlIKmlKamgoKClJSCABASwoKUgpSCgoKCgoKUgpSCgoKCppSmgoKCgpSUgoKSgpSClJT2tIKClIKUgoKUgriUgpSCpgAKDKSUxrKCgoKCgoKClIIABxCCgoKCgoKUgoKCgqaUgoK4lIKCuJSCgoK4poKCgpiigqaCgoKmgriCgoKUlIKCgpSClIKmgoK4gpSm9oLKgoKCtKSigqaiggAKFuKCgtaCppSCgoKUxoKCgoKU2KKCgpSCgqaCptKmgqSClIKClNbSgpKCpIKCgpSmgpKClJS0grS0gpS0ggALFILGpJSCpoLKlIKm1tKCgpKCgqaCgpSClLi4poLWsoKUlIKCgpSClJSCyLKEkoKClJSCgoKUlIKCwoKCgoKUgoK0ktSkpLbo0oKCkoKkgoKUlIKClJSCgpKClNYABxDKAAgKgoKCgoKUgsiCtqKmosaigriixuiUAAgIlLSCgoKmAAgKlLQAEiCUtKaSgoKClKi2ooLK8oKCgpSCgoKmgoLY8oKCgpS0qPKCgoKClKKkwoKClJTmopQACgoACQiiqtKCgqaCABQagoKClKaCgoKCypQADBKCpoLmooKSgpQACQaikoKSgpSC1qKSgtqCtKSigpLWooKS9oKC2LiCpoKmxqKCgpSmsoKClIKCgoKClJSmooK4ooK4ooK4ooK4ooK4ooKUyqKmgoKUggAPHAALAoaClAAeRrKUpNqk+oKm+IKCqIIACxCCgoKmAAsQgqaCpoIADBKCprSCpgAJDKKmogAKCoKmggAJCoKmur70goKCgpSCgpSUAA8igoKUxgAHEAAMBqKClIKmlIKUgpSCpoKCpqaCgu4AEyKUAAwGooKUgoKCgpSCgqamgoLuAA8QgoKCggAHEJQACxCUgsrCgriClIKCgqaUuJSCgqamgpTGADh8ooLKlJSqoqqigpQAAhoACgKmgoK4goKUgoKUgpSCggANHAAIEsqCqJKCgpSUqqKClNakABw0gqaCpoKClIKUgoKCpoKClIKUrsKClIKUpLSkAAgQ0oKUgpSCgoKokoKClIKCgoIALGDSpu7cgoKUlAAaOLKClIKmgoKUlILcgoKClIKCgpS4ooKCAAMUAAkCgpSCyoLcgoKCgoKmgoKCpsqCgoKUgoKUgoKmAAgKlJSmgoKClIKUAB1AtIKClILugoKU3IKCgriigpSClIKmgoKUgoKClLiigtyCgoKUgtyCgsqCgoKCuIIACRSCgoLugoKCgoKUuIKmooKClIKCuIKUpqaygoKClIKClLiCgoKUgpSUpqKClJSCgoKUpqKClIIAHkKCpsaCgpSCyoKCgoK4goKCuIKCgoKUlJSCgoKmgqaCgoKCuJSUgoKCyoKCgoK4goKCuNyCgoKUgriClIKUppSCAAgSpoK2gpSCgpSCgrgACRSUlIKCzM7CAA8guIKCgpSCpriCgoKUgoKCgpSmlIKmgoKCgpSmABAu0qbGgoKUgoKCgoK4goKCuIKCgpSUlIKCgqaCppSCgoKUgriClIKUppSClIKUgoKCgpSCgqY=", "2063-2063:1;2069-2075:2;3226-3276:1;3278-3278:2;3356-3363:1;4308-4308:1;4359-4381:2;4382-4384:3;4388-4400:4;4909-4909:1;5191-5201:1;5547-5547:1;6478-6505:1;6629-6636:1;6906-6920:1;6932-6946:1;6995-6998:1;7274-7319:1;7378-7380:1;7639-7655:1;7659-7661:2;7663-7670:3;7860-7863:1;7890-7893:1;8570-8584:1;8637-8643:1;8745-8754:1;8756-8781:2;8783-8804:3;9018-9020:1;9609-9611:1;9829-9831:1;9915-9917:2;10311-10347:1;10438-10449:1;10684-10688:1;11947-11969:1", "", "1742=Uint32/1/1/4;2102=Sprintf/1/1/1;2108=Sprintf/1/1/1;2355=Uint16/1/1/1,Uint32/1/1/2;2527=Uint32/1/2/2,Uint32/2/2/3;4769=RemoteAddr/1/1/5,String/1/1/5,http2newBufferedWriter/1/1/6,handler/1/1/7,http2newGoroutineLock/1/1/21;5336=maxHeaderListSize/1/1/3;6964=PseudoValue/1/5/1,PseudoValue/2/5/2,PseudoValue/3/5/3,PseudoValue/4/5/4,PseudoValue/5/5/5;7051=WithContext/1/1/13;7839=hasTrailers/1/1/4;8009=http2asciiEqualFold/2/6/1,http2asciiEqualFold/3/6/2,http2asciiEqualFold/4/6/3,http2asciiEqualFold/5/6/4,http2asciiEqualFold/6/6/5;8024=http2cloneHeader/1/1/4,getErrChan/1/1/5;8129=RequestURI/1/1/4,http2cloneHeader/1/1/5;9106=Now/1/1/18;9449=tooIdleLocked/1/1/5;9806=http2actualContentLength/1/1/6,ContextClientTrace/1/1/7;10220=http2actualContentLength/1/1/7;11072=New/1/1/3;11166=StatusText/1/1/5;12963=Bytes/1/1/2;13613=get/1/1/1;13668=get/1/1/1", "4659:23,5327:21,5330:21,5370:25,5376:25,5383:21,5615:21,8490:27,8497:27,8515:23,9211:21,9514:21,9680:25,10116:29,10856:21", "7161:64,7162:13,12275:21,12276:13")]
[assembly: GoPositionMap("net/http/h2_error.go", "h2_error.cs", "AA0agoKCgpSCgoKClIKCgoKmgoKU")]
[assembly: GoPositionMap("net/http/header.go", "header.cs", "AB48wgACENIAAhLiAAIQ0qiSgpSqooKssqiSpoKokoK4goKUgoKCpoKUgoKU1gAFEuKCgoKmpgAIDoIADhru4oKClIKCgqaCgqyypqKCgpSCgoLKlKKCgoKCgqaCpoKCpoIABBTgnMKClIKU7oKmgqaClIKmpoI=", "179-179:1")]
[assembly: GoPositionMap("net/http/http.go", "http.cs", "ACdSgJaAloCWgJaAloCUgoKUuKKCgpSClIKUAA8cABEguJCYooKUqKSokoKCgqamooKCgpSmgpSCgoKCgpSCgqaClO74goLIgg==")]
[assembly: GoPositionMap("net/http/mapping.go", "mapping.cs", "ABwykoKUgoKClJS+8oKUgoKUgoKmqsKClIKCgriCgg==")]
[assembly: GoPositionMap("net/http/pattern.go", "pattern.cs", "ACZKlIIAMFgAFQKClIKCgriygpSCgpSClJSClIKClIKCgoKmuIKmgpSCgpSClIKClIKClIKmgpSClIKCgpSClIKClIKUgpSClIKm5oKCpoKCpqaCgpSUABI2AA8CuJSCpoKUgpSCAAIUAAkCgpSUlIKUlJSClKrmgqiigoKCgtyCyoKUgpTIkoKUgpSClIKUgpSUlIKClKaClAACFgAIApSkpIKUpKSkpMbMopSkpLqygoKCgpSClIL8gpSClKiSgriCgoK8ooKSgoKUpoKCgpSqopSSgoKClIKWyoKCgpSmlIKCpoLKgpSClIK4gpSmpoKClA==", "89-93:1", "", "471=commonPath/1/1/4,differencePath/1/2/4,differencePath/2/2/4", "", "117:21,118:13")]
[assembly: GoPositionMap("net/http/request.go", "request.cs", "ACxSAAsUlpIAChTKuLjKysqWpgDeAfoDAAsCgpQAAhgACQKClIKCggACFgAIAoKUgoKCgoKCgIKCgqSCgqaAgoKCpIKqouqSqJKqooKU2LzCgpSClAAGEgAIAoKClAAHGAAIAvwADBLCgpSClIIACgaCgoKUgpSCgpSCgpSqosiSgpQACy4ADgIAAhLi6gANCgAJAoKCksqCgoKUggAHEIKCgpSUgoLuAAwagpTulIKCgpSCgqaCAAUUwoKCpoKCuIKClILKgoKUgoKCgoKUgsqCgpSCgqaCgqaCgoK4goKmgriCgoKCpoKUgoKCuICCgtqCgoKClKaClAAPEAAJFIKUqqKClIKClIKClOyylKSUgpSClIKUgoKUgoKUpgANHKiSAAY0ABcCuJSClIKUgoKUgoKmggAKFoKUgoKSgsaCgpKCxoKCkoIAFCKCgrgACAyygoKUAAgeABAClIKUgoKUgoKClAAEHAALAsjSgoKClNqCgoKClKaiggACEuKCgqaCAAwcABECgpSWkoKUgoK4goKClIKUgoIACxiCgqaCppS4goKUgoIACRSCgqaUlIKCppTclAAGGgAKAoKUAA0QlAAZFrKClILcgpSUgoKCpoIAABKIlIKmgsaCggALCLKCgpSmgpSClIKCgoKUgoKClJSCgpSCggAMFAAEKgAUAoKCgpSCpoKCgpSCgoKCgqaClIKUpgAGFgAKAoKUgqaUgqaCgqaCgqaClIKUppQAAhwADQKClIKUAAIQ8oKUgpSqwoKUgoKCpoKCgqauwoKUqqKClIKUvMaClIKCgoKUpgAIBoIACAaCgpTmgoKUpoKClAAIBoKCpMqCpqqigpSClAAEFPKUlAAMCqI=", "585-589:1;592-599:2;928-931:1;935-938:2;942-945:3;963-963:4;1090-1094:1", "", "1789=Get/2/2/1,EqualFold/1/1/1", "", "637:25,638:17,644:21,645:13,1215:21,1216:13")]
[assembly: GoPositionMap("net/http/response.go", "response.cs", "ABkwAFzKAZLq/MKCgpSClAAKEuKCyoKCgpSUgoKUgpSCgpSCgpSCuIKCgpSUlJSCgqYACBTygoLuogAPKAARBIKCgoLKpoK4goKEkoKClKaUggAJGoK4goKUgoK4goLKgoKCyoK4goK4poKCAAMW8oKqosqi6qI=", "", "", "310=NewReader/1/1/4,MultiReader/1/1/4;403=HeaderValuesContainsToken/1/1/1")]
[assembly: GoPositionMap("net/http/responsecontroller.go", "responsecontroller.cs", "ABRMABECAAwQkoKClKSCpMTOooKClKTEAAwU0oKClKTEAAwW4oKClKTEAAweAAoCgoKUpMTuog==")]
[assembly: GoPositionMap("net/http/roundtrip.go", "roundtrip.cs", "AAsoAAwS8oKU")]
[assembly: GoPositionMap("net/http/routing_index.go", "routing_index.cs", "ACI+ooKUgpSCgoKUAAQcABIOgoKUgoKCpsqClMqe4oKCgoKUgoKCgoKCgriCgoLKgpQ=", "64-75:1")]
[assembly: GoPositionMap("net/http/routing_tree.go", "routing_tree.cs", "AClY1JSUrNKCgpSCgoKUgoKCgpS8ooKUgqrCgoKUlIKUgoKqwoKUgq7iuIKm7NKClICUpJSCuK7igtyCgpSmyoCCAAgQgoLKpoKUlM7CgpSCgoKUAAgKwoKUgoK4goKUkoKU", "233-238:1")]
[assembly: GoPositionMap("net/http/servemux121.go", "servemux121.cs", "ABgyvKKCggASJOKClIKUgpSCpoKUgoKCpoK0xIKCloKmgoKCyJKClKjWuIKmyoK4gIK2goKCpqoACAKCpoKUgpSClNq0goLKgoKmrvKCgoKClIKCrLKUgoK4goKUgoK4", "82-84:1")]
[assembly: GoPositionMap("net/http/server.go", "server.cs", "ADNW7tzKALQB7gLcAC940oKC2OKClJSCgpSCgqaClIIAIUiCtrKClJSUgoKCgqaCgpSClKaCgpSmgoKUgpSCgrgAR54BgqaCpoKCABYoooKCgoKUpoKClIKmrLKClJSqoqrCgoIADBgACQKCyoKC7oKCgoK4gqaCgoKCpoKCABEQkriClAAgPKKCgrigpNKCgqKk0oKCgpSClIKCoqSigoKCABcwpoKUgoKCptKCgoKUgoKClKKkgoIAARgACgKClIKCuMKCgoKUgoKCgpSUgoKUgoKUgpSCgoKClIKClIKCgpSClIIADRakgqaCgpSmgpSklKaCgoKCuKaCgqaCgoKCgoKmpqKCggAKEoKClKaCAAIQ0oLKgpSCpgASFqKClIKCgoKCgpSUpqKCAA0WAA8G8oKmgoKAgqSAgoLIgpSClIKCgpSmgqaClIKCgpSClIKClIKCuJSCgoKCgsaCxLaUAA0cgpSCggAICqKCuIS4xoK4lIIADxwAChaC/KKCgoKCgoKClIKmAAgGooKCgpSCgoKUlKbugqaCgpSmgpSCpoKCgpSCABIi3oIAAxDygoKClIKCgpSCgoKCggAcGgAIAoKUlIK4gu6CgoKUgpKCgpSClIKUlKaCgoKClIKmgoKmggAPIIKCyoKCgsqUgoKClIKmggATKIKmlKaCABAigpKClJTGtKSUgoKCpoKCgriClLiCgoKmgriCpqaUgqaClIKCgpSClILcgoKCuIKClO6CuIKUgqaC3IaCuIKUuIKCgsqigoKUgoKUkoIACBLigpSUgoKCgqbcooKUAAJKACICpoKosoKCgpSmlKaClIKUgqaCgpSClLiilIKmgoKClILclIK8wrimlMqCpoKmpoKmgqaigpSCgoKUxoKmgqaCpoK6koIAFSgAAxDigoIAFi7MspSUAAcQooKUtKSClIKCgpSCuIKCzKAAChL8nMKClICCpICCpAAcENKClIKCgoKCgoKUgoKUgoKClIK4goKCgoKUiLKCgoKUlIKmgoKUgoKCgriClO6CgoK4goKkgoKUgIK2goKCpoKmgoKUlIKUgpSe0oKC7riCpqaCgpSCgsqCgpSCgpSCgqbKgpQACRSCgoKCgoKUgoKCgpSUgpTKpoCClP6CpoCClAAWHpSigpSClAANFILGggAJDqKCgpSCgoKClIKUgpSCgoKCggAKBgAOGoKCqgAJAoKUgoKmgoLIgoKClNbSgoKClIKCgqbW0oKCgpSCgrQADBKSAAweAAwCAAgSuIKCgqiAmJAAARDigpSCgoKCgoKCgoKClAAEIgAOAtyCgoK4lIKmgoK4goKClLjKlIKClKaCgrgACBSCAAoSggAEEuIAgAGEApLIypKClIKUppSClKaopIKUgoKUAAQuABYCgpSC7vKCgoKUuIKCuLiCmKKCgpSUgoKUgqa4goKSgqaUAAISAA0CgpTclIKCyqYACDoAHAKC3ILKgu7I5oKCgpSClAALCsKCgpSClIKClJQAAhYACAKClAADEtKClNyigpS8ooKU2IKCAAsI0oKUgpSCpoKCyoKClKaClICSgoKmlKSCggAHHAALAoIAAh4ADAKCAHKaAgAOAoKCgtyCgpSCgpQAEkAAGAKUgoKCpIKUgpSUgoKUpoKSgoKUtKQACBTygoKq8oKCgoK4gpSmgpSClNaCgoKCpgAqVgAJEIIADCQACwKCgpSCpsYAARYACQKCgoKCgoKClAAMGgAKAoKUgoKUgoKUAAgOou6UggAIEuoAAxgADwKCpoKClICCtoKUlIKCgoK4lIKCgoKClIKClJSClIKClJSCgoKCpoKCggAHJAASBoCCtoKUgoKCgoKCuIKmgoLKgsqCpoKCgpTK/IKCkpSClLSClKSUgpSClAAEGgAPAoKCgpSCgpSClIKU1tKCgoKUgpS0pIKClKaCgpSmgqaCrsKCgpSmyqKClN7igoKUAAMW8oIAAhDSggAIJAATAoKUgoKmgoKmpNzSggACFgAKAoKmooLYutKClIKClIKClLiUggAEHAALAgAIEgAQGIKClKbSgoKCgqSCgsqC0pKCpoKiksS0gpKCgoKUgrSCkqSCgrSC5gAPHLaSgpSmlNKCgoKUgpTWgpSUpIKCxoLI0oKCogAWFKKCpgANCqKC3IIAECKwlKKCgpSClIKUACQWlqKCgoIACgyygoKCprKCgoKmooKCggANFLKCgoKUxqKCgoKUlKrCpJSokoKCgg==", "979-981:1;1271-1283:1;1896-1915:1;2168-2173:1;2318-2332:1;2685-2688:1;2894-2901:1;3142-3151:1;3306-3317:1;3521-3535:1;3811-3819:1;3812-3816:1.1;4048-4052:1", "", "1165=wantsHttp10KeepAlive/1/1/11,wantsClose/1/1/12;1688=get/1/1/1,hasToken/1/1/1,isProtocolSwitchResponse/1/1/2;4619=Sprintf/1/1/1", "2218:21,3554:21,3841:21,4199:21,4331:25", "1096:29,1097:21,2142:64,2143:13,4348:29,4349:21")]
[assembly: GoPositionMap("net/http/sniff.go", "sniff.cs", "AA8q4oK4gqaCgriqopSUqqKUlAAfEgA7kAKCgpQADBK4gpSClIKUgoKCpgAMCoKCgpSCgoKUgriClMaCAAoIpoKUgoKUgpSClJSCpgANCpSCmqY=")]
[assembly: GoPositionMap("net/http/socks_bundle.go", "socks_bundle.cs", "ABQuggAPBuKCgpSAgoKkgoKCooKCpqK0grTagoKClIKClIKCpoKmgpSClIKClIKCuIKCgoKCgoKClKaClIKClIKCpoKUgpSClIKUgoKUgrSCtIKUtJSClJSClIKUlIIACAaCgoKUgoKUgpQADQyClKSkACEUgpSkpKSkpKSkpKQAIzzUooKUgoKUAAwYwoKUABxEAAwCgoKUgoKUgoKClIKUgoKUgoKCgpQAAhIACAKCgpSCgpSCgoKUAAISAAgCgoKUgoKClJSCgpSCgpQACAaCxpTGlKbCgpKClIKCgpSClKbKogAWIqKUpIKUgoKCgqaAgqSAgqSClIKUlA==", "39-44:1;45-53:2", "", "", "", "60:25,61:17")]
[assembly: GoPositionMap("net/http/status.go", "status.cs", "AMkBogGilKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSk")]
[assembly: GoPositionMap("net/http/transfer.go", "transfer.cs", "ACI+/IIACxCCgpSClIKCABcuoqaClIKUgoKCgoKCgoKCAAgSgqa0goKUgoKCgoKCgoLIgoKCpoKUgoKCyoKmAAQqABYGgpSClLiC3AACJAARAoKCgoKCgpSClIK0gpSCgoKClJSCAAgOuOiCxoKClIKUgqaClIKClKYAFgaigoKUgu6CgIKkgIKkgpSCgpSCyoKCooKUlJSCpoKUgsqo4oKCgoKUggAHEIKCgoKUgoKClIKCgpSUgoKUgoKUgqaCgoK4griUgoK4lN4ACgKClIKClAAFENKAgqSAgoKkABUkgsqilKSklKiCgpaCpqSUqLKmgpSCgoKCgoKCxoKCgqaCxMiCuIKmgoKUgoKUpriCgtyUlOyUgpTGtMaUptqUgoKClIK0goKClIK2qICWgAAIDoLKooIACgiSgoKUpoLugpSCpoLM4oKmyoKCgsqClLiCgoLKgpSClJQADyCCgriCppQABxC47LKCpoKCgqaCpgAICJKCgpQABxCUlIKCgpKClIKCtqaClIKUABYwtOKClIKCgpTowoKUlIKUgoLKgpS4ggAJFIKCgriCpsiClqKmgoKUgqam9JSCgoKUgpSCAAoWgqaCgoKUlJS0pMaCgoKUzLKClKbSgpSCgoKUggAMEqaUpoKCgoKm/KSC1tKCgtrygpSCgtbSgpSCgqIADBCCgpTG+rKClLiCgoKUlIKClAAKErKClIKCgpSClMbCnqKUpL6ytJSClIKUAAoOsoKCgoKm", "210-219:1;341-348:1;787-797:1", "", "", "", "405:21,406:13")]
[assembly: GoPositionMap("net/http/transport.go", "transport.cs", "ADZcAPEBmgSCgpSmgoKUpoKClKiyggAWLoKUgoKUgoKUgoKClJQAEBqCxvjCgoKCAAcQgoKCgoLKlJSCgpSClIKCgpQABxCCgoKUAAgSpoKClIKCuIIAEBakAAQmABICqqIAESiCgpSmooKClMrCypSs4oKUgsaCgoKUgqa4AA4IAAgCgoKUgoKUgoKUgoKUgoK4goK4gpSCgpSCgoKmgoKUgoKUgoIACximgu6UkoK4gqSC6oKCgoLugoKCpoKUlJSC3JSCuIKClKaAgqSAgqSAuKSUpoKCAAoKgrT6AAkQgoKmooKUzuKClIKCrvKClIKUgpSCgpSCgszS7pSUlAAHEJSmlJSUppS4lNgAAxgADwKCgoKClIKClIKirOKCgoKCgoKCgoKmgoKCpoKCugAJDoKCgoKUgoKUgoIAAhIACAKCgoKCAAweooKUqJKCxsKCgoKUgsqigpSCgoKU2oKCgoKCgoLcAA8elILGgoK4goKUAAIQAAoCgpSClIKUgoKCgpSC1qSmgsqC7oKCgqaCgoKCAAcQgoKmgpSUgriClIKUgoKUgoKmgoKCgoLcgoKUpoLcAAkCgqaCuJSUrLKCuICCgoKCyoK4pNyClIKC3IKmlIKUlILagpSCgoKC6PKClIKC2LKClIKCgoLYgoLGgoK4goKCtgAPCoKCgoKUlIKCgpSUACFC4oKU2OKClNjigpSClIKUgpSClNrCgoKCgqaUgoLKggAYLJKokqiSgoKmlIKCgqiSgpSClKrCgoKClIK6koKCgpS8ooKUggAJCLKClJSClK4ACgKCgoKCAAcQlAAIEpKCyoK42IK4goKUlLikgoKU6qSCgpT8AAgClIKUgoKmgoKUgoKmgpSCgoKiqMKCgqKCgoKivPKCgoKCpoKCgriUgrSo8oKmgoKCpu6CgoKCgoKCpoK4lILKgpS0yuSCgpSClIKCgoKAgpK2soKUgoKUlICCgqaUgpSkgoKUgoIACA4ADwTiAAoWgoKUlJSCgoKClICmgpSAgoKClKSCgpS2goKUgoKCgpSC3NiCgoCCpoK4tIKCxoKAgoLogoKCgoKCgqaUgpSAgoKkAAkUgqSCqtKCgoK4gqKStIKC2IKCpoKCgoK4goKCgpTIgoLKmoKCgoKUgoKClKaCgoKUgpSUpoKCgpSUuIKUgoIAFBqygoKs4oKCxgAeOoKCgoKCgqYABxKSgpSokoKUqqKCgpQADhaEkoKUADNqgqaygpSClIKClIKosoKCgqrygoLYsoKCgqbSgoKCoqrygoKSlJSCogACFAALAoIACRTKgriCgoKCppSmgoKmlIKClJT8AAsEwoKCgoKCuIKCgoKClJSClNyCpoKClIKCgpSCgoKClJSClIKClIKmgoKm5pSUlIKClIKClAAHEKbcmoKm5qbKgqaC9oKCqIKCgoKCgqa4goKCgoKCpubcxIqCxoK0toK0pIKClIKCgoKUppSU3rKClIKUzPKCgriCgoKClIKCgpSClJSCgoKCAAgSlJSUgpQADBqClLiCrLKClLKCpMSkpAAKCqKCgpQAFxyygoKUgoKUlAAKBoKClAALEILGwoKCtIKCggAHEJSClIKCpoKCgoLGABQe4sgADBiCkrSkADhogoKCtMiCuJSokoKSgpSCtoLEpKQADgjylIKCgoKClJSClIKClILuggAMIIKmgoKmhqaClMqCgpSCAAcQgoKUgpSClKaCgoKCguSClIKClICCgpSCktbaxoKUpIKUgqSk2sYAEhKigtzCgoIAAhAACgKCgqKkgoKUgoKmgoKUpsaClKSkpLiigoKUqLKCgpQAFi6CtOKCgoKClIKmgoKCgoKUlNbSgoKClIKClILYkoKUgoIAFSKCtKaSgoKClKi2ooLK8oKCgpSCgoKmgoLY8oKCgpS0qPKCgoKClKKkwoKClJTmopQACQqCwgALDoIAAx4ADQKClAAJEpKCgpSCgpSmgoKCgoKokoKCupI=", "514-516:1;667-671:1;911-915:1;930-935:1;972-974:1;1540-1544:1;1628-1633:1;1728-1730:1;1732-1741:2;1782-1788:1;1856-1858:2;1899-1909:3;2293-2299:1;2301-2314:2;2417-2422:3;2423-2434:4;2590-2602:1;2898-2909:1", "", "55=defaultTransportDialContext/1/1/2;323=Clone/1/1/16;1735=key/1/1/2;1764=isReused/1/1/2;2050=key/1/1/2;2126=Username/1/1/1;2257=UnencryptedHTTP2/1/1/2,HTTP1/1/1/3;2771=wroteRequest/1/1/2,Invoke/1/1/3;2850=wroteRequest/1/1/3,Invoke/1/1/4;3354=Get/1/2/1,Get/2/2/2;3376=wantsClose/1/1/1,isProtocolSwitchHeader/1/1/2", "695:33,2084:39,2189:25,2201:33,2697:21,3001:25,3108:21,3192:25,3381:21,3440:37", "705:21,706:13,1745:21,1746:13,2670:21,2671:13")]
[assembly: GoPositionMap("net/http/transport_default_other.go", "transport_default_other.cs", "AAscgg==")]
// </GoSourcePositionMaps>

namespace go.net;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using context = context_package;
using tls = crypto.tls_package;
using base64 = encoding.base64_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using log = log_package;
using ascii = go.net.http.@internal.ascii_package;
using url = go.net.url_package;
using reflect = reflect_package;
using slices = slices_package;
using strings = strings_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using time = time_package;
using crypto;
using encoding;
using go.net;
using go.net.http.@internal;
using go.sync;
using Δhttp = http_package;
using static http_package;
using net = net_package;
using httptrace = go.net.http.httptrace_package;
using go.net.http;
using godebug = go.@internal.godebug_package;
using textproto = go.net.textproto_package;
using strconv = strconv_package;
using go.@internal;
using fs = go.io.fs_package;
using go.io;
using math = math_package;
using mime = mime_package;
using multipart = go.mime.multipart_package;
using os = os_package;
using path = path_package;
using filepath = go.path.filepath_package;
using sort = sort_package;
using go.mime;
using go.path;
using iter = iter_package;
using bufio = bufio_package;
using bytes = bytes_package;
using flate = compress.flate_package;
using gzip = compress.gzip_package;
using rand = crypto.rand_package;
using binary = encoding.binary_package;
using bits = go.math.bits_package;
using mathrand = go.math.rand_package;
using httpcommon = go.net.http.@internal.httpcommon_package;
using runtime = runtime_package;
using httpguts = vendor.golang.org.x.net.http.httpguts_package;
using hpack = vendor.golang.org.x.net.http2.hpack_package;
using idna = vendor.golang.org.x.net.idna_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using compress;
using go.math;
using vendor.golang.org.x.net;
using vendor.golang.org.x.net.http;
using vendor.golang.org.x.net.http2;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸhttp2Setting = Span<http_package.http2Setting>;
using utf8 = go.unicode.utf8_package;
using go.unicode;
using unicode = unicode_package;
using maps = maps_package;
using urlpkg = go.net.url_package;
using @internal = go.net.http.internal_package;
using list = container.list_package;
using httpproxy = vendor.golang.org.x.net.http.httpproxy_package;
using container;
// </GlyphForwarderUsings>

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

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class httpᴛClient {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Response> resp, Func<bool> didTimeout, error err) send(this ref Client c, ж<Request> Ꮡreq, time.Time deadline) => http_package.sendᴍ(ref c, Ꮡreq, deadline);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static time.Time deadline(this ref Client c) => http_package.deadlineᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static RoundTripper transport(this ref Client c) => http_package.transportᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Response> resp, error err) Get(this ж<Client> Ꮡc, @string url) => http_package.Getᴍ(Ꮡc, url);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error checkRedirect(this ref Client c, ж<Request> req, slice<ж<Request>> via) => http_package.checkRedirectᴍ(ref c, req, via);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Response>, error) Do(this ж<Client> Ꮡc, ж<Request> req) => http_package.Doᴍ(Ꮡc, req);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Response> retres, error reterr) @do(this ж<Client> Ꮡc, ж<Request> Ꮡreq) => http_package.@doᴍ(Ꮡc, Ꮡreq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Action<ж<Request>, bool, bool> makeHeadersCopier(this ж<Client> Ꮡc, ж<Request> Ꮡireq) => http_package.makeHeadersCopierᴍ(Ꮡc, Ꮡireq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Response> resp, error err) Post(this ж<Client> Ꮡc, @string url, @string contentType, io.Reader body) => http_package.Postᴍ(Ꮡc, url, contentType, body);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Response> resp, error err) PostForm(this ж<Client> Ꮡc, @string urlΔ1, url.Values data) => http_package.PostFormᴍ(Ꮡc, urlΔ1, data);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Response> resp, error err) Head(this ж<Client> Ꮡc, @string url) => http_package.Headᴍ(Ꮡc, url);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void CloseIdleConnections(this ref Client c) => http_package.CloseIdleConnectionsᴍ(ref c);
}
partial class httpᴛcancelTimerBody {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ref cancelTimerBody b, slice<byte> p) => http_package.Readᴍ(ref b, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ref cancelTimerBody b) => http_package.Closeᴍ(ref b);
}
partial class httpᴛTransport {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<ClientConn>, error) NewClientConn(this ж<Transport> Ꮡt, context.Context ctx, @string scheme, @string address) => http_package.NewClientConnᴍ(Ꮡt, ctx, scheme, address);
}
partial class httpᴛClientConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ж<ClientConn> Ꮡcc) => http_package.Closeᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Err(this ref ClientConn cc) => http_package.Errᴍ(ref cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Response>, error) RoundTrip(this ж<ClientConn> Ꮡcc, ж<Request> req) => http_package.RoundTripᴍ(Ꮡcc, req);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Available(this ref ClientConn cc) => http_package.Availableᴍ(ref cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint InFlight(this ref ClientConn cc) => http_package.InFlightᴍ(ref cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Reserve(this ж<ClientConn> Ꮡcc) => http_package.Reserveᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Release(this ж<ClientConn> Ꮡcc) => http_package.Releaseᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Action<ж<ClientConn>> shouldRunStateHook(this ж<ClientConn> Ꮡcc, bool stopRunning) => http_package.shouldRunStateHookᴍ(Ꮡcc, stopRunning);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void maybeRunStateHook(this ж<ClientConn> Ꮡcc) => http_package.maybeRunStateHookᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetStateHook(this ж<ClientConn> Ꮡcc, Action<ж<ClientConn>> f) => http_package.SetStateHookᴍ(Ꮡcc, f);
}
partial class httpᴛhttp1ClientConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Response>, error) RoundTrip(this http1ClientConn cc, ж<Request> Ꮡreq) => http_package.RoundTripᴍ(cc, Ꮡreq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this http1ClientConn cc) => http_package.Closeᴍ(cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Err(this http1ClientConn cc) => http_package.Errᴍ(cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Available(this http1ClientConn cc) => http_package.Availableᴍ(cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint InFlight(this http1ClientConn cc) => http_package.InFlightᴍ(cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Reserve(this http1ClientConn cc) => http_package.Reserveᴍ(cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Release(this http1ClientConn cc) => http_package.Releaseᴍ(cc);
}
partial class httpᴛCookie {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Cookie> Ꮡc) => http_package.Stringᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Valid(this ж<Cookie> Ꮡc) => http_package.Validᴍ(Ꮡc);
}
partial class httpᴛCrossOriginProtection {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error AddTrustedOrigin(this ж<CrossOriginProtection> Ꮡc, @string origin) => http_package.AddTrustedOriginᴍ(Ꮡc, origin);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void AddInsecureBypassPattern(this ж<CrossOriginProtection> Ꮡc, @string pattern) => http_package.AddInsecureBypassPatternᴍ(Ꮡc, pattern);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetDenyHandler(this ж<CrossOriginProtection> Ꮡc, Handler hʗp) => http_package.SetDenyHandlerᴍ(Ꮡc, hʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Check(this ж<CrossOriginProtection> Ꮡc, ж<Request> Ꮡreq) => http_package.Checkᴍ(Ꮡc, Ꮡreq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isRequestExempt(this ж<CrossOriginProtection> Ꮡc, ж<Request> Ꮡreq) => http_package.isRequestExemptᴍ(Ꮡc, Ꮡreq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Handler Handler(this ж<CrossOriginProtection> Ꮡc, Handler h) => http_package.Handlerᴍ(Ꮡc, h);
}
partial class httpᴛnoopHandler {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ServeHTTP(this noopHandler _Δp0, ResponseWriter _Δp1, ж<Request> _Δp2) => http_package.ServeHTTPᴍ(_Δp0, _Δp1, _Δp2);
}
partial class httpᴛfileTransport {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Response> resp, error err) RoundTrip(this fileTransport t, ж<Request> req) => http_package.RoundTripᴍ(t, req);
}
partial class httpᴛpopulateResponse {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void finish(this ref populateResponse pr) => http_package.finishᴍ(ref pr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void sendResponse(this ref populateResponse pr) => http_package.sendResponseᴍ(ref pr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Header Header(this ref populateResponse pr) => http_package.Headerᴍ(ref pr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void WriteHeader(this ref populateResponse pr, nint code) => http_package.WriteHeaderᴍ(ref pr, code);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Write(this ref populateResponse pr, slice<byte> p) => http_package.Writeᴍ(ref pr, p);
}
partial class httpᴛDir {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (File, error) Open(this Dir d, @string name) => http_package.Openᴍ(d, name);
}
partial class httpᴛfileInfoDirs {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint len(this fileInfoDirs d) => http_package.lenᴍ(d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isDir(this fileInfoDirs d, nint i) => http_package.isDirᴍ(d, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string name(this fileInfoDirs d, nint i) => http_package.nameᴍ(d, i);
}
partial class httpᴛdirEntryDirs {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint len(this dirEntryDirs d) => http_package.lenᴍ(d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isDir(this dirEntryDirs d, nint i) => http_package.isDirᴍ(d, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string name(this dirEntryDirs d, nint i) => http_package.nameᴍ(d, i);
}
partial class httpᴛioFS {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (File, error) Open(this ioFS f, @string name) => http_package.Openᴍ(f, name);
}
partial class httpᴛioFile {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ioFile f) => http_package.Closeᴍ(f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this ioFile f, slice<byte> b) => http_package.Readᴍ(f, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (fs.FileInfo, error) Stat(this ioFile f) => http_package.Statᴍ(f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) Seek(this ioFile f, int64 offset, nint whence) => http_package.Seekᴍ(f, offset, whence);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<fs.DirEntry>, error) ReadDir(this ioFile f, nint count) => http_package.ReadDirᴍ(f, count);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<fs.FileInfo>, error) Readdir(this ioFile f, nint count) => http_package.Readdirᴍ(f, count);
}
partial class httpᴛfileHandler {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ServeHTTP(this ref fileHandler f, ResponseWriter w, ж<Request> Ꮡr) => http_package.ServeHTTPᴍ(ref f, w, Ꮡr);
}
partial class httpᴛhttpRange {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string contentRange(this httpRange r, int64 size) => http_package.contentRangeᴍ(r, size);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static textproto.MIMEHeader mimeHeader(this httpRange r, @string contentType, int64 size) => http_package.mimeHeaderᴍ(r, contentType, size);
}
partial class httpᴛcountingWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Write(this ref countingWriter w, slice<byte> p) => http_package.Writeᴍ(ref w, p);
}
partial class httpᴛhttp2clientConnPool {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<http2ClientConn>, error) GetClientConn(this ж<http2clientConnPool> Ꮡp, ж<Request> req, @string addr) => http_package.GetClientConnᴍ(Ꮡp, req, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<http2ClientConn>, error) getClientConn(this ж<http2clientConnPool> Ꮡp, ж<Request> Ꮡreq, @string addr, bool dialOnMiss) => http_package.getClientConnᴍ(Ꮡp, Ꮡreq, addr, dialOnMiss);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<http2dialCall> getStartDialLocked(this ж<http2clientConnPool> Ꮡp, context.Context ctx, @string addr) => http_package.getStartDialLockedᴍ(Ꮡp, ctx, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bool used, error err) addConnIfNeeded(this ж<http2clientConnPool> Ꮡp, @string key, ж<http2Transport> t, net.Conn c) => http_package.addConnIfNeededᴍ(Ꮡp, key, t, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addConnLocked(this ref http2clientConnPool p, @string key, ж<http2ClientConn> cc) => http_package.addConnLockedᴍ(ref p, key, cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void MarkDead(this ж<http2clientConnPool> Ꮡp, ж<http2ClientConn> cc) => http_package.MarkDeadᴍ(Ꮡp, cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeIdleConnections(this ж<http2clientConnPool> Ꮡp) => http_package.closeIdleConnectionsᴍ(Ꮡp);
}
partial class httpᴛhttp2dialCall {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void dial(this ref http2dialCall c, context.Context ctx, @string addr) => http_package.dialᴍ(ref c, ctx, addr);
}
partial class httpᴛhttp2addConnCall {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void run(this ref http2addConnCall c, ж<http2Transport> t, @string key, net.Conn nc) => http_package.runᴍ(ref c, t, key, nc);
}
partial class httpᴛhttp2noDialClientConnPool {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<http2ClientConn>, error) GetClientConn(this http2noDialClientConnPool p, ж<Request> req, @string addr) => http_package.GetClientConnᴍ(p, req, addr);
}
partial class httpᴛhttp2dataBuffer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this ref http2dataBuffer b, slice<byte> p) => http_package.Readᴍ(ref b, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> bytesFromFirstChunk(this ref http2dataBuffer b) => http_package.bytesFromFirstChunkᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this ref http2dataBuffer b) => http_package.Lenᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Write(this ref http2dataBuffer b, slice<byte> p) => http_package.Writeᴍ(ref b, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> lastChunkOrAlloc(this ref http2dataBuffer b, int64 want) => http_package.lastChunkOrAllocᴍ(ref b, want);
}
partial class httpᴛhttp2ErrCode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this http2ErrCode e) => http_package.Stringᴍ(e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string stringToken(this http2ErrCode e) => http_package.stringTokenᴍ(e);
}
partial class httpᴛhttp2ConnectionError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this http2ConnectionError e) => http_package.Errorᴍ(e);
}
partial class httpᴛhttp2StreamError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this http2StreamError e) => http_package.Errorᴍ(e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeFrame(this http2StreamError se, http2writeContext ctx) => http_package.writeFrameᴍ(se, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool staysWithinBuffer(this http2StreamError se, nint max) => http_package.staysWithinBufferᴍ(se, max);
}
partial class httpᴛhttp2goAwayFlowError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this http2goAwayFlowError _) => http_package.Errorᴍ(_);
}
partial class httpᴛhttp2connError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this http2connError e) => http_package.Errorᴍ(e);
}
partial class httpᴛhttp2pseudoHeaderError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this http2pseudoHeaderError e) => http_package.Errorᴍ(e);
}
partial class httpᴛhttp2duplicatePseudoHeaderError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this http2duplicatePseudoHeaderError e) => http_package.Errorᴍ(e);
}
partial class httpᴛhttp2headerFieldNameError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this http2headerFieldNameError e) => http_package.Errorᴍ(e);
}
partial class httpᴛhttp2headerFieldValueError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this http2headerFieldValueError e) => http_package.Errorᴍ(e);
}
partial class httpᴛhttp2inflow {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref http2inflow f, int32 n) => http_package.initᴍ(ref f, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int32 /*connAdd*/ add(this ref http2inflow f, nint n) => http_package.addᴍ(ref f, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool take(this ref http2inflow f, uint32 n) => http_package.takeᴍ(ref f, n);
}
partial class httpᴛhttp2connOutflow {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref http2connOutflow f) => http_package.initᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool changeInitialWindowSize(this ref http2connOutflow f, int64 size) => http_package.changeInitialWindowSizeᴍ(ref f, size);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool add(this ref http2connOutflow f, int32 n) => http_package.addᴍ(ref f, n);
}
partial class httpᴛhttp2outflow {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int32, bool) available(this ref http2outflow f) => http_package.availableᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void take(this ref http2outflow f, int32 n) => http_package.takeᴍ(ref f, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool add(this ref http2outflow f, int32 n) => http_package.addᴍ(ref f, n);
}
partial class httpᴛhttp2FrameType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this http2FrameType t) => http_package.Stringᴍ(t);
}
partial class httpᴛhttp2Flags {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Has(this http2Flags f, http2Flags v) => http_package.Hasᴍ(f, v);
}
partial class httpᴛhttp2FrameHeader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static http2FrameHeader Header(this http2FrameHeader h) => http_package.Headerᴍ(h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this http2FrameHeader h) => http_package.Stringᴍ(h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeDebug(this http2FrameHeader h, ж<bytes.Buffer> Ꮡbuf) => http_package.writeDebugᴍ(h, Ꮡbuf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkValid(this ref http2FrameHeader h) => http_package.checkValidᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void invalidate(this ref http2FrameHeader h) => http_package.invalidateᴍ(ref h);
}
partial class httpᴛhttp2Framer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint32 maxHeaderListSize(this ref http2Framer fr) => http_package.maxHeaderListSizeᴍ(ref fr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void startWrite(this ref http2Framer f, http2FrameType ftype, http2Flags flags, uint32 streamID) => http_package.startWriteᴍ(ref f, ftype, flags, streamID);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error endWrite(this ж<http2Framer> Ꮡf) => http_package.endWriteᴍ(Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void logWrite(this ж<http2Framer> Ꮡf) => http_package.logWriteᴍ(Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeByte(this ref http2Framer f, byte v) => http_package.writeByteᴍ(ref f, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeBytes(this ref http2Framer f, slice<byte> v) => http_package.writeBytesᴍ(ref f, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeUint16(this ref http2Framer f, uint16 v) => http_package.writeUint16ᴍ(ref f, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeUint32(this ref http2Framer f, uint32 v) => http_package.writeUint32ᴍ(ref f, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void SetReuseFrames(this ref http2Framer fr) => http_package.SetReuseFramesᴍ(ref fr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void SetMaxReadFrameSize(this ref http2Framer fr, uint32 v) => http_package.SetMaxReadFrameSizeᴍ(ref fr, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error ErrorDetail(this ref http2Framer fr) => http_package.ErrorDetailᴍ(ref fr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (http2FrameHeader, error) ReadFrameHeader(this ref http2Framer fr) => http_package.ReadFrameHeaderᴍ(ref fr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (http2Frame, error) ReadFrameForHeader(this ж<http2Framer> Ꮡfr, http2FrameHeader fh) => http_package.ReadFrameForHeaderᴍ(Ꮡfr, fh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (http2Frame, error) ReadFrame(this ж<http2Framer> Ꮡfr) => http_package.ReadFrameᴍ(Ꮡfr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error connError(this ref http2Framer fr, http2ErrCode code, @string reason) => http_package.connErrorᴍ(ref fr, code, reason);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error checkFrameOrder(this ref http2Framer fr, http2FrameHeader fh) => http_package.checkFrameOrderᴍ(ref fr, fh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error WriteData(this ж<http2Framer> Ꮡf, uint32 streamID, bool endStream, slice<byte> data) => http_package.WriteDataᴍ(Ꮡf, streamID, endStream, data);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error WriteDataPadded(this ж<http2Framer> Ꮡf, uint32 streamID, bool endStream, slice<byte> data, slice<byte> pad) => http_package.WriteDataPaddedᴍ(Ꮡf, streamID, endStream, data, pad);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error startWriteDataPadded(this ref http2Framer f, uint32 streamID, bool endStream, slice<byte> data, slice<byte> pad) => http_package.startWriteDataPaddedᴍ(ref f, streamID, endStream, data, pad);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error WriteSettings(this ж<http2Framer> Ꮡf, params ꓸꓸꓸhttp2Setting settingsʗp) => http_package.WriteSettingsᴍ(Ꮡf, settingsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error WriteSettingsAck(this ж<http2Framer> Ꮡf) => http_package.WriteSettingsAckᴍ(Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error WritePing(this ж<http2Framer> Ꮡf, bool ack, /*[8]*/ array<byte> data) => http_package.WritePingᴍ(Ꮡf, ack, data);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error WriteGoAway(this ж<http2Framer> Ꮡf, uint32 maxStreamID, http2ErrCode code, slice<byte> debugData) => http_package.WriteGoAwayᴍ(Ꮡf, maxStreamID, code, debugData);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error WriteWindowUpdate(this ж<http2Framer> Ꮡf, uint32 streamID, uint32 incr) => http_package.WriteWindowUpdateᴍ(Ꮡf, streamID, incr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error WriteHeaders(this ж<http2Framer> Ꮡf, http2HeadersFrameParam p) => http_package.WriteHeadersᴍ(Ꮡf, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error WritePriority(this ж<http2Framer> Ꮡf, uint32 streamID, http2PriorityParam p) => http_package.WritePriorityᴍ(Ꮡf, streamID, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error WriteRSTStream(this ж<http2Framer> Ꮡf, uint32 streamID, http2ErrCode code) => http_package.WriteRSTStreamᴍ(Ꮡf, streamID, code);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error WriteContinuation(this ж<http2Framer> Ꮡf, uint32 streamID, bool endHeaders, slice<byte> headerBlockFragment) => http_package.WriteContinuationᴍ(Ꮡf, streamID, endHeaders, headerBlockFragment);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error WritePushPromise(this ж<http2Framer> Ꮡf, http2PushPromiseParam p) => http_package.WritePushPromiseᴍ(Ꮡf, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error WriteRawFrame(this ж<http2Framer> Ꮡf, http2FrameType t, http2Flags flags, uint32 streamID, slice<byte> payload) => http_package.WriteRawFrameᴍ(Ꮡf, t, flags, streamID, payload);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint maxHeaderStringLen(this ref http2Framer fr) => http_package.maxHeaderStringLenᴍ(ref fr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (http2Frame, error) readMetaFrame(this ж<http2Framer> Ꮡfr, ж<http2HeadersFrame> hf) => http_package.readMetaFrameᴍ(Ꮡfr, hf);
}
partial class httpᴛhttp2frameCache {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<http2DataFrame> getDataFrame(this ж<http2frameCache> Ꮡfc) => http_package.getDataFrameᴍ(Ꮡfc);
}
partial class httpᴛhttp2DataFrame {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool StreamEnded(this ref http2DataFrame f) => http_package.StreamEndedᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> Data(this ref http2DataFrame f) => http_package.Dataᴍ(ref f);
}
partial class httpᴛhttp2SettingsFrame {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool IsAck(this ref http2SettingsFrame f) => http_package.IsAckᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint32 v, bool ok) Value(this ref http2SettingsFrame f, http2SettingID id) => http_package.Valueᴍ(ref f, id);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static http2Setting Setting(this ref http2SettingsFrame f, nint i) => http_package.Settingᴍ(ref f, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint NumSettings(this ref http2SettingsFrame f) => http_package.NumSettingsᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool HasDuplicates(this ref http2SettingsFrame f) => http_package.HasDuplicatesᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error ForeachSetting(this ref http2SettingsFrame f, Func<http2Setting, error> fn) => http_package.ForeachSettingᴍ(ref f, fn);
}
partial class httpᴛhttp2PingFrame {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool IsAck(this ref http2PingFrame f) => http_package.IsAckᴍ(ref f);
}
partial class httpᴛhttp2GoAwayFrame {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> DebugData(this ref http2GoAwayFrame f) => http_package.DebugDataᴍ(ref f);
}
partial class httpᴛhttp2UnknownFrame {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> Payload(this ref http2UnknownFrame f) => http_package.Payloadᴍ(ref f);
}
partial class httpᴛhttp2HeadersFrame {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> HeaderBlockFragment(this ref http2HeadersFrame f) => http_package.HeaderBlockFragmentᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool HeadersEnded(this ref http2HeadersFrame f) => http_package.HeadersEndedᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool StreamEnded(this ref http2HeadersFrame f) => http_package.StreamEndedᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool HasPriority(this ref http2HeadersFrame f) => http_package.HasPriorityᴍ(ref f);
}
partial class httpᴛhttp2PriorityParam {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsZero(this http2PriorityParam p) => http_package.IsZeroᴍ(p);
}
partial class httpᴛhttp2ContinuationFrame {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> HeaderBlockFragment(this ref http2ContinuationFrame f) => http_package.HeaderBlockFragmentᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool HeadersEnded(this ref http2ContinuationFrame f) => http_package.HeadersEndedᴍ(ref f);
}
partial class httpᴛhttp2PushPromiseFrame {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> HeaderBlockFragment(this ref http2PushPromiseFrame f) => http_package.HeaderBlockFragmentᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool HeadersEnded(this ref http2PushPromiseFrame f) => http_package.HeadersEndedᴍ(ref f);
}
partial class httpᴛhttp2MetaHeadersFrame {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string PseudoValue(this ref http2MetaHeadersFrame mh, @string pseudo) => http_package.PseudoValueᴍ(ref mh, pseudo);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<hpack.HeaderField> RegularFields(this ref http2MetaHeadersFrame mh) => http_package.RegularFieldsᴍ(ref mh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<hpack.HeaderField> PseudoFields(this ref http2MetaHeadersFrame mh) => http_package.PseudoFieldsᴍ(ref mh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error checkPseudos(this ref http2MetaHeadersFrame mh) => http_package.checkPseudosᴍ(ref mh);
}
partial class httpᴛhttp2goroutineLock {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void check(this http2goroutineLock g) => http_package.checkᴍ(g);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkNotOn(this http2goroutineLock g) => http_package.checkNotOnᴍ(g);
}
partial class httpᴛhttp2streamState {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this http2streamState st) => http_package.Stringᴍ(st);
}
partial class httpᴛhttp2Setting {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this http2Setting s) => http_package.Stringᴍ(s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Valid(this http2Setting s) => http_package.Validᴍ(s);
}
partial class httpᴛhttp2SettingID {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this http2SettingID s) => http_package.Stringᴍ(s);
}
partial class httpᴛhttp2closeWaiter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Init(this ref http2closeWaiter cw) => http_package.Initᴍ(ref cw);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Close(this http2closeWaiter cw) => http_package.Closeᴍ(cw);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Wait(this http2closeWaiter cw) => http_package.Waitᴍ(cw);
}
partial class httpᴛhttp2bufferedWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Available(this ref http2bufferedWriter w) => http_package.Availableᴍ(ref w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Write(this ж<http2bufferedWriter> Ꮡw, slice<byte> p) => http_package.Writeᴍ(Ꮡw, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Flush(this ref http2bufferedWriter w) => http_package.Flushᴍ(ref w);
}
partial class httpᴛhttp2bufferedWriterTimeoutWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Write(this ref http2bufferedWriterTimeoutWriter w, slice<byte> p) => http_package.Writeᴍ(ref w, p);
}
partial class httpᴛhttp2httpError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this ref http2httpError e) => http_package.Errorᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Timeout(this ref http2httpError e) => http_package.Timeoutᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Temporary(this ref http2httpError e) => http_package.Temporaryᴍ(ref e);
}
partial class httpᴛhttp2sorter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this ref http2sorter s) => http_package.Lenᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Swap(this ref http2sorter s, nint i, nint j) => http_package.Swapᴍ(ref s, i, j);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Less(this ref http2sorter s, nint i, nint j) => http_package.Lessᴍ(ref s, i, j);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<@string> Keys(this ж<http2sorter> Ꮡs, Header h) => http_package.Keysᴍ(Ꮡs, h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void SortStrings(this ж<http2sorter> Ꮡs, slice<@string> ss) => http_package.SortStringsᴍ(Ꮡs, ss);
}
partial class httpᴛhttp2pipe {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setBuffer(this ж<http2pipe> Ꮡp, http2pipeBuffer b) => http_package.setBufferᴍ(Ꮡp, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this ж<http2pipe> Ꮡp) => http_package.Lenᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ж<http2pipe> Ꮡp, slice<byte> d) => http_package.Readᴍ(Ꮡp, d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Write(this ж<http2pipe> Ꮡp, slice<byte> d) => http_package.Writeᴍ(Ꮡp, d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void CloseWithError(this ж<http2pipe> Ꮡp, error err) => http_package.CloseWithErrorᴍ(Ꮡp, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void BreakWithError(this ж<http2pipe> Ꮡp, error err) => http_package.BreakWithErrorᴍ(Ꮡp, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeWithErrorAndCode(this ж<http2pipe> Ꮡp, error err, Action fn) => http_package.closeWithErrorAndCodeᴍ(Ꮡp, err, fn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeWithError(this ж<http2pipe> Ꮡp, ж<error> Ꮡdst, error err, Action fn) => http_package.closeWithErrorᴍ(Ꮡp, Ꮡdst, err, fn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeDoneLocked(this ref http2pipe p) => http_package.closeDoneLockedᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Err(this ж<http2pipe> Ꮡp) => http_package.Errᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static /*<-*/channel<EmptyStruct> Done(this ж<http2pipe> Ꮡp) => http_package.Doneᴍ(Ꮡp);
}
partial class httpᴛhttp2serverInternalState {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void registerConn(this ж<http2serverInternalState> Ꮡs, ж<http2serverConn> sc) => http_package.registerConnᴍ(Ꮡs, sc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void unregisterConn(this ж<http2serverInternalState> Ꮡs, ж<http2serverConn> sc) => http_package.unregisterConnᴍ(Ꮡs, sc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void startGracefulShutdown(this ж<http2serverInternalState> Ꮡs) => http_package.startGracefulShutdownᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static channel<error> getErrChan(this ж<http2serverInternalState> Ꮡs) => http_package.getErrChanᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void putErrChan(this ж<http2serverInternalState> Ꮡs, channel<error> ch) => http_package.putErrChanᴍ(Ꮡs, ch);
}
partial class httpᴛhttp2ServeConnOpts {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static context.Context context(this ж<http2ServeConnOpts> Ꮡo) => http_package.contextᴍ(Ꮡo);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Server> baseConfig(this ж<http2ServeConnOpts> Ꮡo) => http_package.baseConfigᴍ(Ꮡo);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Handler handler(this ж<http2ServeConnOpts> Ꮡo) => http_package.handlerᴍ(Ꮡo);
}
partial class httpᴛhttp2Server {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ServeConn(this ж<http2Server> Ꮡs, net.Conn c, ж<http2ServeConnOpts> Ꮡopts) => http_package.ServeConnᴍ(Ꮡs, c, Ꮡopts);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void serveConn(this ж<http2Server> Ꮡs, net.Conn c, ж<http2ServeConnOpts> Ꮡopts, Action<ж<http2serverConn>> newf) => http_package.serveConnᴍ(Ꮡs, c, Ꮡopts, newf);
}
partial class httpᴛhttp2serverConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void rejectConn(this ref http2serverConn sc, http2ErrCode err, @string debug) => http_package.rejectConnᴍ(ref sc, err, debug);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint32 maxHeaderListSize(this ref http2serverConn sc) => http_package.maxHeaderListSizeᴍ(ref sc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint32 curOpenStreams(this ref http2serverConn sc) => http_package.curOpenStreamsᴍ(ref sc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<http2Framer> Framer(this ref http2serverConn sc) => http_package.Framerᴍ(ref sc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error CloseConn(this ref http2serverConn sc) => http_package.CloseConnᴍ(ref sc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Flush(this ref http2serverConn sc) => http_package.Flushᴍ(ref sc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<hpack.Encoder>, ж<bytes.Buffer>) HeaderEncoder(this ж<http2serverConn> Ꮡsc) => http_package.HeaderEncoderᴍ(Ꮡsc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (http2streamState, ж<http2stream>) state(this ref http2serverConn sc, uint32 streamID) => http_package.stateᴍ(ref sc, streamID);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setConnState(this ref http2serverConn sc, ConnState state) => http_package.setConnStateᴍ(ref sc, state);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void vlogf(this ref http2serverConn sc, @string format, params ꓸꓸꓸany argsʗp) => http_package.vlogfᴍ(ref sc, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void logf(this ref http2serverConn sc, @string format, params ꓸꓸꓸany argsʗp) => http_package.logfᴍ(ref sc, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void condlogf(this ref http2serverConn sc, error err, @string format, params ꓸꓸꓸany argsʗp) => http_package.condlogfᴍ(ref sc, err, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string canonicalHeader(this ref http2serverConn sc, @string v) => http_package.canonicalHeaderᴍ(ref sc, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void readFrames(this ref http2serverConn sc) => http_package.readFramesᴍ(ref sc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeFrameAsync(this ж<http2serverConn> Ꮡsc, http2FrameWriteRequest wr, ж<http2writeData> wd) => http_package.writeFrameAsyncᴍ(Ꮡsc, wr, wd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeAllStreamsOnConnClose(this ж<http2serverConn> Ꮡsc) => http_package.closeAllStreamsOnConnCloseᴍ(Ꮡsc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void stopShutdownTimer(this ref http2serverConn sc) => http_package.stopShutdownTimerᴍ(ref sc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void notePanic(this ж<http2serverConn> Ꮡsc) => http_package.notePanicᴍ(Ꮡsc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void serve(this ж<http2serverConn> Ꮡsc, http2http2Config conf) => http_package.serveᴍ(Ꮡsc, conf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void handlePingTimer(this ж<http2serverConn> Ꮡsc, time.Time lastFrameReadTime) => http_package.handlePingTimerᴍ(Ꮡsc, lastFrameReadTime);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void onSettingsTimer(this ref http2serverConn sc) => http_package.onSettingsTimerᴍ(ref sc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void onIdleTimer(this ref http2serverConn sc) => http_package.onIdleTimerᴍ(ref sc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void onReadIdleTimer(this ref http2serverConn sc) => http_package.onReadIdleTimerᴍ(ref sc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void onShutdownTimer(this ref http2serverConn sc) => http_package.onShutdownTimerᴍ(ref sc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void sendServeMsg(this ref http2serverConn sc, any msg) => http_package.sendServeMsgᴍ(ref sc, msg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error readPreface(this ж<http2serverConn> Ꮡsc) => http_package.readPrefaceᴍ(Ꮡsc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeDataFromHandler(this ref http2serverConn sc, ж<http2stream> Ꮡstream, slice<byte> data, bool endStream) => http_package.writeDataFromHandlerᴍ(ref sc, Ꮡstream, data, endStream);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeFrameFromHandler(this ref http2serverConn sc, http2FrameWriteRequest wr) => http_package.writeFrameFromHandlerᴍ(ref sc, wr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeFrame(this ж<http2serverConn> Ꮡsc, http2FrameWriteRequest wr) => http_package.writeFrameᴍ(Ꮡsc, wr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void startFrameWrite(this ж<http2serverConn> Ꮡsc, http2FrameWriteRequest wr) => http_package.startFrameWriteᴍ(Ꮡsc, wr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void wroteFrame(this ж<http2serverConn> Ꮡsc, http2frameWriteResult res) => http_package.wroteFrameᴍ(Ꮡsc, res);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void scheduleFrameWrite(this ж<http2serverConn> Ꮡsc) => http_package.scheduleFrameWriteᴍ(Ꮡsc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void startGracefulShutdown(this ж<http2serverConn> Ꮡsc) => http_package.startGracefulShutdownᴍ(Ꮡsc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void startGracefulShutdownInternal(this ж<http2serverConn> Ꮡsc) => http_package.startGracefulShutdownInternalᴍ(Ꮡsc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void goAway(this ж<http2serverConn> Ꮡsc, http2ErrCode code) => http_package.goAwayᴍ(Ꮡsc, code);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void shutDownIn(this ж<http2serverConn> Ꮡsc, time.Duration d) => http_package.shutDownInᴍ(Ꮡsc, d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void resetStream(this ж<http2serverConn> Ꮡsc, http2StreamError se) => http_package.resetStreamᴍ(Ꮡsc, se);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool processFrameFromReader(this ж<http2serverConn> Ꮡsc, http2readFrameResult res) => http_package.processFrameFromReaderᴍ(Ꮡsc, res);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processFrame(this ж<http2serverConn> Ꮡsc, http2Frame f) => http_package.processFrameᴍ(Ꮡsc, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processPing(this ж<http2serverConn> Ꮡsc, ж<http2PingFrame> Ꮡf) => http_package.processPingᴍ(Ꮡsc, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processWindowUpdate(this ж<http2serverConn> Ꮡsc, ж<http2WindowUpdateFrame> Ꮡf) => http_package.processWindowUpdateᴍ(Ꮡsc, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processResetStream(this ж<http2serverConn> Ꮡsc, ж<http2RSTStreamFrame> Ꮡf) => http_package.processResetStreamᴍ(Ꮡsc, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeStream(this ж<http2serverConn> Ꮡsc, ж<http2stream> Ꮡst, error err) => http_package.closeStreamᴍ(Ꮡsc, Ꮡst, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processSettings(this ж<http2serverConn> Ꮡsc, ж<http2SettingsFrame> Ꮡf) => http_package.processSettingsᴍ(Ꮡsc, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processSetting(this ж<http2serverConn> Ꮡsc, http2Setting s) => http_package.processSettingᴍ(Ꮡsc, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processSettingInitialWindowSize(this ж<http2serverConn> Ꮡsc, uint32 val) => http_package.processSettingInitialWindowSizeᴍ(Ꮡsc, val);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processData(this ж<http2serverConn> Ꮡsc, ж<http2DataFrame> Ꮡf) => http_package.processDataᴍ(Ꮡsc, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processGoAway(this ж<http2serverConn> Ꮡsc, ж<http2GoAwayFrame> Ꮡf) => http_package.processGoAwayᴍ(Ꮡsc, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processHeaders(this ж<http2serverConn> Ꮡsc, ж<http2MetaHeadersFrame> Ꮡf) => http_package.processHeadersᴍ(Ꮡsc, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void upgradeRequest(this ж<http2serverConn> Ꮡsc, ж<Request> Ꮡreq) => http_package.upgradeRequestᴍ(Ꮡsc, Ꮡreq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error checkPriority(this ж<http2serverConn> Ꮡsc, uint32 streamID, http2PriorityParam p) => http_package.checkPriorityᴍ(Ꮡsc, streamID, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processPriority(this ж<http2serverConn> Ꮡsc, ж<http2PriorityFrame> Ꮡf) => http_package.processPriorityᴍ(Ꮡsc, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<http2stream> newStream(this ж<http2serverConn> Ꮡsc, uint32 id, uint32 pusherID, http2streamState state) => http_package.newStreamᴍ(Ꮡsc, id, pusherID, state);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<http2responseWriter>, ж<Request>, error) newWriterAndRequest(this ж<http2serverConn> Ꮡsc, ж<http2stream> Ꮡst, ж<http2MetaHeadersFrame> Ꮡf) => http_package.newWriterAndRequestᴍ(Ꮡsc, Ꮡst, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<http2responseWriter>, ж<Request>, error) newWriterAndRequestNoBody(this ж<http2serverConn> Ꮡsc, ж<http2stream> Ꮡst, httpcommon.ServerRequestParam rp) => http_package.newWriterAndRequestNoBodyᴍ(Ꮡsc, Ꮡst, rp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<http2responseWriter> newResponseWriter(this ж<http2serverConn> Ꮡsc, ж<http2stream> st, ж<Request> Ꮡreq) => http_package.newResponseWriterᴍ(Ꮡsc, st, Ꮡreq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error scheduleHandler(this ж<http2serverConn> Ꮡsc, ж<http2stream> Ꮡst, ж<http2responseWriter> rw, ж<Request> req, Action<ResponseWriter, ж<Request>> handler) => http_package.scheduleHandlerᴍ(Ꮡsc, Ꮡst, rw, req, handler);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void handlerDone(this ж<http2serverConn> Ꮡsc) => http_package.handlerDoneᴍ(Ꮡsc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void runHandler(this ж<http2serverConn> Ꮡsc, ж<http2responseWriter> rw, ж<Request> req, Action<ResponseWriter, ж<Request>> handler) => http_package.runHandlerᴍ(Ꮡsc, rw, req, handler);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeHeaders(this ref http2serverConn sc, ж<http2stream> Ꮡst, ж<http2writeResHeaders> ᏑheaderData) => http_package.writeHeadersᴍ(ref sc, Ꮡst, ᏑheaderData);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void write100ContinueHeaders(this ref http2serverConn sc, ж<http2stream> Ꮡst) => http_package.write100ContinueHeadersᴍ(ref sc, Ꮡst);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void noteBodyReadFromHandler(this ref http2serverConn sc, ж<http2stream> st, nint n, error err) => http_package.noteBodyReadFromHandlerᴍ(ref sc, st, n, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void noteBodyRead(this ж<http2serverConn> Ꮡsc, ж<http2stream> Ꮡst, nint n) => http_package.noteBodyReadᴍ(Ꮡsc, Ꮡst, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void sendWindowUpdate32(this ж<http2serverConn> Ꮡsc, ж<http2stream> st, int32 n) => http_package.sendWindowUpdate32ᴍ(Ꮡsc, st, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void sendWindowUpdate(this ж<http2serverConn> Ꮡsc, ж<http2stream> Ꮡst, nint n) => http_package.sendWindowUpdateᴍ(Ꮡsc, Ꮡst, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void startPush(this ж<http2serverConn> Ꮡsc, ж<http2startPushRequest> Ꮡmsg) => http_package.startPushᴍ(Ꮡsc, Ꮡmsg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error countError(this ж<http2serverConn> Ꮡsc, @string name, error err) => http_package.countErrorᴍ(Ꮡsc, name, err);
}
partial class httpᴛhttp2stream {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isPushed(this ref http2stream st) => http_package.isPushedᴍ(ref st);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void endStream(this ж<http2stream> Ꮡst) => http_package.endStreamᴍ(Ꮡst);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void copyTrailersToHandlerRequest(this ref http2stream st) => http_package.copyTrailersToHandlerRequestᴍ(ref st);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void onReadTimeout(this ref http2stream st) => http_package.onReadTimeoutᴍ(ref st);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void onWriteTimeout(this ref http2stream st) => http_package.onWriteTimeoutᴍ(ref st);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processTrailerHeaders(this ж<http2stream> Ꮡst, ж<http2MetaHeadersFrame> Ꮡf) => http_package.processTrailerHeadersᴍ(Ꮡst, Ꮡf);
}
partial class httpᴛhttp2requestBody {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ж<http2requestBody> Ꮡb) => http_package.Closeᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ref http2requestBody b, slice<byte> p) => http_package.Readᴍ(ref b, p);
}
partial class httpᴛhttp2chunkWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Write(this http2chunkWriter cw, slice<byte> p) => http_package.Writeᴍ(cw, p);
}
partial class httpᴛhttp2responseWriterState {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool hasTrailers(this ref http2responseWriterState rws) => http_package.hasTrailersᴍ(ref rws);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool hasNonemptyTrailers(this ref http2responseWriterState rws) => http_package.hasNonemptyTrailersᴍ(ref rws);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void declareTrailer(this ref http2responseWriterState rws, @string k) => http_package.declareTrailerᴍ(ref rws, k);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) writeChunk(this ж<http2responseWriterState> Ꮡrws, slice<byte> p) => http_package.writeChunkᴍ(Ꮡrws, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void promoteUndeclaredTrailers(this ref http2responseWriterState rws) => http_package.promoteUndeclaredTrailersᴍ(ref rws);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeHeader(this ref http2responseWriterState rws, nint code) => http_package.writeHeaderᴍ(ref rws, code);
}
partial class httpᴛhttp2responseWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error SetReadDeadline(this ref http2responseWriter w, time.Time deadline) => http_package.SetReadDeadlineᴍ(ref w, deadline);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error SetWriteDeadline(this ref http2responseWriter w, time.Time deadline) => http_package.SetWriteDeadlineᴍ(ref w, deadline);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error EnableFullDuplex(this ref http2responseWriter w) => http_package.EnableFullDuplexᴍ(ref w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Flush(this ref http2responseWriter w) => http_package.Flushᴍ(ref w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error FlushError(this ref http2responseWriter w) => http_package.FlushErrorᴍ(ref w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static /*<-*/channel<bool> CloseNotify(this ref http2responseWriter w) => http_package.CloseNotifyᴍ(ref w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Header Header(this ref http2responseWriter w) => http_package.Headerᴍ(ref w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void WriteHeader(this ref http2responseWriter w, nint code) => http_package.WriteHeaderᴍ(ref w, code);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Write(this ref http2responseWriter w, slice<byte> p) => http_package.Writeᴍ(ref w, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) WriteString(this ref http2responseWriter w, @string s) => http_package.WriteStringᴍ(ref w, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) write(this ref http2responseWriter w, nint lenData, slice<byte> dataB, @string dataS) => http_package.writeᴍ(ref w, lenData, dataB, dataS);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void handlerDone(this ref http2responseWriter w) => http_package.handlerDoneᴍ(ref w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Push(this ref http2responseWriter w, @string target, ж<PushOptions> Ꮡopts) => http_package.Pushᴍ(ref w, target, Ꮡopts);
}
partial class httpᴛhttp2Transport {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint32 maxHeaderListSize(this ref http2Transport t) => http_package.maxHeaderListSizeᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool disableCompression(this ref http2Transport t) => http_package.disableCompressionᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static http2ClientConnPool connPool(this ж<http2Transport> Ꮡt) => http_package.connPoolᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void initConnPool(this ж<http2Transport> Ꮡt) => http_package.initConnPoolᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Response>, error) RoundTrip(this ж<http2Transport> Ꮡt, ж<Request> req) => http_package.RoundTripᴍ(Ꮡt, req);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Response>, error) RoundTripOpt(this ж<http2Transport> Ꮡt, ж<Request> Ꮡreq, http2RoundTripOpt opt) => http_package.RoundTripOptᴍ(Ꮡt, Ꮡreq, opt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void CloseIdleConnections(this ж<http2Transport> Ꮡt) => http_package.CloseIdleConnectionsᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<http2ClientConn>, error) dialClientConn(this ж<http2Transport> Ꮡt, context.Context ctx, @string addr, bool singleUse) => http_package.dialClientConnᴍ(Ꮡt, ctx, addr, singleUse);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<tls.Config> newTLSConfig(this ref http2Transport t, @string host) => http_package.newTLSConfigᴍ(ref t, host);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (net.Conn, error) dialTLS(this ref http2Transport t, context.Context ctx, @string network, @string addr, ж<tls.Config> tlsCfg) => http_package.dialTLSᴍ(ref t, ctx, network, addr, tlsCfg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool disableKeepAlives(this ref http2Transport t) => http_package.disableKeepAlivesᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static time.Duration expectContinueTimeout(this ref http2Transport t) => http_package.expectContinueTimeoutᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<http2ClientConn>, error) NewClientConn(this ж<http2Transport> Ꮡt, net.Conn c) => http_package.NewClientConnᴍ(Ꮡt, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<http2ClientConn>, error) newClientConn(this ж<http2Transport> Ꮡt, net.Conn c, bool singleUse, Action internalStateHook) => http_package.newClientConnᴍ(Ꮡt, c, singleUse, internalStateHook);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void vlogf(this ref http2Transport t, @string format, params ꓸꓸꓸany argsʗp) => http_package.vlogfᴍ(ref t, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void logf(this ref http2Transport t, @string format, params ꓸꓸꓸany argsʗp) => http_package.logfᴍ(ref t, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static time.Duration idleConnTimeout(this ref http2Transport t) => http_package.idleConnTimeoutᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<tls.Conn>, error) dialTLSWithContext(this ref http2Transport t, context.Context ctx, @string network, @string addr, ж<tls.Config> cfg) => http_package.dialTLSWithContextᴍ(ref t, ctx, network, addr, cfg);
}
partial class httpᴛhttp2unencryptedTransport {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Response>, error) RoundTrip(this ж<http2unencryptedTransport> Ꮡt, ж<Request> req) => http_package.RoundTripᴍ(Ꮡt, req);
}
partial class httpᴛhttp2clientStream {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Func<nint, textproto.MIMEHeader, error> get1xxTraceFunc(this ref http2clientStream cs) => http_package.get1xxTraceFuncᴍ(ref cs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void abortStream(this ж<http2clientStream> Ꮡcs, error err) => http_package.abortStreamᴍ(Ꮡcs, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void abortStreamLocked(this ж<http2clientStream> Ꮡcs, error err) => http_package.abortStreamLockedᴍ(Ꮡcs, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void abortRequestBodyWrite(this ж<http2clientStream> Ꮡcs) => http_package.abortRequestBodyWriteᴍ(Ꮡcs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeReqBodyLocked(this ж<http2clientStream> Ꮡcs) => http_package.closeReqBodyLockedᴍ(Ꮡcs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void doRequest(this ж<http2clientStream> Ꮡcs, ж<Request> req, Action<ж<http2clientStream>> streamf) => http_package.doRequestᴍ(Ꮡcs, req, streamf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error /*err*/ writeRequest(this ж<http2clientStream> Ꮡcs, ж<Request> Ꮡreq, Action<ж<http2clientStream>> streamf) => http_package.writeRequestᴍ(Ꮡcs, Ꮡreq, streamf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error encodeAndWriteHeaders(this ж<http2clientStream> Ꮡcs, ж<Request> req) => http_package.encodeAndWriteHeadersᴍ(Ꮡcs, req);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void cleanupWriteRequest(this ж<http2clientStream> Ꮡcs, error err) => http_package.cleanupWriteRequestᴍ(Ꮡcs, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint frameScratchBufferLen(this ref http2clientStream cs, nint maxFrameSize) => http_package.frameScratchBufferLenᴍ(ref cs, maxFrameSize);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error /*err*/ writeRequestBody(this ж<http2clientStream> Ꮡcs, ж<Request> Ꮡreq) => http_package.writeRequestBodyᴍ(Ꮡcs, Ꮡreq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int32 taken, error err) awaitFlowControl(this ж<http2clientStream> Ꮡcs, nint maxBytes) => http_package.awaitFlowControlᴍ(Ꮡcs, maxBytes);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void copyTrailers(this ref http2clientStream cs) => http_package.copyTrailersᴍ(ref cs);
}
partial class httpᴛhttp2stickyErrWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Write(this http2stickyErrWriter sew, slice<byte> p) => http_package.Writeᴍ(sew, p);
}
partial class httpᴛhttp2noCachedConnError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void IsHTTP2NoCachedConnError(this http2noCachedConnError _) => http_package.IsHTTP2NoCachedConnErrorᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this http2noCachedConnError _) => http_package.Errorᴍ(_);
}
partial class httpᴛhttp2ClientConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void healthCheck(this ж<http2ClientConn> Ꮡcc) => http_package.healthCheckᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetDoNotReuse(this ж<http2ClientConn> Ꮡcc) => http_package.SetDoNotReuseᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setGoAway(this ж<http2ClientConn> Ꮡcc, ж<http2GoAwayFrame> Ꮡf) => http_package.setGoAwayᴍ(Ꮡcc, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool CanTakeNewRequest(this ж<http2ClientConn> Ꮡcc) => http_package.CanTakeNewRequestᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool ReserveNewRequest(this ж<http2ClientConn> Ꮡcc) => http_package.ReserveNewRequestᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static http2ClientConnState State(this ж<http2ClientConn> Ꮡcc) => http_package.Stateᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static http2clientConnIdleState idleState(this ж<http2ClientConn> Ꮡcc) => http_package.idleStateᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static http2clientConnIdleState /*st*/ idleStateLocked(this ref http2ClientConn cc) => http_package.idleStateLockedᴍ(ref cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isUsableLocked(this ref http2ClientConn cc) => http_package.isUsableLockedᴍ(ref cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool canReserveLocked(this ref http2ClientConn cc) => http_package.canReserveLockedᴍ(ref cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint currentRequestCountLocked(this ref http2ClientConn cc) => http_package.currentRequestCountLockedᴍ(ref cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool canTakeNewRequestLocked(this ref http2ClientConn cc) => http_package.canTakeNewRequestLockedᴍ(ref cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint availableLocked(this ref http2ClientConn cc) => http_package.availableLockedᴍ(ref cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool tooIdleLocked(this ref http2ClientConn cc) => http_package.tooIdleLockedᴍ(ref cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void onIdleTimeout(this ж<http2ClientConn> Ꮡcc) => http_package.onIdleTimeoutᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeConn(this ж<http2ClientConn> Ꮡcc) => http_package.closeConnᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void forceCloseConn(this ref http2ClientConn cc) => http_package.forceCloseConnᴍ(ref cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeIfIdle(this ж<http2ClientConn> Ꮡcc) => http_package.closeIfIdleᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isDoNotReuseAndIdle(this ж<http2ClientConn> Ꮡcc) => http_package.isDoNotReuseAndIdleᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Shutdown(this ж<http2ClientConn> Ꮡcc, context.Context ctx) => http_package.Shutdownᴍ(Ꮡcc, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error sendGoAway(this ж<http2ClientConn> Ꮡcc) => http_package.sendGoAwayᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void goAwayAndClose(this ж<http2ClientConn> Ꮡcc, http2ErrCode code) => http_package.goAwayAndCloseᴍ(Ꮡcc, code);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeForError(this ж<http2ClientConn> Ꮡcc, error err) => http_package.closeForErrorᴍ(Ꮡcc, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ж<http2ClientConn> Ꮡcc) => http_package.Closeᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeForLostPing(this ж<http2ClientConn> Ꮡcc) => http_package.closeForLostPingᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static time.Duration responseHeaderTimeout(this ref http2ClientConn cc) => http_package.responseHeaderTimeoutᴍ(ref cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void decrStreamReservations(this ж<http2ClientConn> Ꮡcc) => http_package.decrStreamReservationsᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void decrStreamReservationsLocked(this ref http2ClientConn cc) => http_package.decrStreamReservationsLockedᴍ(ref cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Response>, error) RoundTrip(this ж<http2ClientConn> Ꮡcc, ж<Request> req) => http_package.RoundTripᴍ(Ꮡcc, req);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Response>, error) roundTrip(this ж<http2ClientConn> Ꮡcc, ж<Request> Ꮡreq, Action<ж<http2clientStream>> streamf) => http_package.roundTripᴍ(Ꮡcc, Ꮡreq, streamf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error awaitOpenSlotForStreamLocked(this ref http2ClientConn cc, ж<http2clientStream> Ꮡcs) => http_package.awaitOpenSlotForStreamLockedᴍ(ref cc, Ꮡcs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeHeaders(this ref http2ClientConn cc, uint32 streamID, bool endStream, nint maxFrameSize, slice<byte> hdrs) => http_package.writeHeadersᴍ(ref cc, streamID, endStream, maxFrameSize, hdrs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) encodeTrailers(this ref http2ClientConn cc, Header trailer) => http_package.encodeTrailersᴍ(ref cc, trailer);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeHeader(this ref http2ClientConn cc, @string name, @string value) => http_package.writeHeaderᴍ(ref cc, name, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addStreamLocked(this ж<http2ClientConn> Ꮡcc, ж<http2clientStream> Ꮡcs) => http_package.addStreamLockedᴍ(Ꮡcc, Ꮡcs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void forgetStreamID(this ж<http2ClientConn> Ꮡcc, uint32 id) => http_package.forgetStreamIDᴍ(Ꮡcc, id);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void readLoop(this ж<http2ClientConn> Ꮡcc) => http_package.readLoopᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void countReadFrameError(this ref http2ClientConn cc, error err) => http_package.countReadFrameErrorᴍ(ref cc, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Ping(this ж<http2ClientConn> Ꮡcc, context.Context ctx) => http_package.Pingᴍ(Ꮡcc, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeStreamReset(this ж<http2ClientConn> Ꮡcc, uint32 streamID, http2ErrCode code, bool ping, error err) => http_package.writeStreamResetᴍ(Ꮡcc, streamID, code, ping, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void logf(this ref http2ClientConn cc, @string format, params ꓸꓸꓸany argsʗp) => http_package.logfᴍ(ref cc, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void vlogf(this ref http2ClientConn cc, @string format, params ꓸꓸꓸany argsʗp) => http_package.vlogfᴍ(ref cc, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void maybeCallStateHook(this ref http2ClientConn cc) => http_package.maybeCallStateHookᴍ(ref cc);
}
partial class httpᴛhttp2GoAwayError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this http2GoAwayError e) => http_package.Errorᴍ(e);
}
partial class httpᴛhttp2clientConnReadLoop {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void cleanup(this ж<http2clientConnReadLoop> Ꮡrl) => http_package.cleanupᴍ(Ꮡrl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error run(this ж<http2clientConnReadLoop> Ꮡrl) => http_package.runᴍ(Ꮡrl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processHeaders(this ж<http2clientConnReadLoop> Ꮡrl, ж<http2MetaHeadersFrame> Ꮡf) => http_package.processHeadersᴍ(Ꮡrl, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Response>, error) handleResponse(this ref http2clientConnReadLoop rl, ж<http2clientStream> Ꮡcs, ж<http2MetaHeadersFrame> Ꮡf) => http_package.handleResponseᴍ(ref rl, Ꮡcs, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processTrailers(this ж<http2clientConnReadLoop> Ꮡrl, ж<http2clientStream> Ꮡcs, ж<http2MetaHeadersFrame> Ꮡf) => http_package.processTrailersᴍ(Ꮡrl, Ꮡcs, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processData(this ж<http2clientConnReadLoop> Ꮡrl, ж<http2DataFrame> Ꮡf) => http_package.processDataᴍ(Ꮡrl, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void endStream(this ж<http2clientConnReadLoop> Ꮡrl, ж<http2clientStream> Ꮡcs) => http_package.endStreamᴍ(Ꮡrl, Ꮡcs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void endStreamError(this ref http2clientConnReadLoop rl, ж<http2clientStream> Ꮡcs, error err) => http_package.endStreamErrorᴍ(ref rl, Ꮡcs, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void endStreamErrorLocked(this ref http2clientConnReadLoop rl, ж<http2clientStream> Ꮡcs, error err) => http_package.endStreamErrorLockedᴍ(ref rl, Ꮡcs, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<http2clientStream> streamByID(this ж<http2clientConnReadLoop> Ꮡrl, uint32 id, bool headerOrData) => http_package.streamByIDᴍ(Ꮡrl, id, headerOrData);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<http2clientStream> streamByIDLocked(this ref http2clientConnReadLoop rl, uint32 id, bool headerOrData) => http_package.streamByIDLockedᴍ(ref rl, id, headerOrData);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processGoAway(this ref http2clientConnReadLoop rl, ж<http2GoAwayFrame> Ꮡf) => http_package.processGoAwayᴍ(ref rl, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processSettings(this ж<http2clientConnReadLoop> Ꮡrl, ж<http2SettingsFrame> Ꮡf) => http_package.processSettingsᴍ(Ꮡrl, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processSettingsNoWrite(this ж<http2clientConnReadLoop> Ꮡrl, ж<http2SettingsFrame> Ꮡf) => http_package.processSettingsNoWriteᴍ(Ꮡrl, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processWindowUpdate(this ж<http2clientConnReadLoop> Ꮡrl, ж<http2WindowUpdateFrame> Ꮡf) => http_package.processWindowUpdateᴍ(Ꮡrl, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processResetStream(this ж<http2clientConnReadLoop> Ꮡrl, ж<http2RSTStreamFrame> Ꮡf) => http_package.processResetStreamᴍ(Ꮡrl, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processPing(this ж<http2clientConnReadLoop> Ꮡrl, ж<http2PingFrame> Ꮡf) => http_package.processPingᴍ(Ꮡrl, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processPushPromise(this ref http2clientConnReadLoop rl, ж<http2PushPromiseFrame> f) => http_package.processPushPromiseᴍ(ref rl, f);
}
partial class httpᴛhttp2transportResponseBody {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this http2transportResponseBody b, slice<byte> p) => http_package.Readᴍ(b, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this http2transportResponseBody b) => http_package.Closeᴍ(b);
}
partial class httpᴛhttp2noBodyReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this http2noBodyReader _) => http_package.Closeᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this http2noBodyReader _Δp0, slice<byte> _Δp1) => http_package.Readᴍ(_Δp0, _Δp1);
}
partial class httpᴛhttp2missingBody {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this http2missingBody _) => http_package.Closeᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this http2missingBody _Δp0, slice<byte> _Δp1) => http_package.Readᴍ(_Δp0, _Δp1);
}
partial class httpᴛhttp2erringRoundTripper {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error RoundTripErr(this http2erringRoundTripper rt) => http_package.RoundTripErrᴍ(rt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Response>, error) RoundTrip(this http2erringRoundTripper rt, ж<Request> _) => http_package.RoundTripᴍ(rt, _);
}
partial class httpᴛhttp2eofReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this http2eofReader _Δp0, slice<byte> _Δp1) => http_package.Readᴍ(_Δp0, _Δp1);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (byte, error) ReadByte(this http2eofReader _) => http_package.ReadByteᴍ(_);
}
partial class httpᴛhttp2gzipReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<gzip.Reader>, error) acquire(this ж<http2gzipReader> Ꮡgz) => http_package.acquireᴍ(Ꮡgz);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void release(this ж<http2gzipReader> Ꮡgz, ж<gzip.Reader> Ꮡzr) => http_package.releaseᴍ(Ꮡgz, Ꮡzr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void close(this ж<http2gzipReader> Ꮡgz) => http_package.closeᴍ(Ꮡgz);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ж<http2gzipReader> Ꮡgz, slice<byte> p) => http_package.Readᴍ(Ꮡgz, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ж<http2gzipReader> Ꮡgz) => http_package.Closeᴍ(Ꮡgz);
}
partial class httpᴛhttp2errorReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this http2errorReader r, slice<byte> p) => http_package.Readᴍ(r, p);
}
partial class httpᴛhttp2noDialH2RoundTripper {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Response>, error) RoundTrip(this http2noDialH2RoundTripper rt, ж<Request> req) => http_package.RoundTripᴍ(rt, req);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (RoundTripper, error) NewClientConn(this http2noDialH2RoundTripper rt, net.Conn conn, Action internalStateHook) => http_package.NewClientConnᴍ(rt, conn, internalStateHook);
}
partial class httpᴛhttp2netHTTPClientConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Response>, error) RoundTrip(this http2netHTTPClientConn cc, ж<Request> req) => http_package.RoundTripᴍ(cc, req);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this http2netHTTPClientConn cc) => http_package.Closeᴍ(cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Err(this http2netHTTPClientConn cc) => http_package.Errᴍ(cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Reserve(this http2netHTTPClientConn cc) => http_package.Reserveᴍ(cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Release(this http2netHTTPClientConn cc) => http_package.Releaseᴍ(cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Available(this http2netHTTPClientConn cc) => http_package.Availableᴍ(cc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint InFlight(this http2netHTTPClientConn cc) => http_package.InFlightᴍ(cc);
}
partial class httpᴛhttp2flushFrameWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeFrame(this http2flushFrameWriter _, http2writeContext ctx) => http_package.writeFrameᴍ(_, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool staysWithinBuffer(this http2flushFrameWriter _, nint max) => http_package.staysWithinBufferᴍ(_, max);
}
partial class httpᴛhttp2writeSettings {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool staysWithinBuffer(this http2writeSettings s, nint max) => http_package.staysWithinBufferᴍ(s, max);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeFrame(this http2writeSettings s, http2writeContext ctx) => http_package.writeFrameᴍ(s, ctx);
}
partial class httpᴛhttp2writeGoAway {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeFrame(this ref http2writeGoAway p, http2writeContext ctx) => http_package.writeFrameᴍ(ref p, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool staysWithinBuffer(this ref http2writeGoAway _, nint max) => http_package.staysWithinBufferᴍ(ref _, max);
}
partial class httpᴛhttp2writeData {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ref http2writeData w) => http_package.Stringᴍ(ref w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeFrame(this ref http2writeData w, http2writeContext ctx) => http_package.writeFrameᴍ(ref w, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool staysWithinBuffer(this ref http2writeData w, nint max) => http_package.staysWithinBufferᴍ(ref w, max);
}
partial class httpᴛhttp2handlerPanicRST {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeFrame(this http2handlerPanicRST hp, http2writeContext ctx) => http_package.writeFrameᴍ(hp, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool staysWithinBuffer(this http2handlerPanicRST hp, nint max) => http_package.staysWithinBufferᴍ(hp, max);
}
partial class httpᴛhttp2writePing {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeFrame(this http2writePing w, http2writeContext ctx) => http_package.writeFrameᴍ(w, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool staysWithinBuffer(this http2writePing w, nint max) => http_package.staysWithinBufferᴍ(w, max);
}
partial class httpᴛhttp2writePingAck {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeFrame(this http2writePingAck w, http2writeContext ctx) => http_package.writeFrameᴍ(w, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool staysWithinBuffer(this http2writePingAck w, nint max) => http_package.staysWithinBufferᴍ(w, max);
}
partial class httpᴛhttp2writeSettingsAck {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeFrame(this http2writeSettingsAck _, http2writeContext ctx) => http_package.writeFrameᴍ(_, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool staysWithinBuffer(this http2writeSettingsAck _, nint max) => http_package.staysWithinBufferᴍ(_, max);
}
partial class httpᴛhttp2writeResHeaders {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool staysWithinBuffer(this ref http2writeResHeaders w, nint max) => http_package.staysWithinBufferᴍ(ref w, max);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeFrame(this ж<http2writeResHeaders> Ꮡw, http2writeContext ctx) => http_package.writeFrameᴍ(Ꮡw, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeHeaderBlock(this ref http2writeResHeaders w, http2writeContext ctx, slice<byte> frag, bool firstFrag, bool lastFrag) => http_package.writeHeaderBlockᴍ(ref w, ctx, frag, firstFrag, lastFrag);
}
partial class httpᴛhttp2writePushPromise {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool staysWithinBuffer(this ref http2writePushPromise w, nint max) => http_package.staysWithinBufferᴍ(ref w, max);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeFrame(this ж<http2writePushPromise> Ꮡw, http2writeContext ctx) => http_package.writeFrameᴍ(Ꮡw, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeHeaderBlock(this ref http2writePushPromise w, http2writeContext ctx, slice<byte> frag, bool firstFrag, bool lastFrag) => http_package.writeHeaderBlockᴍ(ref w, ctx, frag, firstFrag, lastFrag);
}
partial class httpᴛhttp2write100ContinueHeadersFrame {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeFrame(this http2write100ContinueHeadersFrame w, http2writeContext ctx) => http_package.writeFrameᴍ(w, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool staysWithinBuffer(this http2write100ContinueHeadersFrame w, nint max) => http_package.staysWithinBufferᴍ(w, max);
}
partial class httpᴛhttp2writeWindowUpdate {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool staysWithinBuffer(this http2writeWindowUpdate wu, nint max) => http_package.staysWithinBufferᴍ(wu, max);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeFrame(this http2writeWindowUpdate wu, http2writeContext ctx) => http_package.writeFrameᴍ(wu, ctx);
}
partial class httpᴛhttp2FrameWriteRequest {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static uint32 StreamID(this http2FrameWriteRequest wr) => http_package.StreamIDᴍ(wr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isControl(this http2FrameWriteRequest wr) => http_package.isControlᴍ(wr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint DataSize(this http2FrameWriteRequest wr) => http_package.DataSizeᴍ(wr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (http2FrameWriteRequest, http2FrameWriteRequest, nint) Consume(this http2FrameWriteRequest wr, int32 n) => http_package.Consumeᴍ(wr, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this http2FrameWriteRequest wr) => http_package.Stringᴍ(wr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void replyToWriter(this ref http2FrameWriteRequest wr, error err) => http_package.replyToWriterᴍ(ref wr, err);
}
partial class httpᴛhttp2writeQueue {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool empty(this ref http2writeQueue q) => http_package.emptyᴍ(ref q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void push(this ref http2writeQueue q, http2FrameWriteRequest wr) => http_package.pushᴍ(ref q, wr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static http2FrameWriteRequest shift(this ref http2writeQueue q) => http_package.shiftᴍ(ref q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<http2FrameWriteRequest> peek(this ref http2writeQueue q) => http_package.peekᴍ(ref q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (http2FrameWriteRequest, bool) consume(this ref http2writeQueue q, int32 n) => http_package.consumeᴍ(ref q, n);
}
partial class httpᴛhttp2writeQueuePool {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void put(this ref http2writeQueuePool p, ж<http2writeQueue> Ꮡq) => http_package.putᴍ(ref p, Ꮡq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<http2writeQueue> get(this ref http2writeQueuePool p) => http_package.getᴍ(ref p);
}
partial class httpᴛhttp2priorityNodeRFC7540 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setParent(this ж<http2priorityNodeRFC7540> Ꮡn, ж<http2priorityNodeRFC7540> Ꮡparent) => http_package.setParentᴍ(Ꮡn, Ꮡparent);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addBytes(this ж<http2priorityNodeRFC7540> Ꮡn, int64 b) => http_package.addBytesᴍ(Ꮡn, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool walkReadyInOrder(this ж<http2priorityNodeRFC7540> Ꮡn, bool openParent, ж<slice<ж<http2priorityNodeRFC7540>>> Ꮡtmp, Func<ж<http2priorityNodeRFC7540>, bool, bool> f) => http_package.walkReadyInOrderᴍ(Ꮡn, openParent, Ꮡtmp, f);
}
partial class httpᴛhttp2sortPriorityNodeSiblingsRFC7540 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this http2sortPriorityNodeSiblingsRFC7540 z) => http_package.Lenᴍ(z);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Swap(this http2sortPriorityNodeSiblingsRFC7540 z, nint i, nint k) => http_package.Swapᴍ(z, i, k);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Less(this http2sortPriorityNodeSiblingsRFC7540 z, nint i, nint k) => http_package.Lessᴍ(z, i, k);
}
partial class httpᴛhttp2priorityWriteSchedulerRFC7540 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void OpenStream(this ж<http2priorityWriteSchedulerRFC7540> Ꮡws, uint32 streamID, http2OpenStreamOptions options) => http_package.OpenStreamᴍ(Ꮡws, streamID, options);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void CloseStream(this ж<http2priorityWriteSchedulerRFC7540> Ꮡws, uint32 streamID) => http_package.CloseStreamᴍ(Ꮡws, streamID);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void AdjustStream(this ж<http2priorityWriteSchedulerRFC7540> Ꮡws, uint32 streamID, http2PriorityParam priority) => http_package.AdjustStreamᴍ(Ꮡws, streamID, priority);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Push(this ж<http2priorityWriteSchedulerRFC7540> Ꮡws, http2FrameWriteRequest wr) => http_package.Pushᴍ(Ꮡws, wr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (http2FrameWriteRequest wr, bool ok) Pop(this ж<http2priorityWriteSchedulerRFC7540> Ꮡws) => http_package.Popᴍ(Ꮡws);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addClosedOrIdleNode(this ref http2priorityWriteSchedulerRFC7540 ws, ж<slice<ж<http2priorityNodeRFC7540>>> Ꮡlist, nint maxSize, ж<http2priorityNodeRFC7540> n) => http_package.addClosedOrIdleNodeᴍ(ref ws, Ꮡlist, maxSize, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void removeNode(this ref http2priorityWriteSchedulerRFC7540 ws, ж<http2priorityNodeRFC7540> Ꮡn) => http_package.removeNodeᴍ(ref ws, Ꮡn);
}
partial class httpᴛhttp2priorityWriteSchedulerRFC9218 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void OpenStream(this ref http2priorityWriteSchedulerRFC9218 ws, uint32 streamID, http2OpenStreamOptions opt) => http_package.OpenStreamᴍ(ref ws, streamID, opt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void CloseStream(this ref http2priorityWriteSchedulerRFC9218 ws, uint32 streamID) => http_package.CloseStreamᴍ(ref ws, streamID);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void AdjustStream(this ref http2priorityWriteSchedulerRFC9218 ws, uint32 streamID, http2PriorityParam priority) => http_package.AdjustStreamᴍ(ref ws, streamID, priority);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Push(this ref http2priorityWriteSchedulerRFC9218 ws, http2FrameWriteRequest wr) => http_package.Pushᴍ(ref ws, wr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (http2FrameWriteRequest, bool) Pop(this ref http2priorityWriteSchedulerRFC9218 ws) => http_package.Popᴍ(ref ws);
}
partial class httpᴛhttp2randomWriteScheduler {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void OpenStream(this ref http2randomWriteScheduler ws, uint32 streamID, http2OpenStreamOptions options) => http_package.OpenStreamᴍ(ref ws, streamID, options);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void CloseStream(this ref http2randomWriteScheduler ws, uint32 streamID) => http_package.CloseStreamᴍ(ref ws, streamID);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void AdjustStream(this ref http2randomWriteScheduler ws, uint32 streamID, http2PriorityParam priority) => http_package.AdjustStreamᴍ(ref ws, streamID, priority);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Push(this ref http2randomWriteScheduler ws, http2FrameWriteRequest wr) => http_package.Pushᴍ(ref ws, wr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (http2FrameWriteRequest, bool) Pop(this ref http2randomWriteScheduler ws) => http_package.Popᴍ(ref ws);
}
partial class httpᴛhttp2roundRobinWriteScheduler {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void OpenStream(this ref http2roundRobinWriteScheduler ws, uint32 streamID, http2OpenStreamOptions options) => http_package.OpenStreamᴍ(ref ws, streamID, options);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void CloseStream(this ref http2roundRobinWriteScheduler ws, uint32 streamID) => http_package.CloseStreamᴍ(ref ws, streamID);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void AdjustStream(this ref http2roundRobinWriteScheduler ws, uint32 streamID, http2PriorityParam priority) => http_package.AdjustStreamᴍ(ref ws, streamID, priority);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Push(this ref http2roundRobinWriteScheduler ws, http2FrameWriteRequest wr) => http_package.Pushᴍ(ref ws, wr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (http2FrameWriteRequest, bool) Pop(this ref http2roundRobinWriteScheduler ws) => http_package.Popᴍ(ref ws);
}
partial class httpᴛhttp2StreamError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool As(this http2StreamError e, any target) => http_package.Asᴍ(e, target);
}
partial class httpᴛHeader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Add(this Header h, @string key, @string value) => http_package.Addᴍ(h, key, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Set(this Header h, @string key, @string value) => http_package.Setᴍ(h, key, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Get(this Header h, @string key) => http_package.Getᴍ(h, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<@string> Values(this Header h, @string key) => http_package.Valuesᴍ(h, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string get(this Header h, @string key) => http_package.getᴍ(h, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool has(this Header h, @string key) => http_package.hasᴍ(h, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Del(this Header h, @string key) => http_package.Delᴍ(h, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Write(this Header h, io.Writer w) => http_package.Writeᴍ(h, w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error write(this Header h, io.Writer w, ж<httptrace.ClientTrace> trace) => http_package.writeᴍ(h, w, trace);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Header Clone(this Header h) => http_package.Cloneᴍ(h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<keyValues> kvs, ж<headerSorter> hs) sortedKeyValues(this Header h, map<@string, bool> exclude) => http_package.sortedKeyValuesᴍ(h, exclude);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error WriteSubset(this Header h, io.Writer w, map<@string, bool> exclude) => http_package.WriteSubsetᴍ(h, w, exclude);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeSubset(this Header h, io.Writer w, map<@string, bool> exclude, ж<httptrace.ClientTrace> Ꮡtrace) => http_package.writeSubsetᴍ(h, w, exclude, Ꮡtrace);
}
partial class httpᴛstringWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) WriteString(this stringWriter w, @string s) => http_package.WriteStringᴍ(w, s);
}
partial class httpᴛProtocols {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool HTTP1(this Protocols p) => http_package.HTTP1ᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetHTTP1(this ref Protocols p, bool ok) => http_package.SetHTTP1ᴍ(ref p, ok);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool HTTP2(this Protocols p) => http_package.HTTP2ᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetHTTP2(this ref Protocols p, bool ok) => http_package.SetHTTP2ᴍ(ref p, ok);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool UnencryptedHTTP2(this Protocols p) => http_package.UnencryptedHTTP2ᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetUnencryptedHTTP2(this ref Protocols p, bool ok) => http_package.SetUnencryptedHTTP2ᴍ(ref p, ok);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setBit(this ref Protocols p, uint8 bit, bool ok) => http_package.setBitᴍ(ref p, bit, ok);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Protocols p) => http_package.Stringᴍ(p);
}
partial class httpᴛcontextKey {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref contextKey k) => http_package.Stringᴍ(ref k);
}
partial class httpᴛnoBody {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) Read(this noBody _Δp0, slice<byte> _Δp1) => http_package.Readᴍ(_Δp0, _Δp1);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this noBody _) => http_package.Closeᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (int64, error) WriteTo(this noBody _Δp0, io.Writer _Δp1) => http_package.WriteToᴍ(_Δp0, _Δp1);
}
partial class httpᴛmapping {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void add<K, V>(this ref mapping<K, V> h, K k, V v) => http_package.addᴍ<K, V>(ref h, k, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (V v, bool found) find<K, V>(this ж<mapping<K, V>> Ꮡh, K k) => http_package.findᴍ<K, V>(Ꮡh, k);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void eachPair<K, V>(this ж<mapping<K, V>> Ꮡh, Func<K, V, bool> f) => http_package.eachPairᴍ<K, V>(Ꮡh, f);
}
partial class httpᴛpattern {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ref pattern p) => http_package.Stringᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static segment lastSegment(this ref pattern p) => http_package.lastSegmentᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool conflictsWith(this ref pattern p1, ж<pattern> Ꮡp2) => http_package.conflictsWithᴍ(ref p1, Ꮡp2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static relationship comparePathsAndMethods(this ref pattern p1, ж<pattern> p2) => http_package.comparePathsAndMethodsᴍ(ref p1, p2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static relationship compareMethods(this ref pattern p1, ж<pattern> Ꮡp2) => http_package.compareMethodsᴍ(ref p1, Ꮡp2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static relationship comparePaths(this ref pattern p1, ж<pattern> Ꮡp2) => http_package.comparePathsᴍ(ref p1, Ꮡp2);
}
partial class httpᴛProtocolError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ref ProtocolError pe) => http_package.Errorᴍ(ref pe);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Is(this ж<ProtocolError> Ꮡpe, error err) => http_package.Isᴍ(Ꮡpe, err);
}
partial class httpᴛRequest {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static context.Context Context(this ref Request r) => http_package.Contextᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Request> WithContext(this ref Request r, context.Context ctx) => http_package.WithContextᴍ(ref r, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Request> Clone(this ref Request r, context.Context ctx) => http_package.Cloneᴍ(ref r, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool ProtoAtLeast(this ref Request r, nint major, nint minor) => http_package.ProtoAtLeastᴍ(ref r, major, minor);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string UserAgent(this ref Request r) => http_package.UserAgentᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<ж<Cookie>> Cookies(this ref Request r) => http_package.Cookiesᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<ж<Cookie>> CookiesNamed(this ref Request r, @string name) => http_package.CookiesNamedᴍ(ref r, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Cookie>, error) Cookie(this ref Request r, @string name) => http_package.Cookieᴍ(ref r, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void AddCookie(this ref Request r, ж<Cookie> Ꮡc) => http_package.AddCookieᴍ(ref r, Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Referer(this ref Request r) => http_package.Refererᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<multipart.Reader>, error) MultipartReader(this ref Request r) => http_package.MultipartReaderᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<multipart.Reader>, error) multipartReader(this ref Request r, bool allowMixed) => http_package.multipartReaderᴍ(ref r, allowMixed);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isH2Upgrade(this ref Request r) => http_package.isH2Upgradeᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Write(this ж<Request> Ꮡr, io.Writer w) => http_package.Writeᴍ(Ꮡr, w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error WriteProxy(this ж<Request> Ꮡr, io.Writer w) => http_package.WriteProxyᴍ(Ꮡr, w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error /*err*/ write(this ж<Request> Ꮡr, io.Writer w, bool usingProxy, Header extraHeaders, Func<bool> waitForContinue) => http_package.writeᴍ(Ꮡr, w, usingProxy, extraHeaders, waitForContinue);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (@string username, @string password, bool ok) BasicAuth(this ref Request r) => http_package.BasicAuthᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetBasicAuth(this ref Request r, @string username, @string password) => http_package.SetBasicAuthᴍ(ref r, username, password);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error ParseForm(this ж<Request> Ꮡr) => http_package.ParseFormᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error ParseMultipartForm(this ж<Request> Ꮡr, int64 maxMemory) => http_package.ParseMultipartFormᴍ(Ꮡr, maxMemory);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string FormValue(this ж<Request> Ꮡr, @string key) => http_package.FormValueᴍ(Ꮡr, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string PostFormValue(this ж<Request> Ꮡr, @string key) => http_package.PostFormValueᴍ(Ꮡr, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (multipart.File, ж<multipart.FileHeader>, error) FormFile(this ж<Request> Ꮡr, @string key) => http_package.FormFileᴍ(Ꮡr, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string PathValue(this ref Request r, @string name) => http_package.PathValueᴍ(ref r, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetPathValue(this ref Request r, @string name, @string value) => http_package.SetPathValueᴍ(ref r, name, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint patIndex(this ref Request r, @string name) => http_package.patIndexᴍ(ref r, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool expectsContinue(this ref Request r) => http_package.expectsContinueᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool wantsHttp10KeepAlive(this ref Request r) => http_package.wantsHttp10KeepAliveᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool wantsClose(this ref Request r) => http_package.wantsCloseᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error closeBody(this ref Request r) => http_package.closeBodyᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isReplayable(this ref Request r) => http_package.isReplayableᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 outgoingLength(this ref Request r) => http_package.outgoingLengthᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool requiresHTTP1(this ref Request r) => http_package.requiresHTTP1ᴍ(ref r);
}
partial class httpᴛMaxBytesError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ref MaxBytesError e) => http_package.Errorᴍ(ref e);
}
partial class httpᴛmaxBytesReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ref maxBytesReader l, slice<byte> p) => http_package.Readᴍ(ref l, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ref maxBytesReader l) => http_package.Closeᴍ(ref l);
}
partial class httpᴛResponse {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<ж<Cookie>> Cookies(this ref Response r) => http_package.Cookiesᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<url.URL>, error) Location(this ref Response r) => http_package.Locationᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool ProtoAtLeast(this ref Response r, nint major, nint minor) => http_package.ProtoAtLeastᴍ(ref r, major, minor);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Write(this ref Response r, io.Writer w) => http_package.Writeᴍ(ref r, w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeBody(this ref Response r) => http_package.closeBodyᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool bodyIsWritable(this ref Response r) => http_package.bodyIsWritableᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isProtocolSwitch(this ref Response r) => http_package.isProtocolSwitchᴍ(ref r);
}
partial class httpᴛResponseController {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Flush(this ref ResponseController c) => http_package.Flushᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (net.Conn, ж<bufio.ReadWriter>, error) Hijack(this ref ResponseController c) => http_package.Hijackᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetReadDeadline(this ref ResponseController c, time.Time deadline) => http_package.SetReadDeadlineᴍ(ref c, deadline);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetWriteDeadline(this ref ResponseController c, time.Time deadline) => http_package.SetWriteDeadlineᴍ(ref c, deadline);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error EnableFullDuplex(this ref ResponseController c) => http_package.EnableFullDuplexᴍ(ref c);
}
partial class httpᴛTransport {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Response>, error) RoundTrip(this ж<Transport> Ꮡt, ж<Request> req) => http_package.RoundTripᴍ(Ꮡt, req);
}
partial class httpᴛroutingIndex {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addPattern(this ref routingIndex idx, ж<pattern> Ꮡpat) => http_package.addPatternᴍ(ref idx, Ꮡpat);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error /*err*/ possiblyConflictingPatterns(this ref routingIndex idx, ж<pattern> Ꮡpat, Func<ж<pattern>, error> f) => http_package.possiblyConflictingPatternsᴍ(ref idx, Ꮡpat, f);
}
partial class httpᴛroutingNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addPattern(this ж<routingNode> Ꮡroot, ж<pattern> Ꮡp, Handler h) => http_package.addPatternᴍ(Ꮡroot, Ꮡp, h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addSegments(this ж<routingNode> Ꮡn, slice<segment> segs, ж<pattern> p, Handler h) => http_package.addSegmentsᴍ(Ꮡn, segs, p, h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set(this ref routingNode n, ж<pattern> p, Handler h) => http_package.setᴍ(ref n, p, h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<routingNode> addChild(this ж<routingNode> Ꮡn, @string key) => http_package.addChildᴍ(Ꮡn, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<routingNode> findChild(this ж<routingNode> Ꮡn, @string key) => http_package.findChildᴍ(Ꮡn, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<routingNode>, slice<@string>) match(this ж<routingNode> Ꮡroot, @string host, @string method, @string path) => http_package.matchᴍ(Ꮡroot, host, method, path);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<routingNode>, slice<@string>) matchMethodAndPath(this ж<routingNode> Ꮡn, @string method, @string path) => http_package.matchMethodAndPathᴍ(Ꮡn, method, path);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<routingNode>, slice<@string>) matchPath(this ж<routingNode> Ꮡn, @string path, slice<@string> matches) => http_package.matchPathᴍ(Ꮡn, path, matches);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void matchingMethods(this ж<routingNode> Ꮡroot, @string host, @string path, map<@string, bool> methodSet) => http_package.matchingMethodsᴍ(Ꮡroot, host, path, methodSet);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void matchingMethodsPath(this ж<routingNode> Ꮡn, @string path, map<@string, bool> set) => http_package.matchingMethodsPathᴍ(Ꮡn, path, set);
}
partial class httpᴛserveMux121 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void handle(this ж<serveMux121> Ꮡmux, @string pattern, Handler handler) => http_package.handleᴍ(Ꮡmux, pattern, handler);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void handleFunc(this ж<serveMux121> Ꮡmux, @string pattern, Action<ResponseWriter, ж<Request>> handler) => http_package.handleFuncᴍ(Ꮡmux, pattern, handler);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Handler h, @string pattern) findHandler(this ж<serveMux121> Ꮡmux, ж<Request> Ꮡr) => http_package.findHandlerᴍ(Ꮡmux, Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Handler h, @string pattern) handler(this ж<serveMux121> Ꮡmux, @string host, @string path) => http_package.handlerᴍ(Ꮡmux, host, path);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Handler h, @string pattern) match(this ref serveMux121 mux, @string path) => http_package.matchᴍ(ref mux, path);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<url.URL>, bool) redirectToPathSlash(this ж<serveMux121> Ꮡmux, @string host, @string path, ж<url.URL> Ꮡu) => http_package.redirectToPathSlashᴍ(Ꮡmux, host, path, Ꮡu);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool shouldRedirectRLocked(this ref serveMux121 mux, @string host, @string path) => http_package.shouldRedirectRLockedᴍ(ref mux, host, path);
}
partial class httpᴛexactSig {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string match(this ref exactSig e, slice<byte> data, nint firstNonWS) => http_package.matchᴍ(ref e, data, firstNonWS);
}
partial class httpᴛmaskedSig {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string match(this ref maskedSig m, slice<byte> data, nint firstNonWS) => http_package.matchᴍ(ref m, data, firstNonWS);
}
partial class httpᴛhtmlSig {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string match(this htmlSig h, slice<byte> data, nint firstNonWS) => http_package.matchᴍ(h, data, firstNonWS);
}
partial class httpᴛmp4Sig {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string match(this mp4Sig _, slice<byte> data, nint firstNonWS) => http_package.matchᴍ(_, data, firstNonWS);
}
partial class httpᴛtextSig {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string match(this textSig _, slice<byte> data, nint firstNonWS) => http_package.matchᴍ(_, data, firstNonWS);
}
partial class httpᴛsocksDialer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (net.Addr, error ctxErr) connect(this ж<socksDialer> Ꮡd, context.Context ctx, net.Conn c, @string address) => http_package.connectᴍ(Ꮡd, ctx, c, address);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (net.Conn, error) DialContext(this ж<socksDialer> Ꮡd, context.Context ctx, @string network, @string address) => http_package.DialContextᴍ(Ꮡd, ctx, network, address);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (net.Addr, error) DialWithConn(this ж<socksDialer> Ꮡd, context.Context ctx, net.Conn c, @string network, @string address) => http_package.DialWithConnᴍ(Ꮡd, ctx, c, network, address);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (net.Conn, error) Dial(this ж<socksDialer> Ꮡd, @string network, @string address) => http_package.Dialᴍ(Ꮡd, network, address);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error validateTarget(this ref socksDialer d, @string network, @string address) => http_package.validateTargetᴍ(ref d, network, address);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (net.Addr proxy, net.Addr dst, error err) pathAddrs(this ref socksDialer d, @string address) => http_package.pathAddrsᴍ(ref d, address);
}
partial class httpᴛsocksCommand {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this socksCommand cmd) => http_package.Stringᴍ(cmd);
}
partial class httpᴛsocksReply {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this socksReply code) => http_package.Stringᴍ(code);
}
partial class httpᴛsocksAddr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Network(this ref socksAddr a) => http_package.Networkᴍ(ref a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ж<socksAddr> Ꮡa) => http_package.Stringᴍ(Ꮡa);
}
partial class httpᴛsocksConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static net.Addr BoundAddr(this ж<socksConn> Ꮡc) => http_package.BoundAddrᴍ(Ꮡc);
}
partial class httpᴛsocksUsernamePassword {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Authenticate(this ref socksUsernamePassword up, context.Context ctx, io.ReadWriter rw, socksAuthMethod auth) => http_package.Authenticateᴍ(ref up, ctx, rw, auth);
}
partial class httpᴛerrorReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this errorReader r, slice<byte> p) => http_package.Readᴍ(r, p);
}
partial class httpᴛbyteReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ref byteReader br, slice<byte> p) => http_package.Readᴍ(ref br, p);
}
partial class httpᴛtransferWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool shouldSendChunkedRequestBody(this ж<transferWriter> Ꮡt) => http_package.shouldSendChunkedRequestBodyᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void probeRequestBody(this ж<transferWriter> Ꮡt) => http_package.probeRequestBodyᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool shouldSendContentLength(this ref transferWriter t) => http_package.shouldSendContentLengthᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeHeader(this ref transferWriter t, io.Writer w, ж<httptrace.ClientTrace> Ꮡtrace) => http_package.writeHeaderᴍ(ref t, w, Ꮡtrace);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error /*err*/ writeBody(this ж<transferWriter> Ꮡt, io.Writer w) => http_package.writeBodyᴍ(Ꮡt, w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64 n, error err) doBodyCopy(this ж<transferWriter> Ꮡt, io.Writer dst, io.Reader src) => http_package.doBodyCopyᴍ(Ꮡt, dst, src);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static io.Reader unwrapBody(this ref transferWriter t) => http_package.unwrapBodyᴍ(ref t);
}
partial class httpᴛtransferReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool protoAtLeast(this ref transferReader t, nint m, nint n) => http_package.protoAtLeastᴍ(ref t, m, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error parseTransferEncoding(this ref transferReader t) => http_package.parseTransferEncodingᴍ(ref t);
}
partial class httpᴛunsupportedTEError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this ref unsupportedTEError uste) => http_package.Errorᴍ(ref uste);
}
partial class httpᴛbody {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ж<body> Ꮡb, slice<byte> p) => http_package.Readᴍ(Ꮡb, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) readLocked(this ref body b, slice<byte> p) => http_package.readLockedᴍ(ref b, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error readTrailer(this ref body b) => http_package.readTrailerᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 unreadDataSizeLocked(this ref body b) => http_package.unreadDataSizeLockedᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ж<body> Ꮡb) => http_package.Closeᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool didEarlyClose(this ж<body> Ꮡb) => http_package.didEarlyCloseᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool bodyRemains(this ж<body> Ꮡb) => http_package.bodyRemainsᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void registerOnHitEOF(this ж<body> Ꮡb, Action fn) => http_package.registerOnHitEOFᴍ(Ꮡb, fn);
}
partial class httpᴛbodyLocked {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this bodyLocked bl, slice<byte> p) => http_package.Readᴍ(bl, p);
}
partial class httpᴛfinishAsyncByteRead {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this finishAsyncByteRead fr, slice<byte> p) => http_package.Readᴍ(fr, p);
}
partial class httpᴛbufioFlushWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Write(this bufioFlushWriter fw, slice<byte> p) => http_package.Writeᴍ(fw, p);
}
partial class httpᴛTransport {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint writeBufferSize(this ref Transport t) => http_package.writeBufferSizeᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint readBufferSize(this ref Transport t) => http_package.readBufferSizeᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 maxHeaderResponseSize(this ref Transport t) => http_package.maxHeaderResponseSizeᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Transport> Clone(this ж<Transport> Ꮡt) => http_package.Cloneᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool hasCustomTLSDialer(this ref Transport t) => http_package.hasCustomTLSDialerᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void onceSetNextProtoDefaults(this ж<Transport> Ꮡt) => http_package.onceSetNextProtoDefaultsᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Protocols protocols(this ref Transport t) => http_package.protocolsᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool useRegisteredProtocol(this ref Transport t, ж<Request> Ꮡreq) => http_package.useRegisteredProtocolᴍ(ref t, Ꮡreq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static RoundTripper alternateRoundTripper(this ж<Transport> Ꮡt, ж<Request> Ꮡreq) => http_package.alternateRoundTripperᴍ(Ꮡt, Ꮡreq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Response>, error err) roundTrip(this ж<Transport> Ꮡt, ж<Request> Ꮡreq) => http_package.roundTripᴍ(Ꮡt, Ꮡreq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void RegisterProtocol(this ж<Transport> Ꮡt, @string scheme, RoundTripper rt) => http_package.RegisterProtocolᴍ(Ꮡt, scheme, rt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void CloseIdleConnections(this ж<Transport> Ꮡt) => http_package.CloseIdleConnectionsᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Action<error> prepareTransportCancel(this ж<Transport> Ꮡt, ж<Request> req, Action<error> origCancel) => http_package.prepareTransportCancelᴍ(Ꮡt, req, origCancel);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void CancelRequest(this ж<Transport> Ꮡt, ж<Request> req) => http_package.CancelRequestᴍ(Ꮡt, req);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (connectMethod cm, error err) connectMethodForRequest(this ref Transport t, ж<transportRequest> Ꮡtreq) => http_package.connectMethodForRequestᴍ(ref t, Ꮡtreq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void putOrCloseIdleConn(this ж<Transport> Ꮡt, ж<persistConn> pconn) => http_package.putOrCloseIdleConnᴍ(Ꮡt, pconn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint maxIdleConnsPerHost(this ref Transport t) => http_package.maxIdleConnsPerHostᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error tryPutIdleConn(this ж<Transport> Ꮡt, ж<persistConn> Ꮡpconn) => http_package.tryPutIdleConnᴍ(Ꮡt, Ꮡpconn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool /*delivered*/ queueForIdleConn(this ж<Transport> Ꮡt, ж<wantConn> Ꮡw) => http_package.queueForIdleConnᴍ(Ꮡt, Ꮡw);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool removeIdleConn(this ж<Transport> Ꮡt, ж<persistConn> Ꮡpconn) => http_package.removeIdleConnᴍ(Ꮡt, Ꮡpconn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool removeIdleConnLocked(this ref Transport t, ж<persistConn> Ꮡpconn) => http_package.removeIdleConnLockedᴍ(ref t, Ꮡpconn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (net.Conn, error) dial(this ref Transport t, context.Context ctx, @string network, @string addr) => http_package.dialᴍ(ref t, ctx, network, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (net.Conn conn, error err) customDialTLS(this ref Transport t, context.Context ctx, @string network, @string addr) => http_package.customDialTLSᴍ(ref t, ctx, network, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<persistConn>, error err) getConn(this ж<Transport> Ꮡt, ж<transportRequest> Ꮡtreq, connectMethod cm) => http_package.getConnᴍ(Ꮡt, Ꮡtreq, cm);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void queueForDial(this ж<Transport> Ꮡt, ж<wantConn> Ꮡw) => http_package.queueForDialᴍ(Ꮡt, Ꮡw);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void startDialConnForLocked(this ж<Transport> Ꮡt, ж<wantConn> w) => http_package.startDialConnForLockedᴍ(Ꮡt, w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void dialConnFor(this ж<Transport> Ꮡt, ж<wantConn> Ꮡw) => http_package.dialConnForᴍ(Ꮡt, Ꮡw);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void decConnsPerHost(this ж<Transport> Ꮡt, connectMethodKey key) => http_package.decConnsPerHostᴍ(Ꮡt, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<persistConn> pconn, error err) dialConn(this ж<Transport> Ꮡt, context.Context ctx, connectMethod cm, bool isClientConn, Action internalStateHook) => http_package.dialConnᴍ(Ꮡt, ctx, cm, isClientConn, internalStateHook);
}
partial class httpᴛtransportRequest {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Header extraHeaders(this ref transportRequest tr) => http_package.extraHeadersᴍ(ref tr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setError(this ж<transportRequest> Ꮡtr, error err) => http_package.setErrorᴍ(Ꮡtr, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void logf(this ref transportRequest tr, @string format, params ꓸꓸꓸany argsʗp) => http_package.logfᴍ(ref tr, format, argsʗp);
}
partial class httpᴛreadTrackingBody {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this ref readTrackingBody r, slice<byte> data) => http_package.Readᴍ(ref r, data);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ж<readTrackingBody> Ꮡr) => http_package.Closeᴍ(Ꮡr);
}
partial class httpᴛpersistConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool shouldRetryRequest(this ж<persistConn> Ꮡpc, ж<Request> Ꮡreq, error err) => http_package.shouldRetryRequestᴍ(Ꮡpc, Ꮡreq, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error addTLS(this ref persistConn pconn, context.Context ctx, @string name, ж<httptrace.ClientTrace> Ꮡtrace) => http_package.addTLSᴍ(ref pconn, ctx, name, Ꮡtrace);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 maxHeaderResponseSize(this ref persistConn pc) => http_package.maxHeaderResponseSizeᴍ(ref pc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ref persistConn pc, slice<byte> p) => http_package.Readᴍ(ref pc, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isBroken(this ж<persistConn> Ꮡpc) => http_package.isBrokenᴍ(Ꮡpc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error canceled(this ж<persistConn> Ꮡpc) => http_package.canceledᴍ(Ꮡpc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isReused(this ж<persistConn> Ꮡpc) => http_package.isReusedᴍ(Ꮡpc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void cancelRequest(this ж<persistConn> Ꮡpc, error err) => http_package.cancelRequestᴍ(Ꮡpc, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeConnIfStillIdle(this ж<persistConn> Ꮡpc) => http_package.closeConnIfStillIdleᴍ(Ꮡpc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error mapRoundTripError(this ж<persistConn> Ꮡpc, ж<transportRequest> Ꮡreq, int64 startBytesWritten, error err) => http_package.mapRoundTripErrorᴍ(Ꮡpc, Ꮡreq, startBytesWritten, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void readLoop(this ж<persistConn> Ꮡpc) => http_package.readLoopᴍ(Ꮡpc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void readLoopPeekFailLocked(this ref persistConn pc, error peekErr) => http_package.readLoopPeekFailLockedᴍ(ref pc, peekErr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Response> resp, error err) readResponse(this ref persistConn pc, requestAndChan rc, ж<httptrace.ClientTrace> Ꮡtrace) => http_package.readResponseᴍ(ref pc, rc, Ꮡtrace);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Func<bool> waitForContinue(this ж<persistConn> Ꮡpc, /*<-*/channel<EmptyStruct> continueCh) => http_package.waitForContinueᴍ(Ꮡpc, continueCh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeLoop(this ж<persistConn> Ꮡpc) => http_package.writeLoopᴍ(Ꮡpc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool wroteRequest(this ж<persistConn> Ꮡpc) => http_package.wroteRequestᴍ(Ꮡpc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error waitForAvailability(this ref persistConn pc, context.Context ctx) => http_package.waitForAvailabilityᴍ(ref pc, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Response> resp, error err) roundTrip(this ж<persistConn> Ꮡpc, ж<transportRequest> Ꮡreq) => http_package.roundTripᴍ(Ꮡpc, Ꮡreq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void markReused(this ж<persistConn> Ꮡpc) => http_package.markReusedᴍ(Ꮡpc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void close(this ж<persistConn> Ꮡpc, error err) => http_package.closeᴍ(Ꮡpc, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeLocked(this ref persistConn pc, error err) => http_package.closeLockedᴍ(ref pc, err);
}
partial class httpᴛconnectMethod {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string proxyAuth(this ref connectMethod cm) => http_package.proxyAuthᴍ(ref cm);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static connectMethodKey key(this ref connectMethod cm) => http_package.keyᴍ(ref cm);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string scheme(this ref connectMethod cm) => http_package.schemeᴍ(ref cm);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string addr(this ref connectMethod cm) => http_package.addrᴍ(ref cm);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string tlsHost(this ref connectMethod cm) => http_package.tlsHostᴍ(ref cm);
}
partial class httpᴛtransportReadFromServerError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Unwrap(this transportReadFromServerError e) => http_package.Unwrapᴍ(e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this transportReadFromServerError e) => http_package.Errorᴍ(e);
}
partial class httpᴛwantConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool waiting(this ж<wantConn> Ꮡw) => http_package.waitingᴍ(Ꮡw);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static context.Context getCtxForDial(this ж<wantConn> Ꮡw) => http_package.getCtxForDialᴍ(Ꮡw);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool tryDeliver(this ж<wantConn> Ꮡw, ж<persistConn> pc, error err, time.Time idleAt) => http_package.tryDeliverᴍ(Ꮡw, pc, err, idleAt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void cancel(this ж<wantConn> Ꮡw, ж<Transport> t) => http_package.cancelᴍ(Ꮡw, t);
}
partial class httpᴛwantConnQueue {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint len(this ref wantConnQueue q) => http_package.lenᴍ(ref q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pushBack(this ref wantConnQueue q, ж<wantConn> w) => http_package.pushBackᴍ(ref q, w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<wantConn> popFront(this ref wantConnQueue q) => http_package.popFrontᴍ(ref q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<wantConn> peekFront(this ref wantConnQueue q) => http_package.peekFrontᴍ(ref q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool /*cleaned*/ cleanFrontNotWaiting(this ref wantConnQueue q) => http_package.cleanFrontNotWaitingᴍ(ref q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void cleanFrontCanceled(this ref wantConnQueue q) => http_package.cleanFrontCanceledᴍ(ref q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void all(this ref wantConnQueue q, Action<ж<wantConn>> f) => http_package.allᴍ(ref q, f);
}
partial class httpᴛpersistConnWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Write(this persistConnWriter w, slice<byte> p) => http_package.Writeᴍ(w, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64 n, error err) ReadFrom(this persistConnWriter w, io.Reader r) => http_package.ReadFromᴍ(w, r);
}
partial class httpᴛconnectMethodKey {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this connectMethodKey k) => http_package.Stringᴍ(k);
}
partial class httpᴛreadWriteCloserBody {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this readWriteCloserBody recvᴛ) => http_package.Closeᴍ(recvᴛ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Write(this readWriteCloserBody recvᴛ, slice<byte> p) => http_package.Writeᴍ(recvᴛ, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ref readWriteCloserBody b, slice<byte> p) => http_package.Readᴍ(ref b, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error CloseWrite(this ref readWriteCloserBody b) => http_package.CloseWriteᴍ(ref b);
}
partial class httpᴛnothingWrittenError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Unwrap(this nothingWrittenError nwe) => http_package.Unwrapᴍ(nwe);
}
partial class httpᴛtimeoutError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this ref timeoutError e) => http_package.Errorᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Timeout(this ref timeoutError e) => http_package.Timeoutᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Temporary(this ref timeoutError e) => http_package.Temporaryᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Is(this ref timeoutError e, error err) => http_package.Isᴍ(ref e, err);
}
partial class httpᴛbodyEOFSignal {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ж<bodyEOFSignal> Ꮡes, slice<byte> p) => http_package.Readᴍ(Ꮡes, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ж<bodyEOFSignal> Ꮡes) => http_package.Closeᴍ(Ꮡes);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error condfn(this ref bodyEOFSignal es, error err) => http_package.condfnᴍ(ref es, err);
}
partial class httpᴛeofReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this eofReader _Δp0, slice<byte> _Δp1) => http_package.Readᴍ(_Δp0, _Δp1);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (byte, error) ReadByte(this eofReader _) => http_package.ReadByteᴍ(_);
}
partial class httpᴛgzipReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<gzip.Reader>, error) acquire(this ж<gzipReader> Ꮡgz) => http_package.acquireᴍ(Ꮡgz);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void release(this ж<gzipReader> Ꮡgz, ж<gzip.Reader> Ꮡzr) => http_package.releaseᴍ(Ꮡgz, Ꮡzr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void close(this ж<gzipReader> Ꮡgz) => http_package.closeᴍ(Ꮡgz);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ж<gzipReader> Ꮡgz, slice<byte> p) => http_package.Readᴍ(Ꮡgz, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ж<gzipReader> Ꮡgz) => http_package.Closeᴍ(Ꮡgz);
}
partial class httpᴛtlsHandshakeTimeoutError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Timeout(this tlsHandshakeTimeoutError _) => http_package.Timeoutᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Temporary(this tlsHandshakeTimeoutError _) => http_package.Temporaryᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this tlsHandshakeTimeoutError _) => http_package.Errorᴍ(_);
}
partial class httpᴛfakeLocker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Lock(this fakeLocker _) => http_package.Lockᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Unlock(this fakeLocker _) => http_package.Unlockᴍ(_);
}
partial class httpᴛconnLRU {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void add(this ref connLRU cl, ж<persistConn> pc) => http_package.addᴍ(ref cl, pc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<persistConn> removeOldest(this ref connLRU cl) => http_package.removeOldestᴍ(ref cl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void remove(this ref connLRU cl, ж<persistConn> pc) => http_package.removeᴍ(ref cl, pc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint len(this ref connLRU cl) => http_package.lenᴍ(ref cl);
}
// </GlyphForwarders>
