import { InternalManifestHandle, ManifestHandle, getInstance } from "./wasm";

/**
 * Use this entity for defining the structure of the election and associated
 * information such as candidates, contests, and vote counts.  This class is
 * based on the NIST Election Common Standard Data Specification.  Some deviations
 * from the standard exist.
 *
 * This structure is considered an immutable input object and should not be changed
 * through the course of an election, as it's hash representation is the basis for all
 * other hash representations within an ElectionGuard election context.
 *
 * @see https://developers.google.com/elections-data/reference/election
 */
export class Manifest {
  _handle: ManifestHandle;
  constructor(handle: ManifestHandle) {
    this._handle = handle;
  }

  // TODO: hash

  // TODO: validity check

  toJson(): string {
    return this._handle.toJson();
  }

  /**
   * @brief Creates a Manifest object from a [RFC-8259](https://www.rfc-editor.org/rfc/rfc8259.html#section-8.1) UTF-8 encoded JSON string
   */
  static async fromJson(json: string): Promise<Manifest> {
    var result = (await getInstance()).Manifest.fromJson(json);
    return new Manifest(result);
  }
}

/**
 * `InternalManifest` is a subset of the `Manifest` structure that specifies
 * the components that ElectionGuard uses for conducting an election.  The key component is the
 * `contests` collection, which applies placeholder selections to the `Manifest` contests
 */
export class InternalManifest {
  _handle: InternalManifestHandle;

  constructor(handle: InternalManifestHandle) {
    this._handle = handle;
  }

  toJson(): string {
    return this._handle.toJson();
  }

  /**
   * @brief Creates an Internal Manifest object from a [RFC-8259](https://www.rfc-editor.org/rfc/rfc8259.html#section-8.1) UTF-8 encoded JSON string
   */
  static async fromJson(json: string): Promise<InternalManifest> {
    var result = (await getInstance()).InternalManifest.fromJson(json);
    return new InternalManifest(result);
  }
}
