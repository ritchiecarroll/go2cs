import Queue from "./lib/queue";
import * as math from "./lib/math";
import { missing } from "./lib/nowhere";

const unused = 1;
const q = new Queue<number>();
q.push(12, 18, 27);
const g = math.gcd(q.shift()!, q.shift()!);
let maybe: string | undefined;
console.log(g, math.sum([1, 2, 3]), maybe.length, missing);
q.drain(async x => { console.log(x); }).then(n => console.log(`drained ${n}`));
