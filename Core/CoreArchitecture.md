# Core Layer Architecture

The Core layer is the heart of the TrialWorld application, containing all domain models, business logic, and domain service interfaces. This layer has no dependencies on external frameworks or libraries except basic .NET capabilities.

## Key Components

### Domain Models

Located in `Core/Models/`, these represent the business entities and their behavior:

- **Media Models**: Represent media files, formats, and metadata
- **Transcription Models**: Represent speech-to-text results and segments
- **Analysis Models**: Represent content analysis results (emotions, faces, etc.)
- **Enhancement Models**: Represent media enhancement configurations and results

Domain models should:
- Encapsulate business rules and validation
- Be persistence-ignorant (no EF or DB concerns)
- Focus on behavior and domain logic
- Not be used as DTOs for external communication

### Interfaces

Located in `Core/Interfaces/`, these define service contracts:

- **Service Interfaces**: Contracts for domain services
- **Repository Interfaces**: Contracts for data access
- **Infrastructure Interfaces**: Contracts for external services

Interfaces should:
- Define behavior without implementation
- Be focused on a single responsibility
- Not expose implementation details
- Follow naming conventions (I-prefix)

### Domain Services

Located in `Core/Services/`, these implement core business logic:

- Implement algorithms and complex domain operations
- Operate on domain models
- Enforce domain rules and invariants
- Remain independent of external concerns (UI, DB, etc.)

## Clean Architecture Rules

1. Core should not depend on outer layers
2. Core should not reference UI, database, or external services directly
3. Infrastructure concerns should be abstracted behind interfaces
4. Domain models should not be contaminated with UI or persistence concerns

## Example: Media Processing Flow

1. Domain models define media formats, codecs, and processing options
2. Interfaces specify required services (IMediaProcessor, ITranscriptionService)
3. Domain services implement business logic for validation and processing
4. Application layer orchestrates the process and maps between DTOs and domain models
5. Infrastructure layer provides concrete implementations

## Validation

Domain models should validate their own consistency:
- Properties with restricted values
- Object state invariants
- Business rule enforcement

Application-level validation (user input) belongs in the Application layer, not here.
