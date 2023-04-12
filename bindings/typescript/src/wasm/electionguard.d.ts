export declare type PlaintextBallot = {
  object_id: string;
  style_id: string;

  toJson(): string;
  //fromJson(json: string): PlaintextBallot;
};

export declare type CiphertextBallot = {
  object_id: string;
  style_id: string;

  toJson(): string;
  //fromJson(json: string): CiphertextBallot;
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
  //fromHex(hex: string): ElementModQ;
  //fromUnit64(value: BigInt, unchecked: boolean): ElementModQ;
};

export declare type Manifest = {
  toJson(): string;
  //fromJson(json: string): Manifest;
};

export declare type InternalManifest = {
  toJson(): string;
  //fromJson(json: string): InternalManifest;
};
