# Core Services

## Purpose

This directory contains domain services that implement core business logic.
These services operate on domain models (from Core/Models) and should not
directly use or return DTOs.

## Guidelines

1. Core services should implement interfaces defined in Core/Interfaces
2. Services should operate on domain models, not DTOs
3. Business rules and domain logic belong here
4. Services should not have dependencies on external infrastructure
5. Avoid direct dependencies on UI, database, or external APIs

## Service vs Application Layer

- **Core Services**: Implement domain logic, rules, and algorithms
- **Application Services**: Orchestrate use cases, transform DTOs, and coordinate work

When implementing a new feature:
1. Define domain models in Core/Models
2. Define service interfaces in Core/Interfaces
3. Implement domain logic in Core/Services
4. Create DTOs in the Contracts project
5. Implement use cases in Application layer services
