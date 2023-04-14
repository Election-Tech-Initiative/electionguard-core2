import { ElementModPHandle, ElementModQHandle, getInstance } from "./wasm";

export class ElementModPConverter {
  static async fromNumber(a: number): Promise<ElementModPHandle> {
    var result = await ElementModPConverter.fromBigInt(BigInt(a));
    return result;
  }
  static async fromBigInt(a: BigInt): Promise<ElementModPHandle> {
    var result = (await getInstance()).ElementModP.fromUint64(a, false);
    return result;
  }
  static async fromHex(a: string): Promise<ElementModPHandle> {
    var result = (await getInstance()).ElementModP.fromHex(a, false);
    return result;
  }
}

export class ElementModQConverter {
  static async fromNumber(a: number): Promise<ElementModQHandle> {
    var result = await ElementModQConverter.fromBigInt(BigInt(a));
    return result;
  }
  static async fromBigInt(a: BigInt): Promise<ElementModQHandle> {
    var result = (await getInstance()).ElementModQ.fromUint64(a, false);
    return result;
  }
  static async fromHex(a: string): Promise<ElementModQHandle> {
    var result = (await getInstance()).ElementModQ.fromHex(a, false);
    return result;
  }
}

export class GroupFunctions {
  static async addModQ(a: number, b: number): Promise<BigInt> {
    var elem1 = await ElementModQConverter.fromNumber(a);
    var elem2 = await ElementModQConverter.fromNumber(b);
    var result = (await getInstance()).GroupFunctions.addModQ(elem1, elem2);
    return BigInt("0x" + result.toHex());
  }
}
