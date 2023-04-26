require("./setup");
import { assert } from "chai";
import test_data from "../../../data/test/test-data.json";
import { ElectionContext } from "../src/election";
import { ElementModP, PrecomputeBufferContext } from "../src";

const ciphertextElectionContext = (test_data as unknown as any).election
  .context;

describe("PrecomputeBufferContext Tests", () => {
  const testBufferSize = 100;
  it("should initialize", async () => {
    const context = JSON.stringify(ciphertextElectionContext);
    const result = await ElectionContext.fromJson(context);

    await PrecomputeBufferContext.initialize(
      result.publicKeyRef,
      testBufferSize
    );

    assert.equal(
      await PrecomputeBufferContext.getMaxQueueSize(),
      testBufferSize
    );

    await PrecomputeBufferContext.start();

    // wait for the buffer to fill
    await new Promise((resolve) => setTimeout(resolve, 100));
    await PrecomputeBufferContext.stop();

    assert.isTrue((await PrecomputeBufferContext.getCurrentQueueSize()) > 0);
  });
});
