import Module from "./wasm/electionguard.wasm";
import { CiphertextBallot, PlaintextBallot } from "./wasm/electionguard";

// TODO: wrap the library in a promise

export class PlaintextBallotConverter {
  static fromJson(json: string): PlaintextBallot {
    var result = Module.PlaintextBallot.fromJson(json);
    return result;
  }
}

export class CiphertextBallotConverter {
  static fromJson(json: string): CiphertextBallot {
    var result = Module.CiphertextBallot.fromJson(json);
    return result;
  }
}
