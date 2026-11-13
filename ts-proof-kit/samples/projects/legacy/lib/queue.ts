function sealed(ctor: Function) { Object.seal(ctor); Object.seal(ctor.prototype); }
@sealed
export class Queue<T> {
    private items: T[] = [];
    push(...xs: T[]): number { return this.items.push(...xs); }
    shift(): T | undefined { return this.items.shift(); }
    async drain(f: (x: T) => Promise<void>): Promise<number> {
        let n = 0;
        for (let x = this.shift(); x !== undefined; x = this.shift()) { await f(x); n++; }
        return n;
    }
    get size() { return this.items.length; }
}
export default Queue;
