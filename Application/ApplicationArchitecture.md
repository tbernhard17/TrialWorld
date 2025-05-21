# Application Layer Architecture

The Application layer orchestrates the use cases of the TrialWorld system. It sits between the presentation and core layers, coordinating the flow of data and enforcing application-specific rules.

## Key Components

### Application Services

Located in `Application/Services/`, these implement use cases:

- **Media Application Services**: Coordinate media operations
- **Transcription Services**: Manage speech-to-text workflows
- **Analysis Services**: Coordinate content analysis processes
- **Enhancement Services**: Manage media enhancement workflows

Application services should:
- Accept and return DTOs, not domain models
- Coordinate multiple domain services
- Handle application-level validation
- Not contain business logic (belongs in Core)
- Be thin orchestrators

### Mapping

Located in `Application/MappingProfiles/`, these handle DTO-to-model conversion:

- **DtoMappingProfile**: Maps between DTOs and domain models
- **MapperService**: Provides mapping capabilities to application services

The mapping layer ensures:
- Clean separation between DTOs and domain models
- Centralized mapping logic
- Consistent transformation rules

### Common

Located in `Application/Common/`, these provide cross-cutting functionality:

- **Behaviors**: Cross-cutting concerns like validation, logging, performance
- **Exceptions**: Application-specific exceptions
- **Interfaces**: Internal interfaces for application services

## CQRS Pattern (Optional)

For complex use cases, the CQRS pattern can be used:

- **Commands**: Represent actions that change state
- **Queries**: Represent data retrieval operations
- **Handlers**: Process commands and queries

## Validation Flow

1. Receive DTO from presentation layer
2. Validate DTO structure and content (using FluentValidation)
3. Map to domain model
4. Call domain service for business logic
5. Map result back to DTO
6. Return DTO to presentation layer

## Example: Media Enhancement Flow
