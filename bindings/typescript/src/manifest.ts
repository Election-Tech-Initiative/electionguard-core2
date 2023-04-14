import { InternalManifestHandle, ManifestHandle, getInstance } from "./wasm";

export class ManifestConverter {
  static async fromJson(json: string): Promise<ManifestHandle> {
    var result = (await getInstance()).Manifest.fromJson(json);
    return result;
  }
}

export class InternalManifestConverter {
  static async fromJson(json: string): Promise<InternalManifestHandle> {
    var result = (await getInstance()).InternalManifest.fromJson(json);
    return result;
  }
}
