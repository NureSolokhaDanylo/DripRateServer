# Project Overview: DripRate

DripRate is a social network dedicated to fashion, outfits, and look ratings. Unlike traditional social media platforms that prioritize popular creators, DripRate is designed to give equal attention to all users, focusing on the outfits rather than the "celebrity" status of the poster.

## Core Features

### User Profiles & Authentication
- Users register with an email, username, and password.
- Profiles include an avatar, a display name, and a text bio.
- Users can follow each other.
- Profile settings allow changing display name and password.
- Direct Messages (DMs) are currently not planned.

### Publications (Looks)
- Users post "looks" which consist of an image carousel (no video support).
- Publications can have a text description and specific tags.
- **Wardrobe/Clothes:** Users manage a personal list of clothing items. When creating a publication, they can attach specific clothing items from their list to indicate what they are wearing. Clothing items have flexible, optional fields: name, brand, photo, store link, and estimated price.
- **Interactions:** Users can comment on publications, reply to comments, like comments, and like publications.
- Editing publications is currently not supported (potential future idea), but deletion is available.

### Ratings
Publications are rated by other users on a scale of 1 to 10 across specific categories.
*Categories:*
- **Color Coordination** (Harmony of colors used in the outfit)
- **Fit & Proportions** (How well the clothes fit the body and interact with each other)
- **Originality** (Uniqueness and creativity of the look)
- **Overall Style** (General aesthetic appeal)

### Tags
- Tags are used to categorize publications.
- The list of tags is fixed and stored in the database (populated via a one-time script). Users cannot create custom tags.
*Tags:* Casual, Streetwear, Vintage, Minimalist, Avant-Garde, Y2K, Goth, Sportswear, Formal, Grunge, Techwear.

### Feed Logic
The feed system is designed to provide fair visibility:
- **Global Feed:** Displays publications from all users. It does **not** use algorithms that favor highly-liked, highly-rated, or popular posts. Instead, it is ordered strictly by **publication time** and filtered by the **tags** the user has configured in their preferences (users choose what styles they want to see).
- **Subscription Feed:** Displays publications only from followed users. It ignores the user's tag preferences and is ordered strictly by **publication time**.

# Configuration

template.env and compose.template.env contain required and optional environment variables. The compose version contains only the variables required to run the compose setup. The regular version may differ in some aspects. For example, the regular version may contain a full connection string, while the compose version contains separate values such as username, dbname, and password, which are combined into a connection string.

SharedSettings.json contains non-secret application settings that are required for the application to run (for example Identity password requirements). These settings may be overridden by environment variables. This file must never contain secrets such as database passwords, JWT keys, API tokens, or similar values. It also must not contain placeholders like CHANGE_ME or any invented passwords or keys.

template.env never stores secrets. It only contains placeholder values such as CHANGE_ME.

compose.template.env may contain development secrets (for example db_password=strong_password_123). However, real secrets such as API keys must still use CHANGE_ME.

Variables that already exist in SharedSettings.json should not be duplicated in compose.template.env.

When mapping variables in compose.yaml, a single underscore does not need to be converted to a double underscore. For example:

Jwt__Key=${Jwt_Key}

This is valid and does not require renaming the variable to Jwt__Key in the .env file.

The compose file must not provide default values for environment variables. All required variables must be explicitly defined in the .env file.

compose.yaml is intended only for development and must not be used in production.

# Architecture & Engineering Standards

The project follows CQRS and Clean Architecture principles adapted for ASP.NET Core Web API with Entity Framework Core (SQL Server).

## 1. CQRS and MediatR
- The application is divided into **Commands** (state changes) and **Queries** (data reads).
- Controllers act as the only entry points for the UI and map incoming HTTP requests to Commands and Queries.
- Controllers **do not** contain business logic. They dispatch requests via `IMediator.Send()`.
- Handlers reside in the `Application` layer and are automatically registered.
- Endpoints use small, specific classes or records (DTOs) for request bodies/parameters, which are mapped to MediatR Commands/Queries inside the controller.

## 2. Domain Models & EF Core
- True **Domain Entities** are used to model the business rules (e.g., `User` inheriting from `IdentityUser<Guid>`, `Publication`, `ClothingItem`).
- **Encapsulation:** Properties have `private` or `internal` setters. State changes are strictly made through domain methods to guarantee data consistency.
- **EF Core Mapping:** Entity Framework Core configures the mapping without compromising domain encapsulation. No data annotations (like `[Required]` or `[Key]`) should clutter the domain models. All database mappings, including mapping to private backing fields, must be configured using `IEntityTypeConfiguration<T>` in the `Infrastructure` layer via the Fluent API.

## 3. Read Models (DTOs)
- **Queries** return lightweight Read Models (DTOs) containing only the data required by the client.
- To avoid mapping overhead, EF Core is used to project directly from the database to these Read Models using `.Select()` and `.AsNoTracking()`.
- These models do not contain behavior or unnecessary relational data.

## 4. Validation
- Input validation (syntax, format, required fields) is strictly separated from business logic.
- Validation is implemented via **FluentValidation** and integrated into the MediatR pipeline using a `ValidationBehavior`.
- If validation fails, an `ErrorOr` validation failure is returned immediately. The Handler is never executed.

## 5. Error Handling & Result Pattern (ErrorOr + IExceptionHandler)
- **Business Errors (Expected):** The system uses the **`ErrorOr`** library for control flow instead of exceptions. Handlers return `ErrorOr<T>`, which controllers map to appropriate HTTP responses using `.Match()` (e.g., generating RFC 7807 Problem Details). Errors must be meaningful and actionable for the client without leaking server internals.
- **System Errors (Unexpected):** A global `IExceptionHandler` is used as a safety net to catch unhandled exceptions (e.g., database timeouts, null references), logging the details and returning a generic, polite "Internal Server Error" to the client without exposing the stack trace.

## 6. OpenAPI & Client Generation
- Strict OpenAPI documentation is maintained using the built-in `Microsoft.AspNetCore.OpenApi` tools (not Swashbuckle).
- Specific API error codes (e.g., `User.NotFound`) are documented using custom **OpenAPI Transformers**.
- These transformers read custom attributes (e.g., `[ApiErrors]`) on controller actions and inject the error codes into a custom OpenAPI field (e.g., `x-error-codes`).
- This custom field is parsed by the client generator to produce typed error handling in the client SDK.
- `openapi.json` extraction is automated via existing repository scripts.

## 7. Security
- **Authentication:** The project uses ASP.NET Core Identity with **JWT Bearer** tokens for API authentication.
- **CSRF Protection:** Since JWT Bearer tokens are used and sent via headers (not cookies), CSRF protection is not strictly required and will not be implemented to keep the architecture simple.

## 8. Database Constraints & Deletion Logic
- **SQL Server Cascade Restriction:** SQL Server does not support multiple cascade paths to the same table (e.g., `User -> Publication -> Assessment` and `User -> Assessment`).
- **Implementation Rule:** To resolve this, some relationships are configured with `DeleteBehavior.Restrict` or `NoAction` in the Infrastructure layer.
- **Manual Cleanup:** Consequently, Application layer **Handlers** (Use Cases) are responsible for performing manual cleanup of these restricted dependencies when deleting a primary entity (e.g., a Handler for "Delete User" must manually delete the user's Assessments or Likes before deleting the User entity itself).