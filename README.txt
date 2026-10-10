ILLink IL1012 (NullReferenceException) on a documentation-ID [DynamicDependency] whose method has a parameter
that is a NESTED type defined in ANOTHER assembly.

Versions
  .NET SDK                     10.0.400
  Microsoft.NET.ILLink.Tasks   10.0.11   (implicit, via PublishTrimmed; from obj/project.assets.json)
  RID                          win-x64   (Windows 11 x64)

Layout (three projects, nothing else)
  LibA/   namespace A: public static class Outer { public class Nested { } }   and   public class Flat { }
  LibB/   references LibA. namespace B: Local.Nested (nested, same assembly) and
          public static class P { M(A.Outer.Nested), F(A.Flat), L(B.Local.Nested) }
  App/    references LibB. PublishTrimmed=true, TrimMode=partial.
          [DynamicDependency("M(A.Outer.Nested)", typeof(B.P))] on Main.

Reproduce (from this directory)
  dotnet publish App/App.csproj -c Release -r win-x64

Result: exit code 1
  ILLink : error IL1012: IL Trimmer has encountered an unexpected error. Please report the issue at https://aka.ms/report-illink [App/App.csproj]
    Fatal error in IL Linker
    Unhandled exception. System.NullReferenceException: Object reference not set to an instance of an object.
       at Mono.Linker.DocumentationSignatureGenerator.PartVisitor.VisitTypeReference(TypeReference typeReference, StringBuilder builder, ITryResolveMetadata resolver)
       at Mono.Linker.DocumentationSignatureGenerator.PartVisitor.VisitTypeReference(TypeReference typeReference, StringBuilder builder, ITryResolveMetadata resolver)
       at Mono.Linker.DocumentationSignatureParser.GetSignaturePart(TypeReference type, ITryResolveMetadata resolver)
       at Mono.Linker.DocumentationSignatureParser.AllParametersMatch(Collection`1 methodParameters, List`1 expectedParameters, ITryResolveMetadata resolver)
       at Mono.Linker.DocumentationSignatureParser.GetMatchingMethods(String id, Int32& index, TypeDefinition type, String memberName, Int32 arity, List`1 results, ITryResolveMetadata resolver, Boolean acceptName)
       at Mono.Linker.DocumentationSignatureParser.GetMatchingMembers(...)
       at Mono.Linker.DocumentationSignatureParser.GetMembersByDocumentationSignature(TypeDefinition type, String signature, ITryResolveMetadata resolver, Boolean acceptName)
       at Mono.Linker.Steps.MarkStep.MarkDynamicDependency(DynamicDependency dynamicDependency, IMemberDefinition context, MessageOrigin origin)
       at Mono.Linker.Steps.MarkStep.MarkCustomAttributes(ICustomAttributeProvider provider, DependencyInfo& reason, MessageOrigin origin)
       at Mono.Linker.Steps.MarkStep.ProcessMethod(MethodDefinition method, DependencyInfo& reason)
  error NETSDK1144: Optimizing assemblies for size failed.

Controls (edit the attribute string in App/Program.cs, or TrimMode in App/App.csproj, and re-run the command)
  "M(A.Outer.Nested)"   TrimMode=partial   -> IL1012
  "M(A.Outer.Nested)"   TrimMode=full      -> IL1012
  "M"                   TrimMode=partial   -> succeeds  (no parameter list: parameters are never visited)
  "F(A.Flat)"           TrimMode=partial   -> succeeds  (cross-assembly type, NOT nested)
  "L(B.Local.Nested)"   TrimMode=partial   -> succeeds  (nested type in the SAME assembly as the method)
Native AOT's ILC (PublishAot) accepts the same attribute.

Reading: the trigger is a candidate method parameter that is a nested type from another assembly. The doubled
VisitTypeReference frame fits the nested-type branch visiting its declaring type, with
GetInflatedDeclaringType(resolver) returning null for the cross-assembly reference. That is an inference from the
stack, not verified in the ILLink source.
