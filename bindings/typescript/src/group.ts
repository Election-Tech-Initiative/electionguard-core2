import { ElementModP, ElementModQ } from "./wasm/electionguard";
import Module from "./wasm/electionguard.wasm";

// TODO: wrap the library in a promise

export class ElementModPConverter {
  static fromNumber(a: number): ElementModP {
    var result = ElementModPConverter.fromBigInt(BigInt(a));
    return result;
  }
  static fromBigInt(a: BigInt): ElementModP {
    var result = Module.ElementModP.fromUint64(a, false);
    return result;
  }
  static fromHex(a: string): ElementModP {
    var result = Module.ElementModP.fromHex(a, false);
    return result;
  }
}

export class ElementModQConverter {
  static fromNumber(a: number): ElementModQ {
    var result = ElementModQConverter.fromBigInt(BigInt(a));
    return result;
  }
  static fromBigInt(a: BigInt): ElementModQ {
    var result = Module.ElementModQ.fromUint64(a, false);
    return result;
  }
  static fromHex(a: string): ElementModQ {
    var result = Module.ElementModQ.fromHex(a, false);
    return result;
  }
}

export class GroupFunctions {
  static addModQ(a: number, b: number): BigInt {
    var elem1 = ElementModQConverter.fromNumber(a);
    var elem2 = ElementModQConverter.fromNumber(b);
    var result = Module.GroupFunctions.addModQ(elem1, elem2);
    return BigInt("0x" + result.toHex());
  }
}
