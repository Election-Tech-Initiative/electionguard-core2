import Module from "./wasm/electionguard.wasm";
import { CiphertextElectionContext } from "./wasm/electionguard";

// TODO: wrap the library in a promise

export class ElectionContextConverter {
  static fromJson(json: string): CiphertextElectionContext {
    var result = Module.CiphertextElectionContext.fromJson(json);
    return result;
  }
}
