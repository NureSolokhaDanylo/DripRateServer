#!/bin/bash

# Script to download OpenAPI documentation and version it
# Reads base URL from compose.template.env

BASE_URL="${1:-http://localhost:13000}"

# Parse base URL from compose.template.env if not provided via CLI
if [[ $# -eq 0 ]]; then
    SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    COMPOSE_TEMPLATE="$SCRIPT_DIR/../compose.template.env"
    
    if [[ -f "$COMPOSE_TEMPLATE" ]]; then
        SERVER_PORT=$(grep "^SERVER_PORT=" "$COMPOSE_TEMPLATE" | cut -d'=' -f2)
        if [[ -n "$SERVER_PORT" ]]; then
            BASE_URL="http://localhost:$SERVER_PORT"
        fi
    fi
fi

OPENAPI_FOLDER="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)/openapi"
ROOT_FOLDER="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

# Ensure folder exists
mkdir -p "$OPENAPI_FOLDER"

echo "Downloading OpenAPI JSON from $BASE_URL/openapi/openapi.json..."

# Download the JSON file
JSON_URL="$BASE_URL/openapi/openapi.json"
TEMP_FILE=$(mktemp)

if ! curl -f -s "$JSON_URL" -o "$TEMP_FILE" --max-time 10; then
    echo "✗ Error: Failed to download OpenAPI JSON from $JSON_URL"
    rm -f "$TEMP_FILE"
    exit 1
fi

# Check if content is valid JSON
if ! jq empty "$TEMP_FILE" 2>/dev/null; then
    echo "✗ Error: Downloaded content is not valid JSON"
    rm -f "$TEMP_FILE"
    exit 1
fi

# Copy to root
ROOT_JSON_PATH="$ROOT_FOLDER/openapi.json"
cp "$TEMP_FILE" "$ROOT_JSON_PATH"
echo "✓ Saved to root: $ROOT_JSON_PATH"

# Read the downloaded content
DOWNLOADED_CONTENT=$(cat "$TEMP_FILE")

# Find next available version number
EXISTING_FILES=$(find "$OPENAPI_FOLDER" -maxdepth 1 -name "openapi[0-9]*.json" | sort)
NEXT_VERSION=1

if [[ -n "$EXISTING_FILES" ]]; then
    # Extract version numbers from filenames
    LAST_VERSION=$(echo "$EXISTING_FILES" | grep -oP 'openapi\K\d+' | sort -n | tail -1)
    if [[ -n "$LAST_VERSION" ]]; then
        NEXT_VERSION=$((10#$LAST_VERSION + 1))
    fi
fi

# Check for duplicate content
IS_DUPLICATE=0
DUPLICATE_FILE=""

if [[ -n "$EXISTING_FILES" ]]; then
    DOWNLOADED_NORMALIZED=$(echo "$DOWNLOADED_CONTENT" | jq -S -c .)
    
    while IFS= read -r FILE; do
        EXISTING_CONTENT=$(cat "$FILE")
        EXISTING_NORMALIZED=$(echo "$EXISTING_CONTENT" | jq -S -c .)
        
        if [[ "$DOWNLOADED_NORMALIZED" == "$EXISTING_NORMALIZED" ]]; then
            IS_DUPLICATE=1
            DUPLICATE_FILE=$(basename "$FILE")
            break
        fi
    done <<< "$EXISTING_FILES"
fi

if [[ $IS_DUPLICATE -eq 1 ]]; then
    echo "⚠ Content matches existing file: $DUPLICATE_FILE"
    echo "Skipping versioned save (duplicate content detected)"
else
    VERSIONED_FILENAME=$(printf "openapi%04d.json" "$NEXT_VERSION")
    VERSIONED_PATH="$OPENAPI_FOLDER/$VERSIONED_FILENAME"
    cp "$TEMP_FILE" "$VERSIONED_PATH"
    echo "✓ Saved versioned: $VERSIONED_PATH"
fi

# Clean up
rm -f "$TEMP_FILE"

echo "✓ Successfully downloaded OpenAPI documentation"
exit 0
