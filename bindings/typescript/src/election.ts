import { ElementModP, ElementModQ } from "./group";
import { CiphertextElectionContextHandle, getInstance } from "./wasm";

export class ElectionContext {
  _handle: CiphertextElectionContextHandle;

  constructor(handle: CiphertextElectionContextHandle) {
    this._handle = handle;
  }

  /**
   * @brief The number of guardians necessary to generate the public key
   */
  get numberOfGuardians(): number {
    return this._handle.getNumberOfGuardians();
  }

  /**
   * The `quorum` of guardians necessary to decrypt an election.
   */
  get quorum(): number {
    return this._handle.getNumberOfGuardians();
  }

  /**
   * The `joint public key (K)` in the [ElectionGuard Spec](https://github.com/microsoft/electionguard/wiki)
   */
  get publicKey(): ElementModP {
    var result = this._handle.getElGamalPublicKey();
    return new ElementModP(result);
  }

  /**
   * The hash of the election metadata
   */
  get manifestHash(): ElementModQ {
    var result = this._handle.getManifestHash();
    return new ElementModQ(result);
  }

  /**
   * The `extended base hash code (𝑄')` in the [ElectionGuard Spec](https://github.com/microsoft/electionguard/wiki)
   */
  get cryptoExtendedBaseHash(): ElementModQ {
    var result = this._handle.getCryptoExtendedBaseHash();
    return new ElementModQ(result);
  }

  /**
   * @brief Creates a CiphertextElectionContext object from a [RFC-8259](https://www.rfc-editor.org/rfc/rfc8259.html#section-8.1) UTF-8 encoded JSON string
   */
  static async fromJson(json: string): Promise<ElectionContext> {
    var result = (await getInstance()).CiphertextElectionContext.fromJson(json);
    return new ElectionContext(result);
  }
}
