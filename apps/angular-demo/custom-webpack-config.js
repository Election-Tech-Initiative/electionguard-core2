
// currently the WASM module being built is commingled for both browser and node
// this is a workaround to prevent angular's webpack from trying to bundle the node api's
// this will be fixed in a future release of the WASM module
module.exports = {
    resolve: {
        extensions: [ '.ts', '.js' ],
        fallback: {
            crypto: false,
            fs: false,
            path: false
        },
    },
};
