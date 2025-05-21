# DTOs in the Application Layer

## IMPORTANT: This directory is being deprecated

In Clean Architecture, DTOs (Data Transfer Objects) should be defined in the Contracts project, 
not in the Application layer. This directory is maintained only for backward compatibility 
and all DTOs should be moved to the `/Contracts` project.

## Migration Plan

1. All new DTOs should be created in the `/Contracts` project
2. Existing DTOs in this directory will be gradually moved to the `/Contracts` project
3. References to Application-layer DTOs should be updated to use Contracts-layer DTOs

## Rationale

According to Clean Architecture principles:

- **Domain Models** (in Core): Contain business logic and rules
- **DTOs** (in Contracts): Simple data carriers for external communication
- **Application Services**: Transform between DTOs and domain models

Having DTOs in the Application layer violates these principles and creates confusion.
