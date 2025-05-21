# Application Services

## Purpose

This directory contains application services that implement use cases.
These services coordinate domain operations, handle DTO conversions,
and orchestrate business workflows.

## Guidelines

1. Application services should follow the mediator pattern where possible
2. Accept DTOs from the Contracts project as inputs
3. Return DTOs from the Contracts project as outputs
4. Use the mapper service to convert between DTOs and domain models
5. Coordinate work between multiple domain services
6. Handle cross-cutting concerns like validation and error handling

## Service Responsibility

Application services should:

1. Accept input DTOs from controllers or the UI layer
2. Validate inputs (using FluentValidation or similar)
3. Map DTOs to domain models
4. Call domain services to perform business logic
5. Handle any transaction coordination
6. Map results back to DTOs
7. Return response DTOs to the caller

Application services should NOT:

1. Contain business logic (belongs in Core)
2. Access infrastructure directly (use interfaces)
3. Depend on UI or presentation concerns
