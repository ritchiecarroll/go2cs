using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

/// <summary>
/// M:N row 11 (N10): a multiplexed goroutine that parks on one carrier and resumes on another must read its OWN
/// goroutine-scoped thread-statics there, not the second carrier's. DATA-mn-spike-2.md N10 names them: outside golib,
/// getg's cache and its owner, the held runtime locks and their count, the pending lock-profile flag,
/// and debug.SetPanicOnFault; in golib, the allocation tally and the Gosched backoff streak (6.6). N10 also named the
/// sleep timer; it stays a carrier's resource, because only the blocking Sleep path uses it and a goroutine blocked there
/// stays mounted (Sleepꞏtwin makes its own timer), so it is classed KeptThreadResource in ThreadStateCensusTests.
/// </summary>
/// <remarks>
/// Each row mounts ONE goroutine on thread 1, writes the class's fields there, unmounts it, then mounts it on thread 2
/// and reads them back, which is what a park on one carrier and a resume on another does. Go reads the goroutine's
/// values on thread 2, and each carrier holds the defaults while no goroutine is mounted on it (the invariant
/// Goroutine.SwapLogicalState keeps for golib's own mounted state). The fields are private thread-statics of packages
/// above golib, so the rows read and write them by reflection; a [ThreadStatic] field's value is the calling thread's.
/// </remarks>
[TestClass]
public class MountedThreadStateTests
{
    // Each goroutine-scoped field class: its declaring type and its fields.
    private static readonly Dictionary<string, (Type Type, string[] Fields)> s_classes = new()
    {
        ["getg cache"] = (typeof(runtime_package), ["t_getg", "t_getgOwner"]),
        ["held runtime locks"] = (typeof(runtime_package), ["t_heldLocks", "t_heldCount"]),
        ["lock-profile pending"] = (typeof(runtime_package), ["t_runtimeLockProfilePending"]),
        ["panic on fault"] = (typeof(go.runtime.debug_package), ["t_panicOnFault"]),
        ["allocation tally"] = (typeof(AllocationCounter), ["t_count"]),
        ["Gosched streak"] = (typeof(GoschedBackoff), ["t_consecutiveInertYields"]),
    };

    // A value that is not the field's default: true, 7, a one-element array, a real box for the abstract ж<T> (golib's
    // own `new`), or an uninitialized instance.
    private static object Sample(Type type) =>
        type == typeof(bool) ? true :
        type == typeof(int) ? 7 :
        type == typeof(long) ? 7L :
        type.IsArray ? Array.CreateInstance(type.GetElementType()!, 1) :
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ж<>) ? NewBox(type.GetGenericArguments()[0]) :
        RuntimeHelpers.GetUninitializedObject(Nullable.GetUnderlyingType(type) ?? type);

    private static object NewBox(Type element) =>
        typeof(builtin).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method => method.Name == "new" && method.IsGenericMethodDefinition && method.GetParameters().Length == 0)
            .MakeGenericMethod(element).Invoke(null, null)!;

    private static object? Default(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;

    private static bool Same(object? expected, object? actual) =>
        expected is null ? actual is null : expected.GetType().IsValueType ? expected.Equals(actual) : ReferenceEquals(expected, actual);

    private static void OnThread(Action action)
    {
        Exception? failure = null;
        Thread thread = new(() => { try { action(); } catch (Exception ex) { failure = ex; } });
        thread.Start();
        thread.Join();

        if (failure is not null)
            throw new AssertFailedException($"the thread's work threw: {failure}", failure);
    }

    [TestMethod]
    [DataRow("getg cache")]
    [DataRow("held runtime locks")]
    [DataRow("lock-profile pending")]
    [DataRow("panic on fault")]
    [DataRow("allocation tally")]
    [DataRow("Gosched streak")]
    public void AGoroutineReadsItsOwnValuesOnTheNextCarrier(string fieldClass)
    {
        (Type type, string[] names) = s_classes[fieldClass];
        FieldInfo[] fields = names.Select(name => type.GetField(name, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new AssertFailedException($"{fieldClass}: {type.Name}.{name} is gone; the census owes this row its new name")).ToArray();

        foreach (FieldInfo field in fields)
            Assert.IsTrue(field.IsDefined(typeof(ThreadStaticAttribute)), $"{field.Name} is not a [ThreadStatic]: this row reads nothing");

        object[] samples = fields.Select(field => Sample(field.FieldType)).ToArray();
        Goroutine goroutine = Goroutine.NewMultiplexedForTest();
        object?[] leftOnFirst = new object?[fields.Length], readOnSecond = new object?[fields.Length], leftOnSecond = new object?[fields.Length];

        // Thread 1: the goroutine runs a step, writes its fields and parks.
        OnThread(() =>
        {
            goroutine.Mount();

            for (int i = 0; i < fields.Length; i++)
                fields[i].SetValue(null, samples[i]);

            goroutine.Unmount();

            for (int i = 0; i < fields.Length; i++)
            {
                leftOnFirst[i] = fields[i].GetValue(null);
                fields[i].SetValue(null, Default(fields[i].FieldType));
            }
        });

        // Thread 2: the goroutine resumes there.
        OnThread(() =>
        {
            goroutine.Mount();

            for (int i = 0; i < fields.Length; i++)
                readOnSecond[i] = fields[i].GetValue(null);

            goroutine.Unmount();

            for (int i = 0; i < fields.Length; i++)
            {
                leftOnSecond[i] = fields[i].GetValue(null);
                fields[i].SetValue(null, Default(fields[i].FieldType));
            }
        });

        List<string> wrong = [];

        for (int i = 0; i < fields.Length; i++)
        {
            object? zero = Default(fields[i].FieldType);

            if (!Same(samples[i], readOnSecond[i]))
                wrong.Add($"{fields[i].Name}: the next carrier read {readOnSecond[i] ?? "null"}, not the goroutine's value");

            if (!Same(zero, leftOnFirst[i]))
                wrong.Add($"{fields[i].Name}: the first carrier kept the goroutine's value after it parked");

            if (!Same(zero, leftOnSecond[i]))
                wrong.Add($"{fields[i].Name}: the second carrier kept the goroutine's value after it parked");
        }

        Assert.AreEqual(0, wrong.Count, $"{fieldClass}:\n{string.Join("\n", wrong)}");
    }

    // The packages' swaps register from their types' static initialization; these rows need them registered.
    private static void EnsurePackageSwapsRegistered()
    {
        RuntimeHelpers.RunClassConstructor(typeof(runtime_package).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(go.runtime.debug_package).TypeHandle);
        Assert.IsTrue(GoroutineThreadState.MountedSwapCountForTest >= 3, "the runtime and debug swaps are registered");
    }

    private static readonly FieldInfo s_panicOnFault = typeof(go.runtime.debug_package).GetField("t_panicOnFault", BindingFlags.NonPublic | BindingFlags.Static)!;

    [TestMethod]
    public void AnAllDefaultGoroutineAllocatesNoSavedSlots()
    {
        // P3: "all defaults allocates nothing" had no row; a swap that stored a holder eagerly would make every
        // multiplexed goroutine allocate the array at its first park, and nothing would read red.
        EnsurePackageSwapsRegistered();
        Goroutine goroutine = Goroutine.NewMultiplexedForTest();

        OnThread(() =>
        {
            goroutine.Mount();
            goroutine.Unmount();
        });

        Assert.IsNull(goroutine.SavedRegisteredForTest, "a goroutine whose goroutine-scoped state is all defaults keeps no saved slots");
    }

    // A thread-static of this test's own, moved by a swap the growth row registers once, between a mount and its unmount.
    [ThreadStatic] private static int t_probe;

    private static readonly Lazy<bool> s_probeRegistered = new(() => GoroutineThreadState.RegisterMounted(static (ref object? saved) =>
    {
        int held = saved is int value ? value : 0;
        saved = t_probe == 0 ? null : t_probe;
        t_probe = held;
    }));

    [TestMethod]
    public void ARegistrationBetweenAMountAndItsUnmountGetsASlotAtTheUnmount()
    {
        // P3: the growth branch of GoroutineThreadState.SwapMounted (a package that initializes during a step registers
        // after the goroutine's mount, so its slot exists only from the unmount on).
        EnsurePackageSwapsRegistered();
        Goroutine goroutine = Goroutine.NewMultiplexedForTest();

        // First park, with a value set, so the goroutine HAS saved slots before the late registration.
        OnThread(() =>
        {
            goroutine.Mount();
            s_panicOnFault.SetValue(null, true);
            goroutine.Unmount();
        });

        object?[] before = goroutine.SavedRegisteredForTest ?? throw new AssertFailedException("the first park stored the flag, so the slots exist");
        int beforeLength = before.Length;

        // Second step: the package registers WHILE the goroutine is mounted, and the step sets its field.
        OnThread(() =>
        {
            goroutine.Mount();
            _ = s_probeRegistered.Value;
            t_probe = 41;
            goroutine.Unmount();
            Assert.AreEqual(0, t_probe, "the carrier is left with the default");
        });

        object?[] after = goroutine.SavedRegisteredForTest!;
        Assert.IsTrue(after.Length > beforeLength, $"the slots grew at the unmount ({beforeLength} -> {after.Length})");

        // Third step, on another carrier: the late-registered field and the earlier one are both the goroutine's.
        int probe = 0;
        bool flag = false;

        OnThread(() =>
        {
            goroutine.Mount();
            probe = t_probe;
            flag = (bool)s_panicOnFault.GetValue(null)!;
            goroutine.Unmount();
        });

        Assert.AreEqual(41, probe, "the field registered between a mount and its unmount moved with the goroutine");
        Assert.IsTrue(flag, "the earlier slot survived the growth");
    }
}
