const path = require( 'path' );
const webpack = require('webpack');

// TODO: WIP: complete webpack configuration if needed
module.exports = {

    // bundling mode
    mode: 'development',

    // entry files
    entry: './src/index.ts',

    // output bundles (location)
    output: {
        path: path.resolve( __dirname, 'dist' ),
        filename: 'electionguard.js',
    },

    // file resolutions
    resolve: {
        extensions: [ '.ts', '.js' ],
        fallback: {
            // Issue: https://github.com/kripken/emscripten/issues/6542.
            crypto: false,
            fs: false,
            os: false,
            path: false,
        },
    },

    // loaders
    module: {
        rules: [
            {
                test: /\.wasm$/,
                type: 'javascript/auto',
                loader: 'file-loader',
                exclude: [
                    /node_modules/, 
                ]
            },
            {
                test: /\.ts?/,
                use: 'ts-loader',
                exclude: [
                    /node_modules/, 
                    /test/
                ]
            }
        ]
    }
};
