# Domain Models in Core Layer

## Purpose

This directory contains domain models that represent the business entities and value objects
of the TrialWorld application. These are **NOT** DTOs and should not be used directly for
data transfer across layer boundaries.

## Domain Models vs DTOs

- **Domain Models** (here in Core/Models):
  - Contain business logic and validation rules
  - Represent internal business concepts
  - May have behavior (methods) beyond just data
  - Should not be exposed outside the application

- **DTOs** (in Contracts project):
  - Simple data carriers with no business logic
  - Used for transferring data across boundaries
  - No methods beyond property getters/setters
  - Designed for serialization/deserialization

## Guidelines

1. Do not add DTO-like objects to this directory
2. Keep business logic and validation in domain models
3. Use proper mapping between domain models and DTOs
4. Maintain separation between domain and presentation concerns
