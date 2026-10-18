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
internal static readonly @string goflagsˢ = "GOFLAGS"u8;
internal static readonly @string goroutineDˢ = @"goroutine \d+ \["u8;
internal static readonly @string goroutineˢ = "\n\ngoroutine"u8;
internal static readonly @string leakedˢ = "(leaked)"u8;
internal static readonly @string leakedˢ2 = " (leaked)"u8;
internal static readonly @string createdByˢ = "created by"u8;
internal static readonly @string mainˢ2 = "main."u8;
internal static readonly @string nilRecvˢ = "NilRecv"u8;
internal static readonly @string nilRecvFunc1ChanReceiveˢ = @"NilRecv\.func1\(.* \[chan receive \(nil chan\)\]"u8;
internal static readonly @string nilSendˢ = "NilSend"u8;
internal static readonly @string nilSendFunc1ChanSendNilˢ = @"NilSend\.func1\(.* \[chan send \(nil chan\)\]"u8;
internal static readonly @string selectNoCasesˢ = "SelectNoCases"u8;
internal static readonly @string selectNoCasesFunc1Selectˢ = @"SelectNoCases\.func1\(.* \[select \(no cases\)\]"u8;
internal static readonly @string chanRecvˢ = "ChanRecv"u8;
internal static readonly @string chanRecvFunc1ChanReceiveˢ = @"ChanRecv\.func1\(.* \[chan receive\]"u8;
internal static readonly @string chanSendˢ = "ChanSend"u8;
internal static readonly @string chanSendFunc1ChanSendˢ = @"ChanSend\.func1\(.* \[chan send\]"u8;
internal static readonly @string selectˢ = "Select"u8;
internal static readonly @string selectFunc1Selectˢ = @"Select\.func1\(.* \[select\]"u8;
internal static readonly @string waitGroupˢ = "WaitGroup"u8;
internal static readonly @string waitGroupFunc1Syncˢ = @"WaitGroup\.func1\(.* \[sync\.WaitGroup\.Wait\]"u8;
internal static readonly @string mutexStackˢ = "MutexStack"u8;
internal static readonly @string mutexStackFunc1SyncMutexˢ = @"MutexStack\.func1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string mutexHeapˢ = "MutexHeap"u8;
internal static readonly @string mutexHeapFunc11SyncMutexˢ = @"MutexHeap\.func1.1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string condˢ = "Cond"u8;
internal static readonly @string condFunc1SyncCondWaitˢ = @"Cond\.func1\(.* \[sync\.Cond\.Wait\]"u8;
internal static readonly @string rwMutexRLockˢ = "RWMutexRLock"u8;
internal static readonly @string rwMutexRLockFunc1Syncˢ = @"RWMutexRLock\.func1\(.* \[sync\.RWMutex\.RLock\]"u8;
internal static readonly @string rwMutexLockˢ = "RWMutexLock"u8;
internal static readonly @string rwMutexLockFunc1SyncRwˢ = @"RWMutexLock\.func1\(.* \[sync\.(RW)?Mutex\.Lock\]"u8;
internal static readonly @string mixedˢ = "Mixed"u8;
internal static readonly @string mixedFunc1SyncWaitGroupˢ = @"Mixed\.func1\(.* \[sync\.WaitGroup\.Wait\]"u8;
internal static readonly @string mixedFunc11ChanSendˢ = @"Mixed\.func1.1\(.* \[chan send\]"u8;
internal static readonly @string noLeakGlobalˢ = "NoLeakGlobal"u8;
internal static readonly @string spawnGCˢ = "SpawnGC"u8;
internal static readonly @string spawnGCFunc1ChanReceiveˢ = @"spawnGC.func1\(.* \[chan receive\]"u8;
internal static readonly @string daisyChainˢ = "DaisyChain"u8;
internal static readonly @string noCloseRangeˢ = "NoCloseRange"u8;
internal static readonly @string noCloseRangeChanSendˢ = @"noCloseRange\(.* \[chan send\]"u8;
internal static readonly @string noCloseRangeFunc1Chanˢ = @"noCloseRange\.func1\(.* \[chan receive\]"u8;
internal static readonly @string methodContractViolationˢ = "MethodContractViolation"u8;
internal static readonly @string workerStartFunc1Selectˢ = @"worker\.Start\.func1\(.* \[select\]"u8;
internal static readonly @string doubleSendˢ = "DoubleSend"u8;
internal static readonly @string doubleSendFunc3ChanSendˢ = @"DoubleSend\.func3\(.* \[chan send\]"u8;
internal static readonly @string earlyReturnˢ = "EarlyReturn"u8;
internal static readonly @string earlyReturnFunc1ChanSendˢ = @"earlyReturn\.func1\(.* \[chan send\]"u8;
internal static readonly @string nCastLeakˢ = "NCastLeak"u8;
internal static readonly @string nCastLeakFunc1ChanSendˢ = @"nCastLeak\.func1\(.* \[chan send\]"u8;
internal static readonly @string nCastLeakFunc2Chanˢ = @"NCastLeak\.func2\(.* \[chan receive\]"u8;
internal static readonly @string timeoutˢ = "Timeout"u8;
internal static readonly @string timeoutFunc1ChanSendˢ = @"timeout\.func1\(.* \[chan send\]"u8;
internal static readonly @string cockroach584ˢ = "Cockroach584"u8;
internal static readonly @string cockroach584Func2Syncˢ = @"Cockroach584\.func2\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string cockroach1055ˢ = "Cockroach1055"u8;
internal static readonly @string cockroach1055Func2Chanˢ = @"Cockroach1055\.func2\(.* \[chan receive\]"u8;
internal static readonly @string cockroach1055Func22Syncˢ = @"Cockroach1055\.func2\.2\(.* \[sync\.WaitGroup\.Wait\]"u8;
internal static readonly @string cockroach1055Func21Chanˢ = @"Cockroach1055\.func2\.1\(.* \[chan receive\]"u8;
internal static readonly @string cockroach1055Func21Syncˢ = @"Cockroach1055\.func2\.1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string cockroach1462ˢ = "Cockroach1462"u8;
internal static readonly @string stopperCockroach1462ˢ = @"\(\*Stopper_cockroach1462\)\.RunWorker\.func1\(.* \[chan send\]"u8;
internal static readonly @string cockroach1462Func2Syncˢ = @"Cockroach1462\.func2\(.* \[sync\.WaitGroup\.Wait\]"u8;
internal static readonly @string cockroach2448ˢ = "Cockroach2448"u8;
internal static readonly @string storeCockroach2448ˢ = @"\(\*Store_cockroach2448\)\.processRaft\(.* \[select\]"u8;
internal static readonly @string stateCockroach2448Startˢ = @"\(\*state_cockroach2448\)\.start\(.* \[select\]"u8;
internal static readonly @string cockroach3710ˢ = "Cockroach3710"u8;
internal static readonly @string storeCockroach3710ˢ = @"\(\*Store_cockroach3710\)\.ForceRaftLogScanAndProcess\(.* \[sync\.RWMutex\.RLock\]"u8;
internal static readonly @string storeCockroach3710ˢ2 = @"\(\*Store_cockroach3710\)\.processRaft\.func1\(.* \[sync\.RWMutex\.Lock\]"u8;
internal static readonly @string cockroach6181ˢ = "Cockroach6181"u8;
internal static readonly @string cockroach7504ˢ = "Cockroach7504"u8;
internal static readonly @string cockroach7504Func21Syncˢ = @"Cockroach7504\.func2\.1.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string cockroach7504Func22Syncˢ = @"Cockroach7504\.func2\.2.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string cockroach9935ˢ = "Cockroach9935"u8;
internal static readonly @string loggingTCockroach9935ˢ = @"\(\*loggingT_cockroach9935\)\.outputLogEntry\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string cockroach10214ˢ = "Cockroach10214"u8;
internal static readonly @string storeCockroach10214ˢ = @"\(*Store_cockroach10214\)\.sendQueuedHeartbeats\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string replicaCockroach10214ˢ = @"\(*Replica_cockroach10214\)\.tick\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string cockroach10790ˢ = "Cockroach10790"u8;
internal static readonly @string replicaCockroach10790ˢ = @"\(\*Replica_cockroach10790\)\.beginCmds\.func1\(.* \[chan receive\]"u8;
internal static readonly @string cockroach13197ˢ = "Cockroach13197"u8;
internal static readonly @string txCockroach13197ˢ = @"\(\*Tx_cockroach13197\)\.awaitDone\(.* \[chan receive\]"u8;
internal static readonly @string cockroach13755ˢ = "Cockroach13755"u8;
internal static readonly @string rowsCockroach13755ˢ = @"\(\*Rows_cockroach13755\)\.awaitDone\(.* \[chan receive\]"u8;
internal static readonly @string cockroach16167ˢ = "Cockroach16167"u8;
internal static readonly @string cockroach16167Func2Syncˢ = @"Cockroach16167\.func2\(.* \[sync\.RWMutex\.RLock\]"u8;
internal static readonly @string executorCockroach16167ˢ = @"\(\*Executor_cockroach16167\)\.Start\(.* \[sync\.RWMutex\.Lock\]"u8;
internal static readonly @string cockroach18101ˢ = "Cockroach18101"u8;
internal static readonly @string restoreCockroach18101ˢ = @"restore_cockroach18101\.func1\(.* \[chan send\]"u8;
internal static readonly @string cockroach24808ˢ = "Cockroach24808"u8;
internal static readonly @string cockroach24808Func2Chanˢ = @"Cockroach24808\.func2\(.* \[chan send\]"u8;
internal static readonly @string cockroach25456ˢ = "Cockroach25456"u8;
internal static readonly @string cockroach25456Func2Chanˢ = @"Cockroach25456\.func2\(.* \[chan receive\]"u8;
internal static readonly @string cockroach35073ˢ = "Cockroach35073"u8;
internal static readonly @string cockroach35073Func21Chanˢ = @"Cockroach35073\.func2.1\(.* \[chan send\]"u8;
internal static readonly @string cockroach35073Func2Chanˢ = @"Cockroach35073\.func2\(.* \[chan send\]"u8;
internal static readonly @string cockroach35931ˢ = "Cockroach35931"u8;
internal static readonly @string cockroach35931Func2Chanˢ = @"Cockroach35931\.func2\(.* \[chan send\]"u8;
internal static readonly @string etcd5509ˢ = "Etcd5509"u8;
internal static readonly @string etcd5509Func2SyncRWMutexˢ = @"Etcd5509\.func2\(.* \[sync\.RWMutex\.Lock\]"u8;
internal static readonly @string etcd6708ˢ = "Etcd6708"u8;
internal static readonly @string etcd6708Func2SyncRWMutexˢ = @"Etcd6708\.func2\(.* \[sync\.RWMutex\.RLock\]"u8;
internal static readonly @string etcd6857ˢ = "Etcd6857"u8;
internal static readonly @string nodeEtcd6857StatusChanˢ = @"\(\*node_etcd6857\)\.Status\(.* \[chan send\]"u8;
internal static readonly @string etcd6873ˢ = "Etcd6873"u8;
internal static readonly @string watchBroadcastsEtcd6873ˢ = @"\(\*watchBroadcasts_etcd6873\)\.stop\(.* \[chan receive\]"u8;
internal static readonly @string newWatchBroadcastsˢ = @"newWatchBroadcasts_etcd6873\.func1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string etcd7492ˢ = "Etcd7492"u8;
internal static readonly @string etcd7492Func2Syncˢ = @"Etcd7492\.func2\(.* \[sync\.WaitGroup\.Wait\]"u8;
internal static readonly @string etcd7492Func21ChanSendˢ = @"Etcd7492\.func2\.1\(.* \[chan send\]"u8;
internal static readonly @string simpleTokenTTLKeeperˢ = @"\(\*simpleTokenTTLKeeper_etcd7492\)\.run\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string etcd7902ˢ = "Etcd7902"u8;
internal static readonly @string doRoundsEtcd7902Func1ˢ = @"doRounds_etcd7902\.func1\(.* \[chan receive\]"u8;
internal static readonly @string doRoundsEtcd7902Func1ˢ2 = @"doRounds_etcd7902\.func1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string runElectionFuncEtcd7902ˢ = @"runElectionFunc_etcd7902\(.* \[sync\.WaitGroup\.Wait\]"u8;
internal static readonly @string etcd10492ˢ = "Etcd10492"u8;
internal static readonly @string etcd10492Func2SyncMutexˢ = @"Etcd10492\.func2\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string grpc660ˢ = "Grpc660"u8;
internal static readonly @string benchmarkClientGrpc660ˢ = @"\(\*benchmarkClient_grpc660\)\.doCloseLoopUnary\.func1\(.* \[chan send\]"u8;
internal static readonly @string grpc795ˢ = "Grpc795"u8;
internal static readonly @string serverGrpc795ServeSyncˢ = @"\(\*Server_grpc795\)\.Serve\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string grpc862ˢ = "Grpc862"u8;
internal static readonly @string dialContextGrpc862Func2ˢ = @"DialContext_grpc862\.func2\(.* \[chan receive\]"u8;
internal static readonly @string grpc1275ˢ = "Grpc1275"u8;
internal static readonly @string grpc1424ˢ = "Grpc1424"u8;
internal static readonly @string dialContextGrpc1424Func1ˢ = @"DialContext_grpc1424\.func1\(.* \[chan receive\]"u8;
internal static readonly @string grpc1460ˢ = "Grpc1460"u8;
internal static readonly @string http2ClientGrpc1460ˢ = @"\(\*http2Client_grpc1460\)\.keepalive\(.* \[chan receive\]"u8;
internal static readonly @string http2ClientGrpc1460ˢ2 = @"\(\*http2Client_grpc1460\)\.NewStream\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string grpc3017ˢ = "Grpc3017"u8;
internal static readonly @string grpc3017Func2ChanReceiveˢ = @"Grpc3017\.func2\(.* \[chan receive\]"u8;
internal static readonly @string grpc3017Func21SyncMutexˢ = @"Grpc3017\.func2\.1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string lbCacheClientConnˢ = @"\(\*lbCacheClientConn_grpc3017\)\.RemoveSubConn\.func1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string hugo3251ˢ = "Hugo3251"u8;
internal static readonly @string hugo3251Func2Syncˢ = @"Hugo3251\.func2\(.* \[sync\.WaitGroup\.Wait\]"u8;
internal static readonly @string hugo3251Func21SyncMutexˢ = @"Hugo3251\.func2\.1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string hugo3251Func21Syncˢ = @"Hugo3251\.func2\.1\(.* \[sync\.RWMutex\.RLock\]"u8;
internal static readonly @string hugo5379ˢ = "Hugo5379"u8;
internal static readonly @string pageHugo5379InitContentˢ = @"\(\*Page_hugo5379\)\.initContent\.func1\.1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string pageRendererHugo5379Syncˢ = @"pageRenderer_hugo5379\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string hugo5379Func2Syncˢ = @"Hugo5379\.func2\(.* \[sync\.WaitGroup\.Wait\]"u8;
internal static readonly @string istio16224ˢ = "Istio16224"u8;
internal static readonly @string istio16224Func2SyncMutexˢ = @"Istio16224\.func2\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string controllerIstio16224Runˢ = @"\(\*controller_istio16224\)\.Run\(.* \[chan send\]"u8;
internal static readonly @string controllerIstio16224Runˢ2 = @"\(\*controller_istio16224\)\.Run\(.* \[chan receive\]"u8;
internal static readonly @string istio17860ˢ = "Istio17860"u8;
internal static readonly @string agentIstio17860RunWaitˢ = @"\(\*agent_istio17860\)\.runWait\(.* \[chan send\]"u8;
internal static readonly @string istio18454ˢ = "Istio18454"u8;
internal static readonly @string workerIstio18454Startˢ = @"\(\*Worker_istio18454\)\.Start\.func1\(.* \[chan receive\]"u8;
internal static readonly @string workerIstio18454Startˢ2 = @"\(\*Worker_istio18454\)\.Start\.func1\(.* \[chan send\]"u8;
internal static readonly @string kubernetes5316ˢ = "Kubernetes5316"u8;
internal static readonly @string finishRequestˢ = @"finishRequest_kubernetes5316\.func1\(.* \[chan send\]"u8;
internal static readonly @string kubernetes6632ˢ = "Kubernetes6632"u8;
internal static readonly @string idleAwareFramerˢ = @"\(\*idleAwareFramer_kubernetes6632\)\.monitor\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string idleAwareFramerˢ2 = @"\(\*idleAwareFramer_kubernetes6632\)\.WriteFrame\(.* \[chan send\]"u8;
internal static readonly @string kubernetes10182ˢ = "Kubernetes10182"u8;
internal static readonly @string statusManagerˢ = @"\(\*statusManager_kubernetes10182\)\.Start\.func1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string statusManagerˢ2 = @"\(\*statusManager_kubernetes10182\)\.SetPodStatus\(.* \[chan send\]"u8;
internal static readonly @string kubernetes11298ˢ = "Kubernetes11298"u8;
internal static readonly @string afterKubernetes11298ˢ = @"After_kubernetes11298\.func1\(.* \[chan receive\]"u8;
internal static readonly @string afterKubernetes11298ˢ2 = @"After_kubernetes11298\.func1\(.* \[sync\.Cond\.Wait\]"u8;
internal static readonly @string kubernetes11298Func2Chanˢ = @"Kubernetes11298\.func2\(.* \[chan receive\]"u8;
internal static readonly @string kubernetes13135ˢ = "Kubernetes13135"u8;
internal static readonly @string utilKubernetes13135Syncˢ = @"Util_kubernetes13135\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string watchCacheˢ = @"\(\*WatchCache_kubernetes13135\)\.Add\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string kubernetes25331ˢ = "Kubernetes25331"u8;
internal static readonly @string watchChanKubernetes25331ˢ = @"\(\*watchChan_kubernetes25331\)\.run\(.* \[chan send\]"u8;
internal static readonly @string kubernetes26980ˢ = "Kubernetes26980"u8;
internal static readonly @string kubernetes26980Func2Chanˢ = @"Kubernetes26980\.func2\(.* \[chan receive\]"u8;
internal static readonly @string kubernetes26980Func21ˢ = @"Kubernetes26980\.func2\.1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string processorListenerˢ = @"\(\*processorListener_kubernetes26980\)\.pop\(.* \[chan receive\]"u8;
internal static readonly @string kubernetes30872ˢ = "Kubernetes30872"u8;
internal static readonly @string delayingDelivererˢ = @"\(\*DelayingDeliverer_kubernetes30872\)\.StartWithHandler\.func1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string controllerˢ = @"\(\*Controller_kubernetes30872\)\.Run\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string namespaceControllerˢ = @"\(\*NamespaceController_kubernetes30872\)\.Run\.func1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string kubernetes38669ˢ = "Kubernetes38669"u8;
internal static readonly @string cacheWatcherˢ = @"\(\*cacheWatcher_kubernetes38669\)\.process\(.* \[chan send\]"u8;
internal static readonly @string kubernetes58107ˢ = "Kubernetes58107"u8;
internal static readonly @string resourceQuotaControllerˢ = @"\(\*ResourceQuotaController_kubernetes58107\)\.worker\(.* \[sync\.Cond\.Wait\]"u8;
internal static readonly @string resourceQuotaControllerˢ2 = @"\(\*ResourceQuotaController_kubernetes58107\)\.worker\(.* \[sync\.RWMutex\.RLock\]"u8;
internal static readonly @string resourceQuotaControllerˢ3 = @"\(\*ResourceQuotaController_kubernetes58107\)\.Sync\(.* \[sync\.RWMutex\.Lock\]"u8;
internal static readonly @string kubernetes62464ˢ = "Kubernetes62464"u8;
internal static readonly @string managerKubernetes62464ˢ = @"\(\*manager_kubernetes62464\)\.reconcileState\(.* \[sync\.RWMutex\.RLock\]"u8;
internal static readonly @string staticPolicyˢ = @"\(\*staticPolicy_kubernetes62464\)\.RemoveContainer\(.* \[sync\.(RW)?Mutex\.Lock\]"u8;
internal static readonly @string kubernetes70277ˢ = "Kubernetes70277"u8;
internal static readonly @string kubernetes70277Func2Chanˢ = @"Kubernetes70277\.func2\(.* \[chan receive\]"u8;
internal static readonly @string moby4951ˢ = "Moby4951"u8;
internal static readonly @string deviceSetMoby4951ˢ = @"\(\*DeviceSet_moby4951\)\.DeleteDevice\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string moby7559ˢ = "Moby7559"u8;
internal static readonly @string udpProxyMoby7559RunSyncˢ = @"\(\*UDPProxy_moby7559\)\.Run\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string moby17176ˢ = "Moby17176"u8;
internal static readonly @string moby21233ˢ = "Moby21233"u8;
internal static readonly @string transferMoby21233Watchˢ = @"\(\*Transfer_moby21233\)\.Watch\.func1\(.* \[chan send\]"u8;
internal static readonly @string transferMoby21233Watchˢ2 = @"\(\*Transfer_moby21233\)\.Watch\.func1\(.* \[select\]"u8;
internal static readonly @string testTransferMoby21233ˢ = @"testTransfer_moby21233\(.* \[chan receive\]"u8;
internal static readonly @string moby25348ˢ = "Moby25348"u8;
internal static readonly @string managerMoby25348InitSyncˢ = @"\(\*Manager_moby25348\)\.init\(.* \[sync\.WaitGroup\.Wait\]"u8;
internal static readonly @string moby27782ˢ = "Moby27782"u8;
internal static readonly @string jsonFileLoggerMoby27782ˢ = @"\(\*JSONFileLogger_moby27782\)\.readLogs\(.* \[sync\.Cond\.Wait\]"u8;
internal static readonly @string watcherMoby27782ˢ = @"\(\*Watcher_moby27782\)\.readEvents\(.* \[select\]"u8;
internal static readonly @string moby28462ˢ = "Moby28462"u8;
internal static readonly @string monitorMoby28462Syncˢ = @"monitor_moby28462\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string daemonMoby28462ˢ = @"\(\*Daemon_moby28462\)\.StateChanged\(.* \[chan send\]"u8;
internal static readonly @string moby30408ˢ = "Moby30408"u8;
internal static readonly @string moby30408Func2Chanˢ = @"Moby30408\.func2\(.* \[chan receive\]"u8;
internal static readonly @string testActiveMoby30408Func1ˢ = @"testActive_moby30408\.func1\(.* \[sync\.Cond\.Wait\]"u8;
internal static readonly @string moby33781ˢ = "Moby33781"u8;
internal static readonly @string monitorMoby33781Func1ˢ = @"monitor_moby33781\.func1\(.* \[chan send\]"u8;
internal static readonly @string moby36114ˢ = "Moby36114"u8;
internal static readonly @string serviceVMMoby36114ˢ = @"\(\*serviceVM_moby36114\)\.hotAddVHDsAtStart\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string serving2137ˢ = "Serving2137"u8;
internal static readonly @string breakerServing2137ˢ = @"\(\*Breaker_serving2137\)\.concurrentRequest\.func1\(.* \[chan send\]"u8;
internal static readonly @string breakerServing2137ˢ2 = @"\(\*Breaker_serving2137\)\.concurrentRequest\.func1\(.* \[sync\.Mutex\.Lock\]"u8;
internal static readonly @string serving2137Func2Chanˢ = @"Serving2137\.func2\(.* \[chan receive\]"u8;
internal static readonly @string syncthing4829ˢ = "Syncthing4829"u8;
internal static readonly @string syncthing4829Func2Syncˢ = @"Syncthing4829\.func2\(.* \[sync\.RWMutex\.RLock\]"u8;
internal static readonly @string syncthing5795ˢ = "Syncthing5795"u8;
internal static readonly @string rawConnectionˢ = @"\(\*rawConnection_syncthing5795\)\.dispatcherLoop\(.* \[chan receive\]"u8;
internal static readonly @string syncthing5795Func2Chanˢ = @"Syncthing5795\.func2.* \[chan receive\]"u8;
internal static readonly @string testgoroutineleakprofileˢ = "testgoroutineleakprofile"u8;
internal static readonly @string testgoroutineleakprofileˢ2 = "testgoroutineleakprofile/goker"u8;

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

public static void TestGoroutineLeakProfile(ж<testing.T> Ꮡt) {
    // Some tests have false negatives under mayMoreStackPreempt and mayMoreStackMove.
    // This may be a test-only issue in that they're just sensitive to scheduling, but it
    // needs more investigation.
    foreach (var (_, cfg) in new @string[]{"mayMoreStackPreempt"u8, "mayMoreStackMove"u8}.slice()) {
        if (strings.Contains(Δos.Getenv(goflagsˢ), cfg)) {
            testenv.SkipFlaky(new runtime_test_package.testing_TжTB(Ꮡt), 75729);
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
            repetitions: (nint)(repetitions | 1)
        );

        foreach (var (_, leak) in leaks) {
            if (!flaky){
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
        var stacks = strings.Split(output, goroutineˢ);
        slice<@string> leaks = default!;
        foreach (var (_, stack) in stacks) {
            var lines = strings.Split(stack, "\n"u8);
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
            @string waitReason = goroutineHeaderʗ1.ReplaceAllString(header, "["u8);
            waitReason = strings.ReplaceAll(waitReason, leakedˢ2, ""u8);

            // Get the function name from the stack trace (should be two lines above `created by`).
            @string funcName = default!;
            for (nint i = len(lines) - 1; i >= 0; i--) {
                if (strings.Contains(lines[i], createdByˢ)) {
                    funcName = strings.TrimPrefix(lines[i - 2], mainˢ2);
                    break;
                }
            }
            if (funcName == ""u8) {
                Ꮡt.Fatalf("failed to extract function name from stack trace: %s"u8, lines);
            }

            leaks = append(leaks, funcName + " "u8 + waitReason);
        }
        return leaks;
    }

    // Micro tests involve very simple leaks for each type of concurrency primitive operation.
    var microTests = new TestGoroutineLeakProfile_testCase[]{
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
    }.slice();

    // Stress tests are flaky and we do not strictly care about their output.
    // They are only intended to stress the goroutine leak detector and profiling
    // infrastructure in interesting ways.
    var stressTestCases = new TestGoroutineLeakProfile_testCase[]{
        makeFlakyTest(spawnGCˢ,
            spawnGCFunc1ChanReceiveˢ),
        makeTest(daisyChainˢ)
    }.slice();

    // Common goroutine leak patterns.
    // Extracted from "Unveiling and Vanquishing Goroutine Leaks in Enterprise Microservices: A Dynamic Analysis Approach"
    // doi:10.1109/CGO57630.2024.10444835
    var patternTestCases = new TestGoroutineLeakProfile_testCase[]{
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
    }.slice();

    // GoKer tests from "GoBench: A Benchmark Suite of Real-World Go Concurrency Bugs".
    // Refer to testdata/testgoroutineleakprofile/goker/README.md.
    //
    // This list is curated for tests that are not excessively flaky.
    // Some tests are also excluded because they are redundant.
    //
    // TODO(vsaioc): Some of these might be removable (their patterns may overlap).
    var gokerTestCases = new TestGoroutineLeakProfile_testCase[]{
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
            @"testRangeCacheCoalescedRequests_cockroach6181\(.* \[sync\.WaitGroup\.Wait\]"u8,
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
            @"testInflightStreamClosing_grpc1275\.func1\(.* \[chan receive\]"u8),
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
            @"testDevmapperLockReleasedDeviceDeletion_moby17176\.func1\(.* \[sync\.Mutex\.Lock\]"u8),
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
    }.slice();

    // Combine all test cases into a single list.
    var testCases = appendꓸꓸꓸ(microTests, stressTestCases);
    testCases = appendꓸꓸꓸ(testCases, patternTestCases);

    var extractLeaksʗ1 = extractLeaks;
    void runTests(@string exepath, slice<TestGoroutineLeakProfile_testCase> testCasesΔ1) {
        // Build the test program once.
        var (exe, err) = buildTestProg(Ꮡt, exepath);
        if (err != default!) {
            Ꮡt.Fatal(fmt.Sprintf("building testgoroutineleakprofile failed: %v"u8, err));
        }

        foreach (var (_, vᴛ1) in testCasesΔ1) {
            ref var tcase = ref heap(new TestGoroutineLeakProfile_testCase(), out var Ꮡtcase);
            tcase = vᴛ1;

            var extractLeaksʗ2 = extractLeaksʗ1;
            var tcaseʗ1 = tcase;
            Ꮡt.Run(tcase.name, (ж<testing.T> tΔ1) => {
                tΔ1.Parallel();

                var cmdEnv = new @string[]{
                    "GODEBUG=asyncpreemptoff=1"u8,
                    "GOEXPERIMENT=goroutineleakprofile"u8
                }.slice();

                if (tcaseʗ1.simple) {
                    // If the test is simple, set GOMAXPROCS=1 in order to better
                    // control the behavior of the scheduler.
                    cmdEnv = append(cmdEnv, "GOMAXPROCS=1"u8);
                }

                @string output = default!;
                for (nint i = 0; i < tcaseʗ1.repetitions; i++) {
                    // Run program for one repetition and get runOutput trace.
                    var (runOutput, errΔ1) = runBuiltTestProgErr(tΔ1, exe, tcaseʗ1.name, cmdEnv.ꓸꓸꓸ);
                    if (len(runOutput) == 0) {
                        tΔ1.Errorf("Test %s produced no output. Is the goroutine leak profile collected?"u8, tcaseʗ1.name);
                    }
                    // Test cases must not end in a non-zero exit code, or otherwise experience a failure to
                    // actually execute.
                    if (errΔ1 != default!) {
                        tΔ1.Errorf("unexpected failure\noutput:\n%s\n\n"u8, runOutput);
                    }

                    output += runOutput + "\n\n"u8;
                }

                // Extract all the goroutine leaks
                var foundLeaks = extractLeaksʗ2(output);

                // If the test case was not expected to produce leaks, but some were reported,
                // stop the test immediately. Zero tolerance policy for false positives.
                if (len(tcaseʗ1.expectedLeaks) + len(tcaseʗ1.flakyLeaks) == 0 && len(foundLeaks) > 0) {
                    tΔ1.Errorf("output:\n%s\n\ngoroutines leaks detected in case with no leaks"u8, output);
                }

                var unexpectedLeaks = new slice<@string>(0, len(foundLeaks));

                // Parse every leak and check if it is expected (maybe as a flaky leak).
leaks:
                foreach (var (_, leak) in foundLeaks) {
                    // Check if the leak is expected.
                    // If it is, check whether it has been encountered before.
                    bool foundNew = default!;
                    ж<Δregexp.Regexp> leakPattern = default!;

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

                        unexpectedLeaks = append(unexpectedLeaks, leak);
                    }
continue_leaks:;
                }
break_leaks:;

                var missingLeakStrs = new slice<@string>(0, len(tcaseʗ1.expectedLeaks));
                foreach (var (expectedLeak, found) in tcaseʗ1.expectedLeaks) {
                    if (!found) {
                        missingLeakStrs = append(missingLeakStrs, expectedLeak.String());
                    }
                }

                slice<error> errors = default!;
                if (len(unexpectedLeaks) > 0) {
                    errors = append(errors, fmt.Errorf("unexpected goroutine leaks:\n%s\n"u8, strings.Join(unexpectedLeaks, "\n"u8)));
                }
                if (len(missingLeakStrs) > 0) {
                    errors = append(errors, fmt.Errorf("missing expected leaks:\n%s\n"u8, strings.Join(missingLeakStrs, ", "u8)));
                }
                if (len(errors) > 0) {
                    tΔ1.Fatalf("Failed with the following errors:\n%s\n\noutput:\n%s"u8, errors, output);
                }
            });
        }
    }

    runTests(testgoroutineleakprofileˢ, testCases);
    runTests(testgoroutineleakprofileˢ2, gokerTestCases);
}

} // end runtime_test_package
