import Module from "./wasm/electionguard.wasm";
import { InternalManifest, Manifest } from "./wasm/electionguard";

// TODO: wrap the library in a promise

export class ManifestConverter {
  static fromJson(json: string): Manifest {
    var result = Module.Manifest.fromJson(json);
    return result;
  }
}

export class InternalManifestConverter {
  static fromJson(json: string): InternalManifest {
    var result = Module.InternalManifest.fromJson(json);
    return result;
  }
}
