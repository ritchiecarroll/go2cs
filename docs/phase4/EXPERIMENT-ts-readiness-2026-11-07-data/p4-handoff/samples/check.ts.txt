let x: number = "a";

function len(s: string): number { return s.length; }
const y: string = len("hi");

interface Point { x: number; y: number }
const p: Point = { x: 1, y: "2" };
