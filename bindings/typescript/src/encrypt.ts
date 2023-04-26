import { CiphertextBallot, PlaintextBallot } from "./ballot";
import { ElectionContext } from "./election";
import { ElementModQ } from "./group";
import { InternalManifest } from "./manifest";
import {
  CiphertextElectionContextHandle,
  EncryptionDeviceHandle,
  EncryptionMediatorHandle,
  InternalManifestHandle,
  getInstance,
} from "./wasm";

/**
 * Metadata for encryption device
 *
 * The encryption device is a stateful container that represents abstract hardware
 * authorized to participate in a specific election.
 */
export class EncryptionDevice {
  _handle: EncryptionDeviceHandle;
  constructor(handle: EncryptionDeviceHandle) {
    this._handle = handle;
  }

  // get timestamp(): number {
  //   return this._handle.getTimestamp();
  // }

  /**
   * a unique identifier tied to the device hardware
   */
  get deviceUuid(): number {
    return this._handle.getDeviceUuid();
  }

  /**
   * a unique identifier tied to the runtime session
   */
  get sessionUuid(): number {
    return this._handle.getSessionUuid();
  }

  /**
   * a unique identifer tied to the election
   */
  get launchCode(): number {
    return this._handle.getLaunchCode();
  }

  /**
   * an arbitrary string meaningful to the external system
   * such as a friendly name, description, or some other value
   */
  get location(): string {
    return this._handle.getLocation();
  }

  /**
   * @brief Get the JSON representation of this encrption device
   */
  toJson(): string {
    return this._handle.toJson();
  }

  /**
   * @brief Creates an EncryptionDevice object from a [RFC-8259](https://www.rfc-editor.org/rfc/rfc8259.html#section-8.1) UTF-8 encoded JSON string
   */
  static async fromJson(json: string): Promise<EncryptionDevice> {
    var result = (await getInstance()).EncryptionDevice.fromJson(json);
    return new EncryptionDevice(result);
  }
}

/**
 * An object for caching election and encryption state.
 *
 * the encryption mediator composes ballots by querying the encryption device
 * for a hash of its metadata and incremental timestamps/
 *
 * this is a convenience wrapper around the encrypt methods
 * and may not be suitable for all use cases.
 */
export class EncryptionMediator {
  _handle: EncryptionMediatorHandle;
  constructor(handle: EncryptionMediatorHandle) {
    this._handle = handle;
  }

  /**
   * Encrypt a specific `Ballot` in the context of a specific `CiphertextElectionContext`.
   *
   * This method accepts a ballot representation that only includes `True` selections.
   * It will fill missing selections for a contest with `False` values, and generate `placeholder`
   * selections to represent the number of seats available for a given contest.
   *
   * This method also allows for ballots to exclude passing contests for which the voter made no selections.
   * It will fill missing contests with `False` selections and generate `placeholder` selections that are marked `True`.
   *
   * This function can also take advantage of PrecomputeBuffers to speed up the encryption process.
   * when using precomputed values, the application looks in the `PrecomputeBufferContext` for values
   * and uses them for the encryptions. You must preload the `PrecomputeBufferContext` prior to calling this function
   * with `shouldUsePrecomputedValues` set to `true`, otherwise the function will fall back to realtime generation.
   *
   * @param ballot The plaintext representation of the ballot
   * @param shouldVerifyProofs True if the mediator should verify proofs
   * @param shouldUsePrecomputedValues True if the mediator should use the precomputed values
   **/
  encrypt(
    ballot: PlaintextBallot,
    shouldVerifyProofs: boolean = true,
    shouldUsePrecomputedValues: boolean = false
  ): Promise<CiphertextBallot> {
    const result = this._handle.encrypt(
      ballot._handle,
      shouldVerifyProofs,
      shouldUsePrecomputedValues
    );
    return Promise.resolve(new CiphertextBallot(result));
  }

  static async make(
    internalManifest: InternalManifest,
    contest: ElectionContext,
    device: EncryptionDevice
  ): Promise<EncryptionMediator> {
    var result = new (await getInstance()).EncryptionMediator(
      internalManifest._handle,
      contest._handle,
      device._handle
    );
    return new EncryptionMediator(result);
  }
}

/**
 * A collection of functions for encrypting ballots
 */
export class EncryptFunctions {
  /**
   * Encrypt a specific `Ballot` in the context of a specific `CiphertextElectionContext`.
   *
   * This method accepts a ballot representation that only includes `True` selections.
   * It will fill missing selections for a contest with `False` values, and generate `placeholder`
   * selections to represent the number of seats available for a given contest.
   *
   * This method also allows for ballots to exclude passing contests for which the voter made no selections.
   * It will fill missing contests with `False` selections and generate `placeholder` selections that are marked `True`.
   *
   * Additionally, if the nonce is provided it will be used to determinisitcally construct
   * the ballot in real-time (i.e. the same nonce will always produce the same ballot).
   * If the nonce is not provided, the secret generating mechanism of the OS provides its own.
   *
   * This function can also take advantage of PrecomputeBuffers to speed up the encryption process.
   * when using precomputed values, the application looks in the `PrecomputeBufferContext` for values
   * and uses them for the encryptions. You must preload the `PrecomputeBufferContext` prior to calling this function
   * with `shouldUsePrecomputedValues` set to `true`, otherwise the function will fall back to realtime generation.
   *
   * Because PrecomputeBuffers require a random nonce, calling this function with `shouldUsePrecomputedValues`
   * set to `true` while also providing a nonce will result in an error.
   * @param plaintext: the ballot in the valid input form
   * @param manifest: the `InternalManifest` which defines this ballot's structure
   * @param context: all the cryptographic context for the election
   * @param ballotCodeSeed: Hash from previous ballot or starting hash from device
   * @param nonce: an optional value used to seed the `Nonce` generated for this ballot
   *                if this value is not provided, the secret generating mechanism of the OS provides its own
   * @param timestamp: an optional value used to seed the `Timestamp` generated for this ballot
   * @param shouldVerifyProofs: specify if the proofs should be verified prior to returning (default True)
   */
  static async encryptBallot(
    plaintext: PlaintextBallot,
    internalManifest: InternalManifest,
    context: ElectionContext,
    ballotCodeSeed: ElementModQ,
    nonce: ElementModQ | undefined = undefined,
    timestamp: number = 0,
    shouldVerifyProofs: boolean = true,
    shouldUsePrecomputedValues: boolean = false
  ): Promise<CiphertextBallot> {
    if (nonce) {
      var result = (
        await getInstance()
      ).EncryptFunctions.encryptBallotWithNonce(
        plaintext._handle,
        internalManifest._handle,
        context._handle,
        ballotCodeSeed._handle,
        nonce._handle,
        timestamp,
        shouldVerifyProofs
      );
      return new CiphertextBallot(result);
    }

    var result = (await getInstance()).EncryptFunctions.encryptBallot(
      plaintext._handle,
      internalManifest._handle,
      context._handle,
      ballotCodeSeed._handle,
      shouldVerifyProofs,
      shouldUsePrecomputedValues
    );
    return new CiphertextBallot(result);
  }
}
