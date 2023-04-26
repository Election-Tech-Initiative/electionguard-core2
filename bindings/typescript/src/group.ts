import { ElementModPHandle, ElementModQHandle, getInstance } from "./wasm";

export class ElementModP {
  _handle: ElementModPHandle;

  constructor(handle: ElementModPHandle) {
    this._handle = handle;
  }

  copy(): Promise<ElementModP> {
    var hex = this._handle.toHex();
    return ElementModP.fromHex(hex);
  }

  toHex(): string {
    return this._handle.toHex();
  }

  static async fromNumber(a: number): Promise<ElementModP> {
    var result = await this.fromBigInt(BigInt(a));
    return result;
  }
  static async fromBigInt(a: BigInt): Promise<ElementModP> {
    var result = (await getInstance()).ElementModP.fromUint64(a, false);
    return new ElementModP(result);
  }
  static async fromHex(a: string): Promise<ElementModP> {
    var result = (await getInstance()).ElementModP.fromHex(a, false);
    return new ElementModP(result);
  }
}

export class ElementModQ {
  _handle: ElementModQHandle;

  constructor(handle: ElementModQHandle) {
    this._handle = handle;
  }

  copy(): Promise<ElementModQ> {
    var hex = this._handle.toHex();
    return ElementModQ.fromHex(hex);
  }

  toHex(): string {
    return this._handle.toHex();
  }

  static async fromNumber(a: number): Promise<ElementModQ> {
    var result = await this.fromBigInt(BigInt(a));
    return result;
  }
  static async fromBigInt(a: BigInt): Promise<ElementModQ> {
    var result = (await getInstance()).ElementModQ.fromUint64(a, false);
    return new ElementModQ(result);
  }
  static async fromHex(a: string): Promise<ElementModQ> {
    var result = (await getInstance()).ElementModQ.fromHex(a, false);
    return new ElementModQ(result);
  }
}

export class GroupFunctions {
  static async addModQ(a: number, b: number): Promise<BigInt> {
    var elem1 = await ElementModQ.fromNumber(a);
    var elem2 = await ElementModQ.fromNumber(b);
    var result = (await getInstance()).GroupFunctions.addModQ(
      elem1._handle,
      elem2._handle
    );
    return BigInt("0x" + result.toHex());
  }

  static async randomElementModP(): Promise<ElementModP> {
    var result = (await getInstance()).GroupFunctions.randomElementModP();
    return new ElementModP(result);
  }

  static async randomElementModQ(): Promise<ElementModQ> {
    var result = (await getInstance()).GroupFunctions.randomElementModQ();
    return new ElementModQ(result);
  }
}
