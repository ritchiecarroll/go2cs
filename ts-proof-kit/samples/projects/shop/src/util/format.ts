export function money(amount: number, currency: string): string {
    return `${currency} ${amount.toFixed(2)}`;
}
export const enum Level { Info = "info", Warn = "warn" }
export function log(level: Level, ...parts: unknown[]): void {
    console.log(`[${level}]`, ...parts);
}
