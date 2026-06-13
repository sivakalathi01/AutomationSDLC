# Virtusa Agentic SDLC Platform

An enterprise-grade agentic framework for end-to-end SDLC automation using Microsoft Azure stack, Semantic Kernel, and LangGraph.

## 🏗️ Architecture

The platform is built as a microservices system with the following components:

### Backend (9 Microservices + 1 Orchestrator)
- **Orchestrator API**: Coordinates workflow execution and state management
- **Requirements Agent**: Generates business and technical requirements
- **Specification Agent**: Produces functional and technical specifications
- **Story Agent**: Breaks down specs into epics, features, and stories
- **Architecture Agent**: Captures ADRs and design decisions
- **Task Planning Agent**: Creates implementation task plans
- **Test Design Agent**: Generates test cases and test plans
- **Code Generation Agent**: Produces .NET code
- **IaC & CI-CD Agent**: Generates Bicep templates and pipelines
- **Quality Gate Agent**: Validates all outputs

### Frontend
- React + TypeScript on Azure Static Web Apps
- Fluent UI components for Microsoft-native look
- TanStack Query for data management
- Zustand for state management

### Infrastructure
- Azure OpenAI for LLM capability
- Azure Service Bus for async messaging
- Azure Cosmos DB for event sourcing
- Azure SQL Database for Phase 2 read model/reporting
- Azure Key Vault for Phase 3 secret governance
- User-assigned Managed Identity for Phase 3 workload identity
- Azure Container Apps for microservices
- Azure Static Web Apps for frontend
- Application Insights for observability

### Phase 3 Security Hardening
- Config-driven JWT authentication in Orchestrator API
- Role-based authorization policy scaffolding (`EngineeringLeadOrAdmin`, `SecurityOrAdmin`)
- Key Vault and managed identity modules wired into root Bicep template
- Environment-specific Bicep parameter files for `dev`, `test`, and `prod`
- Environment-specific backend configuration files for `Development`, `Test`, and `Production`
- Operational run-management endpoints protected with authorization policies
- Fail-fast startup validation for missing required secrets in non-development environments
- Orchestrator plus core Phase 1 agents deployed with managed identity + Key Vault-backed Container Apps patterns
- Remaining agent services wired into the same managed identity + Key Vault-backed Container Apps deployment model

### Deployment Parameters
- [infrastructure/bicep/main.dev.bicepparam](infrastructure/bicep/main.dev.bicepparam)
- [infrastructure/bicep/main.test.bicepparam](infrastructure/bicep/main.test.bicepparam)
- [infrastructure/bicep/main.prod.bicepparam](infrastructure/bicep/main.prod.bicepparam)

### Secret Seeding Automation
- [infrastructure/scripts/Seed-KeyVaultSecrets.ps1](infrastructure/scripts/Seed-KeyVaultSecrets.ps1)
- [infrastructure/scripts/keyvault-secrets.sample.json](infrastructure/scripts/keyvault-secrets.sample.json)

### Backend Environment Configs
- [backend/services/OrchestratorApi/appsettings.Development.json](backend/services/OrchestratorApi/appsettings.Development.json)
- [backend/services/OrchestratorApi/appsettings.Test.json](backend/services/OrchestratorApi/appsettings.Test.json)
- [backend/services/OrchestratorApi/appsettings.Production.json](backend/services/OrchestratorApi/appsettings.Production.json)

### Service Discovery
- The orchestrator now consumes agent endpoints through `Services__...` configuration keys.
- In local development these default to `http://localhost:5001` through `http://localhost:5009`.
- In Azure Container Apps these values are supplied from the root Bicep deployment.

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- Node.js 18+
- Docker and Docker Compose
- Azure CLI

### Local Development

1. **Clone the repository**
```bash
git clone https://github.com/virtusa/agentic-platform
cd agentic-platform
```

2. **Start infrastructure with Docker Compose**
```bash
docker-compose -f infrastructure/docker/docker-compose.yml up -d
```

3. **Build and run backend services**
```bash
cd backend/services/OrchestratorApi
dotnet restore
dotnet run
```

4. **Build and run frontend**
```bash
cd frontend
npm install
npm run dev
```

The application will be available at:
- Frontend: http://localhost:3000
- Orchestrator API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger

## 📋 Testing Standards

### Coverage Targets
- Domain logic: 90%+
- General logic: 80%+
- Agent contracts: 100%
- Critical workflows: 100% pass

### Test Frameworks
- Unit & Integration: xUnit + FluentAssertions
- E2E: Playwright
- Accessibility: axe-core
- Load Testing: k6

## 🔄 Workflow

1. **Create Run**: User submits SDLC automation request
2. **Orchestration**: Orchestrator creates execution plan
3. **Agent Execution**: The orchestrator now invokes Requirements, Specification, and Story agent APIs through configured service endpoints for the Phase 1 flow
4. **Phase 2 Delivery Flow**: The orchestrator can also invoke Architecture and Task Planning agent APIs through `POST /api/runs/{id}/execute-phase2`
5. **Quality Gates**: Final validation across all outputs
6. **Approvals**: Role-based review and sign-off
7. **Deployment**: Generated artifacts deployed to Azure

## 🏛️ Agent Contracts

All agents follow strict JSON schemas for inputs and outputs:
- **Input**: Problem domain and constraints
- **Output**: Structured artifacts with traceability

See `backend/shared/Contracts/` for full schema definitions.

## 📊 Observability

- Application Insights for telemetry
- Langfuse for agent tracing and evaluation
- Correlation IDs for end-to-end tracing
- Structured logging with context

## 🔐 Security

- Entra ID for authentication
- RBAC for authorization
- Key Vault for secrets management
- Private endpoints for Azure services
- SAST and dependency scanning in CI/CD

## 📁 Project Structure

```
.
├── backend/
│   ├── services/
│   │   ├── OrchestratorApi/
│   │   ├── AgentRequirements/
│   │   ├── AgentSpecification/
│   │   ├── AgentStory/
│   │   ├── AgentArchitecture/
│   │   ├── AgentTaskPlanning/
│   │   ├── AgentTestDesign/
│   │   ├── AgentCodeGeneration/
│   │   ├── AgentIaCCicd/
│   │   └── QualityGateService/
│   └── shared/
│       ├── Contracts/
│       └── Infrastructure/
├── frontend/
│   ├── src/
│   │   ├── pages/
│   │   ├── components/
│   │   ├── services/
│   │   └── hooks/
│   └── package.json
└── infrastructure/
    ├── docker/
    ├── bicep/
    └── pipelines/
```

## 🛠️ Development Guidelines

### Code Style
- C#: Microsoft coding standards
- TypeScript/React: ESLint + Prettier
- YAML: Consistent indentation (2 spaces)

### Git Workflow
- Feature branches from `develop`
- PR reviews required
- CI/CD gates before merge
- Semantic versioning for releases

### Testing Checklist
- [ ] Unit tests written
- [ ] Integration tests added
- [ ] E2E coverage for critical paths
- [ ] Contract schemas validated
- [ ] Security tests passed
- [ ] Performance benchmarks met

## 📚 Documentation

- [API Reference](./docs/API.md)
- [Testing Strategy](./docs/TESTING.md)
- [Deployment Guide](./docs/DEPLOYMENT.md)
- [Architecture Decisions](./docs/ADR.md)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is proprietary and confidential to Virtusa.

## 📞 Support

For questions or issues, please contact the Agentic Platform team.
