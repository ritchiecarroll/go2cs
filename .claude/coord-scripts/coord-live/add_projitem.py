p = 'src/go2cs/go2cs-src.projitems'
b = open(p, 'rb').read()
assert b.startswith(b'\xef\xbb\xbf') and b'\r\n' in b
anchor = b'    <None Include="$(MSBuildThisFileDirectory)typedNilInterfaceBoxing.go" />\r\n'
new = b'    <None Include="$(MSBuildThisFileDirectory)typedNilUnsafePointer_test.go" />\r\n'
assert b.count(anchor) == 1 and new not in b
open(p, 'wb').write(b.replace(anchor, anchor + new))
print('inserted after typedNilInterfaceBoxing.go (BOM and CRLF preserved)')
