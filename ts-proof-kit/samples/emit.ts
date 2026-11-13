// Emit-heavy sample: enums, namespaces, parameter properties, overloads, generics, async, private names.
export enum Color { Red, Green = 4, Blue }
export const enum Flags { None = 0, A = 1 << 0, B = 1 << 1, AB = A | B }
export namespace Geometry {
    export interface Vec { x: number; y: number }
    export const origin: Vec = { x: 0, y: 0 };
    export function add(a: Vec, b: Vec): Vec { return { x: a.x + b.x, y: a.y + b.y }; }
    export namespace Inner { export let depth = 2; }
}
export abstract class Shape {
    #id: number;
    static count = 0;
    constructor(public readonly name: string, protected sides?: number) { this.#id = ++Shape.count; }
    abstract area(): number;
    get id() { return this.#id; }
}
export class Square extends Shape {
    constructor(private side: number) { super("square", 4); }
    area() { return this.side ** 2; }
}
export function pick(x: string): string;
export function pick(x: number): number;
export function pick(x: any) { return x ?? Flags.AB; }
export async function load<T extends object>(f: () => Promise<T>): Promise<Partial<T> | undefined> {
    try { const v = await f(); return v?.constructor ? { ...v } : undefined; } catch { return undefined; } finally { }
}
export function* range(n: number): Generator<number, void, unknown> { for (let i = 0; i < n; i++) yield i; }
export type Mapped<T> = { readonly [K in keyof T]?: T[K] extends Function ? never : T[K] };
let tuple: [a: string, b?: number, ...rest: boolean[]] = ["x"];
export default tuple;
