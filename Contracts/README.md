# Contracts Layer

## Purpose

The Contracts layer contains all Data Transfer Objects (DTOs) and interface definitions
that are used for communication between the application and external systems or layers.

## Contents

- **DTOs**: Simple data carriers used for transferring data
- **Request/Response Models**: Models for API requests and responses
- **Enums**: Enumerations shared between layers
- **Constants**: Shared constant values

## Guidelines

1. All DTOs should be placed in this project, not in Application or Core
2. DTOs should have no business logic - only data properties
3. DTOs should be designed for serialization and data transfer
4. Follow consistent naming conventions (e.g., suffix with `Dto`)
5. Group related DTOs in appropriate subdirectories
