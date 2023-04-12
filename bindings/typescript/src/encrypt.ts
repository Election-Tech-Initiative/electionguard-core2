import Module from "./wasm/electionguard.wasm";
import {
  CiphertextElectionContext,
  EncryptionDevice,
  EncryptionMediator,
  InternalManifest,
} from "./wasm/electionguard";

// TODO: wrap the library in a promise

export class EncryptionDeviceConverter {
  static fromJson(json: string): EncryptionDevice {
    var result = Module.EncryptionDevice.fromJson(json);
    return result;
  }
}

export class EncryptionMediatorConverter {
  static make(
    internalManifest: InternalManifest,
    contest: CiphertextElectionContext,
    device: EncryptionDevice
  ): EncryptionMediator {
    var result = new Module.EncryptionMediator(
      internalManifest,
      contest,
      device
    );
    return result;
  }
}
