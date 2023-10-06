#!/bin/bash

# Most of the content of this script is derived from the sources below:
# - https://github.com/walles/riff/blob/82f77c82e7306dd69d343640670bdf9d31cc0b0b/release.sh#L132-L136
# - https://stackoverflow.com/questions/66849112/how-do-i-cross-compile-a-rust-application-from-macos-x86-to-macos-silicon
# - https://developer.apple.com/documentation/apple-silicon/building-a-universal-macos-binary#Update-the-Architecture-List-of-Custom-Makefiles

# this script should be run from the solution root for the directories to work as expected

targets="aarch64-apple-darwin x86_64-apple-darwin"

cd native/src || exit

for target in $targets; do
  rustup target add $target

  # https://stackoverflow.com/a/66875783/473672
  SDKROOT=$(xcrun -sdk macosx --show-sdk-path) \
  MACOSX_DEPLOYMENT_TARGET=$(xcrun -sdk macosx --show-sdk-platform-version) \
    cargo build --release "--target=$target"
done

lipo -create \
  -output target/universal-release \
  target/aarch64-apple-darwin/release/onionfruit_worker_native.dylib \
  target/x86_64-apple-darwin/release/onionfruit_worker_native.dylib