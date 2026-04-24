#!/usr/bin/env bash
set -euo pipefail

SPEC_FILE="openapi.json"

echo "Re-generating all SDKs from $SPEC_FILE..."

echo "Generating TypeScript types..."
npx openapi-typescript "$SPEC_FILE" -o sdks/ts/schema.d.ts

echo "Generating JavaScript SDK..."
openapi-generator-cli generate \
  -i "$SPEC_FILE" \
  -g javascript \
  -o sdks/js \
  --skip-validate-spec \
  --additional-properties skipDefaultUserAgent=true,usePromises=true

rm -rf sdks/js/.openapi-generator sdks/js/.openapi-generator-ignore sdks/js/.travis.yml sdks/js/git_push.sh sdks/js/mocha.opts

echo "Generating Kotlin SDK..."
openapi-generator-cli generate \
  -i "$SPEC_FILE" \
  -g kotlin \
  -o sdks/kotlin \
  --library jvm-retrofit2 \
  --skip-validate-spec

echo "Generating Java SDK..."
openapi-generator-cli generate \
  -i "$SPEC_FILE" \
  -g java \
  -o sdks/java \
  --library retrofit2 \
  --skip-validate-spec

echo "Done! All SDKs updated in sdks/ folder."
