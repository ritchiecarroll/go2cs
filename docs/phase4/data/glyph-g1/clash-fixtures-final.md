# Clash fixtures at the seat: master against the ⁔ seats' goldens

Every package-main project whose methods take the ⁔ form (35, the clash list), its main.cs.target at master 1f8d62ddb3 against claude/c1-glyph-methods ad83221105 (the regen and behavioral rebaseline 14a291591f). Unified diffs of each golden; GoPositionMap records are in package_info.cs, not here. Generated 2026-11-22 by `git diff origin/master HEAD -- <fixture>/*.cs.target`.

## ClearBuiltinShadow

```diff
diff --git a/src/tests/Behavioral/ClearBuiltinShadow/main.cs.target b/src/tests/Behavioral/ClearBuiltinShadow/main.cs.target
--- a/src/tests/Behavioral/ClearBuiltinShadow/main.cs.target
+++ b/src/tests/Behavioral/ClearBuiltinShadow/main.cs.target
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -16,25 +15,19 @@ partial struct box {
     internal slice<nint> data;
 }
 
-} partial class mainᴛbox {
-
-internal static void clear(this ref box b) { b.data = default; }
-
-} partial class main_package {
+internal static void clear⁔(/*this*/ ref box b) { b.data = default; }
 
 partial struct guard {
     internal error err;
 }
 
-} partial class mainᴛguard {
-
-internal static void recover(this ж<guard> Ꮡg) {
+internal static void recover⁔(/*this*/ ж<guard> Ꮡg) {
     GoFrame ᒐ = default;
     try {
         var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?
         ref var g = ref Ꮡg.DerefOrNull();
 
-        var e = builtin.recover(calledByDefer); if (e != nil) {
+        var e = recover(calledByDefer); if (e != nil) {
             g.err = fmt.Errorf("recovered: %v"u8, e);
         }
     }
@@ -42,7 +35,7 @@ internal static void recover(this ж<guard> Ꮡg) {
     finally { ᒐ.Run(); }
 }
 
-internal static void run(this ж<guard> Ꮡg) {
+internal static void run⁔(/*this*/ ж<guard> Ꮡg) {
     GoFrame ᒐ = default;
     try {
         defer(Ꮡg.recover, ref ᒐ, recovers: true);
@@ -52,8 +45,6 @@ internal static void run(this ж<guard> Ꮡg) {
     finally { ᒐ.Run(); }
 }
 
-} partial class main_package {
-
 internal static void main() {
     var s = slice<nint>([1, 2, 3]);
     clear(s);
--- a/src/tests/Behavioral/ClearBuiltinShadow/package_info.cs
+++ b/src/tests/Behavioral/ClearBuiltinShadow/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -54,11 +57,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "ABRGAAkI0oK0pKKC9oKCgpSCgpSCgpSCgpSCgoKUgoKcgoKCgoKYgoKCgoKCloKCgoI=")]
+[assembly: GoPositionMap("main.go", "main.cs", "ABFG2NKCtKSigtaCgoKUgoKUgoKUgoKUgoKClIKCnIKCgoKCmIKCgoKCgpaCgoKC")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -94,6 +99,20 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.box)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛbox;
-[GoMethodHost(typeof(main_package.guard)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛguard;
+[GoMethodHost(typeof(main_package.box)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛbox;
+[GoMethodHost(typeof(main_package.guard)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛguard;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛbox {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void clear(this ref box b) => main_package.clear⁔(ref b);
+}
+partial class mainᴛguard {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recover(this ж<guard> Ꮡg) => main_package.recover⁔(Ꮡg);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void run(this ж<guard> Ꮡg) => main_package.run⁔(Ꮡg);
+}
+// </MethodForwarders>
```

## CollisionFieldBoxAccessor

```diff
diff --git a/src/tests/Behavioral/CollisionFieldBoxAccessor/main.cs.target b/src/tests/Behavioral/CollisionFieldBoxAccessor/main.cs.target
--- a/src/tests/Behavioral/CollisionFieldBoxAccessor/main.cs.target
+++ b/src/tests/Behavioral/CollisionFieldBoxAccessor/main.cs.target
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -12,11 +11,7 @@ partial struct mark {
 partial struct tagger {
 }
 
-} partial class mainᴛtagger {
-
-internal static void mark(this tagger _) { }
-
-} partial class main_package {
+internal static void mark⁔(/*this*/ tagger _) { }
 
 partial struct holder {
     internal nint mark;
--- a/src/tests/Behavioral/CollisionFieldBoxAccessor/package_info.cs
+++ b/src/tests/Behavioral/CollisionFieldBoxAccessor/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -55,11 +58,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "ABAeAA4sgoKCgoIABxYAARSCgoKSgoKUAAIcgoKCgoKClIKmhIKUgpSWgoKCgpaCgpqClJSU", "66-70:1;88-92:1")]
+[assembly: GoPositionMap("main.go", "main.cs", "AA0eAAwsgoKCgoIABxYAARSCgoKSgoKUAAIcgoKCgoKClIKmhIKUgpSWgoKCgpaCgpqClJSU", "66-70:1;88-92:1")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -95,5 +100,15 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.tagger)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛtagger;
+[GoMethodHost(typeof(main_package.tagger)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛtagger;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛtagger {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void mark(this tagger _) => main_package.mark⁔(_);
+}
+// </MethodForwarders>
```

## CollisionRenamedGlobalReceiver

```diff
diff --git a/src/tests/Behavioral/CollisionRenamedGlobalReceiver/main.cs.target b/src/tests/Behavioral/CollisionRenamedGlobalReceiver/main.cs.target
--- a/src/tests/Behavioral/CollisionRenamedGlobalReceiver/main.cs.target
+++ b/src/tests/Behavioral/CollisionRenamedGlobalReceiver/main.cs.target
@@ -5,7 +5,6 @@ using atomic = sync.atomic_package;
 using lib = CollisionRenamedGlobalReceiver.lib_package;
 using CollisionRenamedGlobalReceiver;
 using sync;
-using static main_package;
 
 partial class main_package {
 
@@ -38,13 +37,9 @@ partial struct TLS {
     internal int32 id;
 }
 
-} partial class mainᴛTLS {
+internal static int32 tid⁔(/*this*/ ref TLS tls) { return tls.id; }
 
-internal static int32 tid(this ref TLS tls) { return tls.id; }
-
-internal static nint count(this ref TLS tls) { return (nint)tls.id * 10; }
-
-} partial class main_package {
+internal static nint count⁔(/*this*/ ref TLS tls) { return (nint)tls.id * 10; }
 
 internal static void main() {
     var first = Ꮡtid.Add(1);
--- a/src/tests/Behavioral/CollisionRenamedGlobalReceiver/package_info.cs
+++ b/src/tests/Behavioral/CollisionRenamedGlobalReceiver/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -50,11 +53,13 @@ using static go.main_package;
 
 // <GoSourcePositionMaps>
 [assembly: GoPositionMap("dotimport.go", "dotimport.cs", "/IKC")]
-[assembly: GoPositionMap("main.go", "main.cs", "ABUqgoKoloKCgoKCAAgKlLSCgoKCloKCloKWgpaCloKCgoI=")]
+[assembly: GoPositionMap("main.go", "main.cs", "ABQqgoKoloKCgoKC6pSUgoKCgpaCgpaCloKWgpaCgoKC")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -90,5 +95,16 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.TLS)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛTLS;
+[GoMethodHost(typeof(main_package.TLS)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛTLS;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛTLS {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int32 tid(this ref TLS tls) => main_package.tid⁔(ref tls);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint count(this ref TLS tls) => main_package.count⁔(ref tls);
+}
+// </MethodForwarders>
```

## CollisionRenamedLocalBox

```diff
diff --git a/src/tests/Behavioral/CollisionRenamedLocalBox/main.cs.target b/src/tests/Behavioral/CollisionRenamedLocalBox/main.cs.target
--- a/src/tests/Behavioral/CollisionRenamedLocalBox/main.cs.target
+++ b/src/tests/Behavioral/CollisionRenamedLocalBox/main.cs.target
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -12,11 +11,7 @@ partial struct p {
 partial struct tagger {
 }
 
-} partial class mainᴛtagger {
-
-internal static void p(this tagger _) { }
-
-} partial class main_package {
+internal static void p⁔(/*this*/ tagger _) { }
 
 partial struct box {
     internal nint n;
@@ -42,11 +37,7 @@ internal static void bump(ж<nint> Ꮡnp) {
 internal static readonly ж<box> Ꮡcounter = new StaticBox<box>(default(box));
 internal static ref box counter => ref ((StaticBox<box>)Ꮡcounter).ValueSlot;
 
-} partial class mainᴛtagger {
-
-internal static nint counter(this tagger _) { return 1; }
-
-} partial class main_package {
+internal static nint counter⁔(/*this*/ tagger _) { return 1; }
 
 internal static nint usesTypeP() {
     p pv = default;
--- a/src/tests/Behavioral/CollisionRenamedLocalBox/package_info.cs
+++ b/src/tests/Behavioral/CollisionRenamedLocalBox/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -53,11 +56,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "ABAmAAoWoICkoIAABxC2goKCgoKmgoKCiIKCgoI=")]
+[assembly: GoPositionMap("main.go", "main.cs", "AA0mAAgWoICkoIAABRCWgoKCgoKmgoKCiIKCgoI=")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -92,5 +97,16 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.tagger)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛtagger;
+[GoMethodHost(typeof(main_package.tagger)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛtagger;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛtagger {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void p(this tagger _) => main_package.p⁔(_);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint counter(this tagger _) => main_package.counter⁔(_);
+}
+// </MethodForwarders>
```

## CrossPkgUser

```diff
diff --git a/src/tests/Behavioral/CrossPkgUser/main.cs.target b/src/tests/Behavioral/CrossPkgUser/main.cs.target
--- a/src/tests/Behavioral/CrossPkgUser/main.cs.target
+++ b/src/tests/Behavioral/CrossPkgUser/main.cs.target
@@ -5,7 +5,6 @@ namespace go;
 
 using fmt = fmt_package;
 using CrossPkgLib = CrossPkgLib_package;
-using static main_package;
 
 partial class main_package {
 
@@ -372,11 +371,7 @@ partial interface Meter {
     nint Bump();
 }
 
-} partial class mainᴛtagged {
-
-internal static @string Meter(this tagged t) { return "tagged-meter"; }
-
-} partial class main_package {
+internal static @string Meter⁔(/*this*/ tagged t) { return "tagged-meter"; }
 
 partial struct counter {
     /*embed*/ public ж<CrossPkgLib_package.Meter> Meter;
--- a/src/tests/Behavioral/CrossPkgUser/package_info.cs
+++ b/src/tests/Behavioral/CrossPkgUser/package_info.cs
@@ -17,6 +17,9 @@ using CrossPkgLib = go.CrossPkgLib_package;
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -78,11 +81,14 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "AAsgnIIAByKiggAGGgAKHgAFEgAMKoIADyyCAAUWAA0egoKCAAoilJSAvgABEJyCsoKCuJTSlIKCgpaCgpaCloKCnoKCgoKegpaCgoKcgp6CgoKCgp6CgoKCgpqCgoKIioKalLS0upSCgoKCgpyCgpiCgoIAARSCgoKChoKCgoKCgoKUgpSCgpSCnIKCgpqClIKUgoKCgpqCgpSCnIKcgpyCggABEIIAARCCmoKCgoKalgABFIKYgoIAARCCgoKCAAEUgoKCAAEQgoKCgoKCgqIACiaCgpQAAhSCABJEABAq", "196-200:1;435-435:1", "", "", "148:21")]
+[assembly: GoPositionMap("main.go", "main.cs", "AAognIIAByKiggAGGgAKHgAFEgAMKoIADyyCAAUWAA0egoKCAAoilJSAvgABEJyCsoKCuJTSlIKCgpaCgpaCloKCnoKCgoKegpaCgoKcgp6CgoKCgp6CgoKCgpqCgoKIioKalLS0upSCgoKCgpyCgpiCgoIAARSCgoKChoKCgoKCgoKUgpSCgpSCnIKCgpqClIKUgoKCgpqCgpSCnIKcgpyCggABEIIAARCCmoKCgoKalgABFIKYgoIAARCCgoKCAAEUgoKCAAEQgoKCgoKCgqIACiaCgpQAAhSCABBEAA4q", "196-200:1;435-435:1", "", "", "147:21")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+using CrossPkgLib = go.CrossPkgLib_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -139,5 +145,15 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.tagged)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛtagged;
+[GoMethodHost(typeof(main_package.tagged)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛtagged;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛtagged {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Meter(this tagged t) => main_package.Meter⁔(t);
+}
+// </MethodForwarders>
```

## DeferTypelessReturns

```diff
diff --git a/src/tests/Behavioral/DeferTypelessReturns/DeferTypelessReturns.cs.target b/src/tests/Behavioral/DeferTypelessReturns/DeferTypelessReturns.cs.target
--- a/src/tests/Behavioral/DeferTypelessReturns/DeferTypelessReturns.cs.target
+++ b/src/tests/Behavioral/DeferTypelessReturns/DeferTypelessReturns.cs.target
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -88,11 +87,9 @@ partial struct res {
     internal nint id;
 }
 
-} partial class mainᴛres {
-
-internal static error close(this res h) {
+internal static error close⁔(/*this*/ res h) {
     fmt.Println("closed", h.id);
     return default;
 }
 
-}
+} // end main_package
--- a/src/tests/Behavioral/DeferTypelessReturns/package_info.cs
+++ b/src/tests/Behavioral/DeferTypelessReturns/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -49,11 +52,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("DeferTypelessReturns.go", "DeferTypelessReturns.cs", "ABAcmqKCgoKm2oKCqqKCypqigoKU1qKIgpKCgoKCgoKCgoKCgqIACAqCgg==", "", "", "", "22:21,53:21,66:21,69:21")]
+[assembly: GoPositionMap("DeferTypelessReturns.go", "DeferTypelessReturns.cs", "AA8cmqKCgoKm2oKCqqKCypqigoKU1qKIgpKCgoKCgoKCgoKCgqLqgoI=", "", "", "", "21:21,52:21,65:21,68:21")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -88,5 +93,15 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.res)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛres;
+[GoMethodHost(typeof(main_package.res)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛres;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛres {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error close(this res h) => main_package.close⁔(h);
+}
+// </MethodForwarders>
```

## DotImportRenamedType

```diff
diff --git a/src/tests/Behavioral/DotImportRenamedType/main.cs.target b/src/tests/Behavioral/DotImportRenamedType/main.cs.target
--- a/src/tests/Behavioral/DotImportRenamedType/main.cs.target
+++ b/src/tests/Behavioral/DotImportRenamedType/main.cs.target
@@ -4,7 +4,6 @@ using fmt = fmt_package;
 using static DotImportRenamedType.renamedlib_package;
 using DotImportRenamedType;
 using renamedlib = DotImportRenamedType.renamedlib_package;
-using static main_package;
 
 partial class main_package {
 
@@ -12,14 +11,10 @@ partial struct Local {
     public @string Tag;
 }
 
-} partial class mainᴛLocal {
-
-public static @string Local(this Local l) {
+internal static @string Local⁔(/*this*/ Local l) {
     return "L:"u8 + l.Tag;
 }
 
-} partial class main_package {
-
 internal static void main() {
     var m = new Marker(Name: "alpha", Size: 3);
     fmt.Println("literal:", m.Marker());
--- a/src/tests/Behavioral/DotImportRenamedType/package_info.cs
+++ b/src/tests/Behavioral/DotImportRenamedType/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -49,11 +52,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "ABBGgsaEgpaCloKYloCCuIKWgpaClICCuIKUgII=")]
+[assembly: GoPositionMap("main.go", "main.cs", "AA1GgqaEgpaCloKYloCCuIKWgpaClICCuIKUgII=")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -87,5 +92,15 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.Local)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛLocal;
+[GoMethodHost(typeof(main_package.Local)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛLocal;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛLocal {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Local(this Local l) => main_package.Local⁔(l);
+}
+// </MethodForwarders>
--- a/src/tests/Behavioral/DotImportRenamedType/renamedlib/package_info.cs
+++ b/src/tests/Behavioral/DotImportRenamedType/renamedlib/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.DotImportRenamedType.renamedlib_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -49,11 +52,13 @@ using static go.DotImportRenamedType.renamedlib_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("renamedlib.go", "renamedlib.cs", "AA4uggALFoLIggAIFIKogqiC")]
+[assembly: GoPositionMap("renamedlib.go", "renamedlib.cs", "AAsuggAHFoKoggAGFIKogqiC")]
 // </GoSourcePositionMaps>
 
 namespace go.DotImportRenamedType;
 
+using static go.DotImportRenamedType.renamedlib_package;
+
 [GoPackage("renamedlib")]
 public static partial class renamedlib_package
 {
@@ -87,6 +92,20 @@ public static partial class renamedlib_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(renamedlib_package.Detail)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class renamedlibᴛDetail;
-[GoMethodHost(typeof(renamedlib_package.Marker)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class renamedlibᴛMarker;
+[GoMethodHost(typeof(renamedlib_package.Detail)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class renamedlibᴛDetail;
+[GoMethodHost(typeof(renamedlib_package.Marker)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class renamedlibᴛMarker;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class renamedlibᴛDetail {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Show(this Detail d) => renamedlib_package.Show⁔(d);
+}
+partial class renamedlibᴛMarker {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Marker(this Marker m) => renamedlib_package.Marker⁔(m);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Detail Detail(this Marker m) => renamedlib_package.Detail⁔(m);
+}
+// </MethodForwarders>
--- a/src/tests/Behavioral/DotImportRenamedType/renamedlib/renamedlib.cs
+++ b/src/tests/Behavioral/DotImportRenamedType/renamedlib/renamedlib.cs
@@ -1,7 +1,6 @@
 namespace go.DotImportRenamedType;
 
 using fmt = fmt_package;
-using static renamedlib_package;
 
 partial class renamedlib_package {
 
@@ -10,33 +9,23 @@ partial struct Marker {
     public nint Size;
 }
 
-} partial class renamedlibᴛMarker {
-
-public static @string Marker(this Marker m) {
+internal static @string Marker⁔(/*this*/ Marker m) {
     return fmt.Sprintf("%s/%d"u8, m.Name, m.Size);
 }
 
-} partial class renamedlib_package {
-
 partial struct Detail {
     public @string Label;
     public nint Rank;
 }
 
-} partial class renamedlibᴛDetail {
-
-public static @string Show(this Detail d) {
+internal static @string Show⁔(/*this*/ Detail d) {
     return fmt.Sprintf("%s#%d"u8, d.Label, d.Rank);
 }
 
-} partial class renamedlibᴛMarker {
-
-public static Detail Detail(this Marker m) {
+internal static Detail Detail⁔(/*this*/ Marker m) {
     return new Detail(Label: m.Name, Rank: m.Size);
 }
 
-} partial class renamedlib_package {
-
 partial struct Plain {
     public @string Note;
 }
```

## EmbeddedValuePointerMethod

```diff
diff --git a/src/tests/Behavioral/EmbeddedValuePointerMethod/main.cs.target b/src/tests/Behavioral/EmbeddedValuePointerMethod/main.cs.target
--- a/src/tests/Behavioral/EmbeddedValuePointerMethod/main.cs.target
+++ b/src/tests/Behavioral/EmbeddedValuePointerMethod/main.cs.target
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -22,12 +21,8 @@ internal static partial void viaParam(ж<outer> o) { o.of(outer.Ꮡinner).bump(1
 
 partial struct flags /*num:uint8*/;
 
-} partial class mainᴛflags {
-
-internal static void set(this ref flags f, flags bit) { f |= (flags)(bit); }
-internal static void clear(this ref flags f, flags bit) { f &= unchecked((flags)~(flags)(bit)); }
-
-} partial class main_package {
+internal static void set⁔(/*this*/ ref flags f, flags bit) { f |= (flags)(bit); }
+internal static void clear⁔(/*this*/ ref flags f, flags bit) { f &= unchecked((flags)~(flags)(bit)); }
 
 partial struct chunk {
     internal uint16 inUse;
--- a/src/tests/Behavioral/EmbeddedValuePointerMethod/package_info.cs
+++ b/src/tests/Behavioral/EmbeddedValuePointerMethod/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -53,11 +56,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "AAsygoIABhDYggAIFoKCgriCgqyCgoK4goKCgpSCgoKCgpyCgoKClqKC")]
+[assembly: GoPositionMap("main.go", "main.cs", "AAoygoIABhC4ggAGFoKCgriCgqyCgoK4goKCgpSCgoKCgpyCgoKClqKC")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -93,5 +98,16 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.flags)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛflags;
+[GoMethodHost(typeof(main_package.flags)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛflags;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛflags {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void set(this ref flags f, flags bit) => main_package.set⁔(ref f, bit);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void clear(this ref flags f, flags bit) => main_package.clear⁔(ref f, bit);
+}
+// </MethodForwarders>
```

## FieldNameTypeMethodCollision

```diff
diff --git a/src/tests/Behavioral/FieldNameTypeMethodCollision/FieldNameTypeMethodCollision.cs.target b/src/tests/Behavioral/FieldNameTypeMethodCollision/FieldNameTypeMethodCollision.cs.target
--- a/src/tests/Behavioral/FieldNameTypeMethodCollision/FieldNameTypeMethodCollision.cs.target
+++ b/src/tests/Behavioral/FieldNameTypeMethodCollision/FieldNameTypeMethodCollision.cs.target
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -10,14 +9,10 @@ partial struct @event {
     internal @string label;
 }
 
-} partial class mainᴛevent {
-
-internal static Label Label(this @event e) {
+internal static Label Label⁔(/*this*/ @event e) {
     return new Label(ΔLabel: e.label, Resource: e.kind);
 }
 
-} partial class main_package {
-
 partial struct Label {
     public @string ΔLabel;
     public nint Resource;
--- a/src/tests/Behavioral/FieldNameTypeMethodCollision/package_info.cs
+++ b/src/tests/Behavioral/FieldNameTypeMethodCollision/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -49,11 +52,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("FieldNameTypeMethodCollision.go", "FieldNameTypeMethodCollision.cs", "AA4kggAJFoKmgoKCgpaCgg==")]
+[assembly: GoPositionMap("FieldNameTypeMethodCollision.go", "FieldNameTypeMethodCollision.cs", "AAskggAHFoKmgoKCgpaCgg==")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -87,5 +92,15 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.@event)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛevent;
+[GoMethodHost(typeof(main_package.@event)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛevent;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛevent {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Label Label(this @event e) => main_package.Label⁔(e);
+}
+// </MethodForwarders>
```

## FuncMethodSameNameCollision

```diff
diff --git a/src/tests/Behavioral/FuncMethodSameNameCollision/main.cs.target b/src/tests/Behavioral/FuncMethodSameNameCollision/main.cs.target
--- a/src/tests/Behavioral/FuncMethodSameNameCollision/main.cs.target
+++ b/src/tests/Behavioral/FuncMethodSameNameCollision/main.cs.target
@@ -3,7 +3,6 @@ namespace go;
 using fmt = fmt_package;
 using shapes = FuncMethodSameNameCollision.shapes_package;
 using FuncMethodSameNameCollision;
-using static main_package;
 
 partial class main_package {
 
@@ -11,11 +10,7 @@ partial struct Node {
     public nint Depth;
 }
 
-} partial class mainᴛNode {
-
-public static bool Deep(this ref Node n) { return n.Depth > 2; }
-
-} partial class main_package {
+internal static bool Deep⁔(/*this*/ ref Node n) { return n.Depth > 2; }
 
 public static bool Deep(ж<Node> Ꮡn) {
     ref var n = ref Ꮡn.DerefOrNull();
--- a/src/tests/Behavioral/FuncMethodSameNameCollision/package_info.cs
+++ b/src/tests/Behavioral/FuncMethodSameNameCollision/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -49,11 +52,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "AA8ktKCkhIKWgpaCgoKWgg==")]
+[assembly: GoPositionMap("main.go", "main.cs", "AAwklKCkhIKWgpaCgoKWgg==")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -86,5 +91,15 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.Node)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛNode;
+[GoMethodHost(typeof(main_package.Node)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛNode;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛNode {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Deep(this ref Node n) => main_package.Deep⁔(ref n);
+}
+// </MethodForwarders>
--- a/src/tests/Behavioral/FuncMethodSameNameCollision/shapes/package_info.cs
+++ b/src/tests/Behavioral/FuncMethodSameNameCollision/shapes/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.FuncMethodSameNameCollision.shapes_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -49,11 +52,13 @@ using static go.FuncMethodSameNameCollision.shapes_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("shapes.go", "shapes.cs", "AAoUtLi2tKDItKA=")]
+[assembly: GoPositionMap("shapes.go", "shapes.cs", "AAgUlJiWlKColKA=")]
 // </GoSourcePositionMaps>
 
 namespace go.FuncMethodSameNameCollision;
 
+using static go.FuncMethodSameNameCollision.shapes_package;
+
 [GoPackage("shapes")]
 public static partial class shapes_package
 {
@@ -84,5 +89,17 @@ public static partial class shapes_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(shapes_package.Type)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class shapesᴛType;
+[GoMethodHost(typeof(shapes_package.Type)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class shapesᴛType;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class shapesᴛType {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsTupleType(this ж<Type> Ꮡt) => shapes_package.IsTupleType⁔(Ꮡt);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Combine(this ref Type t, nint other) => shapes_package.Combine⁔(ref t, other);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Size(this ref Type t) => shapes_package.Size⁔(ref t);
+}
+// </MethodForwarders>
--- a/src/tests/Behavioral/FuncMethodSameNameCollision/shapes/shapes.cs
+++ b/src/tests/Behavioral/FuncMethodSameNameCollision/shapes/shapes.cs
@@ -1,7 +1,5 @@
 namespace go.FuncMethodSameNameCollision;
 
-using static shapes_package;
-
 partial class shapes_package {
 
 partial struct Type {
@@ -10,19 +8,11 @@ partial struct Type {
 
 internal static bool isTupleType(ref Type t) { return (t.Flags & 1) != 0; }
 
-} partial class shapesᴛType {
-
-public static bool IsTupleType(this ж<Type> Ꮡt) { return isTupleType(ref (Ꮡt).DerefOrNull()); }
-
-} partial class shapes_package {
+internal static bool IsTupleType⁔(/*this*/ ж<Type> Ꮡt) { return isTupleType(ref (Ꮡt).DerefOrNull()); }
 
 public static bool IsTupleType(ж<Type> t) { return isTupleType(ref (t).DerefOrNull()); }
 
-} partial class shapesᴛType {
-
-public static nint Combine(this ref Type t, nint other) { return t.Flags + other; }
-
-} partial class shapes_package {
+internal static nint Combine⁔(/*this*/ ref Type t, nint other) { return t.Flags + other; }
 
 public static nint Combine(ж<Type> Ꮡt, nint other) {
     ref var t = ref Ꮡt.DerefOrNull();
@@ -30,11 +20,7 @@ public static nint Combine(ж<Type> Ꮡt, nint other) {
     return t.Flags * 100 + other;
 }
 
-} partial class shapesᴛType {
-
-public static nint Size(this ref Type t) { return t.Flags; }
-
-} partial class shapes_package {
+internal static nint Size⁔(/*this*/ ref Type t) { return t.Flags; }
 
 public static nint Size(ж<Type> Ꮡt, nint scale) {
     ref var t = ref Ꮡt.DerefOrNull();
```

## FuncValueMethodClash

```diff
diff --git a/src/tests/Behavioral/FuncValueMethodClash/main.cs.target b/src/tests/Behavioral/FuncValueMethodClash/main.cs.target
--- a/src/tests/Behavioral/FuncValueMethodClash/main.cs.target
+++ b/src/tests/Behavioral/FuncValueMethodClash/main.cs.target
@@ -3,18 +3,13 @@ namespace go;
 using fmt = fmt_package;
 using reflect = reflect_package;
 using time = time_package;
-using static main_package;
 
 partial class main_package {
 
 partial struct T {
 }
 
-} partial class mainᴛT {
-
-public static @string Run(this T _) { return "method"; }
-
-} partial class main_package {
+internal static @string Run⁔(/*this*/ T _) { return "method"; }
 
 public static @string Run() { return "func"; }
 
--- a/src/tests/Behavioral/FuncValueMethodClash/package_info.cs
+++ b/src/tests/Behavioral/FuncValueMethodClash/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -49,11 +52,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "AA4ktJSUgoKmlISCgoKCloKCgoKWgoKCgpaC")]
+[assembly: GoPositionMap("main.go", "main.cs", "AAsklJSUgoKmlISCgoKCloKCgoKWgoKCgpaC")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoTestMatchingConsoleOutput]
 [GoPackage("main")]
 public static partial class main_package
@@ -88,5 +93,15 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.T)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛT;
+[GoMethodHost(typeof(main_package.T)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛT;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛT {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Run(this T _) => main_package.Run⁔(_);
+}
+// </MethodForwarders>
```

## FuncVsMethodOverload

```diff
diff --git a/src/tests/Behavioral/FuncVsMethodOverload/main.cs.target b/src/tests/Behavioral/FuncVsMethodOverload/main.cs.target
--- a/src/tests/Behavioral/FuncVsMethodOverload/main.cs.target
+++ b/src/tests/Behavioral/FuncVsMethodOverload/main.cs.target
@@ -2,7 +2,6 @@ namespace go;
 
 using fmt = fmt_package;
 using @unsafe = unsafe_package;
-using static main_package;
 
 partial class main_package {
 
@@ -14,17 +13,13 @@ partial struct nih {
     internal uint32 a, b;
 }
 
-} partial class mainᴛnih {
-
-internal static ж<nih> add(this ж<nih> Ꮡp, uintptr bytes) {
+internal static ж<nih> add⁔(/*this*/ ж<nih> Ꮡp, uintptr bytes) {
     if (bytes == 0) {
         return Ꮡp;
     }
     return @unsafe.Pointer.To<nih>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡp), bytes));
 }
 
-} partial class main_package {
-
 internal static partial uint32 step(ж<uint32> v) {
     var q = @unsafe.Pointer.To<uint32>(add(@unsafe.Pointer.FromPinnedBox(v), /* unsafe.Sizeof(uint32(0)) */ (uintptr)4));
     return q.Value;
--- a/src/tests/Behavioral/FuncVsMethodOverload/package_info.cs
+++ b/src/tests/Behavioral/FuncVsMethodOverload/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -53,11 +56,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "AAgyggAIEIKClM6CggAGEqKmhIKWgpaCgpaCloI=")]
+[assembly: GoPositionMap("main.go", "main.cs", "AAcyggAGEIKClK6CggAGEqKmhIKWgpaCgpaCloI=")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -91,5 +96,15 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.nih)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛnih;
+[GoMethodHost(typeof(main_package.nih)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛnih;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛnih {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<nih> add(this ж<nih> Ꮡp, uintptr bytes) => main_package.add⁔(Ꮡp, bytes);
+}
+// </MethodForwarders>
```

## GenericStructFields

```diff
diff --git a/src/tests/Behavioral/GenericStructFields/GenericStructFields.cs.target b/src/tests/Behavioral/GenericStructFields/GenericStructFields.cs.target
--- a/src/tests/Behavioral/GenericStructFields/GenericStructFields.cs.target
+++ b/src/tests/Behavioral/GenericStructFields/GenericStructFields.cs.target
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -79,8 +78,6 @@ internal static (T, bool) take<T>(this ref pool<T> p) {
 partial struct keeper {
 }
 
-} partial class mainᴛkeeper {
+internal static @string pool⁔(/*this*/ keeper k) { return "kept"; }
 
-internal static @string pool(this keeper k) { return "kept"; }
-
-}
+} // end main_package
--- a/src/tests/Behavioral/GenericStructFields/package_info.cs
+++ b/src/tests/Behavioral/GenericStructFields/package_info.cs
@@ -49,11 +49,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("GenericStructFields.go", "GenericStructFields.cs", "ABcwAAYYhAASKIKCgpqCgoKaguqCgoKUgoL8", "", "", "32=Errorf/1/1/15")]
+[assembly: GoPositionMap("GenericStructFields.go", "GenericStructFields.cs", "ABYwAAYYhAASKIKCgpqCgoKaguqCgoKUgoLc", "", "", "31=Errorf/1/1/15")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -93,3 +95,13 @@ public static partial class main_package
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.keeper)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛkeeper;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛkeeper {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string pool(this keeper k) => main_package.pool⁔(k);
+}
+// </MethodForwarders>
```

## GlobalShadowedByLocal

```diff
diff --git a/src/tests/Behavioral/GlobalShadowedByLocal/main.cs.target b/src/tests/Behavioral/GlobalShadowedByLocal/main.cs.target
--- a/src/tests/Behavioral/GlobalShadowedByLocal/main.cs.target
+++ b/src/tests/Behavioral/GlobalShadowedByLocal/main.cs.target
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -15,11 +14,7 @@ partial struct acquirer {
     internal nint n;
 }
 
-} partial class mainᴛacquirer {
-
-internal static nint trace(this acquirer _) { return 1; }
-
-} partial class main_package {
+internal static nint trace⁔(/*this*/ acquirer _) { return 1; }
 
 internal static partial nint collisionGlobalShadow() {
     nint a = main_package.trace.addr;
--- a/src/tests/Behavioral/GlobalShadowedByLocal/package_info.cs
+++ b/src/tests/Behavioral/GlobalShadowedByLocal/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -53,11 +56,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "AAsS+LyCgoIABBKCgoKoAAEUgoKCgoKUgoKUgpSCAAIQloKClAAEHIKCAAIQgoKU7AABEoKCrpSCgoKCgoKUggAHFg==")]
+[assembly: GoPositionMap("main.go", "main.cs", "AAoS2JyCgoIABBKCgoKoAAEUgoKCgoKUgoKUgpSCAAIQloKClAAEHIKCAAIQgoKU7AABEoKCrpSCgoKCgoKUggAHFg==")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -93,5 +98,15 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.acquirer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛacquirer;
+[GoMethodHost(typeof(main_package.acquirer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛacquirer;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛacquirer {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint trace(this acquirer _) => main_package.trace⁔(_);
+}
+// </MethodForwarders>
```

## GoCallVariations

```diff
diff --git a/src/tests/Behavioral/GoCallVariations/GoCallVariations.cs.target b/src/tests/Behavioral/GoCallVariations/GoCallVariations.cs.target
--- a/src/tests/Behavioral/GoCallVariations/GoCallVariations.cs.target
+++ b/src/tests/Behavioral/GoCallVariations/GoCallVariations.cs.target
@@ -2,7 +2,6 @@ namespace go;
 
 using fmt = fmt_package;
 using time = time_package;
-using static main_package;
 
 partial class main_package {
 
@@ -45,15 +44,11 @@ partial struct accum {
     internal nint total;
 }
 
-} partial class mainᴛaccum {
-
-internal static nint add(this ref accum a, nint n) {
+internal static nint add⁔(/*this*/ ref accum a, nint n) {
     a.total += n;
     return a.total;
 }
 
-} partial class main_package {
-
 internal static void bindAdd(ж<accum> a) {
     var add = (nint p1) => a.add(p1);
     fmt.Println("bound add:", add(5), add(7));
--- a/src/tests/Behavioral/GoCallVariations/package_info.cs
+++ b/src/tests/Behavioral/GoCallVariations/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -53,11 +56,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("GoCallVariations.go", "GoCallVariations.cs", "AAgQgoKClIKklJSUgoKUgpSUgoKUgoKUAAgKgoLOgoKmgoK4goKCrIKCgoKClJC2goKC", "23-25:1;59-61:1;75-79:1;80-80:2", "", "", "10:15,11:15,12:15,16:15,20:15,88:15")]
+[assembly: GoPositionMap("GoCallVariations.go", "GoCallVariations.cs", "AAcQgoKClIKklJSUgoKUgpSUgoKUgoKU6oKCroKCpoKCuIKCgqyCgoKCgpSQtoKCgg==", "23-25:1;59-61:1;75-79:1;80-80:2", "", "", "9:15,10:15,11:15,15:15,19:15,83:15")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 // Cannot test matching console output, go routine output order is non-deterministic
 public static partial class main_package
@@ -91,5 +96,15 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.accum)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛaccum;
+[GoMethodHost(typeof(main_package.accum)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛaccum;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛaccum {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint add(this ref accum a, nint n) => main_package.add⁔(ref a, n);
+}
+// </MethodForwarders>
```

## InterfaceCasting

```diff
diff --git a/src/tests/Behavioral/InterfaceCasting/InterfaceCasting.cs.target b/src/tests/Behavioral/InterfaceCasting/InterfaceCasting.cs.target
--- a/src/tests/Behavioral/InterfaceCasting/InterfaceCasting.cs.target
+++ b/src/tests/Behavioral/InterfaceCasting/InterfaceCasting.cs.target
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -216,12 +215,8 @@ partial struct badge {
     internal nint num;
 }
 
-} partial class mainᴛbadge {
-
-internal static @string @string(this ref badge b) { return b.text; }
-internal static nint @int(this ref badge b) { return b.num; }
-
-} partial class main_package {
+internal static @string string⁔(/*this*/ ref badge b) { return b.text; }
+internal static nint int⁔(/*this*/ ref badge b) { return b.num; }
 
 internal static void replaceAnimal(ref Animal a) {
     a = new CatжAnimal(Ꮡ(new Cat()));
@@ -260,12 +255,8 @@ partial struct fileRdr {
     internal @string name;
 }
 
-} partial class mainᴛfileRdr {
-
-internal static @string read(this fileRdr f) { return "read:"u8 + f.name; }
-internal static @string close(this fileRdr f) { return "close:"u8 + f.name; }
-
-} partial class main_package {
+internal static @string read⁔(/*this*/ fileRdr f) { return "read:"u8 + f.name; }
+internal static @string close⁔(/*this*/ fileRdr f) { return "close:"u8 + f.name; }
 
 internal static rdCloser open(@string name) { return new fileRdr(name); }
 
--- a/src/tests/Behavioral/InterfaceCasting/package_info.cs
+++ b/src/tests/Behavioral/InterfaceCasting/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -72,11 +75,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("InterfaceCasting.go", "InterfaceCasting.cs", "AAsSgqiCAAkUgtyC3ILcggAGGIKmooKmgqaCAAsigq6CgoKUgoK0goK0goK0qpSilJSUgoKmgoKCgpSClIKClIKUgoKmgoKCgoKCnIKCgoKCnKKCnoKSgpqCgpSUlJyCnIKcgoLYgoKUAA4cgrSCAAsYABEi+IK0AAkWAAgSgoKCgoI=", "195-200:1")]
+[assembly: GoPositionMap("InterfaceCasting.go", "InterfaceCasting.cs", "AAoSgqiCAAkUgtyC3ILcggAGGIKmooKmgqaCAAsigq6CgoKUgoK0goK0goK0qpSilJSUgoKmgoKCgpSClIKClIKUgoKmgoKCgoKCnIKCgoKCnKKCnoKSgpqCgpSUlJyCnIKcgoLYgoKUAAwcgpSCAAsYABEi2IKUAAkWAAgSgoKCgoI=", "195-200:1")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -130,6 +135,21 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.badge)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛbadge;
-[GoMethodHost(typeof(main_package.fileRdr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛfileRdr;
+[GoMethodHost(typeof(main_package.badge)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛbadge;
+[GoMethodHost(typeof(main_package.fileRdr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛfileRdr;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛbadge {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string @string(this ref badge b) => main_package.string⁔(ref b);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint @int(this ref badge b) => main_package.int⁔(ref b);
+}
+partial class mainᴛfileRdr {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string read(this fileRdr f) => main_package.read⁔(f);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string close(this fileRdr f) => main_package.close⁔(f);
+}
+// </MethodForwarders>
```

## IotaEnum

```diff
diff --git a/src/tests/Behavioral/IotaEnum/IotaEnum.cs.target b/src/tests/Behavioral/IotaEnum/IotaEnum.cs.target
--- a/src/tests/Behavioral/IotaEnum/IotaEnum.cs.target
+++ b/src/tests/Behavioral/IotaEnum/IotaEnum.cs.target
@@ -2,7 +2,6 @@ namespace go;
 
 using fmt = fmt_package;
 using @unsafe = unsafe_package;
-using static main_package;
 
 partial class main_package {
 
@@ -50,17 +49,13 @@ public static Kind String => 24;
 public static Kind Struct => 25;
 public static Kind UnsafePointer => 26;
 
-} partial class mainᴛKind {
-
-public static @string String(this Kind k) {
+internal static @string String⁔(/*this*/ Kind k) {
     if ((nint)k < len(kindNames)) {
         return kindNames[k];
     }
     return kindNames[0];
 }
 
-} partial class main_package {
-
 [GoLazy] partial class lazyᴛkindNames { internal static slice<@string> kindNames = new golib.SparseArray<@string>{
     [Invalid] = "invalid",
     [Bool] = "bool",
@@ -132,32 +127,28 @@ public static ж<Type> TypeFor<T>() {
     return TypeOf((ж<T>)nil).Elem();
 }
 
-} partial class mainᴛType {
+internal static Kind Kind⁔(/*this*/ ref Type t) { return (Kind)(t.Kind_ & KindMask); }
 
-public static Kind Kind(this ref Type t) { return (Kind)(t.Kind_ & KindMask); }
-
-public static bool HasName(this ref Type t) {
+internal static bool HasName⁔(/*this*/ ref Type t) {
     return (TFlag)(t.TFlag & TFlagNamed) != 0;
 }
 
-public static ж<Type> Elem(this ж<Type> Ꮡt) {
+internal static ж<Type> Elem⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     switch (t.Kind()) {
     case var ᴛ when ᴛ == Array: {
         var tt = Ꮡt.Reinterpret<Type, ArrayType>();
-        return tt.of(main_package.ArrayType.ᏑElem).Value;
+        return tt.of(ArrayType.ᏑElem).Value;
     }
     case var ᴛ when ᴛ == Map: {
         var tt = Ꮡt.Reinterpret<Type, MapType>();
-        return tt.of(main_package.MapType.ᏑElem).Value;
+        return tt.of(MapType.ᏑElem).Value;
     }}
 
     return default;
 }
 
-} partial class main_package {
-
 partial struct MapType {
     /*embed*/ public Type Type;
     public ж<Type> Key;
@@ -170,9 +161,7 @@ partial struct MapType {
     public uint32 Flags;
 }
 
-} partial class mainᴛType {
-
-public static ж<MapType> MapType(this ж<Type> Ꮡt) {
+internal static ж<MapType> MapType⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() != Map) {
@@ -181,8 +170,6 @@ public static ж<MapType> MapType(this ж<Type> Ꮡt) {
     return Ꮡt.Reinterpret<Type, MapType>();
 }
 
-} partial class main_package {
-
 partial struct ArrayType {
     /*embed*/ public Type Type;
     public ж<Type> Elem;
@@ -190,9 +177,7 @@ partial struct ArrayType {
     public uintptr Len;
 }
 
-} partial class mainᴛType {
-
-public static ж<ArrayType> ArrayType(this ж<Type> Ꮡt) {
+internal static ж<ArrayType> ArrayType⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() != Array) {
@@ -201,8 +186,6 @@ public static ж<ArrayType> ArrayType(this ж<Type> Ꮡt) {
     return Ꮡt.Reinterpret<Type, ArrayType>();
 }
 
-} partial class main_package {
-
 internal const nint seqFirst = iota;
 internal const nint seqSecond = 1;
 internal const nint seqAgain = /* iota */ 2;
--- a/src/tests/Behavioral/IotaEnum/package_info.cs
+++ b/src/tests/Behavioral/IotaEnum/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -49,11 +52,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("IotaEnum.go", "IotaEnum.cs", "ADZugoKUxgAtZIKCAAcQooKmgoKClMaUgqailIKkgqQAEh6igpQADRSigpQADjiCqoKUlKSkpoKCgoKCgoI=")]
+[assembly: GoPositionMap("IotaEnum.go", "IotaEnum.cs", "ADNugoKUpgAtZIKCAAcQooKmgoKClKaUgqailIKkgqQADh6igpQACRSigpQADDiCqoKUlKSkpoKCgoKCgoI=")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -94,6 +99,23 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.Kind)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛKind;
-[GoMethodHost(typeof(main_package.Type)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛType;
+[GoMethodHost(typeof(main_package.Kind)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛKind;
+[GoMethodHost(typeof(main_package.Type)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛType;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛKind {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Kind k) => main_package.String⁔(k);
+}
+partial class mainᴛType {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Kind Kind(this ref Type t) => main_package.Kind⁔(ref t);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool HasName(this ref Type t) => main_package.HasName⁔(ref t);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Type> Elem(this ж<Type> Ꮡt) => main_package.Elem⁔(Ꮡt);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<MapType> MapType(this ж<Type> Ꮡt) => main_package.MapType⁔(Ꮡt);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<ArrayType> ArrayType(this ж<Type> Ꮡt) => main_package.ArrayType⁔(Ꮡt);
+}
+// </MethodForwarders>
```

## MethodHolderAlias

```diff
diff --git a/src/tests/Behavioral/MethodHolderAlias/main.cs.target b/src/tests/Behavioral/MethodHolderAlias/main.cs.target
--- a/src/tests/Behavioral/MethodHolderAlias/main.cs.target
+++ b/src/tests/Behavioral/MethodHolderAlias/main.cs.target
@@ -10,7 +10,6 @@ using MethodHolderAlias.geom;
 using MethodHolderAlias.html;
 using geom = MethodHolderAlias.geom_package;
 using plate = MethodHolderAlias.html.plate_package;
-using static main_package;
 
 partial class main_package {
 
@@ -18,11 +17,7 @@ partial struct field {
     internal geom.Shape s;
 }
 
-} partial class mainᴛfield {
-
-internal static nint Area(this field f) { return MethodHolderAlias.geom_package.Area(f.s.W.Len, f.s.H.Len) + (nint)Unit; }
-
-} partial class main_package {
+internal static nint Area⁔(/*this*/ field f) { return Area(f.s.W.Len, f.s.H.Len) + (nint)Unit; }
 
 internal static void main() {
     ref var s = ref heap<geom.Shape>(new Shape(W: new part.Side(Len: 4), H: new part.Side(Len: 5)), out var Ꮡs);
--- a/src/tests/Behavioral/MethodHolderAlias/package_info.cs
+++ b/src/tests/Behavioral/MethodHolderAlias/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -49,11 +52,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "ABYetIKClIKClJSClA==")]
+[assembly: GoPositionMap("main.go", "main.cs", "ABMelIKClIKClJSClA==")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -86,5 +91,15 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.field)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛfield;
+[GoMethodHost(typeof(main_package.field)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛfield;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛfield {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Area(this field f) => main_package.Area⁔(f);
+}
+// </MethodForwarders>
--- a/src/tests/Behavioral/MethodHolderAlias/text/plate/package_info.cs
+++ b/src/tests/Behavioral/MethodHolderAlias/text/plate/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.MethodHolderAlias.text.plate_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -49,11 +52,13 @@ using static go.MethodHolderAlias.text.plate_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("plate.go", "plate.cs", "AA0MtA==")]
+[assembly: GoPositionMap("plate.go", "plate.cs", "AAgMlA==")]
 // </GoSourcePositionMaps>
 
 namespace go.MethodHolderAlias.text;
 
+using static go.MethodHolderAlias.text.plate_package;
+
 [GoPackage("plate")]
 public static partial class plate_package
 {
@@ -84,5 +89,15 @@ public static partial class plate_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(plate_package.Plate)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class plateᴛPlate;
+[GoMethodHost(typeof(plate_package.Plate)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class plateᴛPlate;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class plateᴛPlate {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Render(this Plate p) => plate_package.Render⁔(p);
+}
+// </MethodForwarders>
--- a/src/tests/Behavioral/MethodHolderAlias/text/plate/plate.cs
+++ b/src/tests/Behavioral/MethodHolderAlias/text/plate/plate.cs
@@ -1,19 +1,12 @@
 namespace go.MethodHolderAlias.text;
 
-using plate = plate_package;
-using static plate_package;
-
 partial class plate_package {
 
 partial struct Plate {
     public @string Name;
 }
 
-} partial class plateᴛPlate {
-
-public static @string Render(this Plate p) { return plate.Render(p.Name); }
-
-} partial class plate_package {
+internal static @string Render⁔(/*this*/ Plate p) { return Render(p.Name); }
 
 public static @string Render(@string name) { return "text:"u8 + name; }
 
--- a/src/tests/Behavioral/MethodHolderAlias/tone/package_info.cs
+++ b/src/tests/Behavioral/MethodHolderAlias/tone/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.MethodHolderAlias.tone_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -49,11 +52,13 @@ using static go.MethodHolderAlias.tone_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("tone.go", "tone.cs", "AAoOltiU")]
+[assembly: GoPositionMap("tone.go", "tone.cs", "7pa4lA==")]
 // </GoSourcePositionMaps>
 
 namespace go.MethodHolderAlias;
 
+using static go.MethodHolderAlias.tone_package;
+
 [GoPackage("tone")]
 public static partial class tone_package
 {
@@ -84,5 +89,16 @@ public static partial class tone_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(tone_package.tone)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class toneᴛtone;
+[GoMethodHost(typeof(tone_package.tone)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class toneᴛtone;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class toneᴛtone {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Pitch(this tone t) => tone_package.Pitch⁔(t);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Base(this tone t) => tone_package.Base⁔(t);
+}
+// </MethodForwarders>
--- a/src/tests/Behavioral/MethodHolderAlias/tone/tone.cs
+++ b/src/tests/Behavioral/MethodHolderAlias/tone/tone.cs
@@ -1,18 +1,12 @@
 namespace go.MethodHolderAlias;
 
-using static tone_package;
-
 partial class tone_package {
 
 partial struct tone /*num:nint*/;
 
-} partial class toneᴛtone {
-
-internal static nint Pitch(this tone t) { return tone_package.Pitch((nint)t) + (nint)tone_package.Base; }
-
-internal static nint Base(this tone t) { return tone_package.Base * 2; }
+internal static nint Pitch⁔(/*this*/ tone t) { return Pitch((nint)t) + (nint)Base; }
 
-} partial class tone_package {
+internal static nint Base⁔(/*this*/ tone t) { return Base * 2; }
 
 public static UntypedInt Base => 100;
 
```

## MethodHolderNames

```diff
diff --git a/src/tests/Behavioral/MethodHolderNames/forms.cs.target b/src/tests/Behavioral/MethodHolderNames/forms.cs.target
--- a/src/tests/Behavioral/MethodHolderNames/forms.cs.target
+++ b/src/tests/Behavioral/MethodHolderNames/forms.cs.target
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -20,13 +19,11 @@ partial struct Chest {
     internal slice<Tag> tags;
 }
 
-} partial class mainᴛChest {
+internal static Tag Tag⁔(/*this*/ ref Chest c) { return c.tag; }
 
-public static Tag Tag(this ref Chest c) { return c.tag; }
+internal static ж<nint> Seal⁔(/*this*/ ж<Chest> Ꮡc) { return Ꮡc.Ꮡtag.Ꮡinner.Ꮡn; }
 
-public static ж<nint> Seal(this ж<Chest> Ꮡc) { return Ꮡc.Ꮡtag.Ꮡinner.Ꮡn; }
-
-public static @string Forms(this ж<Chest> Ꮡc, any v) {
+internal static @string Forms⁔(/*this*/ ж<Chest> Ꮡc, any v) {
     ref var c = ref Ꮡc.DerefOrNull();
 
     var lit = new Tag(id: 1, Value: 2);
@@ -41,7 +38,7 @@ public static @string Forms(this ж<Chest> Ꮡc, any v) {
     var fresh = @new<Tag>();
     fresh.Value.Value = 9;
 
-    var valueAddr = ptr.of(main_package.Tag.ᏑValue);
+    var valueAddr = ptr.of(Tag.ᏑValue);
     valueAddr.Value += 4;
 
     var idAddr = Ꮡc.Ꮡtag.Ꮡid;
@@ -53,7 +50,7 @@ public static @string Forms(this ж<Chest> Ꮡc, any v) {
     @string kind = "other";
 
     switch (v.type()) {
-    case Tag _: {
+    case Tag: {
         kind = "tag";
         break;
     }
@@ -68,8 +65,6 @@ public static @string Forms(this ж<Chest> Ꮡc, any v) {
     return fmt.Sprint(lit.id, (~ptr).Value, zero.id, count, (~fresh).Value, c.tag.id, isTag, isPtr, kind, Ꮡc.Seal().Value);
 }
 
-} partial class main_package {
-
 internal static T pick<T>(slice<T> s, nint i) { return s[i]; }
 
 internal static void forms() {
--- a/src/tests/Behavioral/MethodHolderNames/main.cs
+++ b/src/tests/Behavioral/MethodHolderNames/main.cs
@@ -3,8 +3,6 @@ global using Crate = go.main_package.Box;
 namespace go;
 
 using fmt = fmt_package;
-using main = main_package;
-using static main_package;
 
 partial class main_package {
 
@@ -22,23 +20,21 @@ partial struct Box {
     internal nint n;
 }
 
-} partial class mainᴛBox {
+internal static nint Limit⁔(/*this*/ Box b) { return b.n + (nint)Limit; }
 
-public static nint Limit(this Box b) { return b.n + (nint)main.Limit; }
-
-public static nint Count(this ref Box b) {
-    main.Count++;
-    return b.n + main.Count;
+internal static nint Count⁔(/*this*/ ref Box b) {
+    Count++;
+    return b.n + Count;
 }
 
-public static Kind Kind(this Box b) { return (Kind)b.n; }
+internal static Kind Kind⁔(/*this*/ Box b) { return (Kind)b.n; }
 
-public static nint Scale(this Box b) {
-    var f = main.Scale;
-    return main.Scale(b.n) + f(1);
+internal static nint Scale⁔(/*this*/ Box b) {
+    var f = Scale;
+    return Scale(b.n) + f(1);
 }
 
-public static @string Describe(this Box b, any v) {
+internal static @string Describe⁔(/*this*/ Box b, any v) {
     switch (v.type()) {
     case Kind k: {
         return "a Kind: "u8 + k.String();
@@ -54,9 +50,9 @@ public static @string Describe(this Box b, any v) {
     return fmt.Sprint("other: ", v, " zero ", (Kind)0);
 }
 
-public static @string Class(this Box b, any v) {
+internal static @string Class⁔(/*this*/ Box b, any v) {
     switch (v.type()) {
-    case Kind _: {
+    case Kind: {
         return "kind";
     }
     case Box _:
@@ -67,34 +63,22 @@ public static @string Class(this Box b, any v) {
     return "neither";
 }
 
-} partial class main_package {
-
 partial struct Pair<T> {
     internal T a, b;
 }
 
-} partial class mainᴛPair {
-
-public static nint Scale<T>(this Pair<T> p) { return main.Scale(2) + (nint)main.Limit; }
-
-public static nint Limit<T>(this ref Pair<T> p) { return main.Limit * 2; }
+internal static nint Scale⁔<T>(/*this*/ Pair<T> p) { return Scale(2) + (nint)Limit; }
 
-} partial class mainᴛBox {
+internal static nint Limit⁔<T>(/*this*/ ref Pair<T> p) { return Limit * 2; }
 
-public static nint Total(this Crate c) { return c.Limit() + (nint)main.Limit + main.Scale(c.n); }
-
-} partial class main_package {
+internal static nint Total⁔(/*this*/ Crate c) { return c.Limit() + (nint)Limit + Scale(c.n); }
 
 partial struct Shelf {
     /*embed*/ public Box Box;
     internal nint extra;
 }
 
-} partial class mainᴛShelf {
-
-public static nint Limit(this Shelf s) { return s.Box.Limit() + s.extra + (nint)main.Limit; }
-
-} partial class main_package {
+internal static nint Limit⁔(/*this*/ Shelf s) { return s.Box.Limit() + s.extra + (nint)Limit; }
 
 // Hoisted @string literals (single allocation; Go keeps these in RODATA)
 private static readonly object textˢ = (@string)"text"u8;
@@ -118,7 +102,7 @@ internal static void main() {
     var s = new Shelf(Box: new Box(n: 3), extra: 100);
     fmt.Println("embedded", s.Limit(), s.Scale(), s.Kind(), s.Box.Count());
 
-    var limit = ((Func<Box, nint>)(mainᴛBox.Limit));
+    var limit = ((Func<Box, nint>)(main_package.Limit⁔));
     var count = ((Func<ж<Box>, nint>)(mainᴛBox.Count));
     
     var bʗ1 = b;
diff --git a/src/tests/Behavioral/MethodHolderNames/main.cs.target b/src/tests/Behavioral/MethodHolderNames/main.cs.target
--- a/src/tests/Behavioral/MethodHolderNames/main.cs.target
+++ b/src/tests/Behavioral/MethodHolderNames/main.cs.target
@@ -3,8 +3,6 @@ global using Crate = go.main_package.Box;
 namespace go;
 
 using fmt = fmt_package;
-using main = main_package;
-using static main_package;
 
 partial class main_package {
 
@@ -22,23 +20,21 @@ partial struct Box {
     internal nint n;
 }
 
-} partial class mainᴛBox {
+internal static nint Limit⁔(/*this*/ Box b) { return b.n + (nint)Limit; }
 
-public static nint Limit(this Box b) { return b.n + (nint)main.Limit; }
-
-public static nint Count(this ref Box b) {
-    main.Count++;
-    return b.n + main.Count;
+internal static nint Count⁔(/*this*/ ref Box b) {
+    Count++;
+    return b.n + Count;
 }
 
-public static Kind Kind(this Box b) { return (Kind)b.n; }
+internal static Kind Kind⁔(/*this*/ Box b) { return (Kind)b.n; }
 
-public static nint Scale(this Box b) {
-    var f = main.Scale;
-    return main.Scale(b.n) + f(1);
+internal static nint Scale⁔(/*this*/ Box b) {
+    var f = Scale;
+    return Scale(b.n) + f(1);
 }
 
-public static @string Describe(this Box b, any v) {
+internal static @string Describe⁔(/*this*/ Box b, any v) {
     switch (v.type()) {
     case Kind k: {
         return "a Kind: "u8 + k.String();
@@ -54,9 +50,9 @@ public static @string Describe(this Box b, any v) {
     return fmt.Sprint("other: ", v, " zero ", (Kind)0);
 }
 
-public static @string Class(this Box b, any v) {
+internal static @string Class⁔(/*this*/ Box b, any v) {
     switch (v.type()) {
-    case Kind _: {
+    case Kind: {
         return "kind";
     }
     case Box _:
@@ -67,34 +63,22 @@ public static @string Class(this Box b, any v) {
     return "neither";
 }
 
-} partial class main_package {
-
 partial struct Pair<T> {
     internal T a, b;
 }
 
-} partial class mainᴛPair {
-
-public static nint Scale<T>(this Pair<T> p) { return main.Scale(2) + (nint)main.Limit; }
-
-public static nint Limit<T>(this ref Pair<T> p) { return main.Limit * 2; }
+internal static nint Scale⁔<T>(/*this*/ Pair<T> p) { return Scale(2) + (nint)Limit; }
 
-} partial class mainᴛBox {
+internal static nint Limit⁔<T>(/*this*/ ref Pair<T> p) { return Limit * 2; }
 
-public static nint Total(this Crate c) { return c.Limit() + (nint)main.Limit + main.Scale(c.n); }
-
-} partial class main_package {
+internal static nint Total⁔(/*this*/ Crate c) { return c.Limit() + (nint)Limit + Scale(c.n); }
 
 partial struct Shelf {
     /*embed*/ public Box Box;
     internal nint extra;
 }
 
-} partial class mainᴛShelf {
-
-public static nint Limit(this Shelf s) { return s.Box.Limit() + s.extra + (nint)main.Limit; }
-
-} partial class main_package {
+internal static nint Limit⁔(/*this*/ Shelf s) { return s.Box.Limit() + s.extra + (nint)Limit; }
 
 // Hoisted @string literals (single allocation; Go keeps these in RODATA)
 private static readonly object textˢ = (@string)"text"u8;
@@ -118,7 +102,7 @@ internal static void main() {
     var s = new Shelf(Box: new Box(n: 3), extra: 100);
     fmt.Println("embedded", s.Limit(), s.Scale(), s.Kind(), s.Box.Count());
 
-    var limit = ((Func<Box, nint>)(mainᴛBox.Limit));
+    var limit = ((Func<Box, nint>)(main_package.Limit⁔));
     var count = ((Func<ж<Box>, nint>)(mainᴛBox.Count));
     
     var bʗ1 = b;
--- a/src/tests/Behavioral/MethodHolderNames/others.cs
+++ b/src/tests/Behavioral/MethodHolderNames/others.cs
@@ -4,27 +4,21 @@ using fmt = fmt_package;
 using bits = math.bits_package;
 using sort = sort_package;
 using math;
-using main = main_package;
-using static main_package;
 
 partial class main_package {
 
 partial struct words /*[]uint64*/;
 
-} partial class mainᴛwords {
+internal static nint Len⁔(/*this*/ words w) { return len(w); }
+internal static bool Less⁔(/*this*/ words w, nint i, nint j) { return w[i] < w[j]; }
+internal static void Swap⁔(/*this*/ words w, nint i, nint j) { (w[i], w[j]) = (w[j], w[i]); }
 
-internal static nint Len(this words w) { return len(w); }
-internal static bool Less(this words w, nint i, nint j) { return w[i] < w[j]; }
-internal static void Swap(this words w, nint i, nint j) { (w[i], w[j]) = (w[j], w[i]); }
+internal static nint cap⁔(/*this*/ words w) { return cap(w) + 1; }
+internal static nint bits⁔(/*this*/ words w) { return bits.Len64(w[0]); }
 
-internal static nint cap(this words w) { return builtin.cap(w) + 1; }
-internal static nint bits(this words w) { return math.bits_package.Len64(w[0]); }
+internal static void Sort⁔(/*this*/ words w) { Sort(w); }
 
-internal static void Sort(this words w) { main.Sort(w); }
-
-internal static nint Run(this words w) { return len(w); }
-
-} partial class main_package {
+internal static nint Run⁔(/*this*/ words w) { return len(w); }
 
 partial struct gauge {
     internal nint n;
diff --git a/src/tests/Behavioral/MethodHolderNames/others.cs.target b/src/tests/Behavioral/MethodHolderNames/others.cs.target
--- a/src/tests/Behavioral/MethodHolderNames/others.cs.target
+++ b/src/tests/Behavioral/MethodHolderNames/others.cs.target
@@ -4,27 +4,21 @@ using fmt = fmt_package;
 using bits = math.bits_package;
 using sort = sort_package;
 using math;
-using main = main_package;
-using static main_package;
 
 partial class main_package {
 
 partial struct words /*[]uint64*/;
 
-} partial class mainᴛwords {
+internal static nint Len⁔(/*this*/ words w) { return len(w); }
+internal static bool Less⁔(/*this*/ words w, nint i, nint j) { return w[i] < w[j]; }
+internal static void Swap⁔(/*this*/ words w, nint i, nint j) { (w[i], w[j]) = (w[j], w[i]); }
 
-internal static nint Len(this words w) { return len(w); }
-internal static bool Less(this words w, nint i, nint j) { return w[i] < w[j]; }
-internal static void Swap(this words w, nint i, nint j) { (w[i], w[j]) = (w[j], w[i]); }
+internal static nint cap⁔(/*this*/ words w) { return cap(w) + 1; }
+internal static nint bits⁔(/*this*/ words w) { return bits.Len64(w[0]); }
 
-internal static nint cap(this words w) { return builtin.cap(w) + 1; }
-internal static nint bits(this words w) { return math.bits_package.Len64(w[0]); }
+internal static void Sort⁔(/*this*/ words w) { Sort(w); }
 
-internal static void Sort(this words w) { main.Sort(w); }
-
-internal static nint Run(this words w) { return len(w); }
-
-} partial class main_package {
+internal static nint Run⁔(/*this*/ words w) { return len(w); }
 
 partial struct gauge {
     internal nint n;
--- a/src/tests/Behavioral/MethodHolderNames/package_info.cs
+++ b/src/tests/Behavioral/MethodHolderNames/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -57,14 +60,16 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("forms.go", "forms.cs", "ABgolpaigpSUgoKUgpSClIKUgpSUlMS2lMaUgoI=")]
-[assembly: GoPositionMap("main.go", "main.cs", "ABAclPiUgoKmlIKCpoKUpKaCpqiClLSmAAoMlLoAChDkgoKCgoKCgoKClIKUlIKWgoKilIKC", "", "103=main.Box.Scale")]
-[assembly: GoPositionMap("others.go", "others.cs", "AA8cgoKUgpaU+pSUlIKClIKClA==")]
-[assembly: GoPositionMap("shadow.go", "shadow.cs", "AA4SgoKolrSCgg==")]
+[assembly: GoPositionMap("forms.go", "forms.cs", "ABUolpaigpSUgoKUgpSClIKUgpSUlMS2lKaUgoI=")]
+[assembly: GoPositionMap("main.go", "main.cs", "AA4clNiUgoKmlIKCpoKUpKaCpqiClLSm7JSaAAYQxIKCgoKCgoKCgpSClJSCloKCopSCgg==", "", "103=main.Box.Scale")]
+[assembly: GoPositionMap("others.go", "others.cs", "AAscgoKUgpaU2pSUlIKClIKClA==")]
+[assembly: GoPositionMap("shadow.go", "shadow.cs", "AAoSgoKolpSCgg==")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -108,10 +113,52 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.Box)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛBox;
-[GoMethodHost(typeof(main_package.Chest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛChest;
-[GoMethodHost(typeof(main_package.Pair<>)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛPair;
-[GoMethodHost(typeof(main_package.Shelf)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛShelf;
-[GoMethodHost(typeof(main_package.meter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛmeter;
-[GoMethodHost(typeof(main_package.words)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛwords;
+[GoMethodHost(typeof(main_package.Box)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛBox;
+[GoMethodHost(typeof(main_package.Chest)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛChest;
+[GoMethodHost(typeof(main_package.Pair<>)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛPair;
+[GoMethodHost(typeof(main_package.Shelf)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛShelf;
+[GoMethodHost(typeof(main_package.meter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛmeter;
+[GoMethodHost(typeof(main_package.words)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛwords;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛBox {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Limit(this Box b) => main_package.Limit⁔(b);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Count(this ref Box b) => main_package.Count⁔(ref b);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Kind Kind(this Box b) => main_package.Kind⁔(b);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Scale(this Box b) => main_package.Scale⁔(b);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Describe(this Box b, any v) => main_package.Describe⁔(b, v);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Class(this Box b, any v) => main_package.Class⁔(b, v);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Total(this Crate c) => main_package.Total⁔(c);
+}
+partial class mainᴛChest {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Tag Tag(this ref Chest c) => main_package.Tag⁔(ref c);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<nint> Seal(this ж<Chest> Ꮡc) => main_package.Seal⁔(Ꮡc);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Forms(this ж<Chest> Ꮡc, any v) => main_package.Forms⁔(Ꮡc, v);
+}
+partial class mainᴛPair {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Scale<T>(this Pair<T> p) => main_package.Scale⁔<T>(p);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Limit<T>(this ref Pair<T> p) => main_package.Limit⁔<T>(ref p);
+}
+partial class mainᴛShelf {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Limit(this Shelf s) => main_package.Limit⁔(s);
+}
+partial class mainᴛmeter {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Limit(this meter m) => main_package.Limit⁔(m);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Scale(this meter m, nint main) => main_package.Scale⁔(m, main);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Count(this meter m) => main_package.Count⁔(m);
+}
+partial class mainᴛwords {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this words w) => main_package.Len⁔(w);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Less(this words w, nint i, nint j) => main_package.Less⁔(w, i, j);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Swap(this words w, nint i, nint j) => main_package.Swap⁔(w, i, j);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint cap(this words w) => main_package.cap⁔(w);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint bits(this words w) => main_package.bits⁔(w);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Sort(this words w) => main_package.Sort⁔(w);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Run(this words w) => main_package.Run⁔(w);
+}
+// </MethodForwarders>
--- a/src/tests/Behavioral/MethodHolderNames/shadow.cs
+++ b/src/tests/Behavioral/MethodHolderNames/shadow.cs
@@ -1,8 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using main = main_package;
-using static main_package;
 
 partial class main_package {
 
@@ -10,18 +8,14 @@ partial struct meter {
     internal nint main;
 }
 
-} partial class mainᴛmeter {
-
-internal static nint Limit(this meter m) {
+internal static nint Limit⁔(/*this*/ meter m) {
     nint main = m.main + 1;
-    return main + (nint)main_package.Limit;
+    return main + (nint)Limit;
 }
 
-internal static nint Scale(this meter m, nint main) { return main_package.Scale(main) + m.main; }
-
-internal static nint Count(this meter m) { return main.Count + m.main; }
+internal static nint Scale⁔(/*this*/ meter m, nint main) { return Scale(main) + m.main; }
 
-} partial class main_package {
+internal static nint Count⁔(/*this*/ meter m) { return Count + m.main; }
 
 internal static void shadows() {
     var m = new meter(main: 2);
diff --git a/src/tests/Behavioral/MethodHolderNames/shadow.cs.target b/src/tests/Behavioral/MethodHolderNames/shadow.cs.target
--- a/src/tests/Behavioral/MethodHolderNames/shadow.cs.target
+++ b/src/tests/Behavioral/MethodHolderNames/shadow.cs.target
@@ -1,8 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using main = main_package;
-using static main_package;
 
 partial class main_package {
 
@@ -10,18 +8,14 @@ partial struct meter {
     internal nint main;
 }
 
-} partial class mainᴛmeter {
-
-internal static nint Limit(this meter m) {
+internal static nint Limit⁔(/*this*/ meter m) {
     nint main = m.main + 1;
-    return main + (nint)main_package.Limit;
+    return main + (nint)Limit;
 }
 
-internal static nint Scale(this meter m, nint main) { return main_package.Scale(main) + m.main; }
-
-internal static nint Count(this meter m) { return main.Count + m.main; }
+internal static nint Scale⁔(/*this*/ meter m, nint main) { return Scale(main) + m.main; }
 
-} partial class main_package {
+internal static nint Count⁔(/*this*/ meter m) { return Count + m.main; }
 
 internal static void shadows() {
     var m = new meter(main: 2);
```

## NamedMapMethodShadow

```diff
diff --git a/src/tests/Behavioral/NamedMapMethodShadow/main.cs.target b/src/tests/Behavioral/NamedMapMethodShadow/main.cs.target
--- a/src/tests/Behavioral/NamedMapMethodShadow/main.cs.target
+++ b/src/tests/Behavioral/NamedMapMethodShadow/main.cs.target
@@ -2,17 +2,12 @@ namespace go;
 
 using fmt = fmt_package;
 using strings = strings_package;
-using static main_package;
 
 partial class main_package {
 
 partial struct Env /*map[@string, @string]*/;
 
-} partial class mainᴛEnv {
-
-public static void Set(this Env e, @string k, @string v) { e[strings.ToLower(k)] = v; }
-
-} partial class main_package {
+internal static void Set⁔(/*this*/ Env e, @string k, @string v) { e[strings.ToLower(k)] = v; }
 
 partial struct Bag /*map[@string, nint]*/;
 
@@ -39,11 +34,7 @@ public static bool ContainsKey(this Cache c, @string k) { return k == "always"u8
 
 partial struct PEnv /*map[@string, @string]*/;
 
-} partial class mainᴛPEnv {
-
-public static void Set(this ref PEnv p, @string k, @string v) { (p)[strings.ToUpper(k)] = v; }
-
-} partial class main_package {
+internal static void Set⁔(/*this*/ ref PEnv p, @string k, @string v) { (p)[strings.ToUpper(k)] = v; }
 
 partial struct Uniq<T> /*map[T, EmptyStruct]*/;
 
@@ -66,11 +57,7 @@ partial struct Set /*map[@string, nint]*/;
 
 partial struct Hdr /*map[@string, slice<@string>]*/;
 
-} partial class mainᴛHdr {
-
-public static void Set(this Hdr h, @string k, @string v) { h[strings.ToUpper(k)] = [v]; }
-
-} partial class main_package {
+internal static void Set⁔(/*this*/ Hdr h, @string k, @string v) { h[strings.ToUpper(k)] = [v]; }
 
 internal partial struct main_named /*dyn*/ {
     internal map<@string, Env> m;
--- a/src/tests/Behavioral/NamedMapMethodShadow/package_info.cs
+++ b/src/tests/Behavioral/NamedMapMethodShadow/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -49,11 +52,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "AAwi2t6CgoKmgoKCuNragoKCAAYQgoIACBT0goKCgoKUgoKClIKCgoKClIKCloKCgoKCgoKUgoKCgpSClIKClIKC")]
+[assembly: GoPositionMap("main.go", "main.cs", "AAkiut6CgoKmgoKCuLq6goKCAAYQgoIABhTUgoKCgoKUgoKClIKCgoKClIKCloKCgoKCgoKUgoKCgpSClIKClIKC")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoTestMatchingConsoleOutput]
 [GoPackage("main")]
 public static partial class main_package
@@ -95,7 +100,23 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.Env)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛEnv;
-[GoMethodHost(typeof(main_package.Hdr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛHdr;
-[GoMethodHost(typeof(main_package.PEnv)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛPEnv;
+[GoMethodHost(typeof(main_package.Env)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛEnv;
+[GoMethodHost(typeof(main_package.Hdr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛHdr;
+[GoMethodHost(typeof(main_package.PEnv)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛPEnv;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛEnv {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Set(this Env e, @string k, @string v) => main_package.Set⁔(e, k, v);
+}
+partial class mainᴛHdr {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Set(this Hdr h, @string k, @string v) => main_package.Set⁔(h, k, v);
+}
+partial class mainᴛPEnv {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Set(this ref PEnv p, @string k, @string v) => main_package.Set⁔(ref p, k, v);
+}
+// </MethodForwarders>
```

## NamedMapRvalueIndexWrite

```diff
diff --git a/src/tests/Behavioral/NamedMapRvalueIndexWrite/main.cs.target b/src/tests/Behavioral/NamedMapRvalueIndexWrite/main.cs.target
--- a/src/tests/Behavioral/NamedMapRvalueIndexWrite/main.cs.target
+++ b/src/tests/Behavioral/NamedMapRvalueIndexWrite/main.cs.target
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -13,11 +12,7 @@ partial struct response {
     internal Header hdr;
 }
 
-} partial class mainᴛresponse {
-
-internal static Header Header(this ref response r) { return r.hdr; }
-
-} partial class main_package {
+internal static Header Header⁔(/*this*/ ref response r) { return r.hdr; }
 
 internal static ж<response> newResponse() { return Ꮡ(new response(hdr: new Header(new map<@string, slice<@string>>{}))); }
 
--- a/src/tests/Behavioral/NamedMapRvalueIndexWrite/package_info.cs
+++ b/src/tests/Behavioral/NamedMapRvalueIndexWrite/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -53,11 +56,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "ABE2tJSCloKWloKkgpSWlIKWgg==", "43-43:1")]
+[assembly: GoPositionMap("main.go", "main.cs", "AA42lJSCloKWloKkgpSWlIKWgg==", "43-43:1")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -92,5 +97,15 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.response)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛresponse;
+[GoMethodHost(typeof(main_package.response)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛresponse;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛresponse {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Header Header(this ref response r) => main_package.Header⁔(ref r);
+}
+// </MethodForwarders>
```

## NamedTypeOverStruct

```diff
diff --git a/src/tests/Behavioral/NamedTypeOverStruct/NamedTypeOverStruct.cs.target b/src/tests/Behavioral/NamedTypeOverStruct/NamedTypeOverStruct.cs.target
--- a/src/tests/Behavioral/NamedTypeOverStruct/NamedTypeOverStruct.cs.target
+++ b/src/tests/Behavioral/NamedTypeOverStruct/NamedTypeOverStruct.cs.target
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -50,15 +49,11 @@ partial struct @file {
     internal int64 fd;
 }
 
-} partial class mainᴛfile {
-
-internal static int64 bump(this ref @file f) {
+internal static int64 bump⁔(/*this*/ ref @file f) {
     f.fd += 2;
     return f.fd;
 }
 
-} partial class main_package {
-
 partial struct mixed {
     public nint Pub;
     internal nint sec;
--- a/src/tests/Behavioral/NamedTypeOverStruct/package_info.cs
+++ b/src/tests/Behavioral/NamedTypeOverStruct/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -55,11 +58,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("NamedTypeOverStruct.go", "NamedTypeOverStruct.cs", "ABIqooKCqKKCAAISAAUSggANIIKCAAkYgoKClIKCgpSCgpqCgoKClIKCgpSCgoKcgoKClIKClII=")]
+[assembly: GoPositionMap("NamedTypeOverStruct.go", "NamedTypeOverStruct.cs", "ABEqooKCqKKCAAISAAUSggALIIKCAAcYgoKClIKCgpSCgpqCgoKClIKCgpSCgoKcgoKClIKClII=")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -100,5 +105,15 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.@file)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛfile;
+[GoMethodHost(typeof(main_package.@file)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛfile;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛfile {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 bump(this ref @file f) => main_package.bump⁔(ref f);
+}
+// </MethodForwarders>
```

## PointerReceiverPointerLocalField

```diff
diff --git a/src/tests/Behavioral/PointerReceiverPointerLocalField/main.cs.target b/src/tests/Behavioral/PointerReceiverPointerLocalField/main.cs.target
--- a/src/tests/Behavioral/PointerReceiverPointerLocalField/main.cs.target
+++ b/src/tests/Behavioral/PointerReceiverPointerLocalField/main.cs.target
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -9,13 +8,9 @@ partial struct slot {
     internal nint n;
 }
 
-} partial class mainᴛslot {
-
-internal static void inc(this ref slot s) { s.n++; }
-internal static void add(this ref slot s, nint d) { s.n += d; }
-internal static nint get(this ref slot s) { return s.n; }
-
-} partial class main_package {
+internal static void inc⁔(/*this*/ ref slot s) { s.n++; }
+internal static void add⁔(/*this*/ ref slot s, nint d) { s.n += d; }
+internal static nint get⁔(/*this*/ ref slot s) { return s.n; }
 
 partial struct holder {
     internal slot s;
--- a/src/tests/Behavioral/PointerReceiverPointerLocalField/package_info.cs
+++ b/src/tests/Behavioral/PointerReceiverPointerLocalField/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -53,11 +56,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "AA0cgoL+lIKClIKClg==")]
+[assembly: GoPositionMap("main.go", "main.cs", "AAocgoLelIKClIKClg==")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -91,5 +96,17 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.slot)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛslot;
+[GoMethodHost(typeof(main_package.slot)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛslot;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛslot {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void inc(this ref slot s) => main_package.inc⁔(ref s);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void add(this ref slot s, nint d) => main_package.add⁔(ref s, d);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint get(this ref slot s) => main_package.get⁔(ref s);
+}
+// </MethodForwarders>
```

## PromotedForwarderTypeNameCollision

```diff
diff --git a/src/tests/Behavioral/PromotedForwarderTypeNameCollision/main.cs.target b/src/tests/Behavioral/PromotedForwarderTypeNameCollision/main.cs.target
--- a/src/tests/Behavioral/PromotedForwarderTypeNameCollision/main.cs.target
+++ b/src/tests/Behavioral/PromotedForwarderTypeNameCollision/main.cs.target
@@ -5,7 +5,6 @@ using driver = database.sql.driver_package;
 using fmt = fmt_package;
 using reflect = reflect_package;
 using database.sql;
-using static main_package;
 
 partial class main_package {
 
@@ -24,59 +23,43 @@ partial struct @base {
     internal @string tag;
 }
 
-} partial class mainᴛbase {
-
-internal static Driver Driver(this ref @base b) { return new Driver(name: "driver:"u8 + b.tag); }
-internal static @string Name(this ref @base b) { return "base:"u8 + b.tag; }
-
-} partial class main_package {
+internal static Driver Driver⁔(/*this*/ ref @base b) { return new Driver(name: "driver:"u8 + b.tag); }
+internal static @string Name⁔(/*this*/ ref @base b) { return "base:"u8 + b.tag; }
 
 partial struct counting {
     /*embed*/ public Connector Connector;
     internal nint hits;
 }
 
-} partial class mainᴛcounting {
-
 // Go method set entry for the promoted 'Connector.Driver()' - provided ONLY by the embedded
 // interface field in *counting's method set; see the pointer-only satisfaction record.
-internal static Driver Driver(this counting recvᴛ) => recvᴛ.Connector.Driver();
+internal static Driver Driver⁔(/*this*/ counting recvᴛ) => recvᴛ.Connector.Driver();
 
-internal static @string Name(this ref counting c) {
+internal static @string Name⁔(/*this*/ ref counting c) {
     c.hits++;
     return fmt.Sprintf("counting(%d):%s"u8, c.hits, c.Connector.Name());
 }
 
-} partial class main_package {
-
 partial struct fbase {
 }
 
-} partial class mainᴛfbase {
-
-internal static (driver.Conn, error) Connect(this fbase _Δp0, context.Context _Δp1) { return (default, default); }
-internal static driver.Driver Driver(this fbase _) { return new DriverжDriver(Ꮡ(new Driver(name: "foreign"))); }
-
-} partial class main_package {
+internal static (driver.Conn, error) Connect⁔(/*this*/ fbase _Δp0, context.Context _Δp1) { return (default, default); }
+internal static driver.Driver Driver⁔(/*this*/ fbase _) { return new DriverжDriver(Ꮡ(new Driver(name: "foreign"))); }
 
 partial struct fcounting {
     /*embed*/ public database.sql.driver_package.Connector Connector;
     internal nint n;
 }
 
-} partial class mainᴛfcounting {
-
 // Go method set entry for the promoted 'Connector.Driver()' - provided ONLY by the embedded
 // interface field in *fcounting's method set; see the pointer-only satisfaction record.
-internal static driver.Driver Driver(this fcounting recvᴛ) => recvᴛ.Connector.Driver();
+internal static driver.Driver Driver⁔(/*this*/ fcounting recvᴛ) => recvᴛ.Connector.Driver();
 
-internal static (driver.Conn, error) Connect(this ref fcounting c, context.Context ctx) {
+internal static (driver.Conn, error) Connect⁔(/*this*/ ref fcounting c, context.Context ctx) {
     c.n++;
     return c.Connector.Connect(ctx);
 }
 
-} partial class main_package {
-
 internal static @string use(Connector c) { return c.Name() + " / "u8 + c.Driver().name; }
 
 internal static void main() {
--- a/src/tests/Behavioral/PromotedForwarderTypeNameCollision/package_info.cs
+++ b/src/tests/Behavioral/PromotedForwarderTypeNameCollision/package_info.cs
@@ -10,10 +10,15 @@
 // importing type aliases at a namespace level.
 
 // <ImportedTypeAliases>
+using context = go.context_package;
+using driver = go.database.sql.driver_package;
 // </ImportedTypeAliases>
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -59,11 +64,15 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "AA8qAAwSggAOEoKCAAkOggAODoKCxpSCgoKEopSClIKCgpSCpqaCgoKCgg==")]
+[assembly: GoPositionMap("main.go", "main.cs", "AA4qAAoSggAKEoKC3oIACg6CgqaUgoKChKKUgpSCgoKUgqamgoKCgoI=")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+using context = go.context_package;
+using driver = go.database.sql.driver_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -104,8 +113,31 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.@base)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛbase;
-[GoMethodHost(typeof(main_package.counting)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛcounting;
-[GoMethodHost(typeof(main_package.fbase)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛfbase;
-[GoMethodHost(typeof(main_package.fcounting)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛfcounting;
+[GoMethodHost(typeof(main_package.@base)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛbase;
+[GoMethodHost(typeof(main_package.counting)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛcounting;
+[GoMethodHost(typeof(main_package.fbase)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛfbase;
+[GoMethodHost(typeof(main_package.fcounting)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛfcounting;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛbase {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Driver Driver(this ref @base b) => main_package.Driver⁔(ref b);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Name(this ref @base b) => main_package.Name⁔(ref b);
+}
+partial class mainᴛcounting {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Driver Driver(this counting recvᴛ) => main_package.Driver⁔(recvᴛ);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Name(this ref counting c) => main_package.Name⁔(ref c);
+}
+partial class mainᴛfbase {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (driver.Conn, error) Connect(this fbase _Δp0, context.Context _Δp1) => main_package.Connect⁔(_Δp0, _Δp1);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static driver.Driver Driver(this fbase _) => main_package.Driver⁔(_);
+}
+partial class mainᴛfcounting {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static driver.Driver Driver(this fcounting recvᴛ) => main_package.Driver⁔(recvᴛ);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (driver.Conn, error) Connect(this ref fcounting c, context.Context ctx) => main_package.Connect⁔(ref c, ctx);
+}
+// </MethodForwarders>
```

## PtrKeyMapReceiverLookup

```diff
diff --git a/src/tests/Behavioral/PtrKeyMapReceiverLookup/main.cs.target b/src/tests/Behavioral/PtrKeyMapReceiverLookup/main.cs.target
--- a/src/tests/Behavioral/PtrKeyMapReceiverLookup/main.cs.target
+++ b/src/tests/Behavioral/PtrKeyMapReceiverLookup/main.cs.target
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -13,9 +12,7 @@ partial struct tracker {
     internal map<ж<conn>, @string> m;
 }
 
-} partial class mainᴛconn {
-
-internal static @string status(this ж<conn> Ꮡc, ж<tracker> Ꮡt) {
+internal static @string status⁔(/*this*/ ж<conn> Ꮡc, ж<tracker> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     var (s, ok) = t.m[Ꮡc, ꟷ]; if (ok) {
@@ -24,19 +21,19 @@ internal static @string status(this ж<conn> Ꮡc, ж<tracker> Ꮡt) {
     return "unknown";
 }
 
-internal static void rename(this ж<conn> Ꮡc, ж<tracker> Ꮡt, @string s) {
+internal static void rename⁔(/*this*/ ж<conn> Ꮡc, ж<tracker> Ꮡt, @string s) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     t.m[Ꮡc] = s;
 }
 
-internal static @string label(this ж<conn> Ꮡc, ж<tracker> Ꮡt) {
+internal static @string label⁔(/*this*/ ж<conn> Ꮡc, ж<tracker> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     return t.m[Ꮡc];
 }
 
-internal static void close(this ж<conn> Ꮡc, ж<tracker> Ꮡt) {
+internal static void close⁔(/*this*/ ж<conn> Ꮡc, ж<tracker> Ꮡt) {
     GoFrame ᒐ = default;
     try {
         ref var c = ref Ꮡc.DerefOrNull();
@@ -51,8 +48,6 @@ internal static void close(this ж<conn> Ꮡc, ж<tracker> Ꮡt) {
     finally { ᒐ.Run(); }
 }
 
-} partial class main_package {
-
 internal static void main() {
     var a = Ꮡ(new conn(id: 1));
     var b = Ꮡ(new conn(id: 2));
--- a/src/tests/Behavioral/PtrKeyMapReceiverLookup/package_info.cs
+++ b/src/tests/Behavioral/PtrKeyMapReceiverLookup/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -53,11 +56,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "ABEiooKUqKKmoqrSgoK0xIKCgpaWgpaCloI=", "", "", "", "45:21")]
+[assembly: GoPositionMap("main.go", "main.cs", "AA4iooKUqKKmoqrSgoK0pIKCgpaWgpaCloI=", "", "", "", "42:21")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -91,5 +96,18 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.conn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛconn;
+[GoMethodHost(typeof(main_package.conn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛconn;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛconn {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string status(this ж<conn> Ꮡc, ж<tracker> Ꮡt) => main_package.status⁔(Ꮡc, Ꮡt);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void rename(this ж<conn> Ꮡc, ж<tracker> Ꮡt, @string s) => main_package.rename⁔(Ꮡc, Ꮡt, s);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string label(this ж<conn> Ꮡc, ж<tracker> Ꮡt) => main_package.label⁔(Ꮡc, Ꮡt);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void close(this ж<conn> Ꮡc, ж<tracker> Ꮡt) => main_package.close⁔(Ꮡc, Ꮡt);
+}
+// </MethodForwarders>
```

## RenamedReceiverBox

```diff
diff --git a/src/tests/Behavioral/RenamedReceiverBox/main.cs.target b/src/tests/Behavioral/RenamedReceiverBox/main.cs.target
--- a/src/tests/Behavioral/RenamedReceiverBox/main.cs.target
+++ b/src/tests/Behavioral/RenamedReceiverBox/main.cs.target
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -12,11 +11,7 @@ partial struct p {
 partial struct tagger {
 }
 
-} partial class mainᴛtagger {
-
-internal static void p(this tagger _) { }
-
-} partial class main_package {
+internal static void p⁔(/*this*/ tagger _) { }
 
 partial struct counter {
     internal nint n;
--- a/src/tests/Behavioral/RenamedReceiverBox/package_info.cs
+++ b/src/tests/Behavioral/RenamedReceiverBox/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -53,11 +56,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "ABAk+qCkloKCqqKCgpSCpoKCgpSCloKCgoI=", "36-39:1")]
+[assembly: GoPositionMap("main.go", "main.cs", "AA0k2qCkloKCqqKCgpSCpoKCgpSCloKCgoI=", "36-39:1")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -92,5 +97,15 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.tagger)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛtagger;
+[GoMethodHost(typeof(main_package.tagger)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛtagger;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛtagger {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void p(this tagger _) => main_package.p⁔(_);
+}
+// </MethodForwarders>
```

## ReservedNameShadows

```diff
diff --git a/src/tests/Behavioral/ReservedNameShadows/main.cs.target b/src/tests/Behavioral/ReservedNameShadows/main.cs.target
--- a/src/tests/Behavioral/ReservedNameShadows/main.cs.target
+++ b/src/tests/Behavioral/ReservedNameShadows/main.cs.target
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -58,11 +57,7 @@ partial struct box {
     internal slice<nint> items;
 }
 
-} partial class mainᴛbox {
-
-internal static nint len(this ref box b) { return builtin.len(b.items) + 1; }
-
-} partial class main_package {
+internal static nint len⁔(/*this*/ ref box b) { return len(b.items) + 1; }
 
 internal static nint freeLen() {
     var s = slice<nint>([1, 2, 3]);
--- a/src/tests/Behavioral/ReservedNameShadows/package_info.cs
+++ b/src/tests/Behavioral/ReservedNameShadows/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -49,11 +52,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "ACteooKAkgALDrSCgqiCgqiCgoKCgoKogoKCgoKCqIKCgoKU1oKCgoKCgoKCgoKCgoKCgg==", "49-49:1", "", "", "", "48:21,49:13")]
+[assembly: GoPositionMap("main.go", "main.cs", "ACpeooKAkgAJDpSCgqiCgqiCgoKCgoKogoKCgoKCqIKCgoKU1oKCgoKCgoKCgoKCgoKCgg==", "49-49:1", "", "", "", "47:21,48:13")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -95,5 +100,15 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.box)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛbox;
+[GoMethodHost(typeof(main_package.box)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛbox;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛbox {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint len(this ref box b) => main_package.len⁔(ref b);
+}
+// </MethodForwarders>
```

## ReservedTypeMethodCollision

```diff
diff --git a/src/tests/Behavioral/ReservedTypeMethodCollision/main.cs.target b/src/tests/Behavioral/ReservedTypeMethodCollision/main.cs.target
--- a/src/tests/Behavioral/ReservedTypeMethodCollision/main.cs.target
+++ b/src/tests/Behavioral/ReservedTypeMethodCollision/main.cs.target
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -15,14 +14,10 @@ partial struct builder {
     internal nint @base;
 }
 
-} partial class mainᴛbuilder {
-
-internal static Δslice Δslice(this ref builder b, nint n) {
+internal static Δslice Δslice⁔(/*this*/ ref builder b, nint n) {
     return new Δslice(lo: b.@base, hi: b.@base + n);
 }
 
-} partial class main_package {
-
 internal static void main() {
     var b = Ꮡ(new builder(@base: 10));
     var s = b.Δslice(5);
@@ -41,9 +36,7 @@ partial struct Sieve {
     internal slice<nint> vals;
 }
 
-} partial class mainᴛSieve {
-
-public static nint Filter(this Sieve sv, Func<nint, bool> f) {
+internal static nint Filter⁔(/*this*/ Sieve sv, Func<nint, bool> f) {
     nint count = 0;
     foreach (var (_, v) in range(sv.vals)) {
         if (f(v)) {
@@ -53,4 +46,4 @@ public static nint Filter(this Sieve sv, Func<nint, bool> f) {
     return count;
 }
 
-}
+} // end main_package
--- a/src/tests/Behavioral/ReservedTypeMethodCollision/package_info.cs
+++ b/src/tests/Behavioral/ReservedTypeMethodCollision/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -49,11 +52,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "AAsg/oLGgoKCgoKUgoIAChiCgoKCpg==", "35-35:1")]
+[assembly: GoPositionMap("main.go", "main.cs", "AAog3oKmgoKCgoKUgoIACBiCgoKCpg==", "35-35:1")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -88,6 +93,19 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.Sieve)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛSieve;
-[GoMethodHost(typeof(main_package.builder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛbuilder;
+[GoMethodHost(typeof(main_package.Sieve)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛSieve;
+[GoMethodHost(typeof(main_package.builder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛbuilder;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛSieve {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Filter(this Sieve sv, Func<nint, bool> f) => main_package.Filter⁔(sv, f);
+}
+partial class mainᴛbuilder {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Δslice Δslice(this ref builder b, nint n) => main_package.Δslice⁔(ref b, n);
+}
+// </MethodForwarders>
```

## ShadowedImportConstUser

```diff
diff --git a/src/tests/Behavioral/ShadowedImportConstUser/main.cs.target b/src/tests/Behavioral/ShadowedImportConstUser/main.cs.target
--- a/src/tests/Behavioral/ShadowedImportConstUser/main.cs.target
+++ b/src/tests/Behavioral/ShadowedImportConstUser/main.cs.target
@@ -2,7 +2,6 @@ namespace go;
 
 using fmt = fmt_package;
 using ShadowedImportConstLib = ShadowedImportConstLib_package;
-using static main_package;
 
 partial class main_package {
 
@@ -10,11 +9,7 @@ partial struct gauge {
     internal nint level;
 }
 
-} partial class mainᴛgauge {
-
-internal static nint ShadowedImportConstLib(this gauge g) { return g.level; }
-
-} partial class main_package {
+internal static nint ShadowedImportConstLib⁔(/*this*/ gauge g) { return g.level; }
 
 internal static void main() {
     var g = new gauge(level: 3);
--- a/src/tests/Behavioral/ShadowedImportConstUser/package_info.cs
+++ b/src/tests/Behavioral/ShadowedImportConstUser/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -49,11 +52,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "AA4mtIKYgpaC")]
+[assembly: GoPositionMap("main.go", "main.cs", "AAsmlIKYgpaC")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -86,5 +91,15 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.gauge)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛgauge;
+[GoMethodHost(typeof(main_package.gauge)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛgauge;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛgauge {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint ShadowedImportConstLib(this gauge g) => main_package.ShadowedImportConstLib⁔(g);
+}
+// </MethodForwarders>
```

## ShadowedInterfaceEmbed

```diff
diff --git a/src/tests/Behavioral/ShadowedInterfaceEmbed/main.cs.target b/src/tests/Behavioral/ShadowedInterfaceEmbed/main.cs.target
--- a/src/tests/Behavioral/ShadowedInterfaceEmbed/main.cs.target
+++ b/src/tests/Behavioral/ShadowedInterfaceEmbed/main.cs.target
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -9,11 +8,7 @@ partial struct Logger {
     internal @string name;
 }
 
-} partial class mainᴛLogger {
-
-public static @string Handler(this Logger l) { return "logger:"u8 + l.name; }
-
-} partial class main_package {
+internal static @string Handler⁔(/*this*/ Logger l) { return "logger:"u8 + l.name; }
 
 partial interface Handler {
     bool Enabled(nint level);
--- a/src/tests/Behavioral/ShadowedInterfaceEmbed/package_info.cs
+++ b/src/tests/Behavioral/ShadowedInterfaceEmbed/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -53,11 +56,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "AA0oAA0YgoIABhKUgoKUloKCgpaCgoKC")]
+[assembly: GoPositionMap("main.go", "main.cs", "AAooAAsYgoIABhKUgoKUloKCgpaCgoKC")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoTestMatchingConsoleOutput]
 [GoPackage("main")]
 public static partial class main_package
@@ -93,5 +98,15 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.Logger)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛLogger;
+[GoMethodHost(typeof(main_package.Logger)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛLogger;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛLogger {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Handler(this Logger l) => main_package.Handler⁔(l);
+}
+// </MethodForwarders>
```

## SortArrayType

```diff
diff --git a/src/tests/Behavioral/SortArrayType/SortArrayType.cs.target b/src/tests/Behavioral/SortArrayType/SortArrayType.cs.target
--- a/src/tests/Behavioral/SortArrayType/SortArrayType.cs.target
+++ b/src/tests/Behavioral/SortArrayType/SortArrayType.cs.target
@@ -2,7 +2,6 @@ namespace go;
 
 using fmt = fmt_package;
 using sort = sort_package;
-using static main_package;
 
 partial class main_package {
 
@@ -88,14 +87,10 @@ partial struct levelToken /*num:uint32*/;
 
 internal static UntypedInt markerConst => 256;
 
-} partial class mainᴛPeopleByAge {
-
-internal static void sort(this PeopleByAge p) {
-    sort_package.Sort(p);
+internal static void sort⁔(/*this*/ PeopleByAge p) {
+    sort.Sort(p);
 }
 
-} partial class main_package {
-
 public static nint Len(this PeopleByShoeSize p) {
     return len(p);
 }
@@ -111,22 +106,18 @@ public static bool /*a*/ Less(this PeopleByShoeSize p, nint i, nint j) {
     return a;
 }
 
-} partial class mainᴛPeopleByAge {
-
-public static nint Len(this PeopleByAge p) {
+internal static nint Len⁔(/*this*/ PeopleByAge p) {
     return len(p);
 }
 
-public static void Swap(this PeopleByAge p, nint i, nint j) {
+internal static void Swap⁔(/*this*/ PeopleByAge p, nint i, nint j) {
     (p[i], p[j]) = (p[j], p[i]);
 }
 
-public static bool Less(this PeopleByAge p, nint i, nint j) {
+internal static bool Less⁔(/*this*/ PeopleByAge p, nint i, nint j) {
     return (p[i].Age < p[j].Age);
 }
 
-} partial class main_package {
-
 public static (nint E2, @string p) Testing() {
     nint E2 = default;
     @string p = default;
--- a/src/tests/Behavioral/SortArrayType/package_info.cs
+++ b/src/tests/Behavioral/SortArrayType/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -51,11 +54,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("SortArrayType.go", "SortArrayType.cs", "ADR+3oIADiiCAAgSAAcYgsaCpoKmooLGgqaCpoLGspSSlJSCgoKmgoKCppSmgoKClIKUgoKUloKWABs4lIKUgpqCmoKCmoKcgoKU")]
+[assembly: GoPositionMap("SortArrayType.go", "SortArrayType.cs", "ADN+3oIADiiCAAgSAAUYgqaCpoKmooKmgqaCpoKmspSSlJSCgoKmgoKCppSmgoKClIKUgoKUloKWABs4lIKUgpqCmoKCmoKcgoKU")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -94,5 +99,18 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.PeopleByAge)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛPeopleByAge;
+[GoMethodHost(typeof(main_package.PeopleByAge)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛPeopleByAge;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛPeopleByAge {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sort(this PeopleByAge p) => main_package.sort⁔(p);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Len(this PeopleByAge p) => main_package.Len⁔(p);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Swap(this PeopleByAge p, nint i, nint j) => main_package.Swap⁔(p, i, j);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Less(this PeopleByAge p, nint i, nint j) => main_package.Less⁔(p, i, j);
+}
+// </MethodForwarders>
```

## SortMethodSelfCapture

```diff
diff --git a/src/tests/Behavioral/SortMethodSelfCapture/main.cs.target b/src/tests/Behavioral/SortMethodSelfCapture/main.cs.target
--- a/src/tests/Behavioral/SortMethodSelfCapture/main.cs.target
+++ b/src/tests/Behavioral/SortMethodSelfCapture/main.cs.target
@@ -2,28 +2,18 @@ namespace go;
 
 using fmt = fmt_package;
 using sort = sort_package;
-using main = main_package;
-using static main_package;
 
 partial class main_package {
 
 partial struct words /*[]@string*/;
 
-} partial class mainᴛwords {
-
-internal static nint Len(this words w) { return len(w); }
-internal static bool Less(this words w, nint i, nint j) { return w[i] < w[j]; }
-internal static void Swap(this words w, nint i, nint j) { (w[i], w[j]) = (w[j], w[i]); }
-
-} partial class main_package {
+internal static nint Len⁔(/*this*/ words w) { return len(w); }
+internal static bool Less⁔(/*this*/ words w, nint i, nint j) { return w[i] < w[j]; }
+internal static void Swap⁔(/*this*/ words w, nint i, nint j) { (w[i], w[j]) = (w[j], w[i]); }
 
 public static void Sort(sort.Interface data) { sort.Sort(data); }
 
-} partial class mainᴛwords {
-
-internal static void Sort(this words w) { main.Sort(w); }
-
-} partial class main_package {
+internal static void Sort⁔(/*this*/ words w) { Sort(w); }
 
 internal static void main() {
     var ints = new sort.IntSlice(new nint[]{3, 1, 2}.slice());
--- a/src/tests/Behavioral/SortMethodSelfCapture/package_info.cs
+++ b/src/tests/Behavioral/SortMethodSelfCapture/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -50,11 +53,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "AA0sgoK2trSCgoKUgoKUgoKUgoKUgoI=")]
+[assembly: GoPositionMap("main.go", "main.cs", "AAksgoKWlpSCgoKUgoKUgoKUgoKUgoI=")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -88,5 +93,18 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.words)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛwords;
+[GoMethodHost(typeof(main_package.words)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛwords;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛwords {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this words w) => main_package.Len⁔(w);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Less(this words w, nint i, nint j) => main_package.Less⁔(w, i, j);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Swap(this words w, nint i, nint j) => main_package.Swap⁔(w, i, j);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Sort(this words w) => main_package.Sort⁔(w);
+}
+// </MethodForwarders>
```

## StdLibInternalAbi

```diff
diff --git a/src/tests/Behavioral/StdLibInternalAbi/type.cs.target b/src/tests/Behavioral/StdLibInternalAbi/type.cs.target
--- a/src/tests/Behavioral/StdLibInternalAbi/type.cs.target
+++ b/src/tests/Behavioral/StdLibInternalAbi/type.cs.target
@@ -5,7 +5,6 @@
 namespace go;
 
 using @unsafe = unsafe_package;
-using static main_package;
 
 partial class main_package {
 
@@ -71,17 +70,13 @@ partial struct TypeOff /*num:int32*/;
 
 partial struct TextOff /*num:int32*/;
 
-} partial class mainᴛKind {
-
-public static @string String(this Kind k) {
+internal static @string String⁔(/*this*/ Kind k) {
     if ((nint)k < len(kindNames)) {
         return kindNames[k];
     }
     return kindNames[0];
 }
 
-} partial class main_package {
-
 [GoLazy] partial class lazyᴛkindNames { internal static slice<@string> kindNames = new golib.SparseArray<@string>{
     [Invalid] = "invalid",
     [Bool] = "bool",
@@ -127,30 +122,26 @@ public static ж<Type> TypeFor<T>() {
     return TypeOf((ж<T>)nil).Elem();
 }
 
-} partial class mainᴛType {
-
-public static Kind Kind(this ref Type t) { return (Kind)(t.Kind_ & KindMask); }
+internal static Kind Kind⁔(/*this*/ ref Type t) { return (Kind)(t.Kind_ & KindMask); }
 
-public static bool HasName(this ref Type t) {
+internal static bool HasName⁔(/*this*/ ref Type t) {
     return (TFlag)(t.TFlag & TFlagNamed) != 0;
 }
 
-public static bool Pointers(this ref Type t) { return t.PtrBytes != 0; }
+internal static bool Pointers⁔(/*this*/ ref Type t) { return t.PtrBytes != 0; }
 
-public static bool IfaceIndir(this ref Type t) {
+internal static bool IfaceIndir⁔(/*this*/ ref Type t) {
     return (Kind)(t.Kind_ & KindDirectIface) == 0;
 }
 
-public static bool IsDirectIface(this ref Type t) {
+internal static bool IsDirectIface⁔(/*this*/ ref Type t) {
     return (Kind)(t.Kind_ & KindDirectIface) != 0;
 }
 
-public static slice<byte> GcSlice(this ref Type t, uintptr begin, uintptr end) {
+internal static slice<byte> GcSlice⁔(/*this*/ ref Type t, uintptr begin, uintptr end) {
     return @unsafe.Slice(t.GCData, (nint)end).slice((nint)(begin));
 }
 
-} partial class main_package {
-
 partial struct Method {
     public NameOff Name;
     public TypeOff Mtyp;
@@ -200,9 +191,7 @@ partial struct ArrayType {
     public uintptr Len;
 }
 
-} partial class mainᴛType {
-
-public static nint Len(this ж<Type> Ꮡt) {
+internal static nint Len⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() == Array) {
@@ -211,12 +200,10 @@ public static nint Len(this ж<Type> Ꮡt) {
     return 0;
 }
 
-public static ж<Type> Common(this ж<Type> Ꮡt) {
+internal static ж<Type> Common⁔(/*this*/ ж<Type> Ꮡt) {
     return Ꮡt;
 }
 
-} partial class main_package {
-
 partial struct ChanDir /*num:nint*/;
 
 public static ChanDir RecvDir => /* 1 << iota */ 1;
@@ -235,9 +222,7 @@ partial struct structTypeUncommon {
     internal UncommonType u;
 }
 
-} partial class mainᴛType {
-
-public static ChanDir ChanDir(this ж<Type> Ꮡt) {
+internal static ChanDir ChanDir⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() == Chan) {
@@ -247,8 +232,6 @@ public static ChanDir ChanDir(this ж<Type> Ꮡt) {
     return InvalidDir;
 }
 
-} partial class main_package {
-
 internal partial struct Uncommon_u /*dyn*/ {
     /*embed*/ public PtrType PtrType;
     internal UncommonType u;
@@ -289,9 +272,7 @@ internal partial struct Uncommon_uᴛ7 /*dyn*/ {
     internal UncommonType u;
 }
 
-} partial class mainᴛType {
-
-public static ж<UncommonType> Uncommon(this ж<Type> Ꮡt) {
+internal static ж<UncommonType> Uncommon⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if ((TFlag)(t.TFlag & TFlagUncommon) == 0) {
@@ -328,13 +309,13 @@ public static ж<UncommonType> Uncommon(this ж<Type> Ꮡt) {
 
 }
 
-public static ж<Type> Elem(this ж<Type> Ꮡt) {
+internal static ж<Type> Elem⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     switch (t.Kind()) {
     case var ᴛ when ᴛ == Array: {
         var tt = Ꮡt.Reinterpret<Type, ArrayType>();
-        return tt.of(main_package.ArrayType.ᏑElem).Value;
+        return tt.of(ArrayType.ᏑElem).Value;
     }
     case var ᴛ when ᴛ == Chan: {
         var tt = Ꮡt.Reinterpret<Type, ChanType>();
@@ -342,7 +323,7 @@ public static ж<Type> Elem(this ж<Type> Ꮡt) {
     }
     case var ᴛ when ᴛ == Map: {
         var tt = Ꮡt.Reinterpret<Type, MapType>();
-        return tt.of(main_package.MapType.ᏑElem).Value;
+        return tt.of(MapType.ᏑElem).Value;
     }
     case var ᴛ when ᴛ == Pointer: {
         var tt = Ꮡt.Reinterpret<Type, PtrType>();
@@ -356,7 +337,7 @@ public static ж<Type> Elem(this ж<Type> Ꮡt) {
     return default;
 }
 
-public static ж<StructType> StructType(this ж<Type> Ꮡt) {
+internal static ж<StructType> StructType⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() != Struct) {
@@ -365,7 +346,7 @@ public static ж<StructType> StructType(this ж<Type> Ꮡt) {
     return Ꮡt.Reinterpret<Type, StructType>();
 }
 
-public static ж<MapType> MapType(this ж<Type> Ꮡt) {
+internal static ж<MapType> MapType⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() != Map) {
@@ -374,7 +355,7 @@ public static ж<MapType> MapType(this ж<Type> Ꮡt) {
     return Ꮡt.Reinterpret<Type, MapType>();
 }
 
-public static ж<ArrayType> ArrayType(this ж<Type> Ꮡt) {
+internal static ж<ArrayType> ArrayType⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() != Array) {
@@ -383,7 +364,7 @@ public static ж<ArrayType> ArrayType(this ж<Type> Ꮡt) {
     return Ꮡt.Reinterpret<Type, ArrayType>();
 }
 
-public static ж<FuncType> FuncType(this ж<Type> Ꮡt) {
+internal static ж<FuncType> FuncType⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() != Func) {
@@ -392,7 +373,7 @@ public static ж<FuncType> FuncType(this ж<Type> Ꮡt) {
     return Ꮡt.Reinterpret<Type, FuncType>();
 }
 
-public static ж<InterfaceType> InterfaceType(this ж<Type> Ꮡt) {
+internal static ж<InterfaceType> InterfaceType⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() != Interface) {
@@ -401,13 +382,11 @@ public static ж<InterfaceType> InterfaceType(this ж<Type> Ꮡt) {
     return Ꮡt.Reinterpret<Type, InterfaceType>();
 }
 
-public static uintptr Size(this ref Type t) { return t.Size_; }
+internal static uintptr Size⁔(/*this*/ ref Type t) { return t.Size_; }
 
-public static nint Align(this ref Type t) { return (nint)t.Align_; }
+internal static nint Align⁔(/*this*/ ref Type t) { return (nint)t.Align_; }
 
-public static nint FieldAlign(this ref Type t) { return (nint)t.FieldAlign_; }
-
-} partial class main_package {
+internal static nint FieldAlign⁔(/*this*/ ref Type t) { return (nint)t.FieldAlign_; }
 
 partial struct InterfaceType {
     /*embed*/ public Type Type;
@@ -415,9 +394,7 @@ partial struct InterfaceType {
     public slice<Imethod> Methods;
 }
 
-} partial class mainᴛType {
-
-public static slice<Method> ExportedMethods(this ж<Type> Ꮡt) {
+internal static slice<Method> ExportedMethods⁔(/*this*/ ж<Type> Ꮡt) {
     var ut = Ꮡt.Uncommon();
     if (ut == nil) {
         return default;
@@ -425,7 +402,7 @@ public static slice<Method> ExportedMethods(this ж<Type> Ꮡt) {
     return ut.ExportedMethods();
 }
 
-public static nint NumMethod(this ж<Type> Ꮡt) {
+internal static nint NumMethod⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() == Interface) {
@@ -435,8 +412,6 @@ public static nint NumMethod(this ж<Type> Ꮡt) {
     return len(Ꮡt.ExportedMethods());
 }
 
-} partial class main_package {
-
 public static nint NumMethod(this ref InterfaceType t) { return len(t.Methods); }
 
 partial struct MapType {
@@ -467,9 +442,7 @@ public static bool HashMightPanic(this ref MapType mt) {
     return (mt.Flags & 16) != 0;
 }
 
-} partial class mainᴛType {
-
-public static ж<Type> Key(this ж<Type> Ꮡt) {
+internal static ж<Type> Key⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() == Map) {
@@ -478,8 +451,6 @@ public static ж<Type> Key(this ж<Type> Ꮡt) {
     return default;
 }
 
-} partial class main_package {
-
 partial struct SliceType {
     /*embed*/ public Type Type;
     public ж<Type> Elem;
@@ -562,29 +533,27 @@ partial struct Name {
     public ж<byte> Bytes;
 }
 
-} partial class mainᴛName {
-
-public static ж<byte> DataChecked(this Name n, nint off, @string whySafe) {
+internal static ж<byte> DataChecked⁔(/*this*/ Name n, nint off, @string whySafe) {
     return @unsafe.Pointer.To<byte>(addChecked(@unsafe.Pointer.FromPinnedBox(n.Bytes), (uintptr)off, whySafe));
 }
 
-public static ж<byte> Data(this Name n, nint off) {
+internal static ж<byte> Data⁔(/*this*/ Name n, nint off) {
     return @unsafe.Pointer.To<byte>(addChecked(@unsafe.Pointer.FromPinnedBox(n.Bytes), (uintptr)off, "the runtime doesn't need to give you a reason"));
 }
 
-public static bool IsExported(this Name n) {
+internal static bool IsExported⁔(/*this*/ Name n) {
     return (byte)((n.Bytes.Value) & ((byte)(1 << 0))) != 0;
 }
 
-public static bool HasTag(this Name n) {
+internal static bool HasTag⁔(/*this*/ Name n) {
     return (byte)((n.Bytes.Value) & ((byte)(1 << 1))) != 0;
 }
 
-public static bool IsEmbedded(this Name n) {
+internal static bool IsEmbedded⁔(/*this*/ Name n) {
     return (byte)((n.Bytes.Value) & ((byte)(1 << 3))) != 0;
 }
 
-public static (nint, nint) ReadVarint(this Name n, nint off) {
+internal static (nint, nint) ReadVarint⁔(/*this*/ Name n, nint off) {
     nint v = 0;
     for (nint i = 0; ; i++) {
         var x = n.DataChecked(off + i, "read varint").Value;
@@ -595,7 +564,7 @@ public static (nint, nint) ReadVarint(this Name n, nint off) {
     }
 }
 
-public static bool IsBlank(this Name n) {
+internal static bool IsBlank⁔(/*this*/ Name n) {
     if (n.Bytes == nil) {
         return false;
     }
@@ -603,8 +572,6 @@ public static bool IsBlank(this Name n) {
     return l == 1 && n.Data(2).Value == '_';
 }
 
-} partial class main_package {
-
 internal static nint writeVarint(slice<byte> buf, nint n) {
     for (nint i = 0; ; i++) {
         var b = (byte)(n & 0x7f);
@@ -617,9 +584,7 @@ internal static nint writeVarint(slice<byte> buf, nint n) {
     }
 }
 
-} partial class mainᴛName {
-
-public static @string Name(this Name n) {
+internal static @string Name⁔(/*this*/ Name n) {
     if (n.Bytes == nil) {
         return "";
     }
@@ -627,7 +592,7 @@ public static @string Name(this Name n) {
     return @unsafe.String(n.DataChecked(1 + i, "non-empty string"), l);
 }
 
-public static @string Tag(this Name n) {
+internal static @string Tag⁔(/*this*/ Name n) {
     if (!n.HasTag()) {
         return "";
     }
@@ -636,8 +601,6 @@ public static @string Tag(this Name n) {
     return @unsafe.String(n.DataChecked(1 + i + l + i2, "non-empty string"), l2);
 }
 
-} partial class main_package {
-
 public static Name NewName(@string n, @string tag, bool exported, bool embedded) {
     if (len(n) >= 1 << 29) {
         throw panic("abi.NewName: name too long: " + n[..1024] + "...");
```

## UnsafePointerArgPassing

```diff
diff --git a/src/tests/Behavioral/UnsafePointerArgPassing/main.cs.target b/src/tests/Behavioral/UnsafePointerArgPassing/main.cs.target
--- a/src/tests/Behavioral/UnsafePointerArgPassing/main.cs.target
+++ b/src/tests/Behavioral/UnsafePointerArgPassing/main.cs.target
@@ -2,7 +2,6 @@ namespace go;
 
 using fmt = fmt_package;
 using @unsafe = unsafe_package;
-using static main_package;
 
 partial class main_package {
 
@@ -10,11 +9,7 @@ partial struct counter {
     internal nint n;
 }
 
-} partial class mainᴛcounter {
-
-internal static void add(this ref counter c, uintptr d) { c.n += (nint)d; }
-
-} partial class main_package {
+internal static void add⁔(/*this*/ ref counter c, uintptr d) { c.n += (nint)d; }
 
 internal static partial @unsafe.Pointer add(@unsafe.Pointer p, uintptr x) {
     return @unsafe.Add(p, x);
--- a/src/tests/Behavioral/UnsafePointerArgPassing/package_info.cs
+++ b/src/tests/Behavioral/UnsafePointerArgPassing/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -53,11 +56,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "AA4mtoKmgpKUgpaClIKCgg==")]
+[assembly: GoPositionMap("main.go", "main.cs", "AAsmloKmgpKUgpaClIKCgg==")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoPackage("main")]
 [GoTestMatchingConsoleOutput]
 public static partial class main_package
@@ -90,5 +95,15 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.counter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛcounter;
+[GoMethodHost(typeof(main_package.counter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛcounter;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛcounter {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void add(this ref counter c, uintptr d) => main_package.add⁔(ref c, d);
+}
+// </MethodForwarders>
```

## UnsafePointerReceiverCopy

```diff
diff --git a/src/tests/Behavioral/UnsafePointerReceiverCopy/main.cs.target b/src/tests/Behavioral/UnsafePointerReceiverCopy/main.cs.target
--- a/src/tests/Behavioral/UnsafePointerReceiverCopy/main.cs.target
+++ b/src/tests/Behavioral/UnsafePointerReceiverCopy/main.cs.target
@@ -2,7 +2,6 @@ namespace go;
 
 using fmt = fmt_package;
 using @unsafe = unsafe_package;
-using static main_package;
 
 partial class main_package {
 
@@ -19,54 +18,46 @@ internal static holder hold;
 partial struct q {
 }
 
-} partial class mainᴛT {
-
-internal static void q(this ref T h) { }
-
-} partial class main_package {
+internal static void q⁔(/*this*/ ref T h) { }
 
 internal static void poke(@unsafe.Pointer p, nint v) { (@unsafe.Pointer.To<nint>(p)).Value = v; }
 
-} partial class mainᴛT {
-
-internal static void viaReceiver(this ж<T> Ꮡh) {
+internal static void viaReceiver⁔(/*this*/ ж<T> Ꮡh) {
     ref var h = ref Ꮡh.DerefOrNull();
 
     poke((uintptr)@unsafe.Pointer.FromRef(ref h), 7);
 }
 
-internal static void viaLocal(this ж<T> Ꮡh) {
+internal static void viaLocal⁔(/*this*/ ж<T> Ꮡh) {
     var p = Ꮡh;
     poke(@unsafe.Pointer.FromPinnedBox(p), 8);
 }
 
-internal static void viaParam(this ref T h, ж<T> Ꮡother) {
+internal static void viaParam⁔(/*this*/ ref T h, ж<T> Ꮡother) {
     ref var other = ref Ꮡother.DerefOrNull();
 
     poke((uintptr)@unsafe.Pointer.FromRef(ref other), 9);
 }
 
-internal static void viaFolded(this ж<T> Ꮡh) {
+internal static void viaFolded⁔(/*this*/ ж<T> Ꮡh) {
     var p = Ꮡh;
     (p.Reinterpret<T, nint>()).Value = 10;
 }
 
-internal static void viaField(this ref T h) { poke(@unsafe.Pointer.FromPinnedBox(hold.p), 11); }
+internal static void viaField⁔(/*this*/ ref T h) { poke(@unsafe.Pointer.FromPinnedBox(hold.p), 11); }
 
-internal static void viaRenamedReceiver(this ж<T> Ꮡq) {
+internal static void viaRenamedReceiver⁔(/*this*/ ж<T> Ꮡq) {
     ref var q = ref Ꮡq.DerefOrNull();
 
     poke((uintptr)@unsafe.Pointer.FromRef(ref q), 12);
 }
 
-internal static void viaLocalChain(this ж<T> Ꮡh) {
+internal static void viaLocalChain⁔(/*this*/ ж<T> Ꮡh) {
     var p = Ꮡh;
     var r = p;
     poke(@unsafe.Pointer.FromPinnedBox(r), 13);
 }
 
-} partial class main_package {
-
 internal static void main() {
     ref var a = ref heap(new T(), out var Ꮡa);
     ref var b = ref heap(new T(), out var Ꮡb);
--- a/src/tests/Behavioral/UnsafePointerReceiverCopy/package_info.cs
+++ b/src/tests/Behavioral/UnsafePointerReceiverCopy/package_info.cs
@@ -14,6 +14,9 @@
 
 using go;
 using static go.main_package;
+using System.ComponentModel;
+using System.Diagnostics;
+using System.Runtime.CompilerServices;
 
 // For encountered type alias declarations, e.g., `type Table = map[string]int`,
 // go2cs code converter will generate a `global using` statement for the alias in
@@ -49,11 +52,13 @@ using static go.main_package;
 // or has none - golib, the BCL and hand-written conversions - and reports its own C# position.
 
 // <GoSourcePositionMaps>
-[assembly: GoPositionMap("main.go", "main.cs", "ABcotLagpoKCqKCmgoKolqCmgoKCxoLygoKCgoKCgoKUlA==")]
+[assembly: GoPositionMap("main.go", "main.cs", "ABQolJagpoKCqKCmgoKolqCmgoKCpoLygoKCgoKCgoKUlA==")]
 // </GoSourcePositionMaps>
 
 namespace go;
 
+using static go.main_package;
+
 [GoTestMatchingConsoleOutput]
 [GoPackage("main")]
 public static partial class main_package
@@ -88,5 +93,22 @@ public static partial class main_package
 // in Go's declaration order.
 
 // <MethodHolders>
-[GoMethodHost(typeof(main_package.T)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛT;
+[GoMethodHost(typeof(main_package.T)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛT;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛT {
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void q(this ref T h) => main_package.q⁔(ref h);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void viaReceiver(this ж<T> Ꮡh) => main_package.viaReceiver⁔(Ꮡh);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void viaLocal(this ж<T> Ꮡh) => main_package.viaLocal⁔(Ꮡh);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void viaParam(this ref T h, ж<T> Ꮡother) => main_package.viaParam⁔(ref h, Ꮡother);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void viaFolded(this ж<T> Ꮡh) => main_package.viaFolded⁔(Ꮡh);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void viaField(this ref T h) => main_package.viaField⁔(ref h);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void viaRenamedReceiver(this ж<T> Ꮡq) => main_package.viaRenamedReceiver⁔(Ꮡq);
+    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void viaLocalChain(this ж<T> Ꮡh) => main_package.viaLocalChain⁔(Ꮡh);
+}
+// </MethodForwarders>
```

