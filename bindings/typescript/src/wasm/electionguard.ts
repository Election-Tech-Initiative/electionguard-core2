import wasModuleFactory from "./electionguard.wasm";

export interface PlaintextBallotHandle {
  getObjectId(): string;
  getStyleId(): string;

  toJson(): string;
}

export type PlaintextBallotStatic = {
  fromJson(json: string): PlaintextBallotHandle;
};

export type CiphertextBallotHandle = {
  getObjectId(): string;
  getStyleId(): string;

  getManifestHash(): ElementModQHandle;
  getBallotCodeSeed(): ElementModQHandle;
  getBallotCode(): ElementModQHandle;
  getTimestamp(): number;
  getNonce(): ElementModQHandle;

  isValidEncryption(
    manifestHash: ElementModQHandle,
    publicKey: ElementModPHandle,
    extendedHash: ElementModQHandle
  ): boolean;

  cast(): void;
  challenge(): void;
  spoil(): void;
  toJson(withNonces: boolean): string;
};

export type CiphertextBallotStatic = {
  fromJson(json: string): CiphertextBallotHandle;
};

export type CiphertextElectionContextHandle = {
  getNumberOfGuardians(): number;
  getQuorum(): number;
  getElGamalPublicKey(): ElementModPHandle;
  getElGamalPublicKeyRef(): ElementModPHandle;
  getManifestHash(): ElementModQHandle;
  getCryptoExtendedBaseHash(): ElementModQHandle;
  toJson(): string;
};

export type CiphertextElectionContextStatic = {
  fromJson(json: string): CiphertextElectionContextHandle;
};

export type EncryptionDeviceHandle = {
  getTimestamp(): number;
  getDeviceUuid(): number;
  getSessionUuid(): number;
  getLaunchCode(): number;
  getLocation(): string;
  toJson(): string;
};

export type EncryptionDeviceStatic = {
  fromJson(json: string): EncryptionDeviceHandle;
};

export type EncryptionMediatorHandle = {
  new (
    internalManifest: InternalManifestHandle,
    context: CiphertextElectionContextHandle,
    device: EncryptionDeviceHandle
  ): EncryptionMediatorHandle;

  encrypt(
    ballot: PlaintextBallotHandle,
    shouldVerifyProofs: boolean,
    shouldUsePrecomputedValues: boolean
  ): CiphertextBallotHandle;
};

export type EncryptFunctionsStatic = {
  encryptBallot(
    ballot: PlaintextBallotHandle,
    internalManifest: InternalManifestHandle,
    context: CiphertextElectionContextHandle,
    ballotCodeSeed: ElementModQHandle,
    shouldVerifyProofs: boolean,
    shouldUsePrecomputedValues: boolean
  ): CiphertextBallotHandle;
  encryptBallotWithNonce(
    ballot: PlaintextBallotHandle,
    internalManifest: InternalManifestHandle,
    context: CiphertextElectionContextHandle,
    ballotCodeSeed: ElementModQHandle,
    nonce: ElementModQHandle,
    timestamp: number,
    shouldVerifyProofs: boolean
  ): CiphertextBallotHandle;
};

export type ElementModPHandle = {
  copy(): ElementModPHandle;
  isInBounds(): boolean;
  toHex(): string;
};

export type ElementModPStatic = {
  fromHex(hex: string, unchecked: boolean): ElementModPHandle;
  fromUint64(value: BigInt, unchecked: boolean): ElementModPHandle;
};

export type ElementModQHandle = {
  copy(): ElementModQHandle;
  isInBounds(): boolean;
  toHex(): string;
  toElementModP(): ElementModPHandle;
};

export type ElementModQStatic = {
  fromHex(hex: string, unckeched: boolean): ElementModQHandle;
  fromUint64(value: BigInt, unchecked: boolean): ElementModQHandle;
};

export type GroupFunctionStatic = {
  addModQ(a: ElementModQHandle, b: ElementModQHandle): ElementModQHandle;
  randomElementModP(): ElementModPHandle;
  randomElementModQ(): ElementModQHandle;
};

export type ManifestHandle = {
  toJson(): string;
};

export type ManifestStatic = {
  fromJson(json: string): ManifestHandle;
};

export type InternalManifestHandle = {
  new (manifest: ManifestHandle): InternalManifestHandle;
  toJson(): string;
};

export type InternalManifestStatic = {
  fromJson(json: string): InternalManifestHandle;
};

export type PrecomputeBuffersStatic = {
  clear(): void;

  initialize(publicKey: ElementModPHandle, maxQueueSize: number): void;
  start(): void;
  stop(): void;

  getMaxQueueSize(): number;
  getCurrentQueueSize(): number;
};

export interface ElectionguardModule extends EmscriptenModule {
  PlaintextBallot: PlaintextBallotStatic;
  CiphertextBallot: CiphertextBallotStatic;
  CiphertextElectionContext: CiphertextElectionContextStatic;
  EncryptionDevice: EncryptionDeviceStatic;
  EncryptionMediator: EncryptionMediatorHandle;
  EncryptFunctions: EncryptFunctionsStatic;
  ElementModP: ElementModPStatic;
  ElementModQ: ElementModQStatic;
  GroupFunctions: GroupFunctionStatic;
  Manifest: ManifestStatic;
  InternalManifest: InternalManifestStatic;
  PrecomputeBufferContext: PrecomputeBuffersStatic;
}
const createModule: EmscriptenModuleFactory<ElectionguardModule> =
  wasModuleFactory;
export default createModule;
