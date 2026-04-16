# Script to download OpenAPI documentation and version it
# Reads base URL from compose.template.env

param(
    [string]$BaseUrl = "http://localhost:13000",
    [switch]$UseCli
)

# Parse base URL from compose.template.env if not provided
if (-not $UseCli) {
    $composeTemplate = Get-Content -Path (Join-Path $PSScriptRoot ".." "compose.template.env")
    $serverPort = $composeTemplate | Select-String "SERVER_PORT=(\d+)" | ForEach-Object { $_.Matches.Groups[1].Value }
    if ($serverPort) {
        $BaseUrl = "http://localhost:$serverPort"
    }
}

$OpenApiFolder = Join-Path $PSScriptRoot ".." "openapi"
$RootFolder = Join-Path $PSScriptRoot ".."

# Ensure folder exists
if (-not (Test-Path $OpenApiFolder)) {
    New-Item -ItemType Directory -Path $OpenApiFolder | Out-Null
}

Write-Host "Downloading OpenAPI JSON from $BaseUrl/openapi/swagger.json..."

try {
    # Download the JSON file
    $jsonUrl = "$BaseUrl/openapi/swagger.json"
    $tempFile = Join-Path $env:TEMP "openapi_temp_$(Get-Random).json"
    
    Invoke-WebRequest -Uri $jsonUrl -OutFile $tempFile -TimeoutSec 10
    
    if (-not (Test-Path $tempFile)) {
        throw "Failed to download OpenAPI JSON"
    }

    # Read the downloaded content
    $downloadedContent = Get-Content -Path $tempFile -Raw
    
    # Check if content is valid JSON
    try {
        [void]($downloadedContent | ConvertFrom-Json)
    }
    catch {
        throw "Downloaded content is not valid JSON"
    }

    # Copy to root
    $rootJsonPath = Join-Path $RootFolder "swagger.json"
    Copy-Item -Path $tempFile -Destination $rootJsonPath -Force
    Write-Host "✓ Saved to root: $rootJsonPath"

    # Find next available version number
    $existingFiles = @()
    if (Test-Path $OpenApiFolder) {
        $existingFiles = Get-ChildItem -Path $OpenApiFolder -Filter "openapi*.json" | Sort-Object Name
    }

    $nextVersion = 1
    if ($existingFiles.Count -gt 0) {
        # Extract version numbers from filenames
        $versions = $existingFiles | ForEach-Object {
            if ($_.BaseName -match "openapi(\d+)") {
                [int]$matches[1]
            }
        }
        if ($versions) {
            $nextVersion = ($versions | Measure-Object -Maximum).Maximum + 1
        }
    }

    # Check for duplicate content
    $isDuplicate = $false
    $duplicateFile = $null
    
    if ($existingFiles.Count -gt 0) {
        foreach ($file in $existingFiles) {
            $existingContent = Get-Content -Path $file.FullName -Raw
            
            # Normalize both JSON strings for comparison (remove whitespace differences)
            $downloadedNormalized = $downloadedContent | ConvertFrom-Json | ConvertTo-Json -Depth 100 -Compress
            $existingNormalized = $existingContent | ConvertFrom-Json | ConvertTo-Json -Depth 100 -Compress
            
            if ($downloadedNormalized -eq $existingNormalized) {
                $isDuplicate = $true
                $duplicateFile = $file.Name
                break
            }
        }
    }

    if ($isDuplicate) {
        Write-Host "⚠ Content matches existing file: $duplicateFile"
        Write-Host "Skipping versioned save (duplicate content detected)"
    }
    else {
        $versionedFileName = "openapi{0:D4}.json" -f $nextVersion
        $versionedPath = Join-Path $OpenApiFolder $versionedFileName
        Copy-Item -Path $tempFile -Destination $versionedPath
        Write-Host "✓ Saved versioned: $versionedPath"
    }

    # Clean up
    Remove-Item -Path $tempFile -Force -ErrorAction SilentlyContinue

    Write-Host "✓ Successfully downloaded OpenAPI documentation"
    exit 0
}
catch {
    Write-Host "✗ Error: $_" -ForegroundColor Red
    Remove-Item -Path $tempFile -Force -ErrorAction SilentlyContinue
    exit 1
}
