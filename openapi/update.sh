#!/usr/bin/env bash
set -euo pipefail

# Ensure we are in the project root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR/.."

SPEC_FILE="openapi.json"

if [[ ! -f "$SPEC_FILE" ]]; then
    echo "✗ Error: $SPEC_FILE not found in project root."
    echo "Please run ./openapi/download-openapi.sh first."
    exit 1
fi

echo "Re-generating SDKs from $SPEC_FILE..."

echo "Generating TypeScript/JavaScript SDK (Custom)..."
node openapi/generate-ts-sdk.js

echo "Generating Kotlin SDK (Fabrikt)..."
rm -rf sdks/kotlin-fabrikt/*
# Using -u $(id -u):$(id -g) to avoid permission issues with docker-generated files
docker run --rm -u $(id -u):$(id -g) -v "$(pwd):/workspace" ghcr.io/fabrikt-io/fabrikt:latest \
  --api-file "/workspace/$SPEC_FILE" \
  --base-package driprate.sdk \
  --output-directory "/workspace/sdks/kotlin-fabrikt" \
  --targets HTTP_MODELS \
  --targets CLIENT \
  --http-client-target KTOR \
  --serialization-library KOTLINX_SERIALIZATION \
  --type-overrides DATETIME_AS_INSTANT

echo "Done! SDKs updated in sdks/ folder."

# Add Kotlin Extensions for easy error handling
KOTLIN_EXT_DIR="sdks/kotlin-fabrikt/src/main/kotlin/driprate/sdk/client"
mkdir -p "$KOTLIN_EXT_DIR"
cat <<EOF > "$KOTLIN_EXT_DIR/NetworkResultExtensions.kt"
package driprate.sdk.client

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.jsonObject

/**
 * Пытается извлечь код ошибки из тела ответа и привести его к нужному Enum типу.
 */
inline fun <reified E : Enum<E>> NetworkError.Http.errorCode(json: Json = Json { ignoreUnknownKeys = true }): E? {
    val bodyString = body ?: return null
    return try {
        val jsonElement = json.parseToJsonElement(bodyString).jsonObject
        val codeElement = jsonElement["code"] ?: return null
        
        val codeString = codeElement.toString().replace("\"", "")
        enumValues<E>().find { it.name.equals(codeString, ignoreCase = true) || it.toString() == codeString }
    } catch (e: Exception) {
        null
    }
}

/**
 * Удобная проверка кода ошибки прямо из результата
 */
inline fun <reified E : Enum<E>> NetworkResult<*>.hasError(code: E): Boolean {
    if (this !is NetworkResult.Failure) return false
    val error = this.error
    if (error !is NetworkError.Http) return false
    return error.errorCode<E>() == code
}
EOF

# Copy documentation to Kotlin SDK
cp sdks/typescript-custom/ERRORS.md sdks/kotlin-fabrikt/ERRORS.md

