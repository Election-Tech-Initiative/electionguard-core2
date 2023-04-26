#!/bin/bash

version="$1"

# Ensure we have emscripten installed and activated
pushd .cache
    if [ ! -d "emscripten" ]; then
        echo "Downloading Emscripten SDK ..."
        wget https://github.com/emscripten-core/emsdk/archive/master.zip
        unzip -q master.zip
        mv emsdk-master emscripten
        rm master.zip
    fi

    pushd emscripten
            echo "Updating Emscripten SDK ..."
            ./emsdk update

            echo "Installing Emscripten SDK $version ..."
            ./emsdk install $version

            echo "Activating Emscripten SDK $version ..."
            ./emsdk activate $version
    popd
popd