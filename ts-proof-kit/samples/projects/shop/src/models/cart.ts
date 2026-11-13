import type { Product, Currency } from "./product";
import { Stock, stockOf } from "./product";
export class Cart {
    private lines = new Map<string, { product: Product; qty: number }>();
    constructor(public readonly currency: Currency) {}
    add(product: Product, qty = 1): this {
        const line = this.lines.get(product.sku);
        if (line) line.qty += qty; else this.lines.set(product.sku, { product, qty });
        return this;
    }
    total(): number {
        let sum = 0;
        for (const { product, qty } of this.lines.values()) {
            if (product.price.currency !== this.currency) throw new Error(`mixed currency: ${product.sku}`);
            sum += product.price.amount * qty;
        }
        return Math.round(sum * 100) / 100;
    }
    availability(stock: Record<string, number>): Stock[] {
        return [...this.lines.keys()].map(sku => stockOf(stock[sku] ?? 0));
    }
}
