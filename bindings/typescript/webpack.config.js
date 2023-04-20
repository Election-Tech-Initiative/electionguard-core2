const path = require( 'path' );
const webpack = require('webpack');

module.exports = {

    // bundling mode
    mode: 'development',

    // entry files
    entry: './bindings/typescript/src/index.ts',

    // output bundles (location)
    output: {
        path: path.resolve( __dirname, 'dist' ),
        filename: 'electionguard.js',
    },

    // Issue: https://github.com/kripken/emscripten/issues/6542.
    // browser: {
    //     "fs": false
    // },
    // node: { 
    //     fs: "empty"
    // },

    // file resolutions
    resolve: {
        extensions: [ '.ts', '.js' ],
        fallback: {
            crypto: false,
            fs: false,
            path: false
        },
        // fallback: { 
        //     "stream": require.resolve("stream-browserify"),
        //     "buffer": require.resolve("buffer/")
        // }
    },

    // plugins
    // plugins: [
    //     new webpack.ProvidePlugin({
    //       Buffer: ['buffer', 'Buffer'],
    //       process: 'process/browser'
    //     })
    //  ],
    

    // loaders
    module: {
        rules: [
            {
                test: /\.wasm$/,
                type: 'javascript/auto',
                loader: 'file-loader',
                exclude: [
                    /apps/,
                    /node_modules/, 
                ]
            },
            {
                test: /\.test.ts?/,
                use: 'ts-loader',
                exclude: [
                    /apps/,
                    /node_modules/, 
                ]
            }
        ]
    }
};