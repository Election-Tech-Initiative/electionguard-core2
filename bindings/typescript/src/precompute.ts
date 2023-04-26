import { ElementModP } from "./group";
import { getInstance } from "./wasm";

export class PrecomputeBufferContext {
  static async clear(): Promise<void> {
    (await getInstance()).PrecomputeBufferContext.clear();
  }
  static async initialize(
    publicKey: ElementModP,
    maxQueueSize: number = 1000
  ): Promise<void> {
    (await getInstance()).PrecomputeBufferContext.initialize(
      publicKey._handle,
      maxQueueSize
    );
  }
  static async start(): Promise<void> {
    (await getInstance()).PrecomputeBufferContext.start();
  }
  static async stop(): Promise<void> {
    var result = (await getInstance()).PrecomputeBufferContext.stop();
  }
  static async getMaxQueueSize(): Promise<number> {
    var result = (
      await getInstance()
    ).PrecomputeBufferContext.getMaxQueueSize();
    return result;
  }
  static async getCurrentQueueSize(): Promise<number> {
    var result = (
      await getInstance()
    ).PrecomputeBufferContext.getCurrentQueueSize();
    return result;
  }
}
