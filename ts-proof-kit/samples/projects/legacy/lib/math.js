// @ts-check
/**
 * @param {number} a
 * @param {number} b
 * @returns {number}
 */
function gcd(a, b) { return b === 0 ? a : gcd(b, a % b); }

/** @type {(xs: number[]) => number} */
const sum = xs => xs.reduce((s, x) => s + x, 0);

module.exports = { gcd, sum };
gcd("1", 2);
