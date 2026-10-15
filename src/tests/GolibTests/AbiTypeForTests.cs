using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using abi = go.@internal.abi_package;
using reflect = go.reflect_package;

namespace GolibTests;

// Go 1.26.9 rewrote internal/abi.TypeFor as `(*PtrType)(unsafe.Pointer(TypeOf((*T)(nil)))).Elem` (type.go:193):
// a PREFIX DOWNCAST from the Type header to the PtrType record that embeds it. The managed descriptor is
// SYNTHESIZED as a plain Type from a System.Type, so no PtrType stands behind it, and the converted
// `Reinterpret<Type, PtrType>().Value.Elem` panicked ("dereference of a managed pointer with no address
// (*PtrType over an order token)") on EVERY reflect.TypeFor[T]() -- H7 row 57. The same family's
// InterfaceType() (`(*InterfaceType)(unsafe.Pointer(t))`) is reached for every interface descriptor.
// Both are answered from the carried System.Type, like Elem/StructType/MapType (type_impl.cs).
[TestClass]
public class AbiTypeForTests
{
    [TestMethod]
    public void TypeForAStructAnswersItsOwnDescriptor()
    {
        ж<abi.Type> descriptor = abi.TypeFor<time_package.Time>();

        Assert.IsNotNull(descriptor, "abi.TypeFor[time.Time]() answered nil");
        Assert.AreEqual(abi.Struct, descriptor.Value.Kind(), "abi.TypeFor[time.Time]() is not a struct descriptor");
        Assert.AreEqual(abi.TypeOf(default(time_package.Time)).Value.Kind(), descriptor.Value.Kind(),
            "TypeFor and TypeOf disagree for the same struct type");
    }

    [TestMethod]
    public void ReflectTypeForAStructNamesTheType()
    {
        reflect.ΔType t = reflect.TypeFor<time_package.Time>();

        Assert.AreEqual(reflect.Struct, t.Kind(), "reflect.TypeFor[time.Time]().Kind()");
        Assert.AreEqual("Time", t.Name().ToString(), "reflect.TypeFor[time.Time]().Name()");
        Assert.AreEqual("time.Time", t.String().ToString(), "reflect.TypeFor[time.Time]().String()");
    }

    [TestMethod]
    public void InterfaceTypeOfAnInterfaceDescriptorIsNotNilAndCarriesItsMethods()
    {
        ж<abi.Type> descriptor = abi.TypeFor<error>();

        Assert.IsNotNull(descriptor, "abi.TypeFor[error]() answered nil");
        Assert.AreEqual(abi.Interface, descriptor.Value.Kind(), "abi.TypeFor[error]() is not an interface descriptor");

        ж<abi.ΔInterfaceType> iface = abi.InterfaceType(descriptor);

        Assert.IsNotNull(iface, "InterfaceType() of an interface descriptor answered nil");
        Assert.AreEqual(1, (int)iface.NumMethod(), "error's interface descriptor does not carry its one method");

        // Go's own "or nil if its tag does not match": a struct descriptor has no InterfaceType.
        Assert.IsNull(abi.InterfaceType(abi.TypeFor<time_package.Time>()), "InterfaceType() of a struct descriptor is not nil");
    }
}
