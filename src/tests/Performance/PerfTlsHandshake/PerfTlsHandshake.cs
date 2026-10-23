namespace go;

using tls = crypto.tls_package;
using fmt = fmt_package;
using Δio = io_package;
using Δnet = net_package;
using strings = strings_package;
using time = time_package;
using System.Runtime.CompilerServices;
using crypto;

partial class main_package {

internal static slice<byte> localhostCert = slice<byte>("""

-----BEGIN CERTIFICATE-----
MIIDSDCCAjCgAwIBAgIQEP/md970HysdBTpuzDOf0DANBgkqhkiG9w0BAQsFADAS
MRAwDgYDVQQKEwdBY21lIENvMCAXDTcwMDEwMTAwMDAwMFoYDzIwODQwMTI5MTYw
MDAwWjASMRAwDgYDVQQKEwdBY21lIENvMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A
MIIBCgKCAQEAxcl69ROJdxjN+MJZnbFrYxyQooADCsJ6VDkuMyNQIix/Hk15Nk/u
FyBX1Me++aEpGmY3RIY4fUvELqT/srvAHsTXwVVSttMcY8pcAFmXSqo3x4MuUTG/
jCX3Vftj0r3EM5M8ImY1rzA/jqTTLJg00rD+DmuDABcqQvoXw/RV8w1yTRi5BPoH
DFD/AWTt/YgMvk1l2Yq/xI8VbMUIpjBoGXxWsSevQ5i2s1mk9/yZzu0Ysp1tTlzD
qOPa4ysFjBitdXiwfxjxtv5nXqOCP5rheKO0sWLk0fetMp1OV5JSJMAJw6c2ZMkl
U2WMqAEpRjdE/vHfIuNg+yGaRRqI07NZRQIDAQABo4GXMIGUMA4GA1UdDwEB/wQE
AwICpDATBgNVHSUEDDAKBggrBgEFBQcDATAPBgNVHRMBAf8EBTADAQH/MB0GA1Ud
DgQWBBQR5QIzmacmw78ZI1C4MXw7Q0wJ1jA9BgNVHREENjA0ggtleGFtcGxlLmNv
bYINKi5leGFtcGxlLmNvbYcEfwAAAYcQAAAAAAAAAAAAAAAAAAAAATANBgkqhkiG
9w0BAQsFAAOCAQEACrRNgiioUDzxQftd0fwOa6iRRcPampZRDtuaF68yNHoNWbOu
LUwc05eOWxRq3iABGSk2xg+FXM3DDeW4HhAhCFptq7jbVZ+4Jj6HeJG9mYRatAxR
Y/dEpa0D0EHhDxxVg6UzKOXB355n0IetGE/aWvyTV9SiDs6QsaC57Q9qq1/mitx5
2GFBoapol9L5FxCc77bztzK8CpLujkBi25Vk6GAFbl27opLfpyxkM+rX/T6MXCPO
6/YBacNZ7ff1/57Etg4i5mNA6ubCpuc4Gi9oYqCNNohftr2lkJr7REdDR6OW0lsL
rF7r4gUnKeC7mYIH1zypY7laskopiLFAfe96Kg==
-----END CERTIFICATE-----

"""u8.Raw());

internal static slice<byte> localhostKey = slice<byte>(testingKey("""

-----BEGIN RSA TESTING KEY-----
MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDFyXr1E4l3GM34
wlmdsWtjHJCigAMKwnpUOS4zI1AiLH8eTXk2T+4XIFfUx775oSkaZjdEhjh9S8Qu
pP+yu8AexNfBVVK20xxjylwAWZdKqjfHgy5RMb+MJfdV+2PSvcQzkzwiZjWvMD+O
pNMsmDTSsP4Oa4MAFypC+hfD9FXzDXJNGLkE+gcMUP8BZO39iAy+TWXZir/EjxVs
xQimMGgZfFaxJ69DmLazWaT3/JnO7RiynW1OXMOo49rjKwWMGK11eLB/GPG2/mde
o4I/muF4o7SxYuTR960ynU5XklIkwAnDpzZkySVTZYyoASlGN0T+8d8i42D7IZpF
GojTs1lFAgMBAAECggEAIYthUi1lFBDd5gG4Rzlu+BlBIn5JhcqkCqLEBiJIFfOr
/4yuMRrvS3bNzqWt6xJ9MSAC4ZlN/VobRLnxL/QNymoiGYUKCT3Ww8nvPpPzR9OE
sE68TUL9tJw/zZJcRMKwgvrGqSLimfq53MxxkE+kLdOc0v9C8YH8Re26mB5ZcWYa
7YFyZQpKsQYnsmu/05cMbpOQrQWhtmIqRoyn8mG/par2s3NzjtpSE9NINyz26uFc
k/3ovFJQIHkUmTS7KHD3BgY5vuCqP98HramYnOysJ0WoYgvSDNCWw3037s5CCwJT
gCKuM+Ow6liFrj83RrdKBpm5QUGjfNpYP31o+QNP4QKBgQDSrUQ2XdgtAnibAV7u
7kbxOxro0EhIKso0Y/6LbDQgcXgxLqltkmeqZgG8nC3Z793lhlSasz2snhzzooV5
5fTy1y8ikXqjhG0nNkInFyOhsI0auE28CFoDowaQd+5cmCatpN4Grqo5PNRXxm1w
HktfPEgoP11NNCFHvvN5fEKbbQKBgQDwVlOaV20IvW3IPq7cXZyiyabouFF9eTRo
VJka1Uv+JtyvL2P0NKkjYHOdN8gRblWqxQtJoTNk020rVA4UP1heiXALy50gvj/p
hMcybPTLYSPOhAGx838KIcvGR5oskP1aUCmFbFQzGELxhJ9diVVjxUtbG2DuwPKd
tD9TLxT2OQKBgQCcdlHSjp+dzdgERmBa0ludjGfPv9/uuNizUBAbO6D690psPFtY
JQMYaemgSd1DngEOFVWADt4e9M5Lose+YCoqr+UxpxmNlyv5kzJOFcFAs/4XeglB
PHKdgNW/NVKxMc6H54l9LPr+x05sYdGlEtqnP/3W5jhEvhJ5Vjc8YiyVgQKBgQCl
zwjyrGo+42GACy7cPYE5FeIfIDqoVByB9guC5bD98JXEDu/opQQjsgFRcBCJZhOY
M0UsURiB8ROaFu13rpQq9KrmmF0ZH+g8FSzQbzcbsTLg4VXCDXmR5esOKowFPypr
Sm667BfTAGP++D5ya7MLmCv6+RKQ5XD8uEQQAaV2kQKBgAD8qeJuWIXZT0VKkQrn
nIhgtzGERF/6sZdQGW2LxTbUDWG74AfFkkEbeBfwEkCZXY/xmnYqYABhvlSex8jU
supU6Eea21esIxIub2zv/Np0ojUb6rlqTPS4Ox1E27D787EJ3VOXpriSD10vyNnZ
jel6uj2FOP9g54s+GzlSVg/T
-----END RSA TESTING KEY-----

"""u8.Raw()));

internal static @string testingKey(@string s) {
    return strings.ReplaceAll(s, "TESTING KEY", "PRIVATE KEY");
}

internal static partial (nint, uint16, uint16) handshakes(nint n, tls.Certificate cert) {
    GoFrame ᒐ = default;
    try {
        var serverConf = Ꮡ(new tls.Config(
            Certificates: new tls.Certificate[]{cert}.slice(),
            MinVersion: tls.VersionTLS13,
            MaxVersion: tls.VersionTLS13
        ));
        var clientConf = Ꮡ(new tls.Config(
            InsecureSkipVerify: true,
            MinVersion: tls.VersionTLS13,
            MaxVersion: tls.VersionTLS13
        ));

        var (ln, err) = Δnet.Listen("tcp", "127.0.0.1:0");
        if (err != default!) {
            throw panic(err);
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);

        var done = new channel<EmptyStruct>(0);
        var doneʗ1 = done;
        var lnʗ2 = ln;
        var serverConfʗ1 = serverConf;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => close(ᴛ1), doneʗ1, ref ᒐ);
                for (nint i = 0; i < n; i++) {
                    var (c, errΔ1) = lnʗ2.Accept();
                    if (errΔ1 != default!) {
                        return;
                    }
                    var s = tls.Server(c, serverConfʗ1);
                    {
                        var errΔ2 = s.Handshake(); if (errΔ2 == default!) {
                            Δio.CopyN(Δio.Discard, new tls_ConnжReader(s), 1);
                        }
                    }
                    s.Close();
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });

        nint count = 0;
        uint16 version = default!;
        uint16 suite = default!;
        for (nint i = 0; i < n; i++) {
            var (c, errΔ3) = Δnet.Dial("tcp", ln.Addr().String());
            if (errΔ3 != default!) {
                throw panic(errΔ3);
            }
            var t = tls.Client(c, clientConf);
            {
                var errΔ4 = t.Handshake(); if (errΔ4 != default!) {
                    throw panic(errΔ4);
                }
            }
            var st = t.ConnectionState();
            (version, suite) = (st.Version, st.CipherSuite);
            t.Write(new byte[]{0}.slice());
            t.Close();
            count++;
        }
        ᐸꟷ(done);
        return (count, version, suite);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    var (cert, err) = tls.X509KeyPair(localhostCert, localhostKey);
    if (err != default!) {
        throw panic(err);
    }

    handshakes(1, cert);

    var start = time.Now().UnixNano();
    var (count, version, suite) = handshakes(64, cert);
    var elapsed = time.Now().UnixNano() - start;

    fmt.Println("checksum:", count, version, suite);
    fmt.Println("elapsed_ns:", elapsed);
}

} // end main_package
