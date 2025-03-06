#!/bin/bash

# Check if two arguments are provided
if [ "$#" -ne 4 ]; then
    echo "Usage: $0 <mac_zip_build> <windows_zip_build> <path_to_butler_api_key_file> <commit_hash>"
    exit 1
fi

MAC_ZIP_PATH="$1"
WINDOWS_ZIP_PATH="$2"
BUTLERAPIKEYPATH="$3"
GIT_COMMIT_HASH="$4"

# Check if the files exist
if [ ! -f "$MAC_ZIP_PATH" ]; then
    echo "Error: MacOS Zip File $MAC_ZIP_PATH does not exist."
    exit 1
fi

# Check if input contains the target string
if [[ "$MAC_ZIP_PATH" != *"mac"* ]]; then
    echo "Error: MacOS Zip File Input $MAC_ZIP_PATH does not contain 'mac'. Sanity check!"
    exit 1
fi

if [ ! -f "$WINDOWS_ZIP_PATH" ]; then
    echo "Error: Windows Zip File $WINDOWS_ZIP_PATH does not exist."
    exit 1
fi

# Check if input contains the target string
if [[ "$WINDOWS_ZIP_PATH" != *"windows"* ]]; then
    echo "Error: Windows Zip File Input $MAC_ZIP_PATH does not contain 'windows'. Sanity check!"
    exit 1
fi

if [ ! -f "$BUTLERAPIKEYPATH" ]; then
    echo "Error: File $BUTLERAPIKEYPATH does not exist."
    exit 1
fi

if [ -z "$GIT_COMMIT_HASH" ]; then
    echo "Git commit hash $GIT_COMMIT_HASH is empty."
    exit 1
fi

# Add your logic here (e.g., unzip, compare, process, etc.)
echo "Mac Build $MAC_ZIP_PATH"
echo "Windows Build $WINDOWS_ZIP_PATH"

GIT_COMMIT_HASH_SHORT="$(git rev-parse --short $GIT_COMMIT_HASH)"
echo "git commit hash shortened: $GIT_COMMIT_HASH_SHORT"

COMMIT_DATE="$(git show --no-patch --format=%ci $GIT_COMMIT_HASH | cat | awk '{print $1}')"
echo "git commit date: $COMMIT_DATE"

FULL_VERSION="$COMMIT_DATE-$GIT_COMMIT_HASH_SHORT"
echo "full version str: $FULL_VERSION"

butler push -i "$BUTLERAPIKEYPATH" "$MAC_ZIP_PATH" gofacegames/signified:osx-universal --userversion "$FULLVERSION"

butler push -i "$BUTLERAPIKEYPATH" "$WINDOWS_ZIP_PATH" gofacegames/signified:windows --userversion "$FULLVERSION"

# Exit successfully
exit 0
