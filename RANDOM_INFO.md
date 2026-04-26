# Project Information & Guides

## Docker Setup & Execution

### Prerequisites
- **Docker Desktop** must be installed and running.
- **Environment File**: You need to create an `.env` file (e.g., `dev.env`) with actual values. 
  - A template is available in `compose.template.env`.
  - Copy it and fill in the secrets (especially `Jwt_Key`, `BlobStorage__ConnectionString`, and `BlobStorage__ContainerName`).
  - For local development with Azurite, you can use `UseDevelopmentStorage=true` for the connection string.
- **Path Configuration**: Put the **absolute path** to your `.env` file into `Scripts/path_to_env`.
  - Current path in `Scripts/path_to_env`: `/home/nolax/Desktop/mydir/secrets/DripRateServer/dev.env`

### Running the Project
Use the provided scripts in the `Scripts/` directory based on your OS (`win/` or `lin/`):

- **Start with logs**: Run `up.sh` (Linux) or `up.cmd` (Windows). This will build and start containers in the foreground.
- **Start detached**: Run `up_detached.sh` or `up_detached.cmd` to run in the background.
- **Stop**: Run `down.sh` or `down.cmd`.
- **View Logs**: Use `docker compose logs -f api` or `docker compose logs -f sqlserver` to see live output.

### Database Management
- **Automatic Migrations**: The database is automatically migrated and initialized when the `api` container starts.
- **Recreating the Database**: 
  - If you need to clear all data and start fresh, run `recreate_db.sh` or `recreate_db.cmd`.
  - This script stops the containers and **removes volumes** (`down -v`), effectively deleting the SQL Server data.
- **SQL Server**: Runs on the port specified as `DB_PORT` in your `.env` (default is 13001).

---

## Logic & Architecture Nuances

### Authentication
- **Identity**: Uses ASP.NET Core Identity for user management.
- **JWT**: Tokens are issued upon login. Ensure `Jwt_Key` is at least 32 characters long.
- **Password Requirements**:
  - Minimum length: 6 characters.
  - No special character, digit, or case requirements.
  - Lockout: 5 failed attempts → 5 minutes lockout.

### User & Collections
- **System Collections**: When a user registers, two system collections are automatically created: **"Likes"** and **"Saved"**.
  - These collections are used for social features (liking publications, saving items).
  - They are managed by the system and cannot be deleted by the user like regular collections.
- **Aggregate Root**: The `User` entity is the central hub for Wardrobe (Cloth), Publications, and Collections.

### File Storage
- **Azure Blob Storage**: All uploads (avatars, publications, cloth images) go to Azure Blob Storage.
- **Path Structure**:
  - Avatars: `avatars/{userId}/...`
  - Publications: `publications/...`
  - Wardrobe: `clothes/...`
- **Naming**: Files are automatically renamed with a GUID prefix to prevent collisions.

### Error Handling
- **ApiErrorRegistry**: The project uses a centralized error registry (`Domain/Errors/ApiErrorRegistry.cs`).
- **Status Codes**: Domain errors are mapped to specific HTTP status codes (400, 401, 403, 404, 409) based on their type.
- **Validation**: Validation errors from FluentValidation or DataAnnotations are intercepted and returned in a consistent RFC-9110 compliant Problem Details format.

---

## Useful Commands
- `docker compose ps`: Check container status.
- `docker exec -it driprate-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourPassword`: Access SQL Server CLI inside container.
