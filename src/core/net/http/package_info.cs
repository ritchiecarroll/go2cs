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
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using textprotoꓸError = go.net.textproto_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using tlsꓸConnectionState = go.crypto.tls_package.ΔConnectionState;
global using urlꓸError = go.net.url_package.ΔError;
using bufio = go.bufio_package;
using httptrace = go.net.http.httptrace_package;
using tls = go.crypto.tls_package;
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
[assembly: GoDynamicTypeLift("696e746572666163657b432829203c2d6368616e2074696d652e54696d653b20526573657428642074696d652e4475726174696f6e2920626f6f6c3b2053746f70282920626f6f6c7d", "http2timerᴛ1")]
[assembly: GoDynamicTypeLift("696e746572666163657b43616e63656c52657175657374282a6e65742f687474702e52657175657374297d", "setRequestCancel_canceler")]
[assembly: GoDynamicTypeLift("696e746572666163657b436c6f736549646c65436f6e6e656374696f6e7328297d", "CloseIdleConnections_closeIdler")]
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
[assembly: GoTypeAlias("Cookie", "ΔCookie")]
[assembly: GoTypeAlias("Handler", "ΔHandler")]
[assembly: GoTypeAlias("Header", "ΔHeader")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Dir, FileSystem>(Pointer = true)]
[assembly: GoImplement<Dir, FileSystem>]
[assembly: GoImplement<File, io_package.ReadSeeker>]
[assembly: GoImplement<HandlerFunc, ΔHandler>]
[assembly: GoImplement<MaxBytesError, error>(Pointer = true)]
[assembly: GoImplement<ProtocolError, error>(Pointer = true)]
[assembly: GoImplement<ResponseController, Hijacker>(Pointer = true)]
[assembly: GoImplement<ResponseWriter, io_package.Writer>]
[assembly: GoImplement<ServeMux, ΔHandler>(Pointer = true)]
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
[assembly: GoImplement<errorReader, io_package.Reader>]
[assembly: GoImplement<exactSig, sniffSig>(Pointer = true)]
[assembly: GoImplement<expectContinueReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<fakeLocker, sync_package.Locker>]
[assembly: GoImplement<fileHandler, ΔHandler>(Pointer = true)]
[assembly: GoImplement<fileInfoDirs, anyDirs>]
[assembly: GoImplement<fileTransport, RoundTripper>]
[assembly: GoImplement<finishAsyncByteRead, io_package.Reader>]
[assembly: GoImplement<globalOptionsHandler, ΔHandler>]
[assembly: GoImplement<go.net.http.internal_package.FlushAfterChunkWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<gzipReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<htmlSig, sniffSig>]
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
[assembly: GoImplement<http2erringRoundTripper, RoundTripper>]
[assembly: GoImplement<http2flushFrameWriter, http2writeFramer>]
[assembly: GoImplement<http2goAwayFlowError, error>]
[assembly: GoImplement<http2gzipReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<http2handlerPanicRST, http2writeFramer>]
[assembly: GoImplement<http2headerFieldNameError, error>]
[assembly: GoImplement<http2headerFieldValueError, error>]
[assembly: GoImplement<http2httpError, error>(Pointer = true)]
[assembly: GoImplement<http2missingBody, io_package.ReadCloser>]
[assembly: GoImplement<http2noBodyReader, io_package.ReadCloser>]
[assembly: GoImplement<http2noCachedConnError, error>]
[assembly: GoImplement<http2noDialClientConnPool, http2clientConnPoolIdleCloser>]
[assembly: GoImplement<http2noDialH2RoundTripper, RoundTripper>]
[assembly: GoImplement<http2priorityWriteScheduler, http2WriteScheduler>(Pointer = true)]
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
[assembly: GoImplement<http2sortPriorityNodeSiblings, sort_package.Interface>]
[assembly: GoImplement<http2sorter, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<http2stickyErrWriter, io_package.Writer>]
[assembly: GoImplement<http2timeTimer, http2timerᴛ1>]
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
[assembly: GoImplement<initALPNRequest, ΔHandler>]
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
[assembly: GoImplement<redirectHandler, ΔHandler>(Pointer = true)]
[assembly: GoImplement<requestBodyReadError, error>(Promoted = true)]
[assembly: GoImplement<requestBodyReadError, error>]
[assembly: GoImplement<response, ResponseWriter>(Pointer = true)]
[assembly: GoImplement<response, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<rᴛ1, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<rᴛ1, io_package.Reader>]
[assembly: GoImplement<rᴛ1, io_package.WriterTo>(Promoted = true)]
[assembly: GoImplement<serverHandler, ΔHandler>]
[assembly: GoImplement<socksAddr, net_package.ΔAddr>(Pointer = true)]
[assembly: GoImplement<socksConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<socksConn, net_package.Conn>(Promoted = true)]
[assembly: GoImplement<statusError, error>]
[assembly: GoImplement<stringWriter, io_package.StringWriter>]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<textSig, sniffSig>]
[assembly: GoImplement<timeoutError, error>(Pointer = true)]
[assembly: GoImplement<timeoutHandler, ΔHandler>(Pointer = true)]
[assembly: GoImplement<timeoutWriter, Pusher>(Pointer = true)]
[assembly: GoImplement<timeoutWriter, ResponseWriter>(Pointer = true)]
[assembly: GoImplement<tlsHandshakeTimeoutError, error>]
[assembly: GoImplement<transportReadFromServerError, error>]
[assembly: GoImplement<unencryptedHTTP2Request, ΔHandler>]
[assembly: GoImplement<unencryptedNetConnInTLSConn, net_package.Conn>(Promoted = true)]
[assembly: GoImplement<unencryptedNetConnInTLSConn, net_package.Conn>]
[assembly: GoImplement<unsupportedTEError, error>(Pointer = true)]
[assembly: GoImplement<writerOnly, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<writerOnly, io_package.Writer>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<HTTP2Config, ж<HTTP2Config>>(Indirect = true)]
[assembly: GoImplicitConv<Request, ж<Request>>(Indirect = true)]
[assembly: GoImplicitConv<Server, ж<Server>>(Indirect = true)]
[assembly: GoImplicitConv<Transport, ж<Transport>>(Indirect = true)]
[assembly: GoImplicitConv<http2ClientConn, ж<http2ClientConn>>(Indirect = true)]
[assembly: GoImplicitConv<http2MetaHeadersFrame, ж<http2MetaHeadersFrame>>(Indirect = true)]
[assembly: GoImplicitConv<http2Server, ж<http2Server>>(Indirect = true)]
[assembly: GoImplicitConv<http2writeData, ж<http2writeData>>]
[assembly: GoImplicitConv<http2writeResHeaders, ж<http2writeResHeaders>>(Indirect = true)]
[assembly: GoImplicitConv<httptrace.ClientTrace, ж<httptrace.ClientTrace>>]
[assembly: GoImplicitConv<tls.Conn, ж<tls.Conn>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("net/http/client.go", "client.cs", "AG3aAQAiTAAJCoKUgpaC7oKUqOKCgqaCgpSCgIK2poKClKaCgpS4+PKUgoKWgoKWgoK6goKC3oKCloCCgoKCgqaClISCgoKClIC4graUgpQAChaClJSC3KyygoKUAAIQ0pSAgqTEAAcQgpQACBgADAKClIKEpoKWgoKkhIKCloKEpIaCgriCooKCqIKEwsSCtIK0uAACENKCAAI4ABkCAAU0ABkCgoKUpoDsmKKCgpSq8pSCgozCxoKChMrG2qKClICCpAACUAAlAgAICvKCgKSCgsqEAAcYlIKUgoKUlNymgoK4lIKCgpSSuICCtoIACBKCgoKClN6CgqaogIKkuoIAARDSgpSEyoKCqIKCgoCUgoKUppKCgpSmlgAKDubIgoKCqAANGIKCgoKAgoK2goKCgoKmgsyCgpakggAIDIKClAACJgAQAgAGIgAQAoKClIIAAiYAEAIABSIADgIAAiIADgIABR4ADgKCgpQABhTyhoIADhyygoKUgpSClKaCgoKmAA0cgoKuwoLKgsqClKaigoKU", "231-236:1;367-367:1;379-388:2;391-396:3;401-412:4;594-594:1;617-633:2;769-816:1")]
[assembly: go.GoPositionMap("net/http/clone.go", "clone.cs", "AA4uAAkCgrgAAhgACwKClIKCgoKUAAIYAAsCgpSmgoKCgoKUlJQAAhgACwKClIKCggACHgAMAoKClA==")]
[assembly: go.GoPositionMap("net/http/cookie.go", "cookie.cs", "ABUoACBYgoKCgrqCgoKUgIKCgpSkrLKClIKClIKigqKClIKUkoKUlNqigoKUgqKClIKClJKClNyCgoKWgoKClIKCgpaUgoKClJSkpKSkpIKkgqSCtIKClIKUgsSCgoKCgoKmgrSCpIKkpAAFEuKCgsqClIKCgIK2rLKAggAREOKCmKKCgoKChIKClILKgoKUgpSmgoKClIKCpJSClIKUuKSktIKUAAkIsoKUgpSClIKCpoKCgriCgqaCgqYAAhYACAKCgsyCgpSCloKihIKSgoKClKKCgpSClJKClKaokoKUgpSopKyygpSClpSUgoKCgoKUpoK2trKUtpKUgpS0lIKWppSCAAUiAA4CgoKUgpSmgtqipoKmgoKCgpSCgpSClIKCgIK2AAIYAAwEgoKUgoKmpoKClA==")]
[assembly: go.GoPositionMap("net/http/filetransport.go", "filetransport.cs", "ABI+AA0CAAIiAA4Cpu6CkoKUpoKCAAoWAA8ggoKUgpSmgoKUhIKUpoKmgoKUhIKmgoKUgoKU", "61-64:1")]
[assembly: go.GoPositionMap("net/http/fs.go", "fs.cs", "ACFisoKWgoKClIKClIKm2qKCgpSCgpSCgpSCgoKUACNEgKKAooDIgKKAooDkiLKCgIKCgpSCgqaCgoKUlIKCgoKCgoLKgpTKnMKEgtyClIKUpoKWAAJEAB8CgoKClIKClJTuuAAKDOKCgoKWqIKCgoKEkoKCgoKCpqSWgoKClJSCqIKCgtbKgpSCtIKmypQAARqCgAALAoKkgoK0goSCgoKCkrKygoKClICCgqSAgoK2gsgAGTaClISCtKqygoKClIK4goLIpMaqoqqiAAwagoKClIKCgpSCgpSClIKClIKUltaCgoKUgoK6goCCpNaCgoKUgoKCgpSCgpSClIKClIKUlNaCgpSCgpSCgriCgIKk1oKClIKClIKCgpTKgpSCgpSClKaWktaCgrjcgoKCgoKU2tSCgpSCgpSUgoKUgraCgriCgpQACAjSuoKCloKCgoKUlIKCgoKWpoKCgoK2gpSCgpSCqIKUgoKogoKCkoKCgsyCgoKUgoKokqL+0oKUgqaqooCCpIIABTAAFwLcgpSCAAIuABYC3IKUpoKClIKCpqaAAA4cgoKUlIKCqKaAooCigKSSlIKCgpSmgoKClKaCgoKUgoKCgoKUlJSClIKmrLIAAiAADQIAAhgACQKmooKCgpQABxCCpoIACxCigpSCgpSCgqKCgpSCgpSCgtyClIKClIKUgpSCgpSmgpSClJSCgpSClKaUlJTMgoKqwoKCgoKUgoKmooKU", "155-155:1;241-251:1;370-388:1;755-755:1;887-889:1")]
[assembly: go.GoPositionMap("net/http/h2_bundle.go", "h2_bundle.cs", "AEiEAeKClIKCpqiSgpSqooKCpqqigpQAywLGBfIAAaoEpAAYMIIADSCCAAUQxJSCgoKClJSCgoK4gpSCgqaCgpSCgoKCgpSCgpSCABAksoCUpIKClIKSqJKChIKCgpSEAAIWAAoCgoKCgqaCgoKUuIKkhIKClAAJFIKEgoKClIKUgoKokoKCpoKUgpSCptKCgoKCgpSCgpSmoqTSgu6CgsakgoKCgsqClAAJEIKu4pSUuJSmuAAfRKIADBqCgqqiAAkWgqSWgpSCpqKCuIKCgoKClJSClJSCqqaigqiSqJKmooKUgpSClIKUgpSClIKUgpSClIKUgpSClIIADRwACBCClKSkpKTIgpSkpKSkpAARIpiigpSCgoKCgoKClIKCgoKCgqamgoKUqJKokoK4goKUgoKCgoKUpoKCgoKmgoKCABMuABAigoCCpKaCgIKkzoIADRyUgqaCgpQADRSAAA4cgsqCyoLKgsqCqIIADSCSAAIU8oKUhqKClIKmlIKCqqKClIKssoKUgoIADiKApIKCgpSmgoKUgoK8ooKCgpTKAA4mAAwagoCCpAAEEJIAGTgAFj4ADBqCgIKkABc8oNSCgoKCgtaigoKCgoKClIKClIKClLiClKaCgriAyJSCzMKCgtaCgoKUAE68AYKClKaUAAwYxoKClLiCloKClKaigoKCppSCgoKClKaApICkgKSCAAUWsoKU7oKClKiSAAkSkoKUgpSCrsKClAACFPLKmKKAgqQAAhoADAKCgpSCgpSClIKAgqSCgoCCpJSAgqSClIKUrsKCrLKCgoKWgoKCyoLalpSClLgAChaCrsKC9oLcgpSChIKCgoKCgqbKgpSCqIKCgpaCpoIAAhDSAAIYAAsCgIKkqqKClIKClIKClMqCgpSClIKClIKCABIagu6ClAAHEIKUgpSUgoCCuKSmgqaCgoKAgraqooLcgKaSgoK4goKCgoKCuJSCgoKClJSqooKCgIK2AAIQAAgCgoKClK7iggALGIDkgoKClIKClIKCprKCgpSCggALIMKC5oKCgpSCgpQABxCigoKCggAJHtKCpoIADhSCgoKUku6CgpSClAAIFPSClIKCAAscgoKmgqaCpoIACQaipsqClIKCgIKCtoKCgoKClIKCgoKCpoKClIIAGUwACgKClIKClIKUgpSClIKClIKCgpSClIKUgoIAGDaC5oKCgpSCgpSCggAJHAAIAoKUgpSCgoKUgoIADRSCgoKUgoKUruKClIKCAAwUgoKClKaCgqaCruKClIKClIKCAAoWgoKmggAIBqKm7oKYooKAgoK4goKClISUgpSCABRAAAkCgpSCgpSClIKClIKUgoKCqsKCgqaCgpSmgoKUAClaooKClIKmqqKCgqaqooKCpqaCkoKClKSk2oKCuIKUpoKClJTc8oKUpoKEgoKCgqKClJSUgoKCpoKCqIKCloKCgoKClISmpIKCAAgUgoK4zIKCuJaAgqaClICClLiChICCpIKCgpSUgIKCgpSkAAkGgoKClIKCgoKUgpSCxoKCgpSCgsaClLS0xqSmuIKClKaCgpSCuIKClIK4lKKCgoKUgoKClIKCgpTWlILKwpSCloKUgu6UgoKigsa06IK2goKEgoKClLS0tIKCtIKCgpaUgoKUhIKUgoKUloSCqJKClAAHEoKmggA6doKCgoKCuIKCgIKkpoKCgIKkAAgmgoKClIKCgpSCAAo0ABkyAAcQggALGoKopJSCtoK2graCtgAMIgAJFIKAgqQAAhgACQKClIKClIKmpoKUpKQACR7CqJKokgANHIIADh7MgoKUpqKCgoKUpoKCgpSCgoKCyoKs0oKUgoKClJSCgoKmgsqCgpSqopSkpKQACBKApICkgKTc7ICkgKSArOKCgpSCgqbGgoKCAAIgAA0CACVS8oKCgpSipNKCgoKU2gAIAoKCgpSCgpSClIKCgpSClPqCmgAIAoKCgpSCgsqClAAGENCqsKigpPKClIKCgpSClJSCgoKUlIKippKCuNbK4oKCgpTa8oKCgoKUpgALIIKCgpaUgoIAXNQBgoK4goKUqJKClKiSgpQABxCigpSCgqaigpSCgqaigpSCgpQACRAACAKClIKUgoCCgpS2hILagoKa5oIACBaEgpSCloKUgoKe0oaCpO6SypKCgoCClKSClJQAG0KigpSmooKUpqKCgpSCpgACIgAOAubSgpSCggAcNIKWgt6CloKUzIKCgoSCgpSCgoKEgIKCAAoWgoKWAAoYAAoWgriCuICCgqSWgIKmgoKWoqSygoKAgqSmgpSCggBDjgGCgoKUpoKCACFIgKSApICkoqaClICCAAgQgoKmgqassoK4ooK4ooCClAAEEMKAgqSqooKWgt6CgIKAgoKCgIL6pqKClJSUAAoWgoKCgoKUgoKUgpSCgoKClAALIsKCgpCSgoLmlOaUggALIOKCgoKUlKaigoK4goKAgsi0goKUgoCCguikwoKCgoKChIKW7oKUpqiAgqaAgoLsgoSCgpaCgoKWhIKUgoKCguSAgoKktLSGoqTogpSCgoLGtJS0lIKkgqSkgqSkpNa0xP6CgsyCgoL6ooKCgpaCgqaClqaCpt6SkpKSkpaApICkgKSApIKCAAoMmvKClIKkgoCCpJS2gpK0pIKCpvjIzMKCgoLKgpSCxLQAAhKktAAKBoKClAACFPKCtKgABBgADQKGABMogoKAgtyUtKaClMiCgqaCppSs0oKCloKClOr2toCCgoKCgoK4goKCgoKiuIKClIL+mMKCgpSChIKWhIKCgpQAARiKABAEtpaAktbKhAACGgAMAoKClIKCgoLclIKCgpSCgIKClIK2goKClJQAAhQACQKCgAAVKoKmooKCgpSUgoKCpqKCpqKCgoCCztKCgoKCgpSCAAgSpoKClIKCqJSCpIKkgoCCtoKCtIKUlAAICKKWgoCCpN6EgIKClKSWlKSkpKSkpKSo1ILosoKClIK4lNyUguaygpSC2pTclILGksaC1rKEgtyUgoKUprKCgpSCgpSClIKUlIKCgoKUgqaApoSkgIKClLaCgoLmsoKCgriUlLiUgILIgoKmooKAgqSClJSkpKSkpAAEENK21qIACBSCgoKC7qYACAaygoSCggAKFswACBSClISUlJSCqIKClIS4lJSCloKCgriClIIABxKCgpSClKaygoKUlKaCqJKqwoKEgqaClKqigoCU3qKmvKIADg6ygtyC3ICCptyClAAHEIKUhIIABxKClO6WgoKUhIKAgqSWgoKUgoKUgoSClKKCAAkUgoKWprKCgoKCgoKUqILMggAIBrKCgoKUgoKWgpSCgoK4lKaC1oLKlKaygIKkgqaigoKWgu6CgoKCgpaCgoKUlIKWAA0GsoQABxKCloKCggANGpaCgpSClIKWgoKUgoKAgoCClLakuAAPFLKEgoKWkoKmgIKokoKigrqilMiEgoKCgpSCgoKUlsoADRyEgqaigoKCgoKCgoIADBjCgoKCgoKUgpTcpqKCgoKCgpKUlIKUgoKUgoK60oKCgoKCgpSCgsqCgoKClJSUgqLUisKCqtKCgsqUgMqkgsSCpKS2qLIADCCygoIACw6ygoKmupKowoKCgoKUgpSClAARJIKCgqamsoKClIKUgoKUgpSCAA0ekpIAGjyygqaUpoCkgoKAgrassoKUgpSCAAcUAAkCgpaCloKCgpKAgoKAgpTGgoKUpoKCgpSCgJSmggAGEICCgoKCuJIACBKClIKmgpSCuoKClICCuILclAARTgAVAoKClIKCloKCgriCgqaClKKClKaCpJSmpoKCpoKUooKUpoKklKamlKaCpoKCgpSCgtyCgqT6poKCgpSCgoKCgoKigqaCpoKCgpSClKgACxaCuIKCgpTWgoKWlpSEgoKCgoKW3paCgoK4goKCgoKUAAIWAAgCpoLYkoKClIKUgpSClJaClLiCgoKCgrqSlgALBKKCgqiCloKogpSClIKCqIKClIKClIKUgpSCpoKC3IymgILcgpYABhDEpMjEpKSCAAwYsrqUgqiCgsyCqIKmgt6CgpSC3oL+lJaCgpYADB6igoKUgoKUooCC7AAKFrKCgIK2goKUpoKC/rKChoKk5qKClIKClIKClIK0gsSUgoKUggAOHoAAfsICooK4ooKUpqKClKiSgpSokoKUpoKClKaCgoKCgqaClIKUpoKuwoKssqaigriCgIKkgpSClIKUooKAgoLspIKUlIKUqJKCgoKUlM6CpqKCpqKClAB6mgKigIKkpsKCgqKkooKClIKmlLjCgoKSgoK0pKKClIKCkoKCAAoWsoKUgoIACxbUgOqygqYACxqCqqKCkoKUkoKCpoCCtoKU6MK4kramgoKCgoKUgoKCgoKAlIKClIKCgoK0grSC2AAKFoKUlIKClL6ygILKgoKCnuKCuIK6goKClJKCzIKWpoKClICClJSk1qKClIKClIKClKaCgoKUgpSClNaCgqSWgoKUgoCCpIKUqqKmgoKUpqKmooIAFyiCgoKCgpSCloKo3IKCgoKUgoKEgoKEgIKSpriCgIKkgpaCgoKCgoKCqICCgqaSpsKmgpKCgoKClLSm4oKCoqTigoSCloKUgpSCgriUuLjGrgAKAoKC3AAIAoKCgIKkggAgSuKCgoKUhIKCABMi0oKC1qKClILKAAcQlgAFFIKWqqKmgoKq6gACEuKmwoKSoqiigoKUgILIooKCgpSClISClKbSgoLWprKAgraCgrKCgoKCgoKClIKUtJKCtIKkhIKCgsjigoKCgoKUloKUgIKkgIK22sKCgoKUgoLcsoKC6LKCgIKkypSCgqKClKSUgoKUpoKC3PyygIKkgIKkgIKkrLKClIKUptKCgqKkgoK4gtayggANHgAMIJaUosSkpLiiggAJFJSCgriAgraWgoKCAA0cgpSWgtSkrKSCxoKC5IIABBDSgoKmAAESAA0CgoSAgqqigsyClILEpKSCyPakloKClIKAgoKCpIKClISCloKCgpQABhCCgoKWgoKUgoKC5LS0tLSkgoKCqICCgoKmuISCgoCCgpKC2oL0pKSCtKSkAAgKwoKEgqbEpKQACRKCgpSCgoKCgqiCgoKC3uKElN6CgoKClIKCgoKClIKWuKTogoKCgIKCABEkgoKmgoKUlJS2lIKUlIKWgoKCgpaqwoKmlIKClIKClIKCgqT+koKCgoKUgoKC3JSmgryWlAABENKCgoKUgMqkgpQADRqCgpSCgoKClKbigoKEgoKEgoKWgoKCgIKClIKmgoKCgoIAABDygoKUgoKmgoKCgpSkgrS4goKCgoKUgoKCgoLulJSCqLjMgoKCgoKWgpKCgoKCzIKUlICCpN4ACgKCgoKSgoKUgpTEpKTGgIKChJSClIKk+IKCgpSCprimABEGwoKCloKClIKClIKogoKCpJaCgoKCgoKCgpQABxKAgqSAgqbcgoKClIKCgpSClIKWgqKmAAYU7IKClIKC7KKCgoKUgpSClJSCpraWgqaClIKUggAGEIKCgpaCloKWkoKmlIKCqAACENKClIK4lKTKkoSCgoKCpoKWgoKmuIKmpoKClAAIFMKCgoKCgoKCuNKCgoKClIKCgriEgoKClIKWogAIEtKCgpKCgIKCgsQAChSCuIKClIKmwoKShILMgoKC7JQAABDigoKSpoKCloLa1oKiAAgIooKClICCgoKkgoKUgoKUgoKUpqKCgoKCgpSCgoKUgpSAgoCCgpSkpIKkgpSCgIKCpJaUtLS0tLS0tMSkgoKUgpT6soK4lILKlILKlJSClJaCgoCCtsqUlJSCgoKClAAJEgAJAoKWgoKUkoKWgoKC7oKCgpKCgpSCpoLKgoKUuoKClIC4gIIAChSCgIKkgpSCgpS2goIACAqCloKAgoCCAAsSpoKCloKClJSWgoKEgoKCgoKUprKUlIKmlKaWgoKClISCAAsSwoKEgpSCgoKCgoKUgpSCgoKCppSWgoKCkpSEgoKSgpSClJTWlIKChIKEgoKUgqiUgpSClvykxKaygoKCgoKCgpSCAAYQgoKCgoKClIKCgoKmlIKCuJSCgriUgoKCuKaCgoKYooCCpoKCgoCmgriCgoKUhIKCgpSClIKWgoKogpSm9oLKgoKCtKSiggAHFuKCgqaUgoKU1oKCgoKUuKKCgpSCgIK2gqbSpoKUgIKkgoKU1tKCgpSCgoKUloKSlKSCpKzCnIKylISkgqSAggACFAAIAraklIKWgsqUgpbW0oKCgpaClIKClJSCuJaUgtaygpSUgoKClICCpISCqLKEkoKAgqSUgIKCgqSUgoLCgoKCgIKCpICCgsSS1KSkpsjigoKCpICCgqSUgoKUlIKCkoCCpNYABxCqAAgKgoKCgoKUgqiClqKmoqaigriipsiApIDYgKSApIKCgqbqgKSAAAsWooKUgoKCgqamgoCCpILqgKiiqtKCgIK2ggAOEoKCgpSmuIKWgpamooKClKaygoKUgoKCgoKUhKaigriigriigriigriigriipqKCuKKClKqipoKClIIACxwACwKGgpQAHkaylKSq1NqCpoDIgoKoggAHEIKCgqaAAAgQgqaCpoIACBKCpoCkgqaA7KKmouqCpoLagqaAqr7kgoKCgpSCgIKklAAPIoKClKYABxD2ooKEgpaEgpSClIKWgoKWpoKC7gAPIpSmooKEgoKCgoSCgpamgoLuAAcQgoKCggAHEJQABxCApIKqwoK4gpSCgoKmlLiUgoKmpoKUxgA0eKKCgMqklKqiqqKAgqQAAhoACgKWgoKogoKUgpSClIKCAA0cAAgSuoKokoKAgpSkqqKClNakAAgSgKSCpoKClJSCgoKuwoKUgpSktLQABBDSgpSCqJKCgpSCgoKCACpg0qbe3IKClJQAGDiygpSCpoCCgpSUguyCgoKUgoKClLiigpIAAxQACQKClIK6gsyCgoKCgqaCgoKmuoKCgpSCgpSCgqbKgKSApKaCgoKUgpQAGUC0gIKClILugoKU3IKCgriigpSClIKWgoKEkoKCgpS4ooLMgoKClILcgoK6goKCgqiCAAgUgoKC3oKCgoKClKiCpqKCgpSCgriClKamsoKCgpSCgpS4goKCtpSUpqKClJSCgoKUpqKClIKuwgANIKiSgoKUgqaokoKClIKCgoKUppSCpoKAgoKClLYADi7SpqaCgpSCgoKCgriCgoK4goKClJSUgoKCpoKmpIKCgpSCuIKUgpSmlIKUgpSCgoCCgqSCgqY=", "2005-2005:1;2011-2017:2;3112-3147:1;3149-3149:2;3227-3234:1;4357-4379:1;4380-4382:2;4386-4398:3;4904-4904:1;5184-5194:1;5526-5526:1;6507-6528:1;6652-6656:1;6929-6943:1;6955-6969:1;7018-7021:1;7297-7342:1;7401-7403:1;7720-7736:1;7740-7742:2;7744-7751:3;7927-7930:1;7957-7961:1;8607-8622:1;8808-8817:1;8819-8844:2;8846-8867:3;9565-9658:1;9665-9668:2;9678-9689:3;9875-9877:1;10091-10093:1;10538-10588:1;10678-10690:1;10856-10860:1;12009-12031:1")]
[assembly: go.GoPositionMap("net/http/h2_error.go", "h2_error.cs", "AAoagoKCgpSCgoKClIKCgoKmgoKU")]
[assembly: go.GoPositionMap("net/http/header.go", "header.cs", "ABc8wgACENIAAhLiAAIQ0qiSgIKkqqKCrLKokqaCqJKCqIKClIKCgqaClIKClLYABRLigoKCpqbuggAMGs7igoKUgoKCpoKCrLKmooKClIKCgsqUooKCgoCCgraCpoKCpoIAAhTwrMKClIKU7oCCtoKmgIKkgqamgg==", "179-179:1")]
[assembly: go.GoPositionMap("net/http/http.go", "http.cs", "ACRSkKaQppCmkKaQppCkgoKUuIKCgpSClIKUAAocAA8ggKigqKKClKaCqJKCgoKmpoKCgoKUpoKUgoKCgoKUgoKmgpTuyICigKKAqII=")]
[assembly: go.GoPositionMap("net/http/mapping.go", "mapping.cs", "ABgykoKUgoKClJS+8oKUgoKUgoKmqsKClIKCgriCgg==")]
[assembly: go.GoPositionMap("net/http/pattern.go", "pattern.cs", "ACNKgKSCACVYABUCgpSCgoKosoCCpIKClIKUhIKUgoKUgoKAgoK2qIKWgpSCgpSClIKClIKCgJSCpoKUgpSCgoKUgpSCgpSClIKUgpSCtuaCgqaCgqamgoKUlAAKNgAPAriUgqaClIKUggACFAAJAoKUlJSClJSUgpSq5oKYooKCgoLcgsqClIKUqJKClIKUgpSClIKUlJSCgpSmgpQAAhYACAKUpKSClKSUpKTGzKKUpKTKkoKUqLKCgoKClIKUgvyClIKUqJKCuIKCgryigpKCgIKUtoKklKqihJKCgoKUgpbKgoKClKaUgsjsgpSCtriClKampJQ=", "89-93:1")]
[assembly: go.GoPositionMap("net/http/request.go", "request.cs", "ACtSAAkUgKaSAAIUioiIioqKloDGANwB+gMACwKClAACGAAJAoKUgoKCAAIWAAgCgpSCgoKCgoKAgoKCpIKCloCCgoKkgqqi2pKokqqigpS4nMKClIKUAAISAAgCgoCClAAEGAAIAtwACBLCgpSClILmgoKClIKUgoKUgoKUqqKokoKUAAguAA4CAAIS4sraAAkCgoKSyoKCgpSAggAHEIKCgpSUgoLuAAwagpTehIKCtoKCpoIABBTCgIKCpoKCqIKClIK6goKUgoKCgoKUgrqCgpSCgpaCgpaCgoKogoKWgqiCgIKCgraClIKCgqiAgoCC2oKCgoKUloKUAA0QAAkUgpSqooKUgoKUgoKU3LKUpKSClIKUgpSCgpSCgpSmAA0cqJIABTQAFwK4lIKUgpSCgpSCgqaCAAoWgpSCgpKCxoKSkpLGgpKSkgAUIoKCqKyygoKUAAUeABAClIKUgoKUgoKClAACHAALAqjSgoKClNqCgIKCgqSmooIAAhLigoKWggAHHAARAoKEhpKAgqSCgqiCgoKUgpSCgIIACxiCgpaAgqaUqIKClIKCAAgUgoKWhISCgpaUzJQABhoACgKSlAAKEJQAFRaygpSC3IKUhIKCgpaCAAAShoKkgqaCpoKC+LKCgpSmgpSClIKCgIKCpIKCgpSUgoKUgoIADRQAAioAFAKCgoKUgqaCgoKUgoKCgoKmgpSClKYABRYACgKClIKmlIKWgoKWgoKWgpSClJaEAAIcAA0CgpSAgqQAAhDygpSAgqSqwoKUgoKCpoKAgoK2rsKAgqSqooCClIKUzMaClIKCgoKUpqaCpoKClKaCgpSmgoKU5oKClNqCpqqigpSClAACFPKUpNqi", "585-589:1;592-599:2;928-931:1;935-938:2;942-945:3;963-963:4;1090-1094:1")]
[assembly: go.GoPositionMap("net/http/response.go", "response.cs", "ABgwAFrKAZLKnMKCgpSClAAGEuKCuoKCgpSUgoKUgoSCgpSCgpSAgriCgoKUlISEgoKWAAYU8oCCgILuogAKKAARBIKCgoLKloCCuIKChJKCgpSmlIIACRqCqIKClIKCqIKCuoKCgILKgIK4goKopoKCAAMW8oKqoqqiqqI=")]
[assembly: go.GoPositionMap("net/http/responsecontroller.go", "responsecontroller.cs", "ABNMABECAAoQkoKClKSCpMTOooKClKTEAAgU0oKClKTEAAgW4oKClKTEAAgeAAoCgoKUpMTOog==")]
[assembly: go.GoPositionMap("net/http/roundtrip.go", "roundtrip.cs", "AAooAAoS8g==")]
[assembly: go.GoPositionMap("net/http/routing_index.go", "routing_index.cs", "AB8+ooKUgpSCgoKUAAQcABEOgoKUgoKCprqAgqTKnuKCgoKClIKCgoKAgoKCyIKCgrqClA==", "64-75:1")]
[assembly: go.GoPositionMap("net/http/routing_tree.go", "routing_tree.cs", "ACZY1JSUrNKCgpSCgoKUgoKklLyigpSCqsKCgpSUgIKkgoKqwoKUgq7iuICCtqzSgpSAlKSUgILIruKC3IKClKbKgIIACBCCgILagKaClKSuwoKUgoKClKrCgpSCgriCgpSSgIKk", "233-238:1")]
[assembly: go.GoPositionMap("net/http/servemux121.go", "servemux121.cs", "ABcyvKKCggAQJOKChIKUgpSAgqaClIKCgpaCtKSCgpaCpoKCgqiSgpSo5riAgqa6griAgqaCgoKWqgAIAoKWgpSClIKU2rSCgrqCgqau8oKCgoKUgoKssoSCgIK4goKUgoCCuA==", "82-84:1")]
[assembly: go.GoPositionMap("net/http/server.go", "server.cs", "AC1WnpyaAJoB7gKMAC140oKC2OKClISCgoSCgoCCtoIAH0iClrKClJSUgoKCgqaCgpSClKaCgpSmgoKUgpSCgILIAEGaAYKmgqaCggARKKKCgoCCgpS2goKUgqassoKUlKqigoKCvMKCggAIGAAJAoK6goLegoKCgqiCloKCgoKWgoIADBCSuIKUABk2ooKCuKCk0oKCgpSClIKCoqSigoKCABcwgMikgoKCptKCgoKUgoKClKKkgKKAooAAAhgACgKCqJKCgrjCgoKCgpSUgoKUgoKUgpSCgoKClIKChIKCgpSChIIACxakgqSCgpSmgpSkpAACGAAJAoCCgoLIAAIYAAkCggACGAAJAoKCgIKCgrYAAhgACwKCgIIACRKCgpSmggACENKCyoKUgqYADhjCgpSCgoKCgoKUlIKClKaiggAQGJKChIKCgoKEAAoW9vKClpiCgIKkgIKkgoCCgriClIKUgoKClJaCloKEgoKClIKUgoKUgoK4hIKCgoKAgriClgANHoKUgoLqooK4hLimgriUggANHAAKFoLsooKCgoKCgoKUgqamooKCgpSCgoKUhKbegpaCgoSWgoSCloCCgoKUggARIt6CAAEQ8oKCgpSCgoKUgoKCgoIABxoACAKClISCgt6CgoKUgpKCgpSAgqSClJSWgoKCgpSCpoKCloIADiCCgrqCgoK6hIKCgraWggASKICCABAigpTKgpS0xrS0xKaCgpbKgrK86IKCgqiClKiCgoKmgqiClqaUgpaUtv6CgoK4goKU7oKogpSCzIaCgoKogoKCqqKCgpSCgpSigIIACRLigpSUgIKCgoKmzKKClAACSgAiAqaCqLKCgoKUlpSWgpSClIKWgoKUgpS4ooSCloKCgoSohIK8oriWlLqCloKWpoKCpoKmooKUgoKClKaCpoKWgqaCupKCABMoAAEQ4oKAggAWLqyylKQABRCigpSkpIKUgoKClICCyIKCrLAACRKA7JzCgpSAgqSAgqQADwjSgIKkgoKCgIKCgoKkgoKUgoKClIKogIKCgoKClICIsoCCgoKUpIK2goKUgoKAgoCCuIKk3IKClIKChIKCgqaCloKClJSChJ7SgoKuiOKmpoCCgqSCgsqCgpSCtoKWhIKUAAgUgoKCgoKUgoKCgpSUgoTKloCClO6AgqYAEByApKKClIKUAAkUgqaC7qKCgpSCgoKClIKUgpSCgoKmAA4agoKqAAkCgpSCgpaCgriCgoKU1qKClKaClLTEzKKClJSktAAGFpIABx4ADAIABxKogoKCqJCooAACEOKClIKCgoKCgoKCgoKUAAQeAAwCgNyCgpKolIKWgoCCuIKCgpS4uoSCgpSWgoLYAAYUggAIEoIAAhLiAIABhAKSyMqSgpSClKaUgpSmqKSClIKClAACJAARAoKUgt4ACAKCgoKkuIKCuLiCiKKCgpSUgoKUgqa4goKSgqaUAAISAA0CgoSmlIKCgqYABjoAHAKCzIK6gu6o5oKCgpSClNrCgoKUgpSCgpSUAAIS4oKUvrKClLyigpS8ooKUuIKAggAJCNKClIKUgIKmgoK6goKUloKUgJKCgqaUpIKCAAUcAAsCggACHgAMAoIAcJoCAA4CgoKCzIKChIKClAAOQAAYAoSCgoKkgoSClJSCgpSWgpKCgpS0pAAIFPKCgqrygoKCgriClKaClIKU1oKCgoCCtgAKVgAHEIIACCQACwKCgpSClqYAARYACQKCgoKCgoKClAAHGgAKAoKUgoKUgoKUzqLulIIACBLKAAEYAA8CgIKmgoKEgIKmgpSEgoKCgqiEgoKCgoKUgIKClJSAgqSCgqSUgoCCgoK2goKCAAckABIGgIKmgoSCgoKCgoKogqaCgrqCuoKWgoKClKr8goKSlIKUpIKUpJSClIKUAAIaAA8CgoKClIKClIKUgpTW0oKCgpSClLSkgoKUpoKClKaCpoKuwoKClJaqsoKUvuKCgpQAAxbyggACENKCAAUkABMCgpSCgpaCgpaU3NKCAAIWAAoCgqaigria0oKUgoKUgoKUgLikggACHAALAgAIEgANGIKClKbSgoKCgqSCgsqC0pKAgraCopLEtIKSgoKClIK0gpKApIKCpIL2AA0clpKAgqSmgKTSgoKClIKU1oKElKSCgsaC2NKCgqIAEhSigqaA6qKC3IIADCLApKKCgpSClIKUAB4WlqKCgoIACAyygoKCprKCgoKmooKCggAJFLKCgoKUpqKCgoKUlKrClKSokoKSgg==", "1037-1039:1;1323-1335:1;1943-1962:1;2191-2196:1;2346-2360:1;2706-2709:1;2904-2911:1;3152-3161:1;3316-3327:1;3531-3545:1;3821-3829:1;3822-3826:1.1;4058-4062:1")]
[assembly: go.GoPositionMap("net/http/sniff.go", "sniff.cs", "AA4q4oKogpaCgIK4qqKUpKqilKQAIRIAN5ACgoKUAAgSqIKUgpSClIKCgqbKgoKClIKCgpSCuIKUppL4poKUgoKUgpSClJSCptqUgpq2")]
[assembly: go.GoPositionMap("net/http/socks_bundle.go", "socks_bundle.cs", "ABQuggAIBuKCgpSAgoKkgoKCooKCpqK0grTKgoKClIKClIKCpoCCpoCCpIKUgoKUgoCCuIKCgIKAgoKigoKUxoKUgoKkgoCCpoCCpIKUgIKkgpSCgpSCpIKkgIKkpKSClJSAgqSClJSC5oKCgpSCgpSClAAIDIKUpKQAExSClKSkpKSkpKSkpAAkPIDUooKUgoKUAAgYwoKUABdEAAwCgIKCpIKClIKCgpSClIKClIKCgoKUAAISAAgCgIKCpIKClIKCgpQAAhIACAKAgoKkgoKClJSCgpSAgoKk5oK2pLakpsKCkoKUgoKClIKUpqqiABEiopSkgpSCgoKChoCipICCpIKUgpSk", "39-44:1;45-53:2")]
[assembly: go.GoPositionMap("net/http/status.go", "status.cs", "AMYBogGilKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSk")]
[assembly: go.GoPositionMap("net/http/transfer.go", "transfer.cs", "ACI+3IIABxCCgpSClIKCABUuopaClIKUgoKCgoKCgoKCAAES8pa0goKUgoKCgoKCgoK4goKCpoKUgrS6gpYAAioAFgaSlIKUuILcAAIkABECgoKCgoKClIKUgrSClIKkgpS2zsjIsqaCgpSClIKmgpSCgpSWAAgGooKAgqSC3oKAgqSAgqSCtoCCpIK6goKigpSklIKmgIKkgrqo4oKCgoKUgIIABxCCgoKAgqSCgoK2goKUlIKClIKClIKmgoKAgriCqJSCgILIlN4ACgKChIKClAAFENKAgqSAgoKkABEkgqqilKSkpKiCgpaClqS0qLKWgpSCgoKCgoKCxoKCgoaCxNiCqICCpoKClIKAgpS2qIKCzJSU7JSClMa0tqSm2pSCgoKUgrSCgoKUgqaokKaQ/oKqooKokoKClJaC3oKUgpaCrOKClsqCgoK6goSogoKCuoKUgpSUAA8ggoKogpaEAAcQqKyygpaCgoKWgpbYkoKClAAHEJSEgoKCkoKUgoK2poKUgpQAFjCU4oKCgpTowoKUhIKUgoCCyoKkuICCAAkUgoCCgriClqiClqKmgoKUgqamxJSCgoKUgpSCAAkWgpaCgoKUlJS0pKaCgoKUrLKAgqSm0oKCgpSCAAcSgPaUpoKClILs1ILW0oKC2vKCgtbSgoKiAAgQgoKUptqygpSogoKClJSCgpQACBKygpSCgoKUgpSm4p6ilKTOsrSkgIKkgIKkAAgOsoKAgoKCtg==", "210-219:1;341-348:1;787-797:1")]
[assembly: go.GoPositionMap("net/http/transport.go", "transport.cs", "ADVaAO8BmgSCgpSmgoKUqLKCABYugpSCgpSCgpSCgoKUlAAMGoKmmMKCgoIABhCCgIKAgoCCguqAlKSCgpSClIKCgpQABhCAgoKClAAIEqaCgpSCgpiiAAoWAAkEAAImABICqqIADyiCgpSmooKClKrCypSs4oKUgqaCgoKUgqa4+AAIAoKChIKClIKClIKClICCgriAgoK4goSAgoCCpIKCgraCgpSCgpSCggAKGJaC3oSSgqiCpILagoKCgt6CgoKWgpSUlILclIKogoLagIKkgIKkgLiklJaCggAICoK0+gAHEIKCpoKCruKClJKCrvKClIKUgpSCgpSSgqzS7pSUlAAHEJSApqSUlICmpLiUuAABGAAPAoKCgoCCpIKClIKirOKCgoKCgoKCgoKmgoKCpoKAgsoACQ6CgoKClIKClIKCAAISAAgCgoKCggAKHqKClKiSgqbCgoKClIKqooKUgIKCgqS6kpKSkpKSkpwADR6ApIKmgoCCyIKAgqQAAhAACwKClIKUhIK6gt6CgIKCpoKCgoIABxCCgqaClJSCuIKUgpSCgpSCgqaCgoKCgsyCgpSmgtwACQKCloKohJScsoKogIKCgoK6grik3IKUgoLcgqaUgpSUgsqClIKCgoLo4oKC2LKClIKCgoLIkoLGgoK4goKCxgAJCoKCgoKUlIKCgpSUAB1C4oKE2OKChNjigoSClIKUgoSChNrSgoKCgIK2lIKCuoIAFCySqJKokoKCppSCgoKokoKUgpSqwoKCgpSCupKCgoKUvKKClILosoKUlIKUrgAKAoKCgoIABhCEAAgSkoK6gIK4uKK4goKUlLikgoKU2qSCkpT8AAgChIKEgoKWgIKClIKCpoKUgoKCoqjCgoKigoKCorzygoKCgpaCgriUgrSo8oKWgoKCpt6AgoKCgoKCgqaCuJSCyoCClMSq5IKClIKUgoKCgoCCkraygpSCgpSUgIKCppSClKSSgpSCgu7k4gAIEoKClJSUgoKCgpSApoKUgIKCgpSkkoKUtoKClIKCgoCCpICC3MiCkoCCpoK4tICCgtaCgIKC6IKCgoKCgoKmlIKUgIKCpAAFFLKUgprigoKCuIKikrSCgsiSgpaCgoKCqIKCgoKUyIKAgsqIgoKClIKAlKSWgIKAgoKAlKTIgoSCggAQGrKCgqzigoKmABw6goKCgoKCpgAHEpKClKiSgpSqooKClAAKFoSSgpQAK2CCgIKkprKClIKUgoKUgqiygoKCqvKCgtiygoKCptKCgoKiqvKCgpKAlKSCogACFAALAoIACBS6gIK4goKCgpaUloCCgqakgoKUlNyUwpKCgpaCgoCCgoKUpIKUzIKWgoKEgoKChIKCgoKUhIKEgoKUgpaCgpbmlJSEgoKEgoS4ltyKgpbmlrqCloL2goKogoKCpICCtqiCgoKCgoKW5szEioLGgrSmgrSkgoKUgIKCgoKUtpSUvrKClIKUrPKCgIK4goKCgpSCgoKUgpSUgoKCgIIACRKUlJSClAAMGoKUqIKssoKUsoKUxKSkAAgKooKClAAVHLKCgIKkgoKUlAAHEIKmwoKCtIKCgIIABxCkgpSCgqaCgoKCxgAQHuKoAAIYgpK0pABBaICigKKAooCkyJK4lLiikqKSlJKW8oKCgoKEgt6CAAwggpaCgpaGloKEuoKChIIABhCCgpSClIKUloKCgoKC5IKUgoKUgIKClIKS1qrGspSkgpSCpKSqxgASEqKAgszCgoIAAhAACgKCgqKkgoKUgoKCpoKClKam7qKCgIKkqLKCgpQAFi6U4oKCgoKUgpaCgoKCgpSU1tKCgoKUgoKUgtiSgpSCggALGKKCgpSCqIKClISClKaC2oCigNKAAAgOogACHgANAoKUAAcSkoKClIKAgqSmgoKCgoKokoCCgsqS", "506-508:1;659-663:1;901-905:1;920-925:1;962-964:1;1511-1515:1;1599-1604:1;1698-1700:1;1702-1711:2;1750-1756:1;1824-1826:2;1867-1877:3;2244-2247:1;2249-2262:2;2360-2365:3;2366-2377:4;2533-2545:1;2811-2822:1")]
[assembly: go.GoPositionMap("net/http/transport_default_other.go", "transport_default_other.cs", "AAscgg==")]
// </GoSourcePositionMaps>

namespace go.net;

[GoPackage("http")]
public static partial class http_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("closeIdler")] internal partial interface CloseIdleConnections_closeIdler {}
    internal partial interface EnableFullDuplex_type {}
    internal partial interface Flush_type {}
    [GoLocalName("requestTooLarger")] internal partial interface Read_requestTooLarger {}
    internal partial interface SetReadDeadline_type {}
    internal partial interface SetWriteDeadline_type {}
    internal partial interface anyDirs {}
    internal partial interface closeWriter {}
    internal partial interface erringRoundTripper {}
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
    internal partial interface http2synctestGroupInterface {}
    internal partial interface http2timerᴛ1 {}
    internal partial interface http2unencryptedNetConnFromTLSConn_type {}
    internal partial interface http2writeContext {}
    internal partial interface http2writeFramer {}
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
    internal partial struct http2dataBuffer {}
    internal partial struct http2dialCall {}
    internal partial struct http2duplicatePseudoHeaderError {}
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
    internal partial struct http2noBodyReader {}
    internal partial struct http2noCachedConnError {}
    internal partial struct http2noDialClientConnPool {}
    internal partial struct http2noDialH2RoundTripper {}
    internal partial struct http2outflow {}
    internal partial struct http2pipe {}
    internal partial struct http2priorityNode {}
    internal partial struct http2priorityNodeState {}
    internal partial struct http2priorityWriteScheduler {}
    internal partial struct http2pseudoHeaderError {}
    internal partial struct http2randomWriteScheduler {}
    internal partial struct http2readFrameResult {}
    internal partial struct http2requestBody {}
    internal partial struct http2requestParam {}
    internal partial struct http2resAndError {}
    internal partial struct http2responseWriter {}
    internal partial struct http2responseWriterState {}
    internal partial struct http2roundRobinWriteScheduler {}
    [GoValueClone("sentPingData")] internal partial struct http2serverConn {}
    internal partial struct http2serverInternalState {}
    internal partial struct http2serverMessage {}
    internal partial struct http2sortPriorityNodeSiblings {}
    internal partial struct http2sorter {}
    internal partial struct http2startPushRequest {}
    internal partial struct http2stickyErrWriter {}
    internal partial struct http2stream {}
    internal partial struct http2streamState {}
    internal partial struct http2timeTimer {}
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
    internal partial struct writerOnly {}
    internal partial struct ΔwriteRequest {}
    public partial interface CloseNotifier {}
    public partial interface CookieJar {}
    public partial interface File {}
    public partial interface FileSystem {}
    public partial interface Flusher {}
    public partial interface Hijacker {}
    public partial interface Pusher {}
    public partial interface ResponseWriter {}
    public partial interface RoundTripper {}
    public partial interface http2ClientConnPool {}
    public partial interface http2WriteScheduler {}
    public partial interface ΔHandler {}
    public partial struct Client {}
    public partial struct ConnState {}
    public partial struct Dir {}
    public partial struct HTTP2Config {}
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
    public partial struct ΔCookie {}
    public partial struct ΔHeader {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
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
