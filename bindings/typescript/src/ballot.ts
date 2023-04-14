import {
  CiphertextBallotHandle,
  PlaintextBallotHandle,
  getInstance,
} from "./wasm";

export class PlaintextBallot {
  _handle: PlaintextBallotHandle;

  constructor(handle: PlaintextBallotHandle) {
    this._handle = handle;
  }

  get objectId(): string {
    return this._handle.getObjectId();
  }

  get styleId(): string {
    return this._handle.getObjectId();
  }

  toJson(): string {
    return this._handle.toJson();
  }

  static async fromJson(json: string): Promise<PlaintextBallot> {
    var result = (await getInstance()).PlaintextBallot.fromJson(json);
    return new PlaintextBallot(result);
  }
}

export class PlaintextBallotConverter {
  static async fromJson(json: string): Promise<PlaintextBallot> {
    var result = (await getInstance()).PlaintextBallot.fromJson(json);
    return new PlaintextBallot(result);
  }
}

export class CiphertextBallotConverter {
  static async fromJson(json: string): Promise<CiphertextBallotHandle> {
    var result = (await getInstance()).CiphertextBallot.fromJson(json);
    return result;
  }
}
