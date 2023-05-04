import { getInstance } from "./wasm";

export * from "./ballot";
export * from "./election";
export * from "./encrypt";
export * from "./group";
export * from "./manifest";
export * from "./precompute";

/**
 * Initialize the ElectionGuard library.
 */
export const initialize = async (): Promise<void> => {
  // get the wasm instance
  await getInstance();

  // TODO: precompute tables
};

export default initialize;
