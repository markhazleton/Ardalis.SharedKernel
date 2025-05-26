# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Initial release of WebSpark.SharedKernel
- Entity base classes with domain event support
- Value object implementation with proper equality semantics
- Domain event infrastructure with MediatR integration
- Repository pattern abstractions built on WebSpark.Specification
- CQRS pattern support with command and query abstractions
- Comprehensive logging behavior for MediatR requests
- Support for .NET 8.0 and .NET 9.0
- Comprehensive XML documentation
- SafeDictionary utility class
- PersonName value object example

### Dependencies

- MediatR 12.5.0
- Microsoft.Extensions.Logging.Abstractions 9.0.5
- WebSpark.Specification 9.1.0

## [1.0.0] - 2025-05-25

### Added

- Initial release with comprehensive DDD building blocks
- Entity base classes supporting multiple ID types
- Domain event infrastructure
- Value object base implementation
- Repository abstractions
- CQRS support interfaces
- MediatR integration
- Logging behavior implementation
