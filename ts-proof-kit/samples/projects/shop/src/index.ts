import catalog from "./catalog.json";
import { Cart } from "@models/cart";
import type { Product } from "@models/product";
import { money, log, Level } from "./util/format";

const products = catalog as Product[];
const cart = new Cart("USD");
for (const p of products) cart.add(p, 2);
log(Level.Info, BUILD_ID, money(cart.total(), cart.currency));

// Deliberate errors: a wrong argument type and a missing property.
cart.add(products[0], "3");
const bad: Product = { sku: "C-3", name: "Ink" };
export { cart, bad };
