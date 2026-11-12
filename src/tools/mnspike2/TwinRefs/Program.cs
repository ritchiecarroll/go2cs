//******************************************************************************************************
//  Program.cs - M:N PROTOTYPE (A5). Prototype code: nothing from this branch lands on master as it is.
//
//  Reads the member references of built assemblies and prints the ones a named assembly owns (golib by
//  default): the members a caller's IL is bound to, read from its metadata and not listed by hand.
//
//  Usage: TwinRefs [--owner NAME] [--defs] ASSEMBLY...
//  One row per member reference: R, the reading assembly, the owning type, the member and its signature,
//  tab separated. One S row per assembly: its member reference count, the rows the owner holds, the rest.
//  With --defs, one D row per method and field an assembly DEFINES instead, in the same spelling, with
//  public or nonpublic last: public only where the member and every type around it is public.
//******************************************************************************************************

using System;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

Console.OutputEncoding = new System.Text.UTF8Encoding(false);

string owner = "golib";
int first = 0;

bool defs = false;

if (args.Length >= 2 && args[0] == "--owner")
{
    owner = args[1];
    first = 2;
}

if (args.Length > first && args[first] == "--defs")
{
    defs = true;
    first++;
}

if (args.Length <= first)
{
    Console.Error.WriteLine("usage: TwinRefs [--owner NAME] [--defs] ASSEMBLY...");
    return 2;
}

for (int i = first; i < args.Length; i++)
{
    using FileStream stream = File.OpenRead(args[i]);
    using PEReader pe = new(stream);

    if (!pe.HasMetadata)
    {
        Console.WriteLine($"S\t{Path.GetFileName(args[i])}\tno-metadata");
        continue;
    }

    MetadataReader md = pe.GetMetadataReader();
    string reading = md.GetString(md.GetAssemblyDefinition().Name);
    Names names = new(md);
    int owned = 0, other = 0;

    if (defs)
    {
        foreach (TypeDefinitionHandle typeHandle in md.TypeDefinitions)
        {
            TypeDefinition type = md.GetTypeDefinition(typeHandle);
            string typeName = names.GetTypeFromDefinition(md, typeHandle, 0);
            bool open = names.IsPublic(typeHandle);

            foreach (MethodDefinitionHandle methodHandle in type.GetMethods())
            {
                MethodDefinition method = md.GetMethodDefinition(methodHandle);
                bool isPublic = open && (method.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;
                Console.WriteLine($"D\t{reading}\t{typeName}\t{md.GetString(method.Name)}\t{Names.Spell(method.DecodeSignature(names, null))}\t{(isPublic ? "public" : "nonpublic")}");
                owned++;
            }

            foreach (FieldDefinitionHandle fieldHandle in type.GetFields())
            {
                FieldDefinition field = md.GetFieldDefinition(fieldHandle);
                bool isPublic = open && (field.Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public;
                Console.WriteLine($"D\t{reading}\t{typeName}\t{md.GetString(field.Name)}\tfield {field.DecodeSignature(names, null)}\t{(isPublic ? "public" : "nonpublic")}");
                owned++;
            }
        }

        Console.WriteLine($"S\t{reading}\tdefinitions={owned}");
        continue;
    }

    foreach (MemberReferenceHandle handle in md.MemberReferences)
    {
        MemberReference member = md.GetMemberReference(handle);
        (string scope, string type) = names.Parent(member.Parent);

        if (scope != owner)
        {
            other++;
            continue;
        }

        owned++;
        string name = md.GetString(member.Name);
        string signature;

        if (member.GetKind() == MemberReferenceKind.Field)
        {
            signature = "field " + member.DecodeFieldSignature(names, null);
        }
        else
        {
            signature = Names.Spell(member.DecodeMethodSignature(names, null));
        }

        Console.WriteLine($"R\t{reading}\t{type}\t{name}\t{signature}");
    }

    Console.WriteLine($"S\t{reading}\tmember-refs={md.MemberReferences.Count}\t{owner}={owned}\tother={other}");
}

return 0;

// Names of types as a signature spells them, and the assembly that owns a member reference's parent.
internal sealed class Names(MetadataReader md) : ISignatureTypeProvider<string, object?>
{
    // The assembly and the open type a member reference hangs from. A generic instantiation is read down to
    // its definition: the row is about which member is bound, not about which instantiation binds it.
    public (string Scope, string Type) Parent(EntityHandle parent)
    {
        switch (parent.Kind)
        {
            case HandleKind.TypeReference:
                return (Scope((TypeReferenceHandle)parent), TypeName((TypeReferenceHandle)parent));
            case HandleKind.TypeDefinition:
                return ("<self>", TypeName((TypeDefinitionHandle)parent));
            case HandleKind.TypeSpecification:
            {
                BlobReader blob = md.GetBlobReader(md.GetTypeSpecification((TypeSpecificationHandle)parent).Signature);
                SignatureTypeCode code = blob.ReadSignatureTypeCode();

                if (code == SignatureTypeCode.GenericTypeInstance)
                {
                    blob.ReadSignatureTypeCode();
                    return Parent((EntityHandle)blob.ReadTypeHandle());
                }

                return ("<spec>", code.ToString());
            }
            case HandleKind.MethodDefinition:
                return ("<self>", "<method>");
            case HandleKind.ModuleReference:
                return ("<module>", "<module>");
            default:
                return ("<unknown>", parent.Kind.ToString());
        }
    }

    // A method signature as a row spells it.
    public static string Spell(MethodSignature<string> method)
    {
        string generic = method.GenericParameterCount > 0 ? "`" + method.GenericParameterCount : "";
        string instance = method.Header.IsInstance ? "" : "static ";
        return $"{instance}{method.ReturnType} {generic}({string.Join(", ", method.ParameterTypes)})";
    }

    // Whether a type, and every type it is nested in, is public.
    public bool IsPublic(TypeDefinitionHandle handle)
    {
        TypeDefinition type = md.GetTypeDefinition(handle);
        TypeAttributes visibility = type.Attributes & TypeAttributes.VisibilityMask;
        TypeDefinitionHandle outer = type.GetDeclaringType();

        if (outer.IsNil)
            return visibility == TypeAttributes.Public;

        return visibility == TypeAttributes.NestedPublic && IsPublic(outer);
    }

    private string Scope(TypeReferenceHandle handle)
    {
        TypeReference type = md.GetTypeReference(handle);

        return type.ResolutionScope.Kind switch
        {
            HandleKind.AssemblyReference => md.GetString(md.GetAssemblyReference((AssemblyReferenceHandle)type.ResolutionScope).Name),
            HandleKind.TypeReference => Scope((TypeReferenceHandle)type.ResolutionScope),
            _ => "<" + type.ResolutionScope.Kind + ">"
        };
    }

    private string TypeName(TypeReferenceHandle handle)
    {
        TypeReference type = md.GetTypeReference(handle);
        string name = md.GetString(type.Name);

        if (type.ResolutionScope.Kind == HandleKind.TypeReference)
            return TypeName((TypeReferenceHandle)type.ResolutionScope) + "/" + name;

        string ns = md.GetString(type.Namespace);
        return ns.Length == 0 ? name : ns + "." + name;
    }

    private string TypeName(TypeDefinitionHandle handle)
    {
        TypeDefinition type = md.GetTypeDefinition(handle);
        string name = md.GetString(type.Name);
        TypeDefinitionHandle outer = type.GetDeclaringType();

        if (!outer.IsNil)
            return TypeName(outer) + "/" + name;

        string ns = md.GetString(type.Namespace);
        return ns.Length == 0 ? name : ns + "." + name;
    }

    public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode.ToString();

    public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) => TypeName(handle);

    public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) => TypeName(handle);

    public string GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind) =>
        reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);

    public string GetSZArrayType(string elementType) => elementType + "[]";

    public string GetArrayType(string elementType, ArrayShape shape) => elementType + "[" + new string(',', shape.Rank - 1) + "]";

    public string GetByReferenceType(string elementType) => "ref " + elementType;

    public string GetPointerType(string elementType) => elementType + "*";

    public string GetPinnedType(string elementType) => elementType;

    public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments) =>
        genericType + "<" + string.Join(", ", typeArguments) + ">";

    public string GetGenericTypeParameter(object? genericContext, int index) => "!" + index;

    public string GetGenericMethodParameter(object? genericContext, int index) => "!!" + index;

    public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => unmodifiedType;

    public string GetFunctionPointerType(MethodSignature<string> signature) =>
        "fnptr " + signature.ReturnType + " (" + string.Join(", ", signature.ParameterTypes) + ")";
}
