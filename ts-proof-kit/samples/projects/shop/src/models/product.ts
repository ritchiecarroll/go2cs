export type Currency = "USD" | "EUR";
export interface Product {
    readonly sku: string;
    name: string;
    price: { amount: number; currency: Currency };
    tags?: string[];
}
export enum Stock { Out, Low, Plenty }
export function stockOf(qty: number): Stock {
    return qty <= 0 ? Stock.Out : qty < 5 ? Stock.Low : Stock.Plenty;
}
