# DC Platform

Multi-tenant enterprise workflow automation platform by Digital Control d.o.o.

## Overview

DC Platform is the foundation for our productized solutions:
- **Procedure Execution** - Workflow automation
- **Approvals & Decision Control** - Business rules and approvals
- **Regulatory & Operational Reporting** - Compliance reporting
- **Enterprise Integration & Orchestration** - System integration

## Architecture

### Backend Services

| Service | Purpose | Tech |
|---------|---------|------|
| Gateway | API Gateway, BFF, routing | .NET Core |
| Authentication | Keycloak integration, token management | .NET Core |
| Directory | Organizations, Workspaces, Memberships | .NET Core |
| Access Control | RBAC/ABAC permission engine | .NET Core |
| Audit | Immutable compliance event log | .NET Core |
| Notification | Email, SMS, push notifications | .NET Core |
| Configuration | Tenant settings, feature flags | .NET Core |
| Admin API | Administration orchestration | .NET Core |

### Frontend Applications

| App | Purpose | Tech |
|-----|---------|------|
| Shell | Microfrontend container | Vue.js 3 |
| Admin | Tenant/user administration | Vue.js 3 |
| Client | End-user product modules | Vue.js 3 |

### Infrastructure

- **Identity**: Keycloak (single realm, multi-tenant via attributes)
- **Database**: PostgreSQL (per-service schema)
- **Cache**: Redis
- **Messaging**: RabbitMQ
- **Containerization**: Docker, Kubernetes

## Getting Started

### Prerequisites

- .NET SDK 8.0+
- Node.js 20+
- pnpm (`npm install -g pnpm`)
- Docker Desktop
- PostgreSQL 15+ (or via Docker)

### Local Development

```bash
# Clone repository
git clone <repo-url>
cd dc-platform

# Start infrastructure
docker-compose up -d

# Run a backend service
cd services/directory
dotnet restore
dotnet run

# Run frontend
cd apps/shell
pnpm install
pnpm dev
```

## Project Structure

```
dc-platform/
├── services/           # Backend microservices
├── apps/               # Frontend applications
├── packages/           # Shared libraries
├── infrastructure/     # Docker, K8s configs
├── docs/               # Documentation
├── CLAUDE.md           # AI assistant instructions
└── README.md           # This file
```

## Development Guidelines

### Git Workflow

1. Create feature branch: `git checkout -b feature/<service>/<description>`
2. Make changes with atomic commits
3. Push and create Pull Request
4. Code review required before merge
5. Squash merge to main

### Service Development

Each service follows Clean Architecture:
```
ServiceName/
├── ServiceName.API/           # Controllers, DTOs, Middleware
├── ServiceName.Application/   # Use cases, Commands, Queries
├── ServiceName.Domain/        # Entities, Value Objects, Events
└── ServiceName.Infrastructure/# DB, External services, Messaging
```

### Code Standards

- **Backend**: C# coding conventions, async/await, DI
- **Frontend**: Vue.js style guide, TypeScript strict
- **API**: RESTful conventions, consistent error responses
- **Testing**: Unit tests required, integration tests for critical paths

## Documentation

- [Architecture Decision Records](docs/adr/)
- [API Documentation](docs/api/)
- [Deployment Guide](docs/deployment/)

## Team

Digital Control d.o.o. - Enterprise BPM and workflow solutions.

## License

Proprietary - All rights reserved.
