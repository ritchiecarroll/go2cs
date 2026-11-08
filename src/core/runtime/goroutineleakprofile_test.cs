// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δos = os_package;
using Δregexp = regexp_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using static global::go.runtime_internal_test_package;
using ꓸꓸꓸstring = Span<@string>;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goflagsˢ => ref goflagsˢᵇ;
private static readonly @string goflagsˢᵇ = "GOFLAGS"u8;
internal static ref readonly @string goroutineDˢ => ref goroutineDˢᵇ;
private static readonly @string goroutineDˢᵇ = @"goroutine \d+ \["u8;
internal static ref readonly @string goroutineˢ => ref goroutineˢᵇ;
private static readonly @string goroutineˢᵇ = "\n\ngoroutine"u8;
internal static ref readonly @string leakedˢ => ref leakedˢᵇ;
private static readonly @string leakedˢᵇ = "(leaked)"u8;
internal static ref readonly @string leakedˢ2 => ref leakedˢ2ᵇ;
private static readonly @string leakedˢ2ᵇ = " (leaked)"u8;
internal static ref readonly @string createdByˢ => ref createdByˢᵇ;
private static readonly @string createdByˢᵇ = "created by"u8;
internal static ref readonly @string mainˢ2 => ref mainˢ2ᵇ;
private static readonly @string mainˢ2ᵇ = "main."u8;
internal static ref readonly @string nilRecvˢ => ref nilRecvˢᵇ;
private static readonly @string nilRecvˢᵇ = "NilRecv"u8;
internal static ref readonly @string nilRecvFunc1ChanReceiveˢ => ref nilRecvFunc1ChanReceiveˢᵇ;
private static readonly @string nilRecvFunc1ChanReceiveˢᵇ = @"NilRecv\.func1\(.* \[chan receive \(nil chan\)\]"u8;
internal static ref readonly @string nilSendˢ => ref nilSendˢᵇ;
private static readonly @string nilSendˢᵇ = "NilSend"u8;
internal static ref readonly @string nilSendFunc1ChanSendNilˢ => ref nilSendFunc1ChanSendNilˢᵇ;
private static readonly @string nilSendFunc1ChanSendNilˢᵇ = @"NilSend\.func1\(.* \[chan send \(nil chan\)\]"u8;
internal static ref readonly @string selectNoCasesˢ => ref selectNoCasesˢᵇ;
private static readonly @string selectNoCasesˢᵇ = "SelectNoCases"u8;
internal static ref readonly @string selectNoCasesFunc1Selectˢ => ref selectNoCasesFunc1Selectˢᵇ;
private static readonly @string selectNoCasesFunc1Selectˢᵇ = @"SelectNoCases\.func1\(.* \[select \(no cases\)\]"u8;
internal static ref readonly @string chanRecvˢ => ref chanRecvˢᵇ;
private static readonly @string chanRecvˢᵇ = "ChanRecv"u8;
internal static ref readonly @string chanRecvFunc1ChanReceiveˢ => ref chanRecvFunc1ChanReceiveˢᵇ;
private static readonly @string chanRecvFunc1ChanReceiveˢᵇ = @"ChanRecv\.func1\(.* \[chan receive\]"u8;
internal static ref readonly @string chanSendˢ => ref chanSendˢᵇ;
private static readonly @string chanSendˢᵇ = "ChanSend"u8;
internal static ref readonly @string chanSendFunc1ChanSendˢ => ref chanSendFunc1ChanSendˢᵇ;
private static readonly @string chanSendFunc1ChanSendˢᵇ = @"ChanSend\.func1\(.* \[chan send\]"u8;
internal static ref readonly @string selectˢ => ref selectˢᵇ;
private static readonly @string selectˢᵇ = "Select"u8;
internal static ref readonly @string selectFunc1Selectˢ => ref selectFunc1Selectˢᵇ;
private static readonly @string selectFunc1Selectˢᵇ = @"Select\.func1\(.* \[select\]"u8;
internal static ref readonly @string waitGroupˢ => ref waitGroupˢᵇ;
private static readonly @string waitGroupˢᵇ = "WaitGroup"u8;
internal static ref readonly @string waitGroupFunc1Syncˢ => ref waitGroupFunc1Syncˢᵇ;
private static readonly @string waitGroupFunc1Syncˢᵇ = @"WaitGroup\.func1\(.* \[sync\.WaitGroup\.Wait\]"u8;
internal static ref readonly @string mutexStackˢ => ref mutexStackˢᵇ;
private static readonly @string mutexStackˢᵇ = "MutexStack"u8;
internal static ref readonly @string mutexStackFunc1SyncMutexˢ => ref mutexStackFunc1SyncMutexˢᵇ;
private static readonly @string mutexStackFunc1SyncMutexˢᵇ = @"MutexStack\.func1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string mutexHeapˢ => ref mutexHeapˢᵇ;
private static readonly @string mutexHeapˢᵇ = "MutexHeap"u8;
internal static ref readonly @string mutexHeapFunc11SyncMutexˢ => ref mutexHeapFunc11SyncMutexˢᵇ;
private static readonly @string mutexHeapFunc11SyncMutexˢᵇ = @"MutexHeap\.func1.1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string condˢ => ref condˢᵇ;
private static readonly @string condˢᵇ = "Cond"u8;
internal static ref readonly @string condFunc1SyncCondWaitˢ => ref condFunc1SyncCondWaitˢᵇ;
private static readonly @string condFunc1SyncCondWaitˢᵇ = @"Cond\.func1\(.* \[sync\.Cond\.Wait\]"u8;
internal static ref readonly @string rwMutexRLockˢ => ref rwMutexRLockˢᵇ;
private static readonly @string rwMutexRLockˢᵇ = "RWMutexRLock"u8;
internal static ref readonly @string rwMutexRLockFunc1Syncˢ => ref rwMutexRLockFunc1Syncˢᵇ;
private static readonly @string rwMutexRLockFunc1Syncˢᵇ = @"RWMutexRLock\.func1\(.* \[sync\.RWMutex\.RLock\]"u8;
internal static ref readonly @string rwMutexLockˢ => ref rwMutexLockˢᵇ;
private static readonly @string rwMutexLockˢᵇ = "RWMutexLock"u8;
internal static ref readonly @string rwMutexLockFunc1SyncRwˢ => ref rwMutexLockFunc1SyncRwˢᵇ;
private static readonly @string rwMutexLockFunc1SyncRwˢᵇ = @"RWMutexLock\.func1\(.* \[sync\.(RW)?Mutex\.Lock\]"u8;
internal static ref readonly @string mixedˢ => ref mixedˢᵇ;
private static readonly @string mixedˢᵇ = "Mixed"u8;
internal static ref readonly @string mixedFunc1SyncWaitGroupˢ => ref mixedFunc1SyncWaitGroupˢᵇ;
private static readonly @string mixedFunc1SyncWaitGroupˢᵇ = @"Mixed\.func1\(.* \[sync\.WaitGroup\.Wait\]"u8;
internal static ref readonly @string mixedFunc11ChanSendˢ => ref mixedFunc11ChanSendˢᵇ;
private static readonly @string mixedFunc11ChanSendˢᵇ = @"Mixed\.func1.1\(.* \[chan send\]"u8;
internal static ref readonly @string noLeakGlobalˢ => ref noLeakGlobalˢᵇ;
private static readonly @string noLeakGlobalˢᵇ = "NoLeakGlobal"u8;
internal static ref readonly @string spawnGCˢ => ref spawnGCˢᵇ;
private static readonly @string spawnGCˢᵇ = "SpawnGC"u8;
internal static ref readonly @string spawnGCFunc1ChanReceiveˢ => ref spawnGCFunc1ChanReceiveˢᵇ;
private static readonly @string spawnGCFunc1ChanReceiveˢᵇ = @"spawnGC.func1\(.* \[chan receive\]"u8;
internal static ref readonly @string daisyChainˢ => ref daisyChainˢᵇ;
private static readonly @string daisyChainˢᵇ = "DaisyChain"u8;
internal static ref readonly @string noCloseRangeˢ => ref noCloseRangeˢᵇ;
private static readonly @string noCloseRangeˢᵇ = "NoCloseRange"u8;
internal static ref readonly @string noCloseRangeChanSendˢ => ref noCloseRangeChanSendˢᵇ;
private static readonly @string noCloseRangeChanSendˢᵇ = @"noCloseRange\(.* \[chan send\]"u8;
internal static ref readonly @string noCloseRangeFunc1Chanˢ => ref noCloseRangeFunc1Chanˢᵇ;
private static readonly @string noCloseRangeFunc1Chanˢᵇ = @"noCloseRange\.func1\(.* \[chan receive\]"u8;
internal static ref readonly @string methodContractViolationˢ => ref methodContractViolationˢᵇ;
private static readonly @string methodContractViolationˢᵇ = "MethodContractViolation"u8;
internal static ref readonly @string workerStartFunc1Selectˢ => ref workerStartFunc1Selectˢᵇ;
private static readonly @string workerStartFunc1Selectˢᵇ = @"worker\.Start\.func1\(.* \[select\]"u8;
internal static ref readonly @string doubleSendˢ => ref doubleSendˢᵇ;
private static readonly @string doubleSendˢᵇ = "DoubleSend"u8;
internal static ref readonly @string doubleSendFunc3ChanSendˢ => ref doubleSendFunc3ChanSendˢᵇ;
private static readonly @string doubleSendFunc3ChanSendˢᵇ = @"DoubleSend\.func3\(.* \[chan send\]"u8;
internal static ref readonly @string earlyReturnˢ => ref earlyReturnˢᵇ;
private static readonly @string earlyReturnˢᵇ = "EarlyReturn"u8;
internal static ref readonly @string earlyReturnFunc1ChanSendˢ => ref earlyReturnFunc1ChanSendˢᵇ;
private static readonly @string earlyReturnFunc1ChanSendˢᵇ = @"earlyReturn\.func1\(.* \[chan send\]"u8;
internal static ref readonly @string nCastLeakˢ => ref nCastLeakˢᵇ;
private static readonly @string nCastLeakˢᵇ = "NCastLeak"u8;
internal static ref readonly @string nCastLeakFunc1ChanSendˢ => ref nCastLeakFunc1ChanSendˢᵇ;
private static readonly @string nCastLeakFunc1ChanSendˢᵇ = @"nCastLeak\.func1\(.* \[chan send\]"u8;
internal static ref readonly @string nCastLeakFunc2Chanˢ => ref nCastLeakFunc2Chanˢᵇ;
private static readonly @string nCastLeakFunc2Chanˢᵇ = @"NCastLeak\.func2\(.* \[chan receive\]"u8;
internal static ref readonly @string timeoutˢ => ref timeoutˢᵇ;
private static readonly @string timeoutˢᵇ = "Timeout"u8;
internal static ref readonly @string timeoutFunc1ChanSendˢ => ref timeoutFunc1ChanSendˢᵇ;
private static readonly @string timeoutFunc1ChanSendˢᵇ = @"timeout\.func1\(.* \[chan send\]"u8;
internal static ref readonly @string cockroach584ˢ => ref cockroach584ˢᵇ;
private static readonly @string cockroach584ˢᵇ = "Cockroach584"u8;
internal static ref readonly @string cockroach584Func2Syncˢ => ref cockroach584Func2Syncˢᵇ;
private static readonly @string cockroach584Func2Syncˢᵇ = @"Cockroach584\.func2\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string cockroach1055ˢ => ref cockroach1055ˢᵇ;
private static readonly @string cockroach1055ˢᵇ = "Cockroach1055"u8;
internal static ref readonly @string cockroach1055Func2Chanˢ => ref cockroach1055Func2Chanˢᵇ;
private static readonly @string cockroach1055Func2Chanˢᵇ = @"Cockroach1055\.func2\(.* \[chan receive\]"u8;
internal static ref readonly @string cockroach1055Func22Syncˢ => ref cockroach1055Func22Syncˢᵇ;
private static readonly @string cockroach1055Func22Syncˢᵇ = @"Cockroach1055\.func2\.2\(.* \[sync\.WaitGroup\.Wait\]"u8;
internal static ref readonly @string cockroach1055Func21Chanˢ => ref cockroach1055Func21Chanˢᵇ;
private static readonly @string cockroach1055Func21Chanˢᵇ = @"Cockroach1055\.func2\.1\(.* \[chan receive\]"u8;
internal static ref readonly @string cockroach1055Func21Syncˢ => ref cockroach1055Func21Syncˢᵇ;
private static readonly @string cockroach1055Func21Syncˢᵇ = @"Cockroach1055\.func2\.1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string cockroach1462ˢ => ref cockroach1462ˢᵇ;
private static readonly @string cockroach1462ˢᵇ = "Cockroach1462"u8;
internal static ref readonly @string stopperCockroach1462ˢ => ref stopperCockroach1462ˢᵇ;
private static readonly @string stopperCockroach1462ˢᵇ = @"\(\*Stopper_cockroach1462\)\.RunWorker\.func1\(.* \[chan send\]"u8;
internal static ref readonly @string cockroach1462Func2Syncˢ => ref cockroach1462Func2Syncˢᵇ;
private static readonly @string cockroach1462Func2Syncˢᵇ = @"Cockroach1462\.func2\(.* \[sync\.WaitGroup\.Wait\]"u8;
internal static ref readonly @string cockroach2448ˢ => ref cockroach2448ˢᵇ;
private static readonly @string cockroach2448ˢᵇ = "Cockroach2448"u8;
internal static ref readonly @string storeCockroach2448ˢ => ref storeCockroach2448ˢᵇ;
private static readonly @string storeCockroach2448ˢᵇ = @"\(\*Store_cockroach2448\)\.processRaft\(.* \[select\]"u8;
internal static ref readonly @string stateCockroach2448Startˢ => ref stateCockroach2448Startˢᵇ;
private static readonly @string stateCockroach2448Startˢᵇ = @"\(\*state_cockroach2448\)\.start\(.* \[select\]"u8;
internal static ref readonly @string cockroach3710ˢ => ref cockroach3710ˢᵇ;
private static readonly @string cockroach3710ˢᵇ = "Cockroach3710"u8;
internal static ref readonly @string storeCockroach3710ˢ => ref storeCockroach3710ˢᵇ;
private static readonly @string storeCockroach3710ˢᵇ = @"\(\*Store_cockroach3710\)\.ForceRaftLogScanAndProcess\(.* \[sync\.RWMutex\.RLock\]"u8;
internal static ref readonly @string storeCockroach3710ˢ2 => ref storeCockroach3710ˢ2ᵇ;
private static readonly @string storeCockroach3710ˢ2ᵇ = @"\(\*Store_cockroach3710\)\.processRaft\.func1\(.* \[sync\.RWMutex\.Lock\]"u8;
internal static ref readonly @string cockroach6181ˢ => ref cockroach6181ˢᵇ;
private static readonly @string cockroach6181ˢᵇ = "Cockroach6181"u8;
internal static ref readonly @string cockroach7504ˢ => ref cockroach7504ˢᵇ;
private static readonly @string cockroach7504ˢᵇ = "Cockroach7504"u8;
internal static ref readonly @string cockroach7504Func21Syncˢ => ref cockroach7504Func21Syncˢᵇ;
private static readonly @string cockroach7504Func21Syncˢᵇ = @"Cockroach7504\.func2\.1.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string cockroach7504Func22Syncˢ => ref cockroach7504Func22Syncˢᵇ;
private static readonly @string cockroach7504Func22Syncˢᵇ = @"Cockroach7504\.func2\.2.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string cockroach9935ˢ => ref cockroach9935ˢᵇ;
private static readonly @string cockroach9935ˢᵇ = "Cockroach9935"u8;
internal static ref readonly @string loggingTCockroach9935ˢ => ref loggingTCockroach9935ˢᵇ;
private static readonly @string loggingTCockroach9935ˢᵇ = @"\(\*loggingT_cockroach9935\)\.outputLogEntry\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string cockroach10214ˢ => ref cockroach10214ˢᵇ;
private static readonly @string cockroach10214ˢᵇ = "Cockroach10214"u8;
internal static ref readonly @string storeCockroach10214ˢ => ref storeCockroach10214ˢᵇ;
private static readonly @string storeCockroach10214ˢᵇ = @"\(*Store_cockroach10214\)\.sendQueuedHeartbeats\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string replicaCockroach10214ˢ => ref replicaCockroach10214ˢᵇ;
private static readonly @string replicaCockroach10214ˢᵇ = @"\(*Replica_cockroach10214\)\.tick\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string cockroach10790ˢ => ref cockroach10790ˢᵇ;
private static readonly @string cockroach10790ˢᵇ = "Cockroach10790"u8;
internal static ref readonly @string replicaCockroach10790ˢ => ref replicaCockroach10790ˢᵇ;
private static readonly @string replicaCockroach10790ˢᵇ = @"\(\*Replica_cockroach10790\)\.beginCmds\.func1\(.* \[chan receive\]"u8;
internal static ref readonly @string cockroach13197ˢ => ref cockroach13197ˢᵇ;
private static readonly @string cockroach13197ˢᵇ = "Cockroach13197"u8;
internal static ref readonly @string txCockroach13197ˢ => ref txCockroach13197ˢᵇ;
private static readonly @string txCockroach13197ˢᵇ = @"\(\*Tx_cockroach13197\)\.awaitDone\(.* \[chan receive\]"u8;
internal static ref readonly @string cockroach13755ˢ => ref cockroach13755ˢᵇ;
private static readonly @string cockroach13755ˢᵇ = "Cockroach13755"u8;
internal static ref readonly @string rowsCockroach13755ˢ => ref rowsCockroach13755ˢᵇ;
private static readonly @string rowsCockroach13755ˢᵇ = @"\(\*Rows_cockroach13755\)\.awaitDone\(.* \[chan receive\]"u8;
internal static ref readonly @string cockroach16167ˢ => ref cockroach16167ˢᵇ;
private static readonly @string cockroach16167ˢᵇ = "Cockroach16167"u8;
internal static ref readonly @string cockroach16167Func2Syncˢ => ref cockroach16167Func2Syncˢᵇ;
private static readonly @string cockroach16167Func2Syncˢᵇ = @"Cockroach16167\.func2\(.* \[sync\.RWMutex\.RLock\]"u8;
internal static ref readonly @string executorCockroach16167ˢ => ref executorCockroach16167ˢᵇ;
private static readonly @string executorCockroach16167ˢᵇ = @"\(\*Executor_cockroach16167\)\.Start\(.* \[sync\.RWMutex\.Lock\]"u8;
internal static ref readonly @string cockroach18101ˢ => ref cockroach18101ˢᵇ;
private static readonly @string cockroach18101ˢᵇ = "Cockroach18101"u8;
internal static ref readonly @string restoreCockroach18101ˢ => ref restoreCockroach18101ˢᵇ;
private static readonly @string restoreCockroach18101ˢᵇ = @"restore_cockroach18101\.func1\(.* \[chan send\]"u8;
internal static ref readonly @string cockroach24808ˢ => ref cockroach24808ˢᵇ;
private static readonly @string cockroach24808ˢᵇ = "Cockroach24808"u8;
internal static ref readonly @string cockroach24808Func2Chanˢ => ref cockroach24808Func2Chanˢᵇ;
private static readonly @string cockroach24808Func2Chanˢᵇ = @"Cockroach24808\.func2\(.* \[chan send\]"u8;
internal static ref readonly @string cockroach25456ˢ => ref cockroach25456ˢᵇ;
private static readonly @string cockroach25456ˢᵇ = "Cockroach25456"u8;
internal static ref readonly @string cockroach25456Func2Chanˢ => ref cockroach25456Func2Chanˢᵇ;
private static readonly @string cockroach25456Func2Chanˢᵇ = @"Cockroach25456\.func2\(.* \[chan receive\]"u8;
internal static ref readonly @string cockroach35073ˢ => ref cockroach35073ˢᵇ;
private static readonly @string cockroach35073ˢᵇ = "Cockroach35073"u8;
internal static ref readonly @string cockroach35073Func21Chanˢ => ref cockroach35073Func21Chanˢᵇ;
private static readonly @string cockroach35073Func21Chanˢᵇ = @"Cockroach35073\.func2.1\(.* \[chan send\]"u8;
internal static ref readonly @string cockroach35073Func2Chanˢ => ref cockroach35073Func2Chanˢᵇ;
private static readonly @string cockroach35073Func2Chanˢᵇ = @"Cockroach35073\.func2\(.* \[chan send\]"u8;
internal static ref readonly @string cockroach35931ˢ => ref cockroach35931ˢᵇ;
private static readonly @string cockroach35931ˢᵇ = "Cockroach35931"u8;
internal static ref readonly @string cockroach35931Func2Chanˢ => ref cockroach35931Func2Chanˢᵇ;
private static readonly @string cockroach35931Func2Chanˢᵇ = @"Cockroach35931\.func2\(.* \[chan send\]"u8;
internal static ref readonly @string etcd5509ˢ => ref etcd5509ˢᵇ;
private static readonly @string etcd5509ˢᵇ = "Etcd5509"u8;
internal static ref readonly @string etcd5509Func2SyncRWMutexˢ => ref etcd5509Func2SyncRWMutexˢᵇ;
private static readonly @string etcd5509Func2SyncRWMutexˢᵇ = @"Etcd5509\.func2\(.* \[sync\.RWMutex\.Lock\]"u8;
internal static ref readonly @string etcd6708ˢ => ref etcd6708ˢᵇ;
private static readonly @string etcd6708ˢᵇ = "Etcd6708"u8;
internal static ref readonly @string etcd6708Func2SyncRWMutexˢ => ref etcd6708Func2SyncRWMutexˢᵇ;
private static readonly @string etcd6708Func2SyncRWMutexˢᵇ = @"Etcd6708\.func2\(.* \[sync\.RWMutex\.RLock\]"u8;
internal static ref readonly @string etcd6857ˢ => ref etcd6857ˢᵇ;
private static readonly @string etcd6857ˢᵇ = "Etcd6857"u8;
internal static ref readonly @string nodeEtcd6857StatusChanˢ => ref nodeEtcd6857StatusChanˢᵇ;
private static readonly @string nodeEtcd6857StatusChanˢᵇ = @"\(\*node_etcd6857\)\.Status\(.* \[chan send\]"u8;
internal static ref readonly @string etcd6873ˢ => ref etcd6873ˢᵇ;
private static readonly @string etcd6873ˢᵇ = "Etcd6873"u8;
internal static ref readonly @string watchBroadcastsEtcd6873ˢ => ref watchBroadcastsEtcd6873ˢᵇ;
private static readonly @string watchBroadcastsEtcd6873ˢᵇ = @"\(\*watchBroadcasts_etcd6873\)\.stop\(.* \[chan receive\]"u8;
internal static ref readonly @string newWatchBroadcastsˢ => ref newWatchBroadcastsˢᵇ;
private static readonly @string newWatchBroadcastsˢᵇ = @"newWatchBroadcasts_etcd6873\.func1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string etcd7492ˢ => ref etcd7492ˢᵇ;
private static readonly @string etcd7492ˢᵇ = "Etcd7492"u8;
internal static ref readonly @string etcd7492Func2Syncˢ => ref etcd7492Func2Syncˢᵇ;
private static readonly @string etcd7492Func2Syncˢᵇ = @"Etcd7492\.func2\(.* \[sync\.WaitGroup\.Wait\]"u8;
internal static ref readonly @string etcd7492Func21ChanSendˢ => ref etcd7492Func21ChanSendˢᵇ;
private static readonly @string etcd7492Func21ChanSendˢᵇ = @"Etcd7492\.func2\.1\(.* \[chan send\]"u8;
internal static ref readonly @string simpleTokenTTLKeeperˢ => ref simpleTokenTTLKeeperˢᵇ;
private static readonly @string simpleTokenTTLKeeperˢᵇ = @"\(\*simpleTokenTTLKeeper_etcd7492\)\.run\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string etcd7902ˢ => ref etcd7902ˢᵇ;
private static readonly @string etcd7902ˢᵇ = "Etcd7902"u8;
internal static ref readonly @string doRoundsEtcd7902Func1ˢ => ref doRoundsEtcd7902Func1ˢᵇ;
private static readonly @string doRoundsEtcd7902Func1ˢᵇ = @"doRounds_etcd7902\.func1\(.* \[chan receive\]"u8;
internal static ref readonly @string doRoundsEtcd7902Func1ˢ2 => ref doRoundsEtcd7902Func1ˢ2ᵇ;
private static readonly @string doRoundsEtcd7902Func1ˢ2ᵇ = @"doRounds_etcd7902\.func1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string runElectionFuncEtcd7902ˢ => ref runElectionFuncEtcd7902ˢᵇ;
private static readonly @string runElectionFuncEtcd7902ˢᵇ = @"runElectionFunc_etcd7902\(.* \[sync\.WaitGroup\.Wait\]"u8;
internal static ref readonly @string etcd10492ˢ => ref etcd10492ˢᵇ;
private static readonly @string etcd10492ˢᵇ = "Etcd10492"u8;
internal static ref readonly @string etcd10492Func2SyncMutexˢ => ref etcd10492Func2SyncMutexˢᵇ;
private static readonly @string etcd10492Func2SyncMutexˢᵇ = @"Etcd10492\.func2\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string grpc660ˢ => ref grpc660ˢᵇ;
private static readonly @string grpc660ˢᵇ = "Grpc660"u8;
internal static ref readonly @string benchmarkClientGrpc660ˢ => ref benchmarkClientGrpc660ˢᵇ;
private static readonly @string benchmarkClientGrpc660ˢᵇ = @"\(\*benchmarkClient_grpc660\)\.doCloseLoopUnary\.func1\(.* \[chan send\]"u8;
internal static ref readonly @string grpc795ˢ => ref grpc795ˢᵇ;
private static readonly @string grpc795ˢᵇ = "Grpc795"u8;
internal static ref readonly @string serverGrpc795ServeSyncˢ => ref serverGrpc795ServeSyncˢᵇ;
private static readonly @string serverGrpc795ServeSyncˢᵇ = @"\(\*Server_grpc795\)\.Serve\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string grpc862ˢ => ref grpc862ˢᵇ;
private static readonly @string grpc862ˢᵇ = "Grpc862"u8;
internal static ref readonly @string dialContextGrpc862Func2ˢ => ref dialContextGrpc862Func2ˢᵇ;
private static readonly @string dialContextGrpc862Func2ˢᵇ = @"DialContext_grpc862\.func2\(.* \[chan receive\]"u8;
internal static ref readonly @string grpc1275ˢ => ref grpc1275ˢᵇ;
private static readonly @string grpc1275ˢᵇ = "Grpc1275"u8;
internal static ref readonly @string grpc1424ˢ => ref grpc1424ˢᵇ;
private static readonly @string grpc1424ˢᵇ = "Grpc1424"u8;
internal static ref readonly @string dialContextGrpc1424Func1ˢ => ref dialContextGrpc1424Func1ˢᵇ;
private static readonly @string dialContextGrpc1424Func1ˢᵇ = @"DialContext_grpc1424\.func1\(.* \[chan receive\]"u8;
internal static ref readonly @string grpc1460ˢ => ref grpc1460ˢᵇ;
private static readonly @string grpc1460ˢᵇ = "Grpc1460"u8;
internal static ref readonly @string http2ClientGrpc1460ˢ => ref http2ClientGrpc1460ˢᵇ;
private static readonly @string http2ClientGrpc1460ˢᵇ = @"\(\*http2Client_grpc1460\)\.keepalive\(.* \[chan receive\]"u8;
internal static ref readonly @string http2ClientGrpc1460ˢ2 => ref http2ClientGrpc1460ˢ2ᵇ;
private static readonly @string http2ClientGrpc1460ˢ2ᵇ = @"\(\*http2Client_grpc1460\)\.NewStream\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string grpc3017ˢ => ref grpc3017ˢᵇ;
private static readonly @string grpc3017ˢᵇ = "Grpc3017"u8;
internal static ref readonly @string grpc3017Func2ChanReceiveˢ => ref grpc3017Func2ChanReceiveˢᵇ;
private static readonly @string grpc3017Func2ChanReceiveˢᵇ = @"Grpc3017\.func2\(.* \[chan receive\]"u8;
internal static ref readonly @string grpc3017Func21SyncMutexˢ => ref grpc3017Func21SyncMutexˢᵇ;
private static readonly @string grpc3017Func21SyncMutexˢᵇ = @"Grpc3017\.func2\.1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string lbCacheClientConnˢ => ref lbCacheClientConnˢᵇ;
private static readonly @string lbCacheClientConnˢᵇ = @"\(\*lbCacheClientConn_grpc3017\)\.RemoveSubConn\.func1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string hugo3251ˢ => ref hugo3251ˢᵇ;
private static readonly @string hugo3251ˢᵇ = "Hugo3251"u8;
internal static ref readonly @string hugo3251Func2Syncˢ => ref hugo3251Func2Syncˢᵇ;
private static readonly @string hugo3251Func2Syncˢᵇ = @"Hugo3251\.func2\(.* \[sync\.WaitGroup\.Wait\]"u8;
internal static ref readonly @string hugo3251Func21SyncMutexˢ => ref hugo3251Func21SyncMutexˢᵇ;
private static readonly @string hugo3251Func21SyncMutexˢᵇ = @"Hugo3251\.func2\.1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string hugo3251Func21Syncˢ => ref hugo3251Func21Syncˢᵇ;
private static readonly @string hugo3251Func21Syncˢᵇ = @"Hugo3251\.func2\.1\(.* \[sync\.RWMutex\.RLock\]"u8;
internal static ref readonly @string hugo5379ˢ => ref hugo5379ˢᵇ;
private static readonly @string hugo5379ˢᵇ = "Hugo5379"u8;
internal static ref readonly @string pageHugo5379InitContentˢ => ref pageHugo5379InitContentˢᵇ;
private static readonly @string pageHugo5379InitContentˢᵇ = @"\(\*Page_hugo5379\)\.initContent\.func1\.1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string pageRendererHugo5379Syncˢ => ref pageRendererHugo5379Syncˢᵇ;
private static readonly @string pageRendererHugo5379Syncˢᵇ = @"pageRenderer_hugo5379\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string hugo5379Func2Syncˢ => ref hugo5379Func2Syncˢᵇ;
private static readonly @string hugo5379Func2Syncˢᵇ = @"Hugo5379\.func2\(.* \[sync\.WaitGroup\.Wait\]"u8;
internal static ref readonly @string istio16224ˢ => ref istio16224ˢᵇ;
private static readonly @string istio16224ˢᵇ = "Istio16224"u8;
internal static ref readonly @string istio16224Func2SyncMutexˢ => ref istio16224Func2SyncMutexˢᵇ;
private static readonly @string istio16224Func2SyncMutexˢᵇ = @"Istio16224\.func2\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string controllerIstio16224Runˢ => ref controllerIstio16224Runˢᵇ;
private static readonly @string controllerIstio16224Runˢᵇ = @"\(\*controller_istio16224\)\.Run\(.* \[chan send\]"u8;
internal static ref readonly @string controllerIstio16224Runˢ2 => ref controllerIstio16224Runˢ2ᵇ;
private static readonly @string controllerIstio16224Runˢ2ᵇ = @"\(\*controller_istio16224\)\.Run\(.* \[chan receive\]"u8;
internal static ref readonly @string istio17860ˢ => ref istio17860ˢᵇ;
private static readonly @string istio17860ˢᵇ = "Istio17860"u8;
internal static ref readonly @string agentIstio17860RunWaitˢ => ref agentIstio17860RunWaitˢᵇ;
private static readonly @string agentIstio17860RunWaitˢᵇ = @"\(\*agent_istio17860\)\.runWait\(.* \[chan send\]"u8;
internal static ref readonly @string istio18454ˢ => ref istio18454ˢᵇ;
private static readonly @string istio18454ˢᵇ = "Istio18454"u8;
internal static ref readonly @string workerIstio18454Startˢ => ref workerIstio18454Startˢᵇ;
private static readonly @string workerIstio18454Startˢᵇ = @"\(\*Worker_istio18454\)\.Start\.func1\(.* \[chan receive\]"u8;
internal static ref readonly @string workerIstio18454Startˢ2 => ref workerIstio18454Startˢ2ᵇ;
private static readonly @string workerIstio18454Startˢ2ᵇ = @"\(\*Worker_istio18454\)\.Start\.func1\(.* \[chan send\]"u8;
internal static ref readonly @string kubernetes5316ˢ => ref kubernetes5316ˢᵇ;
private static readonly @string kubernetes5316ˢᵇ = "Kubernetes5316"u8;
internal static ref readonly @string finishRequestˢ => ref finishRequestˢᵇ;
private static readonly @string finishRequestˢᵇ = @"finishRequest_kubernetes5316\.func1\(.* \[chan send\]"u8;
internal static ref readonly @string kubernetes6632ˢ => ref kubernetes6632ˢᵇ;
private static readonly @string kubernetes6632ˢᵇ = "Kubernetes6632"u8;
internal static ref readonly @string idleAwareFramerˢ => ref idleAwareFramerˢᵇ;
private static readonly @string idleAwareFramerˢᵇ = @"\(\*idleAwareFramer_kubernetes6632\)\.monitor\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string idleAwareFramerˢ2 => ref idleAwareFramerˢ2ᵇ;
private static readonly @string idleAwareFramerˢ2ᵇ = @"\(\*idleAwareFramer_kubernetes6632\)\.WriteFrame\(.* \[chan send\]"u8;
internal static ref readonly @string kubernetes10182ˢ => ref kubernetes10182ˢᵇ;
private static readonly @string kubernetes10182ˢᵇ = "Kubernetes10182"u8;
internal static ref readonly @string statusManagerˢ => ref statusManagerˢᵇ;
private static readonly @string statusManagerˢᵇ = @"\(\*statusManager_kubernetes10182\)\.Start\.func1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string statusManagerˢ2 => ref statusManagerˢ2ᵇ;
private static readonly @string statusManagerˢ2ᵇ = @"\(\*statusManager_kubernetes10182\)\.SetPodStatus\(.* \[chan send\]"u8;
internal static ref readonly @string kubernetes11298ˢ => ref kubernetes11298ˢᵇ;
private static readonly @string kubernetes11298ˢᵇ = "Kubernetes11298"u8;
internal static ref readonly @string afterKubernetes11298ˢ => ref afterKubernetes11298ˢᵇ;
private static readonly @string afterKubernetes11298ˢᵇ = @"After_kubernetes11298\.func1\(.* \[chan receive\]"u8;
internal static ref readonly @string afterKubernetes11298ˢ2 => ref afterKubernetes11298ˢ2ᵇ;
private static readonly @string afterKubernetes11298ˢ2ᵇ = @"After_kubernetes11298\.func1\(.* \[sync\.Cond\.Wait\]"u8;
internal static ref readonly @string kubernetes11298Func2Chanˢ => ref kubernetes11298Func2Chanˢᵇ;
private static readonly @string kubernetes11298Func2Chanˢᵇ = @"Kubernetes11298\.func2\(.* \[chan receive\]"u8;
internal static ref readonly @string kubernetes13135ˢ => ref kubernetes13135ˢᵇ;
private static readonly @string kubernetes13135ˢᵇ = "Kubernetes13135"u8;
internal static ref readonly @string utilKubernetes13135Syncˢ => ref utilKubernetes13135Syncˢᵇ;
private static readonly @string utilKubernetes13135Syncˢᵇ = @"Util_kubernetes13135\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string watchCacheˢ => ref watchCacheˢᵇ;
private static readonly @string watchCacheˢᵇ = @"\(\*WatchCache_kubernetes13135\)\.Add\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string kubernetes25331ˢ => ref kubernetes25331ˢᵇ;
private static readonly @string kubernetes25331ˢᵇ = "Kubernetes25331"u8;
internal static ref readonly @string watchChanKubernetes25331ˢ => ref watchChanKubernetes25331ˢᵇ;
private static readonly @string watchChanKubernetes25331ˢᵇ = @"\(\*watchChan_kubernetes25331\)\.run\(.* \[chan send\]"u8;
internal static ref readonly @string kubernetes26980ˢ => ref kubernetes26980ˢᵇ;
private static readonly @string kubernetes26980ˢᵇ = "Kubernetes26980"u8;
internal static ref readonly @string kubernetes26980Func2Chanˢ => ref kubernetes26980Func2Chanˢᵇ;
private static readonly @string kubernetes26980Func2Chanˢᵇ = @"Kubernetes26980\.func2\(.* \[chan receive\]"u8;
internal static ref readonly @string kubernetes26980Func21ˢ => ref kubernetes26980Func21ˢᵇ;
private static readonly @string kubernetes26980Func21ˢᵇ = @"Kubernetes26980\.func2\.1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string processorListenerˢ => ref processorListenerˢᵇ;
private static readonly @string processorListenerˢᵇ = @"\(\*processorListener_kubernetes26980\)\.pop\(.* \[chan receive\]"u8;
internal static ref readonly @string kubernetes30872ˢ => ref kubernetes30872ˢᵇ;
private static readonly @string kubernetes30872ˢᵇ = "Kubernetes30872"u8;
internal static ref readonly @string delayingDelivererˢ => ref delayingDelivererˢᵇ;
private static readonly @string delayingDelivererˢᵇ = @"\(\*DelayingDeliverer_kubernetes30872\)\.StartWithHandler\.func1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string controllerˢ => ref controllerˢᵇ;
private static readonly @string controllerˢᵇ = @"\(\*Controller_kubernetes30872\)\.Run\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string namespaceControllerˢ => ref namespaceControllerˢᵇ;
private static readonly @string namespaceControllerˢᵇ = @"\(\*NamespaceController_kubernetes30872\)\.Run\.func1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string kubernetes38669ˢ => ref kubernetes38669ˢᵇ;
private static readonly @string kubernetes38669ˢᵇ = "Kubernetes38669"u8;
internal static ref readonly @string cacheWatcherˢ => ref cacheWatcherˢᵇ;
private static readonly @string cacheWatcherˢᵇ = @"\(\*cacheWatcher_kubernetes38669\)\.process\(.* \[chan send\]"u8;
internal static ref readonly @string kubernetes58107ˢ => ref kubernetes58107ˢᵇ;
private static readonly @string kubernetes58107ˢᵇ = "Kubernetes58107"u8;
internal static ref readonly @string resourceQuotaControllerˢ => ref resourceQuotaControllerˢᵇ;
private static readonly @string resourceQuotaControllerˢᵇ = @"\(\*ResourceQuotaController_kubernetes58107\)\.worker\(.* \[sync\.Cond\.Wait\]"u8;
internal static ref readonly @string resourceQuotaControllerˢ2 => ref resourceQuotaControllerˢ2ᵇ;
private static readonly @string resourceQuotaControllerˢ2ᵇ = @"\(\*ResourceQuotaController_kubernetes58107\)\.worker\(.* \[sync\.RWMutex\.RLock\]"u8;
internal static ref readonly @string resourceQuotaControllerˢ3 => ref resourceQuotaControllerˢ3ᵇ;
private static readonly @string resourceQuotaControllerˢ3ᵇ = @"\(\*ResourceQuotaController_kubernetes58107\)\.Sync\(.* \[sync\.RWMutex\.Lock\]"u8;
internal static ref readonly @string kubernetes62464ˢ => ref kubernetes62464ˢᵇ;
private static readonly @string kubernetes62464ˢᵇ = "Kubernetes62464"u8;
internal static ref readonly @string managerKubernetes62464ˢ => ref managerKubernetes62464ˢᵇ;
private static readonly @string managerKubernetes62464ˢᵇ = @"\(\*manager_kubernetes62464\)\.reconcileState\(.* \[sync\.RWMutex\.RLock\]"u8;
internal static ref readonly @string staticPolicyˢ => ref staticPolicyˢᵇ;
private static readonly @string staticPolicyˢᵇ = @"\(\*staticPolicy_kubernetes62464\)\.RemoveContainer\(.* \[sync\.(RW)?Mutex\.Lock\]"u8;
internal static ref readonly @string kubernetes70277ˢ => ref kubernetes70277ˢᵇ;
private static readonly @string kubernetes70277ˢᵇ = "Kubernetes70277"u8;
internal static ref readonly @string kubernetes70277Func2Chanˢ => ref kubernetes70277Func2Chanˢᵇ;
private static readonly @string kubernetes70277Func2Chanˢᵇ = @"Kubernetes70277\.func2\(.* \[chan receive\]"u8;
internal static ref readonly @string moby4951ˢ => ref moby4951ˢᵇ;
private static readonly @string moby4951ˢᵇ = "Moby4951"u8;
internal static ref readonly @string deviceSetMoby4951ˢ => ref deviceSetMoby4951ˢᵇ;
private static readonly @string deviceSetMoby4951ˢᵇ = @"\(\*DeviceSet_moby4951\)\.DeleteDevice\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string moby7559ˢ => ref moby7559ˢᵇ;
private static readonly @string moby7559ˢᵇ = "Moby7559"u8;
internal static ref readonly @string udpProxyMoby7559RunSyncˢ => ref udpProxyMoby7559RunSyncˢᵇ;
private static readonly @string udpProxyMoby7559RunSyncˢᵇ = @"\(\*UDPProxy_moby7559\)\.Run\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string moby17176ˢ => ref moby17176ˢᵇ;
private static readonly @string moby17176ˢᵇ = "Moby17176"u8;
internal static ref readonly @string moby21233ˢ => ref moby21233ˢᵇ;
private static readonly @string moby21233ˢᵇ = "Moby21233"u8;
internal static ref readonly @string transferMoby21233Watchˢ => ref transferMoby21233Watchˢᵇ;
private static readonly @string transferMoby21233Watchˢᵇ = @"\(\*Transfer_moby21233\)\.Watch\.func1\(.* \[chan send\]"u8;
internal static ref readonly @string transferMoby21233Watchˢ2 => ref transferMoby21233Watchˢ2ᵇ;
private static readonly @string transferMoby21233Watchˢ2ᵇ = @"\(\*Transfer_moby21233\)\.Watch\.func1\(.* \[select\]"u8;
internal static ref readonly @string testTransferMoby21233ˢ => ref testTransferMoby21233ˢᵇ;
private static readonly @string testTransferMoby21233ˢᵇ = @"testTransfer_moby21233\(.* \[chan receive\]"u8;
internal static ref readonly @string moby25348ˢ => ref moby25348ˢᵇ;
private static readonly @string moby25348ˢᵇ = "Moby25348"u8;
internal static ref readonly @string managerMoby25348InitSyncˢ => ref managerMoby25348InitSyncˢᵇ;
private static readonly @string managerMoby25348InitSyncˢᵇ = @"\(\*Manager_moby25348\)\.init\(.* \[sync\.WaitGroup\.Wait\]"u8;
internal static ref readonly @string moby27782ˢ => ref moby27782ˢᵇ;
private static readonly @string moby27782ˢᵇ = "Moby27782"u8;
internal static ref readonly @string jsonFileLoggerMoby27782ˢ => ref jsonFileLoggerMoby27782ˢᵇ;
private static readonly @string jsonFileLoggerMoby27782ˢᵇ = @"\(\*JSONFileLogger_moby27782\)\.readLogs\(.* \[sync\.Cond\.Wait\]"u8;
internal static ref readonly @string watcherMoby27782ˢ => ref watcherMoby27782ˢᵇ;
private static readonly @string watcherMoby27782ˢᵇ = @"\(\*Watcher_moby27782\)\.readEvents\(.* \[select\]"u8;
internal static ref readonly @string moby28462ˢ => ref moby28462ˢᵇ;
private static readonly @string moby28462ˢᵇ = "Moby28462"u8;
internal static ref readonly @string monitorMoby28462Syncˢ => ref monitorMoby28462Syncˢᵇ;
private static readonly @string monitorMoby28462Syncˢᵇ = @"monitor_moby28462\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string daemonMoby28462ˢ => ref daemonMoby28462ˢᵇ;
private static readonly @string daemonMoby28462ˢᵇ = @"\(\*Daemon_moby28462\)\.StateChanged\(.* \[chan send\]"u8;
internal static ref readonly @string moby30408ˢ => ref moby30408ˢᵇ;
private static readonly @string moby30408ˢᵇ = "Moby30408"u8;
internal static ref readonly @string moby30408Func2Chanˢ => ref moby30408Func2Chanˢᵇ;
private static readonly @string moby30408Func2Chanˢᵇ = @"Moby30408\.func2\(.* \[chan receive\]"u8;
internal static ref readonly @string testActiveMoby30408Func1ˢ => ref testActiveMoby30408Func1ˢᵇ;
private static readonly @string testActiveMoby30408Func1ˢᵇ = @"testActive_moby30408\.func1\(.* \[sync\.Cond\.Wait\]"u8;
internal static ref readonly @string moby33781ˢ => ref moby33781ˢᵇ;
private static readonly @string moby33781ˢᵇ = "Moby33781"u8;
internal static ref readonly @string monitorMoby33781Func1ˢ => ref monitorMoby33781Func1ˢᵇ;
private static readonly @string monitorMoby33781Func1ˢᵇ = @"monitor_moby33781\.func1\(.* \[chan send\]"u8;
internal static ref readonly @string moby36114ˢ => ref moby36114ˢᵇ;
private static readonly @string moby36114ˢᵇ = "Moby36114"u8;
internal static ref readonly @string serviceVMMoby36114ˢ => ref serviceVMMoby36114ˢᵇ;
private static readonly @string serviceVMMoby36114ˢᵇ = @"\(\*serviceVM_moby36114\)\.hotAddVHDsAtStart\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string serving2137ˢ => ref serving2137ˢᵇ;
private static readonly @string serving2137ˢᵇ = "Serving2137"u8;
internal static ref readonly @string breakerServing2137ˢ => ref breakerServing2137ˢᵇ;
private static readonly @string breakerServing2137ˢᵇ = @"\(\*Breaker_serving2137\)\.concurrentRequest\.func1\(.* \[chan send\]"u8;
internal static ref readonly @string breakerServing2137ˢ2 => ref breakerServing2137ˢ2ᵇ;
private static readonly @string breakerServing2137ˢ2ᵇ = @"\(\*Breaker_serving2137\)\.concurrentRequest\.func1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static ref readonly @string serving2137Func2Chanˢ => ref serving2137Func2Chanˢᵇ;
private static readonly @string serving2137Func2Chanˢᵇ = @"Serving2137\.func2\(.* \[chan receive\]"u8;
internal static ref readonly @string syncthing4829ˢ => ref syncthing4829ˢᵇ;
private static readonly @string syncthing4829ˢᵇ = "Syncthing4829"u8;
internal static ref readonly @string syncthing4829Func2Syncˢ => ref syncthing4829Func2Syncˢᵇ;
private static readonly @string syncthing4829Func2Syncˢᵇ = @"Syncthing4829\.func2\(.* \[sync\.RWMutex\.RLock\]"u8;
internal static ref readonly @string syncthing5795ˢ => ref syncthing5795ˢᵇ;
private static readonly @string syncthing5795ˢᵇ = "Syncthing5795"u8;
internal static ref readonly @string rawConnectionˢ => ref rawConnectionˢᵇ;
private static readonly @string rawConnectionˢᵇ = @"\(\*rawConnection_syncthing5795\)\.dispatcherLoop\(.* \[chan receive\]"u8;
internal static ref readonly @string syncthing5795Func2Chanˢ => ref syncthing5795Func2Chanˢᵇ;
private static readonly @string syncthing5795Func2Chanˢᵇ = @"Syncthing5795\.func2.* \[chan receive\]"u8;
internal static ref readonly @string testgoroutineleakprofileˢ => ref testgoroutineleakprofileˢᵇ;
private static readonly @string testgoroutineleakprofileˢᵇ = "testgoroutineleakprofile"u8;
internal static ref readonly @string testgoroutineleakprofileˢ2 => ref testgoroutineleakprofileˢ2ᵇ;
private static readonly @string testgoroutineleakprofileˢ2ᵇ = "testgoroutineleakprofile/goker"u8;

// Goroutine leak test case.
//
// Test cases can be configured with test name, the name of the entry point function,
// a set of expected leaks identified by regular expressions, and the number of times
// the test should be repeated.
//
// Repeated runs reduce flakiness in some tests.
internal partial struct TestGoroutineLeakProfile_testCase /*dyn*/ {
    internal @string name;
    internal bool simple;
    internal nint repetitions;
    internal map<ж<Δregexp.Regexp>, bool> expectedLeaks;
    // flakyLeaks are goroutine leaks that are too flaky to be reliably detected.
    // Still, they might pop up every once in a while. The test will pass regardless
    // if they occur or nor, as they are not unexpected.
    //
    // Note that all flaky leaks are true positives, i.e. real goroutine leaks,
    // and it is only their detection that is unreliable due to scheduling
    // non-determinism.
    internal map<ж<Δregexp.Regexp>, EmptyStruct> flakyLeaks;
}

public static void TestGoroutineLeakProfile(ж<testing.T> t) {
    // Some tests have false negatives under mayMoreStackPreempt and mayMoreStackMove.
    // This may be a test-only issue in that they're just sensitive to scheduling, but it
    // needs more investigation.
    foreach (var (_, cfg) in range(slice<@string>(["mayMoreStackPreempt", "mayMoreStackMove"]))) {
        if (strings.Contains(Δos.Getenv(goflagsˢ), cfg)) {
            testenv.SkipFlaky(new runtime_test_package.testing_TжTB(t), 75729);
        }
    }

    // makeAnyTest is a short-hand for creating test cases.
    // Each of the leaks in the list is identified by a regular expression.
    // If a leak is flaky, it is added to the flakyLeaks map.
    TestGoroutineLeakProfile_testCase makeAnyTest(@string name, bool flaky, nint repetitions, params ꓸꓸꓸstring leaksʗp) {
        var leaks = leaksʗp.sslice();
        var tc = new TestGoroutineLeakProfile_testCase(
            name: name,
            expectedLeaks: new map<ж<Δregexp.Regexp>, bool>(len(leaks)),
            flakyLeaks: new map<ж<Δregexp.Regexp>, EmptyStruct>(len(leaks)),
            // Make sure the test is repeated at least once.
            repetitions: (repetitions | 1)
        );

        foreach (var (_, leak) in range(leaks)) {
            if (!flaky) {
                tc.expectedLeaks[Δregexp.MustCompile(leak)] = false;
            } else {
                tc.flakyLeaks[Δregexp.MustCompile(leak)] = new EmptyStruct();
            }
        }

        return tc;
    }

    // makeTest is a short-hand for creating non-flaky test cases.
    var makeAnyTestʗ1 = makeAnyTest;
    TestGoroutineLeakProfile_testCase makeTest(@string name, params ꓸꓸꓸstring leaksʗp) {
        var leaks = leaksʗp.sslice();
        var tcase = makeAnyTestʗ1(name, false, 2, leaks.ꓸꓸꓸ);
        tcase.simple = true;
        return tcase;
    }

    // makeFlakyTest is a short-hand for creating flaky test cases.
    var makeAnyTestʗ2 = makeAnyTest;
    TestGoroutineLeakProfile_testCase makeFlakyTest(@string name, params ꓸꓸꓸstring leaksʗp) {
        var leaks = leaksʗp.sslice();
        if (testing.Short()) {
            return makeAnyTestʗ2(name, true, 2, leaks.ꓸꓸꓸ);
        }
        return makeAnyTestʗ2(name, true, 10, leaks.ꓸꓸꓸ);
    }

    var goroutineHeader = Δregexp.MustCompile(goroutineDˢ);

    // extractLeaks takes the output of a test and splits it into a
    // list of strings denoting goroutine leaks.
    //
    // If the input is:
    //
    // goroutine 1 [wait reason (leaked)]:
    // main.leaked()
    // 	./testdata/testgoroutineleakprofile/foo.go:37 +0x100
    // created by main.main()
    // 	./testdata/testgoroutineleakprofile/main.go:10 +0x20
    //
    // goroutine 2 [wait reason (leaked)]:
    // main.leaked2()
    // 	./testdata/testgoroutineleakprofile/foo.go:37 +0x100
    // created by main.main()
    // 	./testdata/testgoroutineleakprofile/main.go:10 +0x20
    //
    // The output is (as a list of strings):
    //
    // leaked() [wait reason]
    // leaked2() [wait reason]
    var goroutineHeaderʗ1 = goroutineHeader;
    slice<@string> extractLeaks(@string output) {
        var leaksᴷ = new stackAppend<@string>(moves: true);
        var stacks = strings.Split(output, goroutineˢ);
        slice<@string> leaks = default;
        foreach (var (_, stack) in range(stacks)) {
            var lines = strings.Split(stack, "\n");
            if (len(lines) < 5) {
                // Expecting at least the following lines (where n=len(lines)-1):
                //
                // [0] goroutine n [wait reason (leaked)]
                // ...
                // [n-3] bottom.leak.frame(...)
                // [n-2]  ./bottom/leak/frame/source.go:line
                // [n-1] created by go.instruction()
                // [n] 	  ./go/instruction/source.go:line
                continue;
            }

            if (!strings.Contains(lines[0], leakedˢ)) {
                // Ignore non-leaked goroutines.
                continue;
            }

            // Get the wait reason from the goroutine header.
            @string header = lines[0];
            @string waitReason = goroutineHeaderʗ1.ReplaceAllString(header, "[");
            waitReason = strings.ReplaceAll(waitReason, leakedˢ2, ""u8);

            // Get the function name from the stack trace (should be two lines above `created by`).
            @string funcName = default;
            for (nint i = len(lines) - 1; i >= 0; i--) {
                if (strings.Contains(lines[i], createdByˢ)) {
                    funcName = strings.TrimPrefix(lines[i - 2], mainˢ2);
                    break;
                }
            }
            if (funcName == ""u8) {
                t.Fatalf("failed to extract function name from stack trace: %s", lines);
            }

            leaks = append(leaksᴷ, leaks, funcName + " "u8 + waitReason);
        }
        leaks = move2heap(leaksᴷ, leaks);
        return leaks;
    }

    // Micro tests involve very simple leaks for each type of concurrency primitive operation.
    var microTests = slice<TestGoroutineLeakProfile_testCase>([
        makeTest(nilRecvˢ,
            nilRecvFunc1ChanReceiveˢ),
        makeTest(nilSendˢ,
            nilSendFunc1ChanSendNilˢ),
        makeTest(selectNoCasesˢ,
            selectNoCasesFunc1Selectˢ),
        makeTest(chanRecvˢ,
            chanRecvFunc1ChanReceiveˢ),
        makeTest(chanSendˢ,
            chanSendFunc1ChanSendˢ),
        makeTest(selectˢ,
            selectFunc1Selectˢ),
        makeTest(waitGroupˢ,
            waitGroupFunc1Syncˢ),
        makeTest(mutexStackˢ,
            mutexStackFunc1SyncMutexˢ),
        makeTest(mutexHeapˢ,
            mutexHeapFunc11SyncMutexˢ),
        makeTest(condˢ,
            condFunc1SyncCondWaitˢ),
        makeTest(rwMutexRLockˢ,
            rwMutexRLockFunc1Syncˢ),
        makeTest(rwMutexLockˢ,
            rwMutexLockFunc1SyncRwˢ),
        makeTest(mixedˢ,
            mixedFunc1SyncWaitGroupˢ,
            mixedFunc11ChanSendˢ),
        makeTest(noLeakGlobalˢ)
    ]);

    // Stress tests are flaky and we do not strictly care about their output.
    // They are only intended to stress the goroutine leak detector and profiling
    // infrastructure in interesting ways.
    var stressTestCases = slice<TestGoroutineLeakProfile_testCase>([
        makeFlakyTest(spawnGCˢ,
            spawnGCFunc1ChanReceiveˢ),
        makeTest(daisyChainˢ)
    ]);

    // Common goroutine leak patterns.
    // Extracted from "Unveiling and Vanquishing Goroutine Leaks in Enterprise Microservices: A Dynamic Analysis Approach"
    // doi:10.1109/CGO57630.2024.10444835
    var patternTestCases = slice<TestGoroutineLeakProfile_testCase>([
        makeTest(noCloseRangeˢ,
            noCloseRangeChanSendˢ,
            noCloseRangeFunc1Chanˢ),
        makeTest(methodContractViolationˢ,
            workerStartFunc1Selectˢ),
        makeTest(doubleSendˢ,
            doubleSendFunc3ChanSendˢ),
        makeTest(earlyReturnˢ,
            earlyReturnFunc1ChanSendˢ),
        makeTest(nCastLeakˢ,
            nCastLeakFunc1ChanSendˢ,
            nCastLeakFunc2Chanˢ),
        makeTest(timeoutˢ,
            // (vsaioc): Timeout is *theoretically* flaky, but the
            // pseudo-random choice for select case branches makes it
            // practically impossible for it to fail.
            timeoutFunc1ChanSendˢ)
    ]);

    // GoKer tests from "GoBench: A Benchmark Suite of Real-World Go Concurrency Bugs".
    // Refer to testdata/testgoroutineleakprofile/goker/README.md.
    //
    // This list is curated for tests that are not excessively flaky.
    // Some tests are also excluded because they are redundant.
    //
    // TODO(vsaioc): Some of these might be removable (their patterns may overlap).
    var gokerTestCases = slice<TestGoroutineLeakProfile_testCase>([
        makeFlakyTest(cockroach584ˢ,
            cockroach584Func2Syncˢ),
        makeFlakyTest(cockroach1055ˢ,
            cockroach1055Func2Chanˢ,
            cockroach1055Func22Syncˢ,
            cockroach1055Func21Chanˢ,
            cockroach1055Func21Syncˢ),
        makeFlakyTest(cockroach1462ˢ,
            stopperCockroach1462ˢ,
            cockroach1462Func2Syncˢ),
        makeFlakyTest(cockroach2448ˢ,
            storeCockroach2448ˢ,
            stateCockroach2448Startˢ),
        makeFlakyTest(cockroach3710ˢ,
            storeCockroach3710ˢ,
            storeCockroach3710ˢ2),
        makeFlakyTest(cockroach6181ˢ,
            @"testRangeCacheCoalescedRequests_cockroach6181\(.* \[sync\.WaitGroup\.Wait\]",
            @"testRangeCacheCoalescedRequests_cockroach6181\.func1\.1\(.* \[sync\.(RW)?Mutex\.Lock\]",
            @"testRangeCacheCoalescedRequests_cockroach6181\.func1\.1\(.* \[sync\.RWMutex\.RLock\]"),
        makeTest(cockroach7504ˢ,
            cockroach7504Func21Syncˢ,
            cockroach7504Func22Syncˢ),
        makeFlakyTest(cockroach9935ˢ,
            loggingTCockroach9935ˢ),
        makeFlakyTest(cockroach10214ˢ,
            storeCockroach10214ˢ,
            replicaCockroach10214ˢ),
        makeFlakyTest(cockroach10790ˢ,
            replicaCockroach10790ˢ),
        makeTest(cockroach13197ˢ,
            txCockroach13197ˢ),
        makeTest(cockroach13755ˢ,
            rowsCockroach13755ˢ),
        makeFlakyTest(cockroach16167ˢ,
            cockroach16167Func2Syncˢ,
            executorCockroach16167ˢ),
        makeFlakyTest(cockroach18101ˢ,
            restoreCockroach18101ˢ),
        makeTest(cockroach24808ˢ,
            cockroach24808Func2Chanˢ),
        makeTest(cockroach25456ˢ,
            cockroach25456Func2Chanˢ),
        makeTest(cockroach35073ˢ,
            cockroach35073Func21Chanˢ,
            cockroach35073Func2Chanˢ),
        makeTest(cockroach35931ˢ,
            cockroach35931Func2Chanˢ),
        makeTest(etcd5509ˢ,
            etcd5509Func2SyncRWMutexˢ),
        makeTest(etcd6708ˢ,
            etcd6708Func2SyncRWMutexˢ),
        makeFlakyTest(etcd6857ˢ,
            nodeEtcd6857StatusChanˢ),
        makeFlakyTest(etcd6873ˢ,
            watchBroadcastsEtcd6873ˢ,
            newWatchBroadcastsˢ),
        makeFlakyTest(etcd7492ˢ,
            etcd7492Func2Syncˢ,
            etcd7492Func21ChanSendˢ,
            simpleTokenTTLKeeperˢ),
        makeFlakyTest(etcd7902ˢ,
            doRoundsEtcd7902Func1ˢ,
            doRoundsEtcd7902Func1ˢ2,
            runElectionFuncEtcd7902ˢ),
        makeTest(etcd10492ˢ,
            etcd10492Func2SyncMutexˢ),
        makeTest(grpc660ˢ,
            benchmarkClientGrpc660ˢ),
        makeFlakyTest(grpc795ˢ,
            serverGrpc795ServeSyncˢ,
            @"testServerGracefulStopIdempotent_grpc795\(.* \[sync\.Mutex\.Lock\]"),
        makeTest(grpc862ˢ,
            dialContextGrpc862Func2ˢ),
        makeTest(grpc1275ˢ,
            @"testInflightStreamClosing_grpc1275\.func1\(.* \[chan receive\]"),
        makeTest(grpc1424ˢ,
            dialContextGrpc1424Func1ˢ),
        makeFlakyTest(grpc1460ˢ,
            http2ClientGrpc1460ˢ,
            http2ClientGrpc1460ˢ2),
        makeFlakyTest(grpc3017ˢ,
            // grpc/3017 involves a goroutine leak that also simultaneously engages many GC assists.
            grpc3017Func2ChanReceiveˢ,
            grpc3017Func21SyncMutexˢ,
            lbCacheClientConnˢ),
        makeFlakyTest(hugo3251ˢ,
            hugo3251Func2Syncˢ,
            hugo3251Func21SyncMutexˢ,
            hugo3251Func21Syncˢ),
        makeFlakyTest(hugo5379ˢ,
            pageHugo5379InitContentˢ,
            pageRendererHugo5379Syncˢ,
            hugo5379Func2Syncˢ),
        makeFlakyTest(istio16224ˢ,
            istio16224Func2SyncMutexˢ,
            controllerIstio16224Runˢ,
            controllerIstio16224Runˢ2),
        makeFlakyTest(istio17860ˢ,
            agentIstio17860RunWaitˢ),
        makeFlakyTest(istio18454ˢ,
            workerIstio18454Startˢ,
            workerIstio18454Startˢ2),
        // NOTE(vsaioc):
        // Kubernetes/1321 is excluded due to a race condition in the original program
        // that may, in extremely rare cases, lead to nil pointer dereference crashes.
        // (Reproducible even with regular GC). Only kept here for posterity.
        //
        // makeTest(testCase{name: "Kubernetes1321"},
        // 	`NewMux_kubernetes1321\.gowrap1\(.* \[chan send\]`,
        // 	`testMuxWatcherClose_kubernetes1321\(.* \[sync\.Mutex\.Lock\]`),
        makeTest(kubernetes5316ˢ,
            finishRequestˢ),
        makeFlakyTest(kubernetes6632ˢ,
            idleAwareFramerˢ,
            idleAwareFramerˢ2),
        makeFlakyTest(kubernetes10182ˢ,
            statusManagerˢ,
            statusManagerˢ2),
        makeFlakyTest(kubernetes11298ˢ,
            afterKubernetes11298ˢ,
            afterKubernetes11298ˢ2,
            kubernetes11298Func2Chanˢ),
        makeFlakyTest(kubernetes13135ˢ,
            utilKubernetes13135Syncˢ,
            watchCacheˢ),
        makeTest(kubernetes25331ˢ,
            watchChanKubernetes25331ˢ),
        makeFlakyTest(kubernetes26980ˢ,
            kubernetes26980Func2Chanˢ,
            kubernetes26980Func21ˢ,
            processorListenerˢ),
        makeFlakyTest(kubernetes30872ˢ,
            delayingDelivererˢ,
            controllerˢ,
            namespaceControllerˢ),
        makeTest(kubernetes38669ˢ,
            cacheWatcherˢ),
        makeFlakyTest(kubernetes58107ˢ,
            resourceQuotaControllerˢ,
            resourceQuotaControllerˢ2,
            resourceQuotaControllerˢ3),
        makeFlakyTest(kubernetes62464ˢ,
            managerKubernetes62464ˢ,
            staticPolicyˢ),
        makeFlakyTest(kubernetes70277ˢ,
            kubernetes70277Func2Chanˢ),
        makeFlakyTest(moby4951ˢ,
            deviceSetMoby4951ˢ),
        makeTest(moby7559ˢ,
            udpProxyMoby7559RunSyncˢ),
        makeTest(moby17176ˢ,
            @"testDevmapperLockReleasedDeviceDeletion_moby17176\.func1\(.* \[sync\.Mutex\.Lock\]"),
        makeFlakyTest(moby21233ˢ,
            transferMoby21233Watchˢ,
            transferMoby21233Watchˢ2,
            testTransferMoby21233ˢ),
        makeTest(moby25348ˢ,
            managerMoby25348InitSyncˢ),
        makeFlakyTest(moby27782ˢ,
            jsonFileLoggerMoby27782ˢ,
            watcherMoby27782ˢ),
        makeFlakyTest(moby28462ˢ,
            monitorMoby28462Syncˢ,
            daemonMoby28462ˢ),
        makeTest(moby30408ˢ,
            moby30408Func2Chanˢ,
            testActiveMoby30408Func1ˢ),
        makeFlakyTest(moby33781ˢ,
            monitorMoby33781Func1ˢ),
        makeFlakyTest(moby36114ˢ,
            serviceVMMoby36114ˢ),
        makeFlakyTest(serving2137ˢ,
            breakerServing2137ˢ,
            breakerServing2137ˢ2,
            serving2137Func2Chanˢ),
        makeTest(syncthing4829ˢ,
            syncthing4829Func2Syncˢ),
        makeTest(syncthing5795ˢ,
            rawConnectionˢ,
            syncthing5795Func2Chanˢ)
    ]);

    // Combine all test cases into a single list.
    var testCases = appendꓸꓸꓸ(microTests, stressTestCases);
    testCases = appendꓸꓸꓸ(testCases, patternTestCases);

    var extractLeaksʗ1 = extractLeaks;
    void runTests(@string exepath, slice<TestGoroutineLeakProfile_testCase> testCasesΔ1) {
        // Build the test program once.
        var (exe, err) = buildTestProg(t, exepath);
        if (err != nil) {
            t.Fatal(fmt.Sprintf("building testgoroutineleakprofile failed: %v"u8, err));
        }

        foreach (var (_, vᴛ1) in range(testCasesΔ1)) {
            ref var tcase = ref heap(new TestGoroutineLeakProfile_testCase(), out var Ꮡtcase);
            tcase = vᴛ1;

            var extractLeaksʗ2 = extractLeaksʗ1;
            var tcaseʗ1 = tcase;
            t.Run(tcase.name, (ж<testing.T> tΔ1) => {
                var cmdEnvᴷ = new stackAppend<@string>();
                var unexpectedLeaksᴷ = new stackAppend<@string>();
                var missingLeakStrsᴷ = new stackAppend<@string>();
                tΔ1.Parallel();

                var cmdEnv = slice<@string>([
                    "GODEBUG=asyncpreemptoff=1",
                    "GOEXPERIMENT=goroutineleakprofile"
                ]);

                if (tcaseʗ1.simple) {
                    // If the test is simple, set GOMAXPROCS=1 in order to better
                    // control the behavior of the scheduler.
                    cmdEnv = append(cmdEnvᴷ, cmdEnv, (@string)"GOMAXPROCS=1");
                }

                @string output = default;
                for (nint i = 0; i < tcaseʗ1.repetitions; i++) {
                    // Run program for one repetition and get runOutput trace.
                    var (runOutput, errΔ1) = runBuiltTestProgErr(tΔ1, exe, tcaseʗ1.name, cmdEnv.ꓸꓸꓸ);
                    if (len(runOutput) == 0) {
                        tΔ1.Errorf("Test %s produced no output. Is the goroutine leak profile collected?", tcaseʗ1.name);
                    }
                    // Test cases must not end in a non-zero exit code, or otherwise experience a failure to
                    // actually execute.
                    if (errΔ1 != nil) {
                        tΔ1.Errorf("unexpected failure\noutput:\n%s\n\n", runOutput);
                    }

                    output += runOutput + "\n\n"u8;
                }

                // Extract all the goroutine leaks
                var foundLeaks = extractLeaksʗ2(output);

                // If the test case was not expected to produce leaks, but some were reported,
                // stop the test immediately. Zero tolerance policy for false positives.
                if (len(tcaseʗ1.expectedLeaks) + len(tcaseʗ1.flakyLeaks) == 0 && len(foundLeaks) > 0) {
                    tΔ1.Errorf("output:\n%s\n\ngoroutines leaks detected in case with no leaks", output);
                }

                var unexpectedLeaks = new slice<@string>(0, len(foundLeaks));

                // Parse every leak and check if it is expected (maybe as a flaky leak).
leaks:
                foreach (var (_, leak) in range(foundLeaks)) {
                    // Check if the leak is expected.
                    // If it is, check whether it has been encountered before.
                    bool foundNew = default;
                    ж<Δregexp.Regexp> leakPattern = default;

                    foreach (var (expectedLeak, ok) in tcaseʗ1.expectedLeaks) {
                        if (expectedLeak.MatchString(leak)) {
                            if (!ok) {
                                foundNew = true;
                            }

                            leakPattern = expectedLeak;
                            break;
                        }
                    }

                    if (foundNew) {
                        // Only bother writing if we found a new leak.
                        tcaseʗ1.expectedLeaks[leakPattern] = true;
                    }

                    if (leakPattern == nil) {
                        // We are dealing with a leak not marked as expected.
                        // Check if it is a flaky leak.
                        foreach (var (flakyLeak, _) in tcaseʗ1.flakyLeaks) {
                            if (flakyLeak.MatchString(leak)) {
                                // The leak is flaky. Carry on to the next line.
                                goto continue_leaks;
                            }
                        }

                        unexpectedLeaks = append(unexpectedLeaksᴷ, unexpectedLeaks, leak);
                    }
continue_leaks:;
                }
break_leaks:;

                var missingLeakStrs = new slice<@string>(0, len(tcaseʗ1.expectedLeaks));
                foreach (var (expectedLeak, found) in tcaseʗ1.expectedLeaks) {
                    if (!found) {
                        missingLeakStrs = append(missingLeakStrsᴷ, missingLeakStrs, expectedLeak.String());
                    }
                }

                slice<error> errors = default;
                if (len(unexpectedLeaks) > 0) {
                    errors = append(errors, fmt.Errorf("unexpected goroutine leaks:\n%s\n"u8, strings.Join(unexpectedLeaks, "\n")));
                }
                if (len(missingLeakStrs) > 0) {
                    errors = append(errors, fmt.Errorf("missing expected leaks:\n%s\n"u8, strings.Join(missingLeakStrs, ", ")));
                }
                if (len(errors) > 0) {
                    tΔ1.Fatalf("Failed with the following errors:\n%s\n\noutput:\n%s", errors, output);
                }
            });
        }
    }

    runTests(testgoroutineleakprofileˢ, testCases);
    runTests(testgoroutineleakprofileˢ2, gokerTestCases);
}

} // end runtime_test_package
