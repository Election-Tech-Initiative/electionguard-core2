import {
  CiphertextElectionContextHandle,
  EncryptionDeviceHandle,
  EncryptionMediatorHandle,
  InternalManifestHandle,
  getInstance,
} from "./wasm";

export class EncryptionDeviceConverter {
  static async fromJson(json: string): Promise<EncryptionDeviceHandle> {
    var result = (await getInstance()).EncryptionDevice.fromJson(json);
    return result;
  }
}

export class EncryptionMediatorConverter {
  static async make(
    internalManifest: InternalManifestHandle,
    contest: CiphertextElectionContextHandle,
    device: EncryptionDeviceHandle
  ): Promise<EncryptionMediatorHandle> {
    var result = new (await getInstance()).EncryptionMediator(
      internalManifest,
      contest,
      device
    );
    return result;
  }
}
