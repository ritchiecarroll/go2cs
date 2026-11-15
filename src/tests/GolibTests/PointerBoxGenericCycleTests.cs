// PointerBoxGenericCycleTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards the edge that closes Native AOT's generic cycle through the pointer box: no method of
/// <c>ж&lt;T&gt;</c>, or of a type nested in it, may name <c>ElemRefBox&lt;&gt;</c>.
/// </summary>
/// <remarks>
/// <para>
/// <c>ElemRefBox&lt;T&gt;</c> names <c>array&lt;T&gt;</c>, and <c>array&lt;T&gt;.AliasPointer</c> names
/// <c>ж&lt;array&lt;T&gt;&gt;</c>, which the converter's emitted calls need. So one mention of the element box from the
/// pointer box, even a type test, makes every <c>ж&lt;T&gt;</c> reach <c>ж&lt;array&lt;T&gt;&gt;</c>, and that one
/// <c>ж&lt;array&lt;array&lt;T&gt;&gt;&gt;</c>: a loop that grows one nesting level a lap. ILC expands it to its
/// cut-off (IL3054 lines) for every type a program touches. The cycle was broken once and closed again a week
/// later by a type test in the field-view lookup, and no gate saw it return, because only a Native AOT publish
/// shows it. This reads the IL instead, so it runs wherever GolibTests runs.
/// </para>
/// <para>
/// It guards this edge, not the absence of every cycle: a different growth path would pass here. The
/// publish-time count of IL3054 lines is the general reading.
/// </para>
/// </remarks>
[TestClass]
public class PointerBoxGenericCycleTests
{
    private static readonly OpCode[] s_oneByteOpCodes = new OpCode[0x100];
    private static readonly OpCode[] s_twoByteOpCodes = new OpCode[0x100];

    static PointerBoxGenericCycleTests()
    {
        foreach (FieldInfo field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.GetValue(null) is not OpCode opCode)
                continue;

            ushort value = unchecked((ushort)opCode.Value);

            if (opCode.Size == 1)
                s_oneByteOpCodes[value] = opCode;
            else
                s_twoByteOpCodes[value & 0xFF] = opCode;
        }
    }

    [TestMethod]
    public void ThePointerBoxNamesNoElementBoxInItsOwnMethods()
    {
        List<string> offenders = [];
        int methods = 0;

        foreach (Type type in WithNestedTypes(typeof(ж<>)))
        {
            foreach (MethodBase method in DeclaredMethods(type))
            {
                methods++;

                if (TypesNamedBy(method).Any(IsElementBox))
                    offenders.Add($"{type.Name}.{method.Name}");
            }
        }

        // A reader that finds no methods cannot find an offender either.
        Assert.IsTrue(methods > 50, $"read only {methods} methods of ж<T> and its nested types: the walk is not seeing the type");

        Assert.AreEqual(0, offenders.Count,
            $"ж<T> names ElemRefBox<> in: {string.Join(", ", offenders.Distinct())}. That one mention closes Native AOT's " +
            "generic cycle (ж<T> -> ElemRefBox<T> -> array<T> -> ж<array<T>> -> ...): ask a non-generic interface " +
            "the element box implements instead (IElementViewSlot is the existing one)");
    }

    [TestMethod]
    public void TheReaderSeesAnElementBoxTypeTestWhereThereIsOne()
    {
        // The positive control: the same reader over a method that DOES test for the element box.
        MethodInfo control = typeof(PointerBoxGenericCycleTests).GetMethod(nameof(TestsForTheElementBox), BindingFlags.NonPublic | BindingFlags.Static)!;

        Assert.IsTrue(TypesNamedBy(control).Any(IsElementBox),
            "the IL reader did not report ElemRefBox<> for a method whose body is `box is ElemRefBox<T>`: it cannot guard anything");
    }

    private static bool TestsForTheElementBox<T>(ж<T> box)
    {
        return box is ElemRefBox<T>;
    }

    private static bool IsElementBox(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ElemRefBox<>);
    }

    private static IEnumerable<Type> WithNestedTypes(Type type)
    {
        yield return type;

        foreach (Type nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
        {
            foreach (Type inner in WithNestedTypes(nested))
                yield return inner;
        }
    }

    private static IEnumerable<MethodBase> DeclaredMethods(Type type)
    {
        const BindingFlags Declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (MethodInfo method in type.GetMethods(Declared))
            yield return method;

        foreach (ConstructorInfo constructor in type.GetConstructors(Declared))
            yield return constructor;
    }

    // Every type a method names: in its signature, its locals, and the type, method and field tokens of its body,
    // each with the types nested in its instantiation.
    private static IEnumerable<Type> TypesNamedBy(MethodBase method)
    {
        List<Type> named = [];

        foreach (ParameterInfo parameter in method.GetParameters())
            named.Add(parameter.ParameterType);

        if (method is MethodInfo info)
            named.Add(info.ReturnType);

        MethodBody? body = method.GetMethodBody();

        if (body is not null)
        {
            foreach (LocalVariableInfo local in body.LocalVariables)
                named.Add(local.LocalType);

            Type[]? typeArguments = method.DeclaringType is { IsGenericType: true } declaring ? declaring.GetGenericArguments() : null;
            Type[]? methodArguments = method.IsGenericMethod ? method.GetGenericArguments() : null;

            foreach (int token in Tokens(body.GetILAsByteArray()!))
            {
                MemberInfo? member;

                try
                {
                    member = method.Module.ResolveMember(token, typeArguments, methodArguments);
                }
                catch (ArgumentException)
                {
                    // ldtoken of something that is not a member of this kind: not a type this method names
                    continue;
                }

                switch (member)
                {
                    case Type type:
                        named.Add(type);
                        break;
                    case MethodBase called:
                        if (called.DeclaringType is not null)
                            named.Add(called.DeclaringType);

                        if (called.IsGenericMethod)
                            named.AddRange(called.GetGenericArguments());

                        break;
                    case FieldInfo field:
                        if (field.DeclaringType is not null)
                            named.Add(field.DeclaringType);

                        named.Add(field.FieldType);
                        break;
                }
            }
        }

        return named.SelectMany(Flatten);
    }

    private static IEnumerable<Type> Flatten(Type type)
    {
        yield return type;

        if (type.HasElementType)
        {
            foreach (Type inner in Flatten(type.GetElementType()!))
                yield return inner;
        }

        if (type.IsGenericType)
        {
            foreach (Type argument in type.GetGenericArguments())
            {
                foreach (Type inner in Flatten(argument))
                    yield return inner;
            }
        }
    }

    // The metadata tokens of a method body's type, method, field and ldtoken operands.
    private static IEnumerable<int> Tokens(byte[] il)
    {
        int position = 0;

        while (position < il.Length)
        {
            byte first = il[position++];
            OpCode opCode = first == 0xFE ? s_twoByteOpCodes[il[position++]] : s_oneByteOpCodes[first];

            switch (opCode.OperandType)
            {
                case OperandType.InlineNone:
                    break;
                case OperandType.ShortInlineBrTarget:
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar:
                    position += 1;
                    break;
                case OperandType.InlineVar:
                    position += 2;
                    break;
                case OperandType.InlineI8:
                case OperandType.InlineR:
                    position += 8;
                    break;
                case OperandType.InlineSwitch:
                    position += 4 + 4 * BitConverter.ToInt32(il, position);
                    break;
                case OperandType.InlineField:
                case OperandType.InlineMethod:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                    yield return BitConverter.ToInt32(il, position);
                    position += 4;
                    break;
                default:
                    // InlineBrTarget, InlineI, InlineString, InlineSig, ShortInlineR
                    position += 4;
                    break;
            }
        }
    }
}
