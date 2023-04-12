import Module from "./wasm/electionguard.wasm";
import { PlaintextBallot } from "./wasm/electionguard";

// TODO: wrap the library in a promise

export function fromJson(json: string): PlaintextBallot {
  var result = Module.PlaintextBallot.fromJson(json);
  return result;
}
