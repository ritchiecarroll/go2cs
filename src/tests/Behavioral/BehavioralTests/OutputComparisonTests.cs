// OutputComparisonTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BehavioralTests;

[TestClass]
public class D4_OutputComparisonTests : BehavioralTestBase
{
    [ClassInitialize]
    public static void Initialize(TestContext context) => Init(context);

    // Run "UpdateTestTargets" utility to add new project test methods below this line
    // Only projects marked with "GoTestMatchingConsoleOutput" attribute will be added

    // <TestMethods>

    [TestMethod]
    public void CheckAdapterNameInterfaceCollision() => CheckTarget("AdapterNameInterfaceCollision");

    [TestMethod]
    public void CheckAddressOfParamWrite() => CheckTarget("AddressOfParamWrite");

    [TestMethod]
    public void CheckAliasStructComposite() => CheckTarget("AliasStructComposite");

    [TestMethod]
    public void CheckAndNotAssignNarrow() => CheckTarget("AndNotAssignNarrow");

    [TestMethod]
    public void CheckAnonIfaceMethodSetWidening() => CheckTarget("AnonIfaceMethodSetWidening");

    [TestMethod]
    public void CheckAnonIfaceThroughPointerAdapter() => CheckTarget("AnonIfaceThroughPointerAdapter");

    [TestMethod]
    public void CheckAnonInterfaceConversion() => CheckTarget("AnonInterfaceConversion");

    [TestMethod]
    public void CheckAnonInterfaceCrossFile() => CheckTarget("AnonInterfaceCrossFile");

    [TestMethod]
    public void CheckAnonInterfaceSignatureAssert() => CheckTarget("AnonInterfaceSignatureAssert");

    [TestMethod]
    public void CheckAnonInterfaceVarWitness() => CheckTarget("AnonInterfaceVarWitness");

    [TestMethod]
    public void CheckAnonStructArrayElement() => CheckTarget("AnonStructArrayElement");

    [TestMethod]
    public void CheckAnonStructAssertLiftDedupe() => CheckTarget("AnonStructAssertLiftDedupe");

    [TestMethod]
    public void CheckAnonStructComposedTypes() => CheckTarget("AnonStructComposedTypes");

    [TestMethod]
    public void CheckAnonStructCrossFile() => CheckTarget("AnonStructCrossFile");

    [TestMethod]
    public void CheckAnonymousInterfaces() => CheckTarget("AnonymousInterfaces");

    [TestMethod]
    public void CheckAnonymousStructs() => CheckTarget("AnonymousStructs");

    [TestMethod]
    public void CheckAnyBoxedUntypedConst() => CheckTarget("AnyBoxedUntypedConst");

    [TestMethod]
    public void CheckAnyKeyMap() => CheckTarget("AnyKeyMap");

    [TestMethod]
    public void CheckAnyStringLitAssign() => CheckTarget("AnyStringLitAssign");

    [TestMethod]
    public void CheckAnyStringLitChanSend() => CheckTarget("AnyStringLitChanSend");

    [TestMethod]
    public void CheckAnyStringLitComposite() => CheckTarget("AnyStringLitComposite");

    [TestMethod]
    public void CheckAppendNamedSliceElement() => CheckTarget("AppendNamedSliceElement");

    [TestMethod]
    public void CheckAppendNilSliceElement() => CheckTarget("AppendNilSliceElement");

    [TestMethod]
    public void CheckAppendUntypedConst() => CheckTarget("AppendUntypedConst");

    [TestMethod]
    public void CheckArrayCastDerefClone() => CheckTarget("ArrayCastDerefClone");

    [TestMethod]
    public void CheckArrayLiteralDeclaredLength() => CheckTarget("ArrayLiteralDeclaredLength");

    [TestMethod]
    public void CheckArrayOfCrossPackageType() => CheckTarget("ArrayOfCrossPackageType");

    [TestMethod]
    public void CheckArrayPassByValue() => CheckTarget("ArrayPassByValue");

    [TestMethod]
    public void CheckArrayPointerElementAlias() => CheckTarget("ArrayPointerElementAlias");

    [TestMethod]
    public void CheckArrayRangeSnapshot() => CheckTarget("ArrayRangeSnapshot");

    [TestMethod]
    public void CheckArrayValueCopySites() => CheckTarget("ArrayValueCopySites");

    [TestMethod]
    public void CheckArrayWideIndexAddress() => CheckTarget("ArrayWideIndexAddress");

    [TestMethod]
    public void CheckAssignThroughTypeAssert() => CheckTarget("AssignThroughTypeAssert");

    [TestMethod]
    public void CheckAtomicFieldThroughPointer() => CheckTarget("AtomicFieldThroughPointer");

    [TestMethod]
    public void CheckAtomicPointerToNil() => CheckTarget("AtomicPointerToNil");

    [TestMethod]
    public void CheckAtomicValue() => CheckTarget("AtomicValue");

    [TestMethod]
    public void CheckAtomicValueTypedNilFunc() => CheckTarget("AtomicValueTypedNilFunc");

    [TestMethod]
    public void CheckAtomicValues() => CheckTarget("AtomicValues");

    [TestMethod]
    public void CheckBclTypeNameShadow() => CheckTarget("BclTypeNameShadow");

    [TestMethod]
    public void CheckBigUntypedConstComparison() => CheckTarget("BigUntypedConstComparison");

    [TestMethod]
    public void CheckBitwiseUntypedConst() => CheckTarget("BitwiseUntypedConst");

    [TestMethod]
    public void CheckBlankIdentifierCollision() => CheckTarget("BlankIdentifierCollision");

    [TestMethod]
    public void CheckBlankImportSideEffects() => CheckTarget("BlankImportSideEffects");

    [TestMethod]
    public void CheckBlankMultiResult() => CheckTarget("BlankMultiResult");

    [TestMethod]
    public void CheckBlankNamedReturn() => CheckTarget("BlankNamedReturn");

    [TestMethod]
    public void CheckBoxedMapFieldWrite() => CheckTarget("BoxedMapFieldWrite");

    [TestMethod]
    public void CheckBuiltinShadowLocal() => CheckTarget("BuiltinShadowLocal");

    [TestMethod]
    public void CheckByteSliceStringVerbs() => CheckTarget("ByteSliceStringVerbs");

    [TestMethod]
    public void CheckByteTableStringConst() => CheckTarget("ByteTableStringConst");

    [TestMethod]
    public void CheckByteTableStringVar() => CheckTarget("ByteTableStringVar");

    [TestMethod]
    public void CheckCanonicalTypeIdentity() => CheckTarget("CanonicalTypeIdentity");

    [TestMethod]
    public void CheckCaptureHoistThroughConversion() => CheckTarget("CaptureHoistThroughConversion");

    [TestMethod]
    public void CheckCaptureModeFieldAddress() => CheckTarget("CaptureModeFieldAddress");

    [TestMethod]
    public void CheckCaptureModeFuncLitParam() => CheckTarget("CaptureModeFuncLitParam");

    [TestMethod]
    public void CheckCaptureModeParamClosure() => CheckTarget("CaptureModeParamClosure");

    [TestMethod]
    public void CheckCaptureModeValueParam() => CheckTarget("CaptureModeValueParam");

    [TestMethod]
    public void CheckCaptureModeValueParamUser() => CheckTarget("CaptureModeValueParamUser");

    [TestMethod]
    public void CheckCastNegativeNamedType() => CheckTarget("CastNegativeNamedType");

    [TestMethod]
    public void CheckChanDirectionChain() => CheckTarget("ChanDirectionChain");

    [TestMethod]
    public void CheckChannelCapLen() => CheckTarget("ChannelCapLen");

    [TestMethod]
    public void CheckChannelReceiveFromClosed() => CheckTarget("ChannelReceiveFromClosed");

    [TestMethod]
    public void CheckChannelRendezvous() => CheckTarget("ChannelRendezvous");

    [TestMethod]
    public void CheckClearBuiltinShadow() => CheckTarget("ClearBuiltinShadow");

    [TestMethod]
    public void CheckCloseWakesBlocked() => CheckTarget("CloseWakesBlocked");

    [TestMethod]
    public void CheckClosureBareReturnNamedResults() => CheckTarget("ClosureBareReturnNamedResults");

    [TestMethod]
    public void CheckClosureCapturedPointerAddress() => CheckTarget("ClosureCapturedPointerAddress");

    [TestMethod]
    public void CheckClosureDefer() => CheckTarget("ClosureDefer");

    [TestMethod]
    public void CheckClosureEmbeddedPromotedPtrMethod() => CheckTarget("ClosureEmbeddedPromotedPtrMethod");

    [TestMethod]
    public void CheckClosureLocalNoHeapBox() => CheckTarget("ClosureLocalNoHeapBox");

    [TestMethod]
    public void CheckClosureMixedReturnUnsigned() => CheckTarget("ClosureMixedReturnUnsigned");

    [TestMethod]
    public void CheckClosureParamShadow() => CheckTarget("ClosureParamShadow");

    [TestMethod]
    public void CheckClosurePtrLocalFieldMethod() => CheckTarget("ClosurePtrLocalFieldMethod");

    [TestMethod]
    public void CheckClosureReassignsPtrParam() => CheckTarget("ClosureReassignsPtrParam");

    [TestMethod]
    public void CheckClosureReturnAnonStruct() => CheckTarget("ClosureReturnAnonStruct");

    [TestMethod]
    public void CheckClosureSelfShadowCapture() => CheckTarget("ClosureSelfShadowCapture");

    [TestMethod]
    public void CheckClosureWriteVisibility() => CheckTarget("ClosureWriteVisibility");

    [TestMethod]
    public void CheckCollidingPackageNames() => CheckTarget("CollidingPackageNames");

    [TestMethod]
    public void CheckCollisionFieldBoxAccessor() => CheckTarget("CollisionFieldBoxAccessor");

    [TestMethod]
    public void CheckCollisionRenamedLocalBox() => CheckTarget("CollisionRenamedLocalBox");

    [TestMethod]
    public void CheckCombinedStructFields() => CheckTarget("CombinedStructFields");

    [TestMethod]
    public void CheckComplexConstContext() => CheckTarget("ComplexConstContext");

    [TestMethod]
    public void CheckComplexFormat() => CheckTarget("ComplexFormat");

    [TestMethod]
    public void CheckComplexImaginaryShadow() => CheckTarget("ComplexImaginaryShadow");

    [TestMethod]
    public void CheckCompositeElementStringConcat() => CheckTarget("CompositeElementStringConcat");

    [TestMethod]
    public void CheckCompositeLiteralElements() => CheckTarget("CompositeLiteralElements");

    [TestMethod]
    public void CheckConstShadowsParam() => CheckTarget("ConstShadowsParam");

    [TestMethod]
    public void CheckConstSubexprOverflow() => CheckTarget("ConstSubexprOverflow");

    [TestMethod]
    public void CheckConstrainedSliceParamInPlace() => CheckTarget("ConstrainedSliceParamInPlace");

    [TestMethod]
    public void CheckConstraintProxyEmbeddedInterface() => CheckTarget("ConstraintProxyEmbeddedInterface");

    [TestMethod]
    public void CheckCrossPackageArrayZeroValue() => CheckTarget("CrossPackageArrayZeroValue");

    [TestMethod]
    public void CheckCrossPackagePointerReceiverVar() => CheckTarget("CrossPackagePointerReceiverVar");

    [TestMethod]
    public void CheckCrossPkgLiteralNestedField() => CheckTarget("CrossPkgLiteralNestedField");

    [TestMethod]
    public void CheckCrossPkgUser() => CheckTarget("CrossPkgUser");

    [TestMethod]
    public void CheckCtorFieldInitializerOmitted() => CheckTarget("CtorFieldInitializerOmitted");

    [TestMethod]
    public void CheckDeadPointerParamAlias() => CheckTarget("DeadPointerParamAlias");

    [TestMethod]
    public void CheckDeepEqual() => CheckTarget("DeepEqual");

    [TestMethod]
    public void CheckDeepSelectRecursion() => CheckTarget("DeepSelectRecursion");

    [TestMethod]
    public void CheckDeferArgEnclosingCapture() => CheckTarget("DeferArgEnclosingCapture");

    [TestMethod]
    public void CheckDeferCallOrder() => CheckTarget("DeferCallOrder");

    [TestMethod]
    public void CheckDeferClosure() => CheckTarget("DeferClosure");

    [TestMethod]
    public void CheckDeferDiscardedMultiValue() => CheckTarget("DeferDiscardedMultiValue");

    [TestMethod]
    public void CheckDeferEvalParam() => CheckTarget("DeferEvalParam");

    [TestMethod]
    public void CheckDeferEvalParamFunc() => CheckTarget("DeferEvalParamFunc");

    [TestMethod]
    public void CheckDeferFinallyLowering() => CheckTarget("DeferFinallyLowering");

    [TestMethod]
    public void CheckDeferFrameScopes() => CheckTarget("DeferFrameScopes");

    [TestMethod]
    public void CheckDeferHeapFieldPtrMethod() => CheckTarget("DeferHeapFieldPtrMethod");

    [TestMethod]
    public void CheckDeferHeapLocalPtrMethod() => CheckTarget("DeferHeapLocalPtrMethod");

    [TestMethod]
    public void CheckDeferInterfaceReturn() => CheckTarget("DeferInterfaceReturn");

    [TestMethod]
    public void CheckDeferLambdaParam() => CheckTarget("DeferLambdaParam");

    [TestMethod]
    public void CheckDeferLoopCapture() => CheckTarget("DeferLoopCapture");

    [TestMethod]
    public void CheckDeferMultiValueSpread() => CheckTarget("DeferMultiValueSpread");

    [TestMethod]
    public void CheckDeferPanicArg() => CheckTarget("DeferPanicArg");

    [TestMethod]
    public void CheckDeferSimple() => CheckTarget("DeferSimple");

    [TestMethod]
    public void CheckDeferTypelessReturns() => CheckTarget("DeferTypelessReturns");

    [TestMethod]
    public void CheckDeferValueFieldPtrReceiver() => CheckTarget("DeferValueFieldPtrReceiver");

    [TestMethod]
    public void CheckDeferVariadicCallee() => CheckTarget("DeferVariadicCallee");

    [TestMethod]
    public void CheckDefinedElemStringConversion() => CheckTarget("DefinedElemStringConversion");

    [TestMethod]
    public void CheckDefinedOverNamedComposite() => CheckTarget("DefinedOverNamedComposite");

    [TestMethod]
    public void CheckDefinedTypeOverForeignStruct() => CheckTarget("DefinedTypeOverForeignStruct");

    [TestMethod]
    public void CheckDefinedTypeOverInterface() => CheckTarget("DefinedTypeOverInterface");

    [TestMethod]
    public void CheckDefinedTypeOverPkgType() => CheckTarget("DefinedTypeOverPkgType");

    [TestMethod]
    public void CheckDerefPointerToField() => CheckTarget("DerefPointerToField");

    [TestMethod]
    public void CheckDerivedInterfaceStructuralProbe() => CheckTarget("DerivedInterfaceStructuralProbe");

    [TestMethod]
    public void CheckDescriptorCarrierFieldName() => CheckTarget("DescriptorCarrierFieldName");

    [TestMethod]
    public void CheckDirectBoxReceiverPassedWhole() => CheckTarget("DirectBoxReceiverPassedWhole");

    [TestMethod]
    public void CheckDivideByZeroPanic() => CheckTarget("DivideByZeroPanic");

    [TestMethod]
    public void CheckDotImportRenamedPackage() => CheckTarget("DotImportRenamedPackage");

    [TestMethod]
    public void CheckDotImportRenamedType() => CheckTarget("DotImportRenamedType");

    [TestMethod]
    public void CheckDynIfaceParamNameCollision() => CheckTarget("DynIfaceParamNameCollision");

    [TestMethod]
    public void CheckDynamicInterfaceKeywordMethod() => CheckTarget("DynamicInterfaceKeywordMethod");

    [TestMethod]
    public void CheckElementAddressUnsignedIndex() => CheckTarget("ElementAddressUnsignedIndex");

    [TestMethod]
    public void CheckElidedNestedPtrComposite() => CheckTarget("ElidedNestedPtrComposite");

    [TestMethod]
    public void CheckElidedPtrElemIfaceAssign() => CheckTarget("ElidedPtrElemIfaceAssign");

    [TestMethod]
    public void CheckElidedStructInterfaceField() => CheckTarget("ElidedStructInterfaceField");

    [TestMethod]
    public void CheckEmbeddedInterfaceWitness() => CheckTarget("EmbeddedInterfaceWitness");

    [TestMethod]
    public void CheckEmbeddedPointerFieldIdentity() => CheckTarget("EmbeddedPointerFieldIdentity");

    [TestMethod]
    public void CheckEmbeddedPointerNilAssign() => CheckTarget("EmbeddedPointerNilAssign");

    [TestMethod]
    public void CheckEmbeddedStructValueCopy() => CheckTarget("EmbeddedStructValueCopy");

    [TestMethod]
    public void CheckEmbeddedTypeNameCollision() => CheckTarget("EmbeddedTypeNameCollision");

    [TestMethod]
    public void CheckEmbeddedValuePointerMethod() => CheckTarget("EmbeddedValuePointerMethod");

    [TestMethod]
    public void CheckEmptyStructMapSet() => CheckTarget("EmptyStructMapSet");

    [TestMethod]
    public void CheckEnvironBlockWalk() => CheckTarget("EnvironBlockWalk");

    [TestMethod]
    public void CheckErrorfFormatting() => CheckTarget("ErrorfFormatting");

    [TestMethod]
    public void CheckEscapedLoopVarSiblingIndex() => CheckTarget("EscapedLoopVarSiblingIndex");

    [TestMethod]
    public void CheckExprSwitch() => CheckTarget("ExprSwitch");

    [TestMethod]
    public void CheckFieldChainBoxReceiver() => CheckTarget("FieldChainBoxReceiver");

    [TestMethod]
    public void CheckFieldDimsCargo() => CheckTarget("FieldDimsCargo");

    [TestMethod]
    public void CheckFieldNameShadowsLoopVar() => CheckTarget("FieldNameShadowsLoopVar");

    [TestMethod]
    public void CheckFieldNameTypeMethodCollision() => CheckTarget("FieldNameTypeMethodCollision");

    [TestMethod]
    public void CheckFieldNamedAsType() => CheckTarget("FieldNamedAsType");

    [TestMethod]
    public void CheckFileNameBuildConstraints() => CheckTarget("FileNameBuildConstraints");

    [TestMethod]
    public void CheckFindFirstFileData() => CheckTarget("FindFirstFileData");

    [TestMethod]
    public void CheckFixedArrayBufferPointer() => CheckTarget("FixedArrayBufferPointer");

    [TestMethod]
    public void CheckFloatConstIntContext() => CheckTarget("FloatConstIntContext");

    [TestMethod]
    public void CheckFloatFormatExponent() => CheckTarget("FloatFormatExponent");

    [TestMethod]
    public void CheckFloatFormatting() => CheckTarget("FloatFormatting");

    [TestMethod]
    public void CheckFloatKindConstArrayLength() => CheckTarget("FloatKindConstArrayLength");

    [TestMethod]
    public void CheckForInitMixedTypes() => CheckTarget("ForInitMixedTypes");

    [TestMethod]
    public void CheckForInitShadowedUse() => CheckTarget("ForInitShadowedUse");

    [TestMethod]
    public void CheckForLoopPerIterationVars() => CheckTarget("ForLoopPerIterationVars");

    [TestMethod]
    public void CheckForMethodInitPost() => CheckTarget("ForMethodInitPost");

    [TestMethod]
    public void CheckForVarMasksBlockLevel() => CheckTarget("ForVarMasksBlockLevel");

    [TestMethod]
    public void CheckForVarMasksFuncLevel() => CheckTarget("ForVarMasksFuncLevel");

    [TestMethod]
    public void CheckForeignIfaceFieldPointer() => CheckTarget("ForeignIfaceFieldPointer");

    [TestMethod]
    public void CheckForeignPairNumericConv() => CheckTarget("ForeignPairNumericConv");

    [TestMethod]
    public void CheckForeignPointerImplementSuppression() => CheckTarget("ForeignPointerImplementSuppression");

    [TestMethod]
    public void CheckForeignPtrEmbedIfaceUser() => CheckTarget("ForeignPtrEmbedIfaceUser");

    [TestMethod]
    public void CheckForeignValueImplementSuppression() => CheckTarget("ForeignValueImplementSuppression");

    [TestMethod]
    public void CheckFormatTypeAdapters() => CheckTarget("FormatTypeAdapters");

    [TestMethod]
    public void CheckFuncFieldNestedTupleParam() => CheckTarget("FuncFieldNestedTupleParam");

    [TestMethod]
    public void CheckFuncFieldUnexportedType() => CheckTarget("FuncFieldUnexportedType");

    [TestMethod]
    public void CheckFuncForPCName() => CheckTarget("FuncForPCName");

    [TestMethod]
    public void CheckFuncLitArgCapture() => CheckTarget("FuncLitArgCapture");

    [TestMethod]
    public void CheckFuncLitCaptureInCondition() => CheckTarget("FuncLitCaptureInCondition");

    [TestMethod]
    public void CheckFuncLitNumericTupleReturn() => CheckTarget("FuncLitNumericTupleReturn");

    [TestMethod]
    public void CheckFuncLitStringConcatReturn() => CheckTarget("FuncLitStringConcatReturn");

    [TestMethod]
    public void CheckFuncLitUntypedConstReturn() => CheckTarget("FuncLitUntypedConstReturn");

    [TestMethod]
    public void CheckFuncLiteralCallerNames() => CheckTarget("FuncLiteralCallerNames");

    [TestMethod]
    public void CheckFuncTypeNilConversion() => CheckTarget("FuncTypeNilConversion");

    [TestMethod]
    public void CheckFuncTypeParam() => CheckTarget("FuncTypeParam");

    [TestMethod]
    public void CheckFuncVsMethodOverload() => CheckTarget("FuncVsMethodOverload");

    [TestMethod]
    public void CheckGenericArrayConstraint() => CheckTarget("GenericArrayConstraint");

    [TestMethod]
    public void CheckGenericAtomicPointerField() => CheckTarget("GenericAtomicPointerField");

    [TestMethod]
    public void CheckGenericCompositeLiterals() => CheckTarget("GenericCompositeLiterals");

    [TestMethod]
    public void CheckGenericCompositeType() => CheckTarget("GenericCompositeType");

    [TestMethod]
    public void CheckGenericEmbedPromotion() => CheckTarget("GenericEmbedPromotion");

    [TestMethod]
    public void CheckGenericFuncCall() => CheckTarget("GenericFuncCall");

    [TestMethod]
    public void CheckGenericFuncDecl() => CheckTarget("GenericFuncDecl");

    [TestMethod]
    public void CheckGenericInterfaceConstraint() => CheckTarget("GenericInterfaceConstraint");

    [TestMethod]
    public void CheckGenericNamedArrayType() => CheckTarget("GenericNamedArrayType");

    [TestMethod]
    public void CheckGenericNegation() => CheckTarget("GenericNegation");

    [TestMethod]
    public void CheckGenericPointerInterfaceImpl() => CheckTarget("GenericPointerInterfaceImpl");

    [TestMethod]
    public void CheckGenericReceiverFieldAddress() => CheckTarget("GenericReceiverFieldAddress");

    [TestMethod]
    public void CheckGenericResultLambdaInfer() => CheckTarget("GenericResultLambdaInfer");

    [TestMethod]
    public void CheckGenericStringTypeArg() => CheckTarget("GenericStringTypeArg");

    [TestMethod]
    public void CheckGenericStructEquality() => CheckTarget("GenericStructEquality");

    [TestMethod]
    public void CheckGenericStructFields() => CheckTarget("GenericStructFields");

    [TestMethod]
    public void CheckGenericTypeAssertions() => CheckTarget("GenericTypeAssertions");

    [TestMethod]
    public void CheckGenericTypeDecl() => CheckTarget("GenericTypeDecl");

    [TestMethod]
    public void CheckGenericTypeInference() => CheckTarget("GenericTypeInference");

    [TestMethod]
    public void CheckGenericTypeInstantiation() => CheckTarget("GenericTypeInstantiation");

    [TestMethod]
    public void CheckGenericUntypedConstInfer() => CheckTarget("GenericUntypedConstInfer");

    [TestMethod]
    public void CheckGenericUntypedIntArg() => CheckTarget("GenericUntypedIntArg");

    [TestMethod]
    public void CheckGenericValueInterfaceImpl() => CheckTarget("GenericValueInterfaceImpl");

    [TestMethod]
    public void CheckGenericVariadicFunc() => CheckTarget("GenericVariadicFunc");

    [TestMethod]
    public void CheckGlobalArrayElementFieldAddress() => CheckTarget("GlobalArrayElementFieldAddress");

    [TestMethod]
    public void CheckGlobalArrayElementMethod() => CheckTarget("GlobalArrayElementMethod");

    [TestMethod]
    public void CheckGlobalAtomicDefer() => CheckTarget("GlobalAtomicDefer");

    [TestMethod]
    public void CheckGlobalAtomicFieldMethod() => CheckTarget("GlobalAtomicFieldMethod");

    [TestMethod]
    public void CheckGlobalCapturedInClosure() => CheckTarget("GlobalCapturedInClosure");

    [TestMethod]
    public void CheckGlobalNestedFieldAddress() => CheckTarget("GlobalNestedFieldAddress");

    [TestMethod]
    public void CheckGlobalPointerWalk() => CheckTarget("GlobalPointerWalk");

    [TestMethod]
    public void CheckGlobalShadowedByLocal() => CheckTarget("GlobalShadowedByLocal");

    [TestMethod]
    public void CheckGlobalStructFieldPointers() => CheckTarget("GlobalStructFieldPointers");

    [TestMethod]
    public void CheckGlobalTupleVarDecl() => CheckTarget("GlobalTupleVarDecl");

    [TestMethod]
    public void CheckGoNamespaceShadow() => CheckTarget("GoNamespaceShadow");

    [TestMethod]
    public void CheckGoOnlyFloatLiteralForms() => CheckTarget("GoOnlyFloatLiteralForms");

    [TestMethod]
    public void CheckGoShiftSemantics() => CheckTarget("GoShiftSemantics");

    [TestMethod]
    public void CheckGoStmtReceiverLambda() => CheckTarget("GoStmtReceiverLambda");

    [TestMethod]
    public void CheckGoStmtValueReturn() => CheckTarget("GoStmtValueReturn");

    [TestMethod]
    public void CheckGoSyntaxIfaceFieldPointer() => CheckTarget("GoSyntaxIfaceFieldPointer");

    [TestMethod]
    public void CheckGoUntypedConstArg() => CheckTarget("GoUntypedConstArg");

    [TestMethod]
    public void CheckGoexitDefers() => CheckTarget("GoexitDefers");

    [TestMethod]
    public void CheckGoroutinePanicExitCode() => CheckTarget("GoroutinePanicExitCode");

    [TestMethod]
    public void CheckGoroutineParkStorm() => CheckTarget("GoroutineParkStorm");

    [TestMethod]
    public void CheckGoroutineWaitState() => CheckTarget("GoroutineWaitState");

    [TestMethod]
    public void CheckGuardedNilPointerParamDeref() => CheckTarget("GuardedNilPointerParamDeref");

    [TestMethod]
    public void CheckHeapKeywordVar() => CheckTarget("HeapKeywordVar");

    [TestMethod]
    public void CheckHexByteStringLiteral() => CheckTarget("HexByteStringLiteral");

    [TestMethod]
    public void CheckIfaceChainPointerAssert() => CheckTarget("IfaceChainPointerAssert");

    [TestMethod]
    public void CheckIfaceFieldEmbedAdapter() => CheckTarget("IfaceFieldEmbedAdapter");

    [TestMethod]
    public void CheckIfaceFieldMethodValueBind() => CheckTarget("IfaceFieldMethodValueBind");

    [TestMethod]
    public void CheckIfaceToIfaceNarrow() => CheckTarget("IfaceToIfaceNarrow");

    [TestMethod]
    public void CheckImmediatelyInvokedFunc() => CheckTarget("ImmediatelyInvokedFunc");

    [TestMethod]
    public void CheckImportSegmentTypeShadow() => CheckTarget("ImportSegmentTypeShadow");

    [TestMethod]
    public void CheckIncDecPointerField() => CheckTarget("IncDecPointerField");

    [TestMethod]
    public void CheckIndexExprCaseLabel() => CheckTarget("IndexExprCaseLabel");

    [TestMethod]
    public void CheckIndexedElementDirectBoxMethod() => CheckTarget("IndexedElementDirectBoxMethod");

    [TestMethod]
    public void CheckInferredForeignTypeNoImport() => CheckTarget("InferredForeignTypeNoImport");

    [TestMethod]
    public void CheckInitOrderTupleSpecs() => CheckTarget("InitOrderTupleSpecs");

    [TestMethod]
    public void CheckIntFormFloatConst() => CheckTarget("IntFormFloatConst");

    [TestMethod]
    public void CheckIntMinLiterals() => CheckTarget("IntMinLiterals");

    [TestMethod]
    public void CheckInterfaceAssertionMapKey() => CheckTarget("InterfaceAssertionMapKey");

    [TestMethod]
    public void CheckInterfaceFieldNamedScalar() => CheckTarget("InterfaceFieldNamedScalar");

    [TestMethod]
    public void CheckInterfaceImplementation() => CheckTarget("InterfaceImplementation");

    [TestMethod]
    public void CheckInterfaceInheritance() => CheckTarget("InterfaceInheritance");

    [TestMethod]
    public void CheckInterfaceIntraFunction() => CheckTarget("InterfaceIntraFunction");

    [TestMethod]
    public void CheckInterfaceKeywordParamNames() => CheckTarget("InterfaceKeywordParamNames");

    [TestMethod]
    public void CheckInterfaceMapKeyPointer() => CheckTarget("InterfaceMapKeyPointer");

    [TestMethod]
    public void CheckInterfaceToInterfaceAdapter() => CheckTarget("InterfaceToInterfaceAdapter");

    [TestMethod]
    public void CheckInterfaceToInterfaceAssertion() => CheckTarget("InterfaceToInterfaceAssertion");

    [TestMethod]
    public void CheckInterfaceUntypedIntCompare() => CheckTarget("InterfaceUntypedIntCompare");

    [TestMethod]
    public void CheckInvalidRuneString() => CheckTarget("InvalidRuneString");

    [TestMethod]
    public void CheckIotaEnum() => CheckTarget("IotaEnum");

    [TestMethod]
    public void CheckIpAdapterAddresses() => CheckTarget("IpAdapterAddresses");

    [TestMethod]
    public void CheckItabLateRegistration() => CheckTarget("ItabLateRegistration");

    [TestMethod]
    public void CheckIterPullRendezvous() => CheckTarget("IterPullRendezvous");

    [TestMethod]
    public void CheckJsonFixedArrayUnmarshal() => CheckTarget("JsonFixedArrayUnmarshal");

    [TestMethod]
    public void CheckJsonUnmarshalerDispatch() => CheckTarget("JsonUnmarshalerDispatch");

    [TestMethod]
    public void CheckKeyedLiteralIfaceAssign() => CheckTarget("KeyedLiteralIfaceAssign");

    [TestMethod]
    public void CheckKeywordNamedTypes() => CheckTarget("KeywordNamedTypes");

    [TestMethod]
    public void CheckKeywordTrueFalseIdent() => CheckTarget("KeywordTrueFalseIdent");

    [TestMethod]
    public void CheckLabeledEmptyStmt() => CheckTarget("LabeledEmptyStmt");

    [TestMethod]
    public void CheckLambdaFunctions() => CheckTarget("LambdaFunctions");

    [TestMethod]
    public void CheckLambdaNilOnlyReturnInference() => CheckTarget("LambdaNilOnlyReturnInference");

    [TestMethod]
    public void CheckLambdaReturnsPointerParam() => CheckTarget("LambdaReturnsPointerParam");

    [TestMethod]
    public void CheckLargeUintptrConst() => CheckTarget("LargeUintptrConst");

    [TestMethod]
    public void CheckLibraryImportPartial() => CheckTarget("LibraryImportPartial");

    [TestMethod]
    public void CheckLiftAccessibilityTier() => CheckTarget("LiftAccessibilityTier");

    [TestMethod]
    public void CheckLiftedLocalTypes() => CheckTarget("LiftedLocalTypes");

    [TestMethod]
    public void CheckLinknameVarPull() => CheckTarget("LinknameVarPull");

    [TestMethod]
    public void CheckLinuxSpawnBasics() => CheckTarget("LinuxSpawnBasics");

    [TestMethod]
    public void CheckLocalFunctionEmission() => CheckTarget("LocalFunctionEmission");

    [TestMethod]
    public void CheckLocalNamedTypeDecls() => CheckTarget("LocalNamedTypeDecls");

    [TestMethod]
    public void CheckLocalShadowsEmbedHopType() => CheckTarget("LocalShadowsEmbedHopType");

    [TestMethod]
    public void CheckLocalStructFieldAddr() => CheckTarget("LocalStructFieldAddr");

    [TestMethod]
    public void CheckLocalTimeZone() => CheckTarget("LocalTimeZone");

    [TestMethod]
    public void CheckLocalTypeAliasScope() => CheckTarget("LocalTypeAliasScope");

    [TestMethod]
    public void CheckLocalTypeSliceElement() => CheckTarget("LocalTypeSliceElement");

    [TestMethod]
    public void CheckLocalValueIfaceCallConversion() => CheckTarget("LocalValueIfaceCallConversion");

    [TestMethod]
    public void CheckLongPathRoundTrip() => CheckTarget("LongPathRoundTrip");

    [TestMethod]
    public void CheckLookupServicePort() => CheckTarget("LookupServicePort");

    [TestMethod]
    public void CheckMakeLenNamedNumeric() => CheckTarget("MakeLenNamedNumeric");

    [TestMethod]
    public void CheckMakeSlicePanicRange() => CheckTarget("MakeSlicePanicRange");

    [TestMethod]
    public void CheckMakeSliceUintptrLen() => CheckTarget("MakeSliceUintptrLen");

    [TestMethod]
    public void CheckManagedAtomicPointer() => CheckTarget("ManagedAtomicPointer");

    [TestMethod]
    public void CheckManualConversionSiblingState() => CheckTarget("ManualConversionSiblingState");

    [TestMethod]
    public void CheckMapAnonStructValue() => CheckTarget("MapAnonStructValue");

    [TestMethod]
    public void CheckMapArrayValueZero() => CheckTarget("MapArrayValueZero");

    [TestMethod]
    public void CheckMapCloneLinkname() => CheckTarget("MapCloneLinkname");

    [TestMethod]
    public void CheckMapCommaOk() => CheckTarget("MapCommaOk");

    [TestMethod]
    public void CheckMapMutateDuringRange() => CheckTarget("MapMutateDuringRange");

    [TestMethod]
    public void CheckMapPointerElementLiteral() => CheckTarget("MapPointerElementLiteral");

    [TestMethod]
    public void CheckMapSamePackageTypes() => CheckTarget("MapSamePackageTypes");

    [TestMethod]
    public void CheckMapStringBytesLookup() => CheckTarget("MapStringBytesLookup");

    [TestMethod]
    public void CheckMathFloatBits() => CheckTarget("MathFloatBits");

    [TestMethod]
    public void CheckMethodExprDotImport() => CheckTarget("MethodExprDotImport");

    [TestMethod]
    public void CheckMethodExpression() => CheckTarget("MethodExpression");

    [TestMethod]
    public void CheckMethodGroupGenericArg() => CheckTarget("MethodGroupGenericArg");

    [TestMethod]
    public void CheckMethodOnBoxedGlobalIndex() => CheckTarget("MethodOnBoxedGlobalIndex");

    [TestMethod]
    public void CheckMethodSelector() => CheckTarget("MethodSelector");

    [TestMethod]
    public void CheckMethodValuePointeeCopy() => CheckTarget("MethodValuePointeeCopy");

    [TestMethod]
    public void CheckMethodValueReassignCapture() => CheckTarget("MethodValueReassignCapture");

    [TestMethod]
    public void CheckMethodValueReceiverEscape() => CheckTarget("MethodValueReceiverEscape");

    [TestMethod]
    public void CheckMethodValueReceiverSnapshot() => CheckTarget("MethodValueReceiverSnapshot");

    [TestMethod]
    public void CheckMethodlessFuncType() => CheckTarget("MethodlessFuncType");

    [TestMethod]
    public void CheckMethodlessFuncTypeAssert() => CheckTarget("MethodlessFuncTypeAssert");

    [TestMethod]
    public void CheckMinMaxBuiltin() => CheckTarget("MinMaxBuiltin");

    [TestMethod]
    public void CheckMixedEmbedKindPromotion() => CheckTarget("MixedEmbedKindPromotion");

    [TestMethod]
    public void CheckMultiFileInitOrder() => CheckTarget("MultiFileInitOrder");

    [TestMethod]
    public void CheckMultiPointerEmbedPromotion() => CheckTarget("MultiPointerEmbedPromotion");

    [TestMethod]
    public void CheckMultiValueReturnOrder() => CheckTarget("MultiValueReturnOrder");

    [TestMethod]
    public void CheckMulticastGroupJoin() => CheckTarget("MulticastGroupJoin");

    [TestMethod]
    public void CheckNamedAnySliceType() => CheckTarget("NamedAnySliceType");

    [TestMethod]
    public void CheckNamedArrayAnonElement() => CheckTarget("NamedArrayAnonElement");

    [TestMethod]
    public void CheckNamedArrayComposite() => CheckTarget("NamedArrayComposite");

    [TestMethod]
    public void CheckNamedArrayKeyedLiteral() => CheckTarget("NamedArrayKeyedLiteral");

    [TestMethod]
    public void CheckNamedArrayPointerConversion() => CheckTarget("NamedArrayPointerConversion");

    [TestMethod]
    public void CheckNamedArrayWrapper() => CheckTarget("NamedArrayWrapper");

    [TestMethod]
    public void CheckNamedBooleanLogic() => CheckTarget("NamedBooleanLogic");

    [TestMethod]
    public void CheckNamedByteSliceFromStringLit() => CheckTarget("NamedByteSliceFromStringLit");

    [TestMethod]
    public void CheckNamedChannelType() => CheckTarget("NamedChannelType");

    [TestMethod]
    public void CheckNamedConstConversionPrecedence() => CheckTarget("NamedConstConversionPrecedence");

    [TestMethod]
    public void CheckNamedConstFloatFold() => CheckTarget("NamedConstFloatFold");

    [TestMethod]
    public void CheckNamedDelegateStructuralParam() => CheckTarget("NamedDelegateStructuralParam");

    [TestMethod]
    public void CheckNamedFuncResultPointerArg() => CheckTarget("NamedFuncResultPointerArg");

    [TestMethod]
    public void CheckNamedFuncTypeMapParam() => CheckTarget("NamedFuncTypeMapParam");

    [TestMethod]
    public void CheckNamedFuncTypeStateMachine() => CheckTarget("NamedFuncTypeStateMachine");

    [TestMethod]
    public void CheckNamedFuncTypeStructuralField() => CheckTarget("NamedFuncTypeStructuralField");

    [TestMethod]
    public void CheckNamedImportInitOrder() => CheckTarget("NamedImportInitOrder");

    [TestMethod]
    public void CheckNamedIntSignednessConv() => CheckTarget("NamedIntSignednessConv");

    [TestMethod]
    public void CheckNamedInterfaceAdapterIdentity() => CheckTarget("NamedInterfaceAdapterIdentity");

    [TestMethod]
    public void CheckNamedInterfaceLateAssert() => CheckTarget("NamedInterfaceLateAssert");

    [TestMethod]
    public void CheckNamedInterfacePointerMethodSet() => CheckTarget("NamedInterfacePointerMethodSet");

    [TestMethod]
    public void CheckNamedMapCrossPkgKey() => CheckTarget("NamedMapCrossPkgKey");

    [TestMethod]
    public void CheckNamedMapMakeNonNil() => CheckTarget("NamedMapMakeNonNil");

    [TestMethod]
    public void CheckNamedMapRvalueIndexWrite() => CheckTarget("NamedMapRvalueIndexWrite");

    [TestMethod]
    public void CheckNamedMapValuesCollision() => CheckTarget("NamedMapValuesCollision");

    [TestMethod]
    public void CheckNamedNumericConstCast() => CheckTarget("NamedNumericConstCast");

    [TestMethod]
    public void CheckNamedNumericConversion() => CheckTarget("NamedNumericConversion");

    [TestMethod]
    public void CheckNamedNumericIncDec() => CheckTarget("NamedNumericIncDec");

    [TestMethod]
    public void CheckNamedNumericIntCast() => CheckTarget("NamedNumericIntCast");

    [TestMethod]
    public void CheckNamedNumericOperatorConstraint() => CheckTarget("NamedNumericOperatorConstraint");

    [TestMethod]
    public void CheckNamedNumericPointerReinterpret() => CheckTarget("NamedNumericPointerReinterpret");

    [TestMethod]
    public void CheckNamedNumericShiftConv() => CheckTarget("NamedNumericShiftConv");

    [TestMethod]
    public void CheckNamedNumericSliceIndex() => CheckTarget("NamedNumericSliceIndex");

    [TestMethod]
    public void CheckNamedNumericSwitchLiteral() => CheckTarget("NamedNumericSwitchLiteral");

    [TestMethod]
    public void CheckNamedPointerReinterpret() => CheckTarget("NamedPointerReinterpret");

    [TestMethod]
    public void CheckNamedResultAddressEscape() => CheckTarget("NamedResultAddressEscape");

    [TestMethod]
    public void CheckNamedResultDeferCapture() => CheckTarget("NamedResultDeferCapture");

    [TestMethod]
    public void CheckNamedResultLambdaInfer() => CheckTarget("NamedResultLambdaInfer");

    [TestMethod]
    public void CheckNamedReturnDefer() => CheckTarget("NamedReturnDefer");

    [TestMethod]
    public void CheckNamedSliceCaptureMethod() => CheckTarget("NamedSliceCaptureMethod");

    [TestMethod]
    public void CheckNamedSliceChildPkg() => CheckTarget("NamedSliceChildPkg");

    [TestMethod]
    public void CheckNamedSliceConversion() => CheckTarget("NamedSliceConversion");

    [TestMethod]
    public void CheckNamedSliceNilVsEmpty() => CheckTarget("NamedSliceNilVsEmpty");

    [TestMethod]
    public void CheckNamedSlicePointerElements() => CheckTarget("NamedSlicePointerElements");

    [TestMethod]
    public void CheckNamedSlicePointerReinterpret() => CheckTarget("NamedSlicePointerReinterpret");

    [TestMethod]
    public void CheckNamedStringConcat() => CheckTarget("NamedStringConcat");

    [TestMethod]
    public void CheckNamedStringConsts() => CheckTarget("NamedStringConsts");

    [TestMethod]
    public void CheckNamedStringConversion() => CheckTarget("NamedStringConversion");

    [TestMethod]
    public void CheckNamedStringDefine() => CheckTarget("NamedStringDefine");

    [TestMethod]
    public void CheckNamedStringZeroValue() => CheckTarget("NamedStringZeroValue");

    [TestMethod]
    public void CheckNamedTypeBitwiseConst() => CheckTarget("NamedTypeBitwiseConst");

    [TestMethod]
    public void CheckNamedTypeOverStruct() => CheckTarget("NamedTypeOverStruct");

    [TestMethod]
    public void CheckNarrowArithmeticArg() => CheckTarget("NarrowArithmeticArg");

    [TestMethod]
    public void CheckNarrowByteArithFirstOperandCast() => CheckTarget("NarrowByteArithFirstOperandCast");

    [TestMethod]
    public void CheckNarrowByteArithReturn() => CheckTarget("NarrowByteArithReturn");

    [TestMethod]
    public void CheckNarrowShiftVarCount() => CheckTarget("NarrowShiftVarCount");

    [TestMethod]
    public void CheckNativeIntConstMask() => CheckTarget("NativeIntConstMask");

    [TestMethod]
    public void CheckNativeIntWideConstAssign() => CheckTarget("NativeIntWideConstAssign");

    [TestMethod]
    public void CheckNativeIntWideConstElement() => CheckTarget("NativeIntWideConstElement");

    [TestMethod]
    public void CheckNestedAliasUser() => CheckTarget("NestedAliasUser");

    [TestMethod]
    public void CheckNestedEmbeddingPromotion() => CheckTarget("NestedEmbeddingPromotion");

    [TestMethod]
    public void CheckNestedFieldElementAddr() => CheckTarget("NestedFieldElementAddr");

    [TestMethod]
    public void CheckNestedFieldPointerAssign() => CheckTarget("NestedFieldPointerAssign");

    [TestMethod]
    public void CheckNestedFixedArrays() => CheckTarget("NestedFixedArrays");

    [TestMethod]
    public void CheckNestedGenericTypes() => CheckTarget("NestedGenericTypes");

    [TestMethod]
    public void CheckNestedLambdaReceiverField() => CheckTarget("NestedLambdaReceiverField");

    [TestMethod]
    public void CheckNestedMapAssign() => CheckTarget("NestedMapAssign");

    [TestMethod]
    public void CheckNestedPromotedEmbedInit() => CheckTarget("NestedPromotedEmbedInit");

    [TestMethod]
    public void CheckNestedSelectRecvTarget() => CheckTarget("NestedSelectRecvTarget");

    [TestMethod]
    public void CheckNestedVarShadow() => CheckTarget("NestedVarShadow");

    [TestMethod]
    public void CheckNetDeadlineMatrix() => CheckTarget("NetDeadlineMatrix");

    [TestMethod]
    public void CheckNetListenSmoke() => CheckTarget("NetListenSmoke");

    [TestMethod]
    public void CheckNewAnonStructIfaceEmbed() => CheckTarget("NewAnonStructIfaceEmbed");

    [TestMethod]
    public void CheckNilAdapterOpsClosure() => CheckTarget("NilAdapterOpsClosure");

    [TestMethod]
    public void CheckNilChannelInSelect() => CheckTarget("NilChannelInSelect");

    [TestMethod]
    public void CheckNilChannelSelectDefault() => CheckTarget("NilChannelSelectDefault");

    [TestMethod]
    public void CheckNilMapKey() => CheckTarget("NilMapKey");

    [TestMethod]
    public void CheckNilMapOperations() => CheckTarget("NilMapOperations");

    [TestMethod]
    public void CheckNilPointerPanic() => CheckTarget("NilPointerPanic");

    [TestMethod]
    public void CheckNilPointerParamMethods() => CheckTarget("NilPointerParamMethods");

    [TestMethod]
    public void CheckNilPointerParamUnsafePointer() => CheckTarget("NilPointerParamUnsafePointer");

    [TestMethod]
    public void CheckNilPointerUintptr() => CheckTarget("NilPointerUintptr");

    [TestMethod]
    public void CheckNilReceiverMethods() => CheckTarget("NilReceiverMethods");

    [TestMethod]
    public void CheckNilReceiverNormalization() => CheckTarget("NilReceiverNormalization");

    [TestMethod]
    public void CheckNilSliceConversion() => CheckTarget("NilSliceConversion");

    [TestMethod]
    public void CheckNilVarNamedFuncConv() => CheckTarget("NilVarNamedFuncConv");

    [TestMethod]
    public void CheckOptionalInterfaceStructuralAssertion() => CheckTarget("OptionalInterfaceStructuralAssertion");

    [TestMethod]
    public void CheckPackageAliasRootedTypeArgs() => CheckTarget("PackageAliasRootedTypeArgs");

    [TestMethod]
    public void CheckPackageNameShadowing() => CheckTarget("PackageNameShadowing");

    [TestMethod]
    public void CheckPackageShadowParam() => CheckTarget("PackageShadowParam");

    [TestMethod]
    public void CheckPackageShadowPointerParam() => CheckTarget("PackageShadowPointerParam");

    [TestMethod]
    public void CheckPackageVarFuncLitPointerParam() => CheckTarget("PackageVarFuncLitPointerParam");

    [TestMethod]
    public void CheckPackageVarFuncLitTypeLift() => CheckTarget("PackageVarFuncLitTypeLift");

    [TestMethod]
    public void CheckPackageVarInitOrder() => CheckTarget("PackageVarInitOrder");

    [TestMethod]
    public void CheckPanicDeferCalleeFrame() => CheckTarget("PanicDeferCalleeFrame");

    [TestMethod]
    public void CheckPanicRecover() => CheckTarget("PanicRecover");

    [TestMethod]
    public void CheckPanicValueRendering() => CheckTarget("PanicValueRendering");

    [TestMethod]
    public void CheckParallelAssignmentHazard() => CheckTarget("ParallelAssignmentHazard");

    [TestMethod]
    public void CheckParenIifeNilFuncConv() => CheckTarget("ParenIifeNilFuncConv");

    [TestMethod]
    public void CheckParenTypeConversionCall() => CheckTarget("ParenTypeConversionCall");

    [TestMethod]
    public void CheckParenthesizedConcatContext() => CheckTarget("ParenthesizedConcatContext");

    [TestMethod]
    public void CheckPartialRedeclaration() => CheckTarget("PartialRedeclaration");

    [TestMethod]
    public void CheckPipeCloseUnblocksRead() => CheckTarget("PipeCloseUnblocksRead");

    [TestMethod]
    public void CheckPkgLevelFuncLitLocals() => CheckTarget("PkgLevelFuncLitLocals");

    [TestMethod]
    public void CheckPointerArrayRange() => CheckTarget("PointerArrayRange");

    [TestMethod]
    public void CheckPointerArraySlice() => CheckTarget("PointerArraySlice");

    [TestMethod]
    public void CheckPointerCastSliceRange() => CheckTarget("PointerCastSliceRange");

    [TestMethod]
    public void CheckPointerCastSliceReinterpret() => CheckTarget("PointerCastSliceReinterpret");

    [TestMethod]
    public void CheckPointerCopyWalk() => CheckTarget("PointerCopyWalk");

    [TestMethod]
    public void CheckPointerCoreConstraints() => CheckTarget("PointerCoreConstraints");

    [TestMethod]
    public void CheckPointerEmbedBoxReceiver() => CheckTarget("PointerEmbedBoxReceiver");

    [TestMethod]
    public void CheckPointerEmbedValueChainPromotion() => CheckTarget("PointerEmbedValueChainPromotion");

    [TestMethod]
    public void CheckPointerEmbeddingPromotion() => CheckTarget("PointerEmbeddingPromotion");

    [TestMethod]
    public void CheckPointerFieldArrayElementAddress() => CheckTarget("PointerFieldArrayElementAddress");

    [TestMethod]
    public void CheckPointerFieldOfBoxedGlobal() => CheckTarget("PointerFieldOfBoxedGlobal");

    [TestMethod]
    public void CheckPointerInterfaceStructField() => CheckTarget("PointerInterfaceStructField");

    [TestMethod]
    public void CheckPointerOutParameter() => CheckTarget("PointerOutParameter");

    [TestMethod]
    public void CheckPointerParamCapturedInClosure() => CheckTarget("PointerParamCapturedInClosure");

    [TestMethod]
    public void CheckPointerParamInClosure() => CheckTarget("PointerParamInClosure");

    [TestMethod]
    public void CheckPointerParamNilWalk() => CheckTarget("PointerParamNilWalk");

    [TestMethod]
    public void CheckPointerParamWalk() => CheckTarget("PointerParamWalk");

    [TestMethod]
    public void CheckPointerReceiverNilCompare() => CheckTarget("PointerReceiverNilCompare");

    [TestMethod]
    public void CheckPointerReceiverPointerLocalField() => CheckTarget("PointerReceiverPointerLocalField");

    [TestMethod]
    public void CheckPointerReceiverRepoint() => CheckTarget("PointerReceiverRepoint");

    [TestMethod]
    public void CheckPointerReceiverSliceSwap() => CheckTarget("PointerReceiverSliceSwap");

    [TestMethod]
    public void CheckPointerReinterpretIdentity() => CheckTarget("PointerReinterpretIdentity");

    [TestMethod]
    public void CheckPointerRvalueFieldReceiver() => CheckTarget("PointerRvalueFieldReceiver");

    [TestMethod]
    public void CheckPointerSelectorDeref() => CheckTarget("PointerSelectorDeref");

    [TestMethod]
    public void CheckPointerToArrayElementAddress() => CheckTarget("PointerToArrayElementAddress");

    [TestMethod]
    public void CheckPointerToInterfaceParamDeref() => CheckTarget("PointerToInterfaceParamDeref");

    [TestMethod]
    public void CheckPointerToNilPointerIdentity() => CheckTarget("PointerToNilPointerIdentity");

    [TestMethod]
    public void CheckPointerToPointer() => CheckTarget("PointerToPointer");

    [TestMethod]
    public void CheckPointerValueToInterfaceArg() => CheckTarget("PointerValueToInterfaceArg");

    [TestMethod]
    public void CheckPrintfFormatCommaParen() => CheckTarget("PrintfFormatCommaParen");

    [TestMethod]
    public void CheckPrintfWidthFlags() => CheckTarget("PrintfWidthFlags");

    [TestMethod]
    public void CheckPromotedEmbedAnonIfaceWitness() => CheckTarget("PromotedEmbedAnonIfaceWitness");

    [TestMethod]
    public void CheckPromotedEmbedUser() => CheckTarget("PromotedEmbedUser");

    [TestMethod]
    public void CheckPromotedEmbedZeroValueField() => CheckTarget("PromotedEmbedZeroValueField");

    [TestMethod]
    public void CheckPromotedFieldNameIsType() => CheckTarget("PromotedFieldNameIsType");

    [TestMethod]
    public void CheckPromotedFieldPointerDeref() => CheckTarget("PromotedFieldPointerDeref");

    [TestMethod]
    public void CheckPromotedValueEmbedExprRecv() => CheckTarget("PromotedValueEmbedExprRecv");

    [TestMethod]
    public void CheckPromotedValueEmbedUser() => CheckTarget("PromotedValueEmbedUser");

    [TestMethod]
    public void CheckPtrKeyMapReceiverLookup() => CheckTarget("PtrKeyMapReceiverLookup");

    [TestMethod]
    public void CheckPublicizedFieldType() => CheckTarget("PublicizedFieldType");

    [TestMethod]
    public void CheckPublicizedFuncTypeParam() => CheckTarget("PublicizedFuncTypeParam");

    [TestMethod]
    public void CheckPublicizedInterfaceAnonAlias() => CheckTarget("PublicizedInterfaceAnonAlias");

    [TestMethod]
    public void CheckPublicizedInterfaceParam() => CheckTarget("PublicizedInterfaceParam");

    [TestMethod]
    public void CheckRangeExprFuncLitCapture() => CheckTarget("RangeExprFuncLitCapture");

    [TestMethod]
    public void CheckRangeIntIndexAppend() => CheckTarget("RangeIntIndexAppend");

    [TestMethod]
    public void CheckRangeOverIntegerTypes() => CheckTarget("RangeOverIntegerTypes");

    [TestMethod]
    public void CheckRangeShadowSelectorMethod() => CheckTarget("RangeShadowSelectorMethod");

    [TestMethod]
    public void CheckRangeVarHeapBox() => CheckTarget("RangeVarHeapBox");

    [TestMethod]
    public void CheckRangeVarReassign() => CheckTarget("RangeVarReassign");

    [TestMethod]
    public void CheckReceiverCapturedInClosure() => CheckTarget("ReceiverCapturedInClosure");

    [TestMethod]
    public void CheckReceiverFieldAddress() => CheckTarget("ReceiverFieldAddress");

    [TestMethod]
    public void CheckReceiverFieldMethodCall() => CheckTarget("ReceiverFieldMethodCall");

    [TestMethod]
    public void CheckReceiverNestedFieldAddress() => CheckTarget("ReceiverNestedFieldAddress");

    [TestMethod]
    public void CheckReceiverPointerValue() => CheckTarget("ReceiverPointerValue");

    [TestMethod]
    public void CheckRecvArrayFieldElementAddress() => CheckTarget("RecvArrayFieldElementAddress");

    [TestMethod]
    public void CheckRecvMapElementDeref() => CheckTarget("RecvMapElementDeref");

    [TestMethod]
    public void CheckRefLoweredNilTiming() => CheckTarget("RefLoweredNilTiming");

    [TestMethod]
    public void CheckRefLoweredParams() => CheckTarget("RefLoweredParams");

    [TestMethod]
    public void CheckRefPrimaryFieldReceiver() => CheckTarget("RefPrimaryFieldReceiver");

    [TestMethod]
    public void CheckReflectArrayOf() => CheckTarget("ReflectArrayOf");

    [TestMethod]
    public void CheckReflectAssignabilityNamed() => CheckTarget("ReflectAssignabilityNamed");

    [TestMethod]
    public void CheckReflectBridgeClosure() => CheckTarget("ReflectBridgeClosure");

    [TestMethod]
    public void CheckReflectChanDirection() => CheckTarget("ReflectChanDirection");

    [TestMethod]
    public void CheckReflectChanNarrowing() => CheckTarget("ReflectChanNarrowing");

    [TestMethod]
    public void CheckReflectConvertAssignable() => CheckTarget("ReflectConvertAssignable");

    [TestMethod]
    public void CheckReflectEmptyContainerIdentity() => CheckTarget("ReflectEmptyContainerIdentity");

    [TestMethod]
    public void CheckReflectFieldAddrWrite() => CheckTarget("ReflectFieldAddrWrite");

    [TestMethod]
    public void CheckReflectFuncArrayParamDims() => CheckTarget("ReflectFuncArrayParamDims");

    [TestMethod]
    public void CheckReflectMakeFunc() => CheckTarget("ReflectMakeFunc");

    [TestMethod]
    public void CheckReflectMapRangeNilKey() => CheckTarget("ReflectMapRangeNilKey");

    [TestMethod]
    public void CheckReflectMethodTableWalk() => CheckTarget("ReflectMethodTableWalk");

    [TestMethod]
    public void CheckReflectStringWindow() => CheckTarget("ReflectStringWindow");

    [TestMethod]
    public void CheckReflectStructOf() => CheckTarget("ReflectStructOf");

    [TestMethod]
    public void CheckReflectStructTagCopy() => CheckTarget("ReflectStructTagCopy");

    [TestMethod]
    public void CheckReflectTypedNilInterface() => CheckTarget("ReflectTypedNilInterface");

    [TestMethod]
    public void CheckReflectUnexportedFieldFlags() => CheckTarget("ReflectUnexportedFieldFlags");

    [TestMethod]
    public void CheckReflectVariadicCall() => CheckTarget("ReflectVariadicCall");

    [TestMethod]
    public void CheckReflectZeroAndGrow() => CheckTarget("ReflectZeroAndGrow");

    [TestMethod]
    public void CheckReflectliteTypeName() => CheckTarget("ReflectliteTypeName");

    [TestMethod]
    public void CheckReinterpretPinLifetime() => CheckTarget("ReinterpretPinLifetime");

    [TestMethod]
    public void CheckReinterpretPointerLifetime() => CheckTarget("ReinterpretPointerLifetime");

    [TestMethod]
    public void CheckRelationalPatternGuard() => CheckTarget("RelationalPatternGuard");

    [TestMethod]
    public void CheckRenamedReceiverBox() => CheckTarget("RenamedReceiverBox");

    [TestMethod]
    public void CheckReservedNameShadows() => CheckTarget("ReservedNameShadows");

    [TestMethod]
    public void CheckReservedTypeMethodCollision() => CheckTarget("ReservedTypeMethodCollision");

    [TestMethod]
    public void CheckResolveErrIdentity() => CheckTarget("ResolveErrIdentity");

    [TestMethod]
    public void CheckReturnPointerFieldOfParam() => CheckTarget("ReturnPointerFieldOfParam");

    [TestMethod]
    public void CheckReturnTupleFuncLitArg() => CheckTarget("ReturnTupleFuncLitArg");

    [TestMethod]
    public void CheckReverseSortNaNOrder() => CheckTarget("ReverseSortNaNOrder");

    [TestMethod]
    public void CheckRingPointerMethods() => CheckTarget("RingPointerMethods");

    [TestMethod]
    public void CheckRuntimeCallerFrames() => CheckTarget("RuntimeCallerFrames");

    [TestMethod]
    public void CheckSStringElision() => CheckTarget("SStringElision");

    [TestMethod]
    public void CheckSamePackageImplementNoWitness() => CheckTarget("SamePackageImplementNoWitness");

    [TestMethod]
    public void CheckSameUnderlyingNamedConv() => CheckTarget("SameUnderlyingNamedConv");

    [TestMethod]
    public void CheckScmRightsSeam() => CheckTarget("ScmRightsSeam");

    [TestMethod]
    public void CheckSelectEscapeBinding() => CheckTarget("SelectEscapeBinding");

    [TestMethod]
    public void CheckSelectOperandOnceEval() => CheckTarget("SelectOperandOnceEval");

    [TestMethod]
    public void CheckSelectOperandSourceOrder() => CheckTarget("SelectOperandSourceOrder");

    [TestMethod]
    public void CheckSelectRandomFairness() => CheckTarget("SelectRandomFairness");

    [TestMethod]
    public void CheckSelectRecvCommShadowRename() => CheckTarget("SelectRecvCommShadowRename");

    [TestMethod]
    public void CheckSelectSendDefault() => CheckTarget("SelectSendDefault");

    [TestMethod]
    public void CheckSelectSendRecvMix() => CheckTarget("SelectSendRecvMix");

    [TestMethod]
    public void CheckSelectSingleFire() => CheckTarget("SelectSingleFire");

    [TestMethod]
    public void CheckSendtoSeam() => CheckTarget("SendtoSeam");

    [TestMethod]
    public void CheckSetFinalizerBridge() => CheckTarget("SetFinalizerBridge");

    [TestMethod]
    public void CheckSetegidBroadcastSeam() => CheckTarget("SetegidBroadcastSeam");

    [TestMethod]
    public void CheckShadowLocalOverRecvName() => CheckTarget("ShadowLocalOverRecvName");

    [TestMethod]
    public void CheckShadowRangeVarOverRecvName() => CheckTarget("ShadowRangeVarOverRecvName");

    [TestMethod]
    public void CheckShadowedCompoundAssign() => CheckTarget("ShadowedCompoundAssign");

    [TestMethod]
    public void CheckShadowedHeapBoxReceiver() => CheckTarget("ShadowedHeapBoxReceiver");

    [TestMethod]
    public void CheckShadowedImportConstUser() => CheckTarget("ShadowedImportConstUser");

    [TestMethod]
    public void CheckShadowedInterfaceEmbed() => CheckTarget("ShadowedInterfaceEmbed");

    [TestMethod]
    public void CheckShadowedPointerParam() => CheckTarget("ShadowedPointerParam");

    [TestMethod]
    public void CheckShadowedVarMethodCallLHS() => CheckTarget("ShadowedVarMethodCallLHS");

    [TestMethod]
    public void CheckSharedEmbeddedInterfaceMember() => CheckTarget("SharedEmbeddedInterfaceMember");

    [TestMethod]
    public void CheckShellForwardArity() => CheckTarget("ShellForwardArity");

    [TestMethod]
    public void CheckShiftNegativeWideConst() => CheckTarget("ShiftNegativeWideConst");

    [TestMethod]
    public void CheckShiftPrecedenceUnsigned() => CheckTarget("ShiftPrecedenceUnsigned");

    [TestMethod]
    public void CheckSiblingTestAddressedGlobal() => CheckTarget("SiblingTestAddressedGlobal");

    [TestMethod]
    public void CheckSignalPrimitives() => CheckTarget("SignalPrimitives");

    [TestMethod]
    public void CheckSlice3IndexWideBound() => CheckTarget("Slice3IndexWideBound");

    [TestMethod]
    public void CheckSliceAliasing() => CheckTarget("SliceAliasing");

    [TestMethod]
    public void CheckSliceElementFieldAddress() => CheckTarget("SliceElementFieldAddress");

    [TestMethod]
    public void CheckSliceFieldElementAddress() => CheckTarget("SliceFieldElementAddress");

    [TestMethod]
    public void CheckSliceNilVsEmpty() => CheckTarget("SliceNilVsEmpty");

    [TestMethod]
    public void CheckSliceOfArrayTypeName() => CheckTarget("SliceOfArrayTypeName");

    [TestMethod]
    public void CheckSlicePointerIdentity() => CheckTarget("SlicePointerIdentity");

    [TestMethod]
    public void CheckSliceToArrayPointerAlias() => CheckTarget("SliceToArrayPointerAlias");

    [TestMethod]
    public void CheckSockaddrRoundTrip() => CheckTarget("SockaddrRoundTrip");

    [TestMethod]
    public void CheckSolitaire() => CheckTarget("Solitaire");

    [TestMethod]
    public void CheckSortArrayType() => CheckTarget("SortArrayType");

    [TestMethod]
    public void CheckSparseArrayIfaceElem() => CheckTarget("SparseArrayIfaceElem");

    [TestMethod]
    public void CheckSparseArrayNamedIntKey() => CheckTarget("SparseArrayNamedIntKey");

    [TestMethod]
    public void CheckSpreadOperator() => CheckTarget("SpreadOperator");

    [TestMethod]
    public void CheckStatLayoutTruth() => CheckTarget("StatLayoutTruth");

    [TestMethod]
    public void CheckStdLibInternalAbi() => CheckTarget("StdLibInternalAbi");

    [TestMethod]
    public void CheckStdoutCloseEofBarrier() => CheckTarget("StdoutCloseEofBarrier");

    [TestMethod]
    public void CheckStringByteSemantics() => CheckTarget("StringByteSemantics");

    [TestMethod]
    public void CheckStringByteUnionConstraint() => CheckTarget("StringByteUnionConstraint");

    [TestMethod]
    public void CheckStringConvNamedInt() => CheckTarget("StringConvNamedInt");

    [TestMethod]
    public void CheckStringConvPostfix() => CheckTarget("StringConvPostfix");

    [TestMethod]
    public void CheckStringDataIdentity() => CheckTarget("StringDataIdentity");

    [TestMethod]
    public void CheckStringLenUtf8Bytes() => CheckTarget("StringLenUtf8Bytes");

    [TestMethod]
    public void CheckStringLiteralHoisting() => CheckTarget("StringLiteralHoisting");

    [TestMethod]
    public void CheckStringLiteralIndexLoop() => CheckTarget("StringLiteralIndexLoop");

    [TestMethod]
    public void CheckStringLiteralSliceConversion() => CheckTarget("StringLiteralSliceConversion");

    [TestMethod]
    public void CheckStringPassByValue() => CheckTarget("StringPassByValue");

    [TestMethod]
    public void CheckStringSliceAndUnsignedConst() => CheckTarget("StringSliceAndUnsignedConst");

    [TestMethod]
    public void CheckStringZeroValueConcat() => CheckTarget("StringZeroValueConcat");

    [TestMethod]
    public void CheckStructArrayFieldValueCopy() => CheckTarget("StructArrayFieldValueCopy");

    [TestMethod]
    public void CheckStructFieldNamedOther() => CheckTarget("StructFieldNamedOther");

    [TestMethod]
    public void CheckStructPointerPromotionWithInterface() => CheckTarget("StructPointerPromotionWithInterface");

    [TestMethod]
    public void CheckStructPromotion() => CheckTarget("StructPromotion");

    [TestMethod]
    public void CheckStructPromotionWithInterface() => CheckTarget("StructPromotionWithInterface");

    [TestMethod]
    public void CheckStructWithDelegate() => CheckTarget("StructWithDelegate");

    [TestMethod]
    public void CheckStructuralAssertFailSoftMiss() => CheckTarget("StructuralAssertFailSoftMiss");

    [TestMethod]
    public void CheckSubpackageFuncTypeParam() => CheckTarget("SubpackageFuncTypeParam");

    [TestMethod]
    public void CheckSwitchBreakBeforeFallthrough() => CheckTarget("SwitchBreakBeforeFallthrough");

    [TestMethod]
    public void CheckSwitchBreakContinueWrapper() => CheckTarget("SwitchBreakContinueWrapper");

    [TestMethod]
    public void CheckSwitchBreakInCase() => CheckTarget("SwitchBreakInCase");

    [TestMethod]
    public void CheckSwitchDefaultOrder() => CheckTarget("SwitchDefaultOrder");

    [TestMethod]
    public void CheckSwitchFallthroughDefault() => CheckTarget("SwitchFallthroughDefault");

    [TestMethod]
    public void CheckSwitchFallthroughDefaultReturn() => CheckTarget("SwitchFallthroughDefaultReturn");

    [TestMethod]
    public void CheckSwitchNonConstCaseLabel() => CheckTarget("SwitchNonConstCaseLabel");

    [TestMethod]
    public void CheckSyncTimerChannel() => CheckTarget("SyncTimerChannel");

    [TestMethod]
    public void CheckSynthesizedDelegateChildPkg() => CheckTarget("SynthesizedDelegateChildPkg");

    [TestMethod]
    public void CheckSynthesizedDelegateCrossPkg() => CheckTarget("SynthesizedDelegateCrossPkg");

    [TestMethod]
    public void CheckSyscallKeystonePulls() => CheckTarget("SyscallKeystonePulls");

    [TestMethod]
    public void CheckSystemCertVerify() => CheckTarget("SystemCertVerify");

    [TestMethod]
    public void CheckSystemCollidingTypeName() => CheckTarget("SystemCollidingTypeName");

    [TestMethod]
    public void CheckTcpLoopbackRoundTrip() => CheckTarget("TcpLoopbackRoundTrip");

    [TestMethod]
    public void CheckTransitiveAliasPreload() => CheckTarget("TransitiveAliasPreload");

    [TestMethod]
    public void CheckTupleDestructureEscapingLocal() => CheckTarget("TupleDestructureEscapingLocal");

    [TestMethod]
    public void CheckTupleMixedDeclareReassign() => CheckTarget("TupleMixedDeclareReassign");

    [TestMethod]
    public void CheckTupleSpreadIntoCall() => CheckTarget("TupleSpreadIntoCall");

    [TestMethod]
    public void CheckTypeAssert() => CheckTarget("TypeAssert");

    [TestMethod]
    public void CheckTypeConversion() => CheckTarget("TypeConversion");

    [TestMethod]
    public void CheckTypeConversionInterfaceParam() => CheckTarget("TypeConversionInterfaceParam");

    [TestMethod]
    public void CheckTypeInference() => CheckTarget("TypeInference");

    [TestMethod]
    public void CheckTypeSwitch() => CheckTarget("TypeSwitch");

    [TestMethod]
    public void CheckTypeSwitchBindingAddress() => CheckTarget("TypeSwitchBindingAddress");

    [TestMethod]
    public void CheckTypeSwitchGuardShadow() => CheckTarget("TypeSwitchGuardShadow");

    [TestMethod]
    public void CheckTypeSwitchImpureTag() => CheckTarget("TypeSwitchImpureTag");

    [TestMethod]
    public void CheckTypeSwitchMultiCase() => CheckTarget("TypeSwitchMultiCase");

    [TestMethod]
    public void CheckTypeSwitchNamedInterfaceCase() => CheckTarget("TypeSwitchNamedInterfaceCase");

    [TestMethod]
    public void CheckTypeSwitchPointerAdapter() => CheckTarget("TypeSwitchPointerAdapter");

    [TestMethod]
    public void CheckTypeSwitchTagShadowRename() => CheckTarget("TypeSwitchTagShadowRename");

    [TestMethod]
    public void CheckTypedErrorAssertThroughAdapter() => CheckTarget("TypedErrorAssertThroughAdapter");

    [TestMethod]
    public void CheckTypedNilFuncBoundaries() => CheckTarget("TypedNilFuncBoundaries");

    [TestMethod]
    public void CheckTypedNilInterface() => CheckTarget("TypedNilInterface");

    [TestMethod]
    public void CheckTypedNilPtrArrayDims() => CheckTarget("TypedNilPtrArrayDims");

    [TestMethod]
    public void CheckTypedNilPtrArrayPositions() => CheckTarget("TypedNilPtrArrayPositions");

    [TestMethod]
    public void CheckTypedPointerCastDeref() => CheckTarget("TypedPointerCastDeref");

    [TestMethod]
    public void CheckUdpLoopbackRoundTrip() => CheckTarget("UdpLoopbackRoundTrip");

    [TestMethod]
    public void CheckUdpWriteMsgAddrPort() => CheckTarget("UdpWriteMsgAddrPort");

    [TestMethod]
    public void CheckUintptrUnsafePointerIdiom() => CheckTarget("UintptrUnsafePointerIdiom");

    [TestMethod]
    public void CheckUncomparableEquality() => CheckTarget("UncomparableEquality");

    [TestMethod]
    public void CheckUnexportedEmbeddedMarker() => CheckTarget("UnexportedEmbeddedMarker");

    [TestMethod]
    public void CheckUnexportedIfaceDynamicAssert() => CheckTarget("UnexportedIfaceDynamicAssert");

    [TestMethod]
    public void CheckUnicodeConsoleOutput() => CheckTarget("UnicodeConsoleOutput");

    [TestMethod]
    public void CheckUnixAbstractAddrName() => CheckTarget("UnixAbstractAddrName");

    [TestMethod]
    public void CheckUnnamedMapNilConversion() => CheckTarget("UnnamedMapNilConversion");

    [TestMethod]
    public void CheckUnnamedParams() => CheckTarget("UnnamedParams");

    [TestMethod]
    public void CheckUnsafeBuiltinIntegerLen() => CheckTarget("UnsafeBuiltinIntegerLen");

    [TestMethod]
    public void CheckUnsafeOperations() => CheckTarget("UnsafeOperations");

    [TestMethod]
    public void CheckUnsafePointerArgPassing() => CheckTarget("UnsafePointerArgPassing");

    [TestMethod]
    public void CheckUnsafePointerInferredNoImport() => CheckTarget("UnsafePointerInferredNoImport");

    [TestMethod]
    public void CheckUnsafePointerKeywordParam() => CheckTarget("UnsafePointerKeywordParam");

    [TestMethod]
    public void CheckUnsafePointerParamPin() => CheckTarget("UnsafePointerParamPin");

    [TestMethod]
    public void CheckUnsafePointerPrint() => CheckTarget("UnsafePointerPrint");

    [TestMethod]
    public void CheckUnsafePointerWordRead() => CheckTarget("UnsafePointerWordRead");

    [TestMethod]
    public void CheckUnsafeSliceDataAliasing() => CheckTarget("UnsafeSliceDataAliasing");

    [TestMethod]
    public void CheckUnsafeStringEmpty() => CheckTarget("UnsafeStringEmpty");

    [TestMethod]
    public void CheckUnsignedNamedNumeric() => CheckTarget("UnsignedNamedNumeric");

    [TestMethod]
    public void CheckUntypedConstArithmetic() => CheckTarget("UntypedConstArithmetic");

    [TestMethod]
    public void CheckUntypedConstDefine() => CheckTarget("UntypedConstDefine");

    [TestMethod]
    public void CheckUntypedConstFloatContext() => CheckTarget("UntypedConstFloatContext");

    [TestMethod]
    public void CheckUntypedConstWideMask() => CheckTarget("UntypedConstWideMask");

    [TestMethod]
    public void CheckUntypedFloatConstExpr() => CheckTarget("UntypedFloatConstExpr");

    [TestMethod]
    public void CheckUntypedFloatDefault() => CheckTarget("UntypedFloatDefault");

    [TestMethod]
    public void CheckUntypedIntFloatContexts() => CheckTarget("UntypedIntFloatContexts");

    [TestMethod]
    public void CheckUntypedIntInterfaceBox() => CheckTarget("UntypedIntInterfaceBox");

    [TestMethod]
    public void CheckUntypedIntWideShift() => CheckTarget("UntypedIntWideShift");

    [TestMethod]
    public void CheckUntypedNestedSliceComposite() => CheckTarget("UntypedNestedSliceComposite");

    [TestMethod]
    public void CheckValueAdapterDynamicType() => CheckTarget("ValueAdapterDynamicType");

    [TestMethod]
    public void CheckVarNamedAsType() => CheckTarget("VarNamedAsType");

    [TestMethod]
    public void CheckVariableCapture() => CheckTarget("VariableCapture");

    [TestMethod]
    public void CheckVariadicBoxReceiver() => CheckTarget("VariadicBoxReceiver");

    [TestMethod]
    public void CheckVariadicClosureSpread() => CheckTarget("VariadicClosureSpread");

    [TestMethod]
    public void CheckVariadicFuncFields() => CheckTarget("VariadicFuncFields");

    [TestMethod]
    public void CheckVariadicFuncTypeAssert() => CheckTarget("VariadicFuncTypeAssert");

    [TestMethod]
    public void CheckVariadicFuncValues() => CheckTarget("VariadicFuncValues");

    [TestMethod]
    public void CheckVariadicPointerParam() => CheckTarget("VariadicPointerParam");

    [TestMethod]
    public void CheckVariadicSlotInterfaces() => CheckTarget("VariadicSlotInterfaces");

    [TestMethod]
    public void CheckVersionedImport() => CheckTarget("VersionedImport");

    [TestMethod]
    public void CheckWritevIovecSeam() => CheckTarget("WritevIovecSeam");

    [TestMethod]
    public void CheckWrittenCaptureParam() => CheckTarget("WrittenCaptureParam");

    [TestMethod]
    public void CheckWsaProtocolInfo() => CheckTarget("WsaProtocolInfo");

    [TestMethod]
    public void CheckZeroSizeFieldLayout() => CheckTarget("ZeroSizeFieldLayout");

    [TestMethod]
    public void CheckZeroValueArrayField() => CheckTarget("ZeroValueArrayField");

    [TestMethod]
    public void CheckZeroValueArrayNamedResult() => CheckTarget("ZeroValueArrayNamedResult");

    [TestMethod]
    public void CheckZeroValueStructVar() => CheckTarget("ZeroValueStructVar");

    // </TestMethods>

    private void CheckTarget(string targetProject)
    {
        SkipIfPlatformExclusive(targetProject);

        string projPath = Path.GetFullPath($"{TestRootPath}{targetProject}");

        // Transpile project, if needed
        TranspileProject(targetProject);

        // Compile C# project, if needed
        CompileCSProject(targetProject);

        // Compile Go project
        CompileGoProject(targetProject);

        // Set stop watch for performance measurement
        Stopwatch stopwatch = new();

        string csExe = GetCSExeFile(projPath, targetProject);
        Assert.IsTrue(File.Exists(csExe), $"Expected C# executable does not exist: {csExe}");

        StringBuilder csOutput = new();
        StringBuilder csError = new();

        stopwatch.Start();
        int csExitCode = Exec(csExe, null, Path.GetDirectoryName(projPath), csOutputHandler, RunExecTimeoutMs, csErrorHandler);
        stopwatch.Stop();

        TestContext?.WriteLine($"C# execution Time: {stopwatch.ElapsedMilliseconds:N0} ms");

        string goExe = GetGoExeFile(projPath, targetProject);
        Assert.IsTrue(File.Exists(goExe), $"Expected Go executable does not exist: {goExe}");

        StringBuilder goOutput = new();
        StringBuilder goError = new();

        stopwatch.Restart();
        int goExitCode = Exec(goExe, null, Path.GetDirectoryName(projPath), goOutputHandler, RunExecTimeoutMs, goErrorHandler);
        stopwatch.Stop();

        TestContext?.WriteLine($"Go execution Time: {stopwatch.ElapsedMilliseconds:N0} ms");

        // The Go binary is the oracle: exit codes must MATCH rather than both be zero, so a program
        // that legitimately crashes (e.g. an unrecovered panic exits 2, like Go) is validated
        // differentially instead of being rejected outright.
        Assert.AreEqual(goExitCode, csExitCode, $"Exit code mismatch: C# executable exited {csExitCode:N0}, Go executable exited {goExitCode:N0}");

        Assert.AreEqual(csOutput.ToString(), goOutput.ToString(), "Output mismatch between C# and Go executables");

        // stderr is compared by FIRST LINE only: Go's panic report appends a machine-specific
        // goroutine stack trace, so a full comparison can never match. The first line carries the
        // deterministic report (e.g. "panic: goroutine boom") and is empty for clean runs.
        Assert.AreEqual(FirstLine(goError), FirstLine(csError), "stderr first-line mismatch between C# and Go executables");

        return;

        void csOutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is not null)
                csOutput.AppendLine(e.Data);
        }

        void csErrorHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is not null)
                csError.AppendLine(e.Data);
        }

        void goOutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is not null)
                goOutput.AppendLine(e.Data);
        }

        void goErrorHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is not null)
                goError.AppendLine(e.Data);
        }
    }

    private static string FirstLine(StringBuilder buffer)
    {
        string text = buffer.ToString();
        int index = text.IndexOf('\n');

        return (index < 0 ? text : text[..index]).TrimEnd('\r');
    }
}
