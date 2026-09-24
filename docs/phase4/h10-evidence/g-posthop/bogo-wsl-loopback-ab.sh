set -u
export GOROOT=$HOME/sdk/go1.24.13 PATH=$HOME/sdk/go1.24.13/bin:$PATH GOTOOLCHAIN=local CGO_ENABLED=0; unset GOFLAGS
W=$HOME/h10/bogo-ab; rm -rf $W; mkdir -p $W
cp /etc/hosts $W/hosts.control
cp /etc/hosts $W/hosts.v6; printf '::1 localhost\n' >> $W/hosts.v6
cat > $W/res.go <<'G'
package main
import ("fmt";"net")
func main(){ a,err:=net.DefaultResolver.LookupHost(nil,"localhost"); fmt.Println("localhost ->",a,err) }
G
for arm in control v6; do
  unshare -rm bash -c "mount --bind $W/hosts.$arm /etc/hosts && cd /tmp && go run $W/res.go && t0=\$(date +%s) && go test -count=1 -run '^TestBogoSuite\$' -v crypto/tls > $W/$arm.log 2>&1; echo \"rc=\$? wall=\$(( \$(date +%s) - t0 ))s\"" | sed "s/^/$arm: /"
  L=$W/$arm.log
  echo "$arm: RUN $(grep -c '^=== RUN   TestBogoSuite/' $L) PASS $(grep -cE '^\s+--- PASS: TestBogoSuite/' $L) FAIL $(grep -cE '^\s+--- FAIL: TestBogoSuite/' $L) SKIP $(grep -cE '^\s+--- SKIP: TestBogoSuite/' $L) refused $(grep -c 'connection refused' $L) root: $(grep -E '^--- (PASS|FAIL|SKIP): TestBogoSuite ' $L | cut -c1-40)"
done
echo "system hosts untouched: $(cmp -s /etc/hosts $W/hosts.control && echo yes || echo NO)"
