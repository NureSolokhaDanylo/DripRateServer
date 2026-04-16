# OpenAPI Documentation Storage

This folder contains versioned snapshots of the OpenAPI documentation.

## Generated Files

- `openapi0001.json` - First version of OpenAPI spec
- `openapi0002.json` - Second version (only if content differs from previous)
- etc.

## Endpoints

- **JSON**: `GET /openapi/swagger.json`
- **YAML**: `GET /openapi/swagger.yaml`

## Scripts

### Download and Version Documentation

Run the appropriate script from the `Scripts` folder to download the latest OpenAPI documentation and save it with automatic versioning.

**PowerShell (Windows)**:
```powershell
.\Scripts\download-openapi.ps1
```

**Bash (Linux/Mac)**:
```bash
./Scripts/download-openapi.sh
```

The scripts will:
1. Download the JSON from the running API
2. Save it to the project root as `swagger.json`
3. Check if the content matches any existing versioned file
4. If unique, save it as `openapiXXXX.json` with the next available version number
5. If duplicate content is found, skip saving to avoid redundant files

### Script Options

**PowerShell**:
```powershell
.\Scripts\download-openapi.ps1 -BaseUrl "http://custom-host:8080"
.\Scripts\download-openapi.ps1 -UseCli  # Use base URL from script parameter instead of compose.template.env
```

**Bash**:
```bash
./Scripts/download-openapi.sh http://custom-host:8080
```

