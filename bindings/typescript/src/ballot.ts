import { ElementModQ } from "./group";
import {
  CiphertextBallotHandle,
  PlaintextBallotHandle,
  getInstance,
} from "./wasm";

/**
 * A PlaintextBallot represents a voters selections for a given ballot and ballot style.
 *
 * This class can be either a partial or a complete representation of the expected values of a ballot dataset.
 * Specifically, a partial representation must include at a minimum the "affirmative" selections
 * of every contest where a selection is made.  A partial representation may exclude contests for which
 * no selection is made.
 *
 * A complete representation of a ballot must include both affirmative and negative selections of
 * every contest, AND the placeholder selections necessary to satisfy the NIZKPs for each contest and selection.
 */
export class PlaintextBallot {
  _handle: PlaintextBallotHandle;

  constructor(handle: PlaintextBallotHandle) {
    this._handle = handle;
  }

  /**
   * @brief The unique ballot id that is meaningful to the consuming application.
   */
  get objectId(): string {
    return this._handle.getObjectId();
  }

  /**
   * @brief The Object Id of the ballot style in the election manifest.  This value is used to determine which contests to expect on the ballot, to fill in missing values, and to validate that the ballot is well-formed
   */
  get styleId(): string {
    return this._handle.getObjectId();
  }

  /**
   * Export the ballot representation as JSON
   */
  toJson(): string {
    return this._handle.toJson();
  }

  /**
   * @brief Creates a PlaintextBallot object from a [RFC-8259](https://www.rfc-editor.org/rfc/rfc8259.html#section-8.1) UTF-8 encoded JSON string
   * @param json the JSON string
   */
  static async fromJson(json: string): Promise<PlaintextBallot> {
    var result = (await getInstance()).PlaintextBallot.fromJson(json);
    return new PlaintextBallot(result);
  }
}

/**
 * A CiphertextBallot represents a voters encrypted selections for a given ballot and ballot style.
 *
 * When a ballot is in it's complete, encrypted state, the `nonce` is the seed nonce
 * from which all other nonces can be derived to encrypt the ballot.  Allong with the `nonce`
 * fields on `Ballotcontest` and `BallotSelection`, this value is sensitive.
 *
 * Don't make this directly. Use `make_ciphertext_ballot` instead, or call `eg_encrypt_ballot` in encrypt.h.
 */
export class CiphertextBallot {
  _handle: CiphertextBallotHandle;

  constructor(handle: CiphertextBallotHandle) {
    this._handle = handle;
  }

  /**
   * @brief The unique ballot id that is meaningful to the consuming application.
   */
  get objectId(): string {
    return this._handle.getObjectId();
  }

  /**
   * @brief The Object Id of the ballot style in the election manifest.  This value is used to determine which contests to expect on the ballot, to fill in missing values, and to validate that the ballot is well-formed
   */
  get styleId(): string {
    return this._handle.getObjectId();
  }

  /**
   * @brief Hash of the complete Election Manifest to which this ballot belongs
   */
  get manifestHash(): ElementModQ {
    var result = this._handle.getManifestHash();
    return new ElementModQ(result);
  }

  /**
   * @brief The seed hash for the ballot.  It may be the encryption device hash,
   * the hash of a previous ballot or the hash of some other value that is meaningful to the consuming application.
   */
  get ballotCodeSeed(): ElementModQ {
    var result = this._handle.getBallotCodeSeed();
    return new ElementModQ(result);
  }

  /**
   * @brief The unique ballot code for this ballot that is derived from the ballot seed,
   *  the timestamp, and the hash of the encrypted values
   */
  get ballotCode(): ElementModQ {
    var result = this._handle.getBallotCode();
    return new ElementModQ(result);
  }

  get timestamp(): number {
    return this._handle.getTimestamp();
  }

  /**
   * The nonce value used to encrypt all values in the ballot.
   * Sensitive & should be treated as a secret
   */
  get nonce(): ElementModQ {
    var result = this._handle.getNonce();
    return new ElementModQ(result);
  }

  // TODO: get state

  /**
   * A helper function to mark the ballot as cast and remove sensitive values like the nonce.
   */
  cast(): void {
    this._handle.cast();
  }

  /**
   * A helper function to mark the ballot as challenged and remove sensitive values like the nonce.
   */
  challenge(): void {
    this._handle.cast();
  }

  /**
   * A helper function to mark the ballot as spoiled and remove sensitive values like the nonce.
   */
  spoil(): void {
    this._handle.spoil();
  }

  /**
   * Export the ballot representation as JSON
   */
  toJson(withNonces: boolean): string {
    // TODO: maybe just always return without the nonces
    return this._handle.toJson(withNonces);
  }

  /**
   * @brief Creates a CiphertextBallot object from a [RFC-8259](https://www.rfc-editor.org/rfc/rfc8259.html#section-8.1) UTF-8 encoded JSON string
   * @param json the JSON string
   */
  static async fromJson(json: string): Promise<CiphertextBallot> {
    var result = (await getInstance()).CiphertextBallot.fromJson(json);
    return new CiphertextBallot(result);
  }
}
