import { Injectable, OnInit } from '@angular/core';

import test_data from '../../../../data/test/test-data.json';

import {
  CiphertextBallot,
  ElectionContext,
  EncryptionDevice,
  EncryptionMediator,
  GroupFunctions,
  InternalManifest,
  Manifest,
  PlaintextBallot,
  initialize as egInitialize,
} from '@infernored/electionguard-experimental';
import { Logger } from './logger.service';

@Injectable()
export class ElectionguardService implements OnInit {
  private _isReady = false;

  // Define a device
  private testDeviceData = {
    device_id: 12345, // could be a static id passed from the server
    session_id: 23456, // could be a static id generated client side
    launch_code: 34567, // should be a random number generated as part of the election setup
    location: 'Location', // could be the browser's UserAgent string
  };

  // Define a mediator
  private _mediator: EncryptionMediator | undefined;

  constructor(private logger: Logger) {}

  async ngOnInit(): Promise<void> {
    this.logger.log('ElectionguardService ngOnInit');

    // initialize the library which loads wasm
    // and does a few other things to prepare for encryption
    await egInitialize();
    this._isReady = true;
  }

  // Configure the service with the election config data
  async configure(): Promise<void> {
    if (!this._isReady) {
      await egInitialize();
      this._isReady = true;
    }

    // simulate loading the election config data from the server
    const { context, manifest } = await this.fetchElectionConfigData();

    // simulate loading the device data
    const device = await EncryptionDevice.fromJson(
      JSON.stringify(this.testDeviceData)
    );

    // create the encryption mediator
    this._mediator = await EncryptionMediator.make(manifest, context, device);
  }

  // Encrypt a ballot
  async encryptBallot(ballot: PlaintextBallot): Promise<CiphertextBallot> {
    if (!this._mediator) {
      throw new Error('Mediator not configured');
    }

    // encrypt the ballot
    const result = await this._mediator.encrypt(ballot, false);
    return result;
  }

  // simulate a ballot by loading one from the test data
  async getTestBallot(): Promise<PlaintextBallot> {
    const plaintextBallot = (test_data as unknown as any).plaintext_ballots[0];
    const data = await PlaintextBallot.fromJson(
      JSON.stringify(plaintextBallot)
    );

    this.logger.log(`electionguard testBallot: ${data.toJson()}`);

    return data;
  }

  // a simple test function to demonstrate the library is working
  async testAddNumbers(a: number, b: number): Promise<BigInt> {
    const result = await GroupFunctions.addModQ(a, b);
    this.logger.log(`electionguard addResult: ${result}`);
    return result;
  }

  // simulate loading the election config data from the server
  private async fetchElectionConfigData(): Promise<{
    context: ElectionContext;
    manifest: InternalManifest;
  }> {
    const ciphertextElectionContext = (test_data as unknown as any).election
      .context;
    const manifest_data = (test_data as unknown as any).election.manifest;

    // import the manifest
    const manifest = await Manifest.fromJson(JSON.stringify(manifest_data));

    const context = await ElectionContext.fromJson(
      JSON.stringify(ciphertextElectionContext)
    );

    // convert the manifest to an internal manifest
    const internalManifest = await InternalManifest.fromManifest(manifest);

    return { context, manifest: internalManifest };
  }
}
