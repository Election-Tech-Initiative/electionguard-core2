export declare type PlaintextBallot = {
  getObjectId(): string;
  getStyleId(): string;

  toJson(): string;
  //fromJson(json: string): PlaintextBallot;
};

export declare type CiphertextBallot = {
  getObjectId(): string;
  getStyleId(): string;

  cast(): void;
  spoil(): void;
  toJson(withNonces: bool): string;
};

export declare type CiphertextElectionContext = {
  toJson(): string;
  //fromJson(json: string): CiphertextElectionContext;
};

export declare type EncryptionDevice = {
  toJson(): string;
  //fromJson(json: string): EncryptionDevice;
};

export declare type EncryptionMediator = {
  new (
    internalManifest: InternalManifest,
    contest: CiphertextElectionContext,
    device: EncryptionDevice
  ): EncryptionMediator;

  encrypt(ballot: PlaintextBallot, shouldVerifyProofs: boolean);
};

export declare type ElementModP = {
  toHex(): string;
  //fromHex(hex: string): ElementModP;
  //fromUnit64(value: BigInt, unchecked: boolean): ElementModP;
};

export declare type ElementModQ = {
  toHex(): string;
};

export declare type Manifest = {
  toJson(): string;
  //fromJson(json: string): Manifest;
};

export declare type InternalManifest = {
  new (manifest: Manifest): InternalManifest;
  toJson(): string;
  //fromJson(json: string): InternalManifest;
};
