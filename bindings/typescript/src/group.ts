import Module from "./wasm/electionguard.wasm";

// TODO: wrap the library in a promise

export function addModQ(a: number, b: number): BigInt {
  var elem1 = Module.ElementModQ.fromUint64(BigInt(a), false);
  var elem2 = Module.ElementModQ.fromUint64(BigInt(b), false);
  var result = Module.GroupFunctions.addModQ(elem1, elem2);
  return BigInt("0x" + result.toHex());
}
