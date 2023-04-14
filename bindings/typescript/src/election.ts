import { CiphertextElectionContextHandle, getInstance } from "./wasm";

export class ElectionContextConverter {
  static async fromJson(
    json: string
  ): Promise<CiphertextElectionContextHandle> {
    var result = (await getInstance()).CiphertextElectionContext.fromJson(json);
    return result;
  }
}
