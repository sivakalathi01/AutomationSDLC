# Getting Started - Next Steps

## ✅ Completed Scaffolding

Your Agentic SDLC platform has been fully scaffolded with:

### Backend Services (10 microservices)
- ✅ Orchestrator API with workflow coordination
- ✅ 9 Agent microservices (Requirements, Specification, Story, Architecture, Task Planning, Test Design, Code Generation, IaC/CI-CD, Quality Gate)
- ✅ Shared contracts and schemas
- ✅ Event sourcing with Cosmos DB
- ✅ Async messaging with Service Bus

### Frontend
- ✅ React + TypeScript with Vite
- ✅ Fluent UI components
- ✅ TanStack Query for data management
- ✅ Zustand for state
- ✅ MSAL for Azure AD authentication

### Infrastructure
- ✅ Bicep templates for Azure resources
- ✅ Docker Compose for local development
- ✅ GitHub Actions CI/CD pipelines

### Testing & Quality
- ✅ xUnit test framework setup
- ✅ Test standards matrix
- ✅ JSON schema contracts for all agents

---

## 🎯 Immediate Next Steps (Week 1)

### 1. Set Up Development Environment
```bash
# Clone and setup
cd c:\Work\virtusa

# Install .NET dependencies
dotnet restore backend/services/OrchestratorApi/OrchestratorApi.csproj

# Install frontend dependencies
cd frontend && npm install

# Start Docker Compose for local infra
docker-compose -f infrastructure/docker/docker-compose.yml up -d
```

### 2. Configure Local Environment
- Copy `.env.example` to `.env` (create if missing)
- Set Azure OpenAI credentials
- Configure Service Bus and Cosmos DB connection strings

### 3. Implement Prompt Templates
- Create prompt registry in shared library
- Define system prompts for each agent
- Add few-shot examples

### 4. Build Agent Implementations
- Implement Semantic Kernel integration for each agent
- Add LLM tool calling for Requirements, Specification, Story agents
- Implement deterministic logic for Task Planning and Code Generation

---

## 📋 Phase 1 Implementation Roadmap (30 days)

### Week 1: Foundation
- [ ] Local dev environment working
- [ ] Orchestrator API with basic run management
- [ ] Cosmos DB event store operational
- [ ] Requirements agent MVP

### Week 2: Core Agents
- [ ] Specification agent working
- [ ] Story agent working
- [ ] Basic frontend dashboard
- [ ] Service Bus messaging operational

### Week 3: Code Generation & Testing
- [ ] Code Generation agent MVP
- [ ] Test Design agent MVP
- [ ] Unit tests for all agents
- [ ] Quality gate validation working

### Week 4: Integration & Polish
- [ ] IaC/CI-CD agent
- [ ] End-to-end run execution
- [ ] Approval workflows
- [ ] API documentation

---

## 🚀 Phase 2 Kickoff (Data and Enterprise Hardening)

### Phase 2 Goals
- Introduce Azure SQL Database as read-model and reporting store
- Keep Cosmos DB as immutable event store
- Build projection pipelines from events to relational reporting tables
- Harden security with Key Vault-backed secret flow and managed identity

### Initial Phase 2 Tasks
- [ ] Deploy SQL module from [infrastructure/bicep/sqlDatabase.bicep](./infrastructure/bicep/sqlDatabase.bicep)
- [ ] Run root template with SQL secure parameters
- [ ] Add reporting endpoints backed by SQL read-model
- [ ] Add migration/versioning strategy for relational schema

### Deployment Note for Phase 2
When deploying [infrastructure/bicep/main.bicep](./infrastructure/bicep/main.bicep), provide `sqlAdminPassword` as a secure parameter:

```bash
az deployment sub create \
   --template-file infrastructure/bicep/main.bicep \
   --parameters resourceGroupName=rg-dev-agentic environment=dev sqlAdminLogin=sqladminagentic sqlAdminPassword='<strong-password>' \
   --location eastus
```

---

## 🔐 Phase 3 Kickoff (Security and Governance)

### Phase 3 Goals
- Introduce workload identity via user-assigned managed identity
- Centralize secret governance with Azure Key Vault
- Enable optional JWT authentication and RBAC policies in Orchestrator API

### Initial Phase 3 Tasks
- [ ] Configure Entra app registration and expose API scope
- [ ] Set `Security:EnableAuthentication=true` in production config
- [ ] Set `Security:Jwt:Authority` and `Security:Jwt:Audience`
- [ ] Assign managed identity access to Key Vault (RBAC role assignment)
- [ ] Populate Key Vault and production appsettings with environment-specific values
- [ ] Review authorization coverage for run-management endpoints in non-dev environments

### Deployment Note for Phase 3
Root deployment now includes Key Vault and managed identity modules, so use the same deployment command with secure SQL parameters.

---

## 🔑 Key Files to Review

1. **Contracts**: [backend/shared/Contracts](./backend/shared/Contracts/)
   - Agent input/output schemas
   - Event store models
   - Service Bus message types

2. **Orchestration**: [backend/services/OrchestratorApi](./backend/services/OrchestratorApi/)
   - RunsController.cs - run lifecycle API
   - OrchestrationService.cs - workflow coordination

3. **Frontend**: [frontend/src/pages](./frontend/src/pages/)
   - Dashboard.tsx - entry point
   - RunCreate.tsx - new run wizard

4. **Infrastructure**: [infrastructure/bicep/main.bicep](./infrastructure/bicep/main.bicep)
   - Complete resource definitions
   - Deployment orchestration

---

## 🧪 Testing Your Setup

### 1. Backend Health Check
```bash
cd backend/services/OrchestratorApi
dotnet run

# In another terminal:
curl http://localhost:5000/health
```

### 2. Frontend Health Check
```bash
cd frontend
npm run dev

# Open: http://localhost:5000
```

### 3. Run Integration Test
```bash
dotnet test backend/services/OrchestratorApi/OrchestratorApi.csproj -c Release
```

---

## 📚 Key Concepts

### Event Sourcing
- Every run state change is recorded as immutable events in Cosmos DB
- Enables audit trail and replay capability
- See: [EventStoreContracts.cs](./backend/shared/Contracts/EventStoreContracts.cs)

### Agent Contracts
- Strict JSON schemas for all inputs/outputs
- Enables validation and schema evolution
- See: [*AgentContracts.cs files](./backend/shared/Contracts/)

### Async Messaging
- Service Bus Topics for inter-service communication
- Commands for synchronous patterns
- Events for publish-subscribe
- See: [ServiceBusContracts.cs](./backend/shared/Contracts/ServiceBusContracts.cs)

### Observability
- Correlation IDs for end-to-end tracing
- Structured logging with context
- Integration points for Langfuse (tracing/eval)

---

## 🚀 Deployment Checklist

Before deploying to Azure:

- [ ] All tests passing locally
- [ ] Secrets configured in Key Vault
- [ ] GitHub Actions credentials set up
- [ ] Bicep templates validated: `az bicep build -f infrastructure/bicep/main.bicep`
- [ ] Docker images build successfully
- [ ] Environment variables documented

See [DEPLOYMENT.md](./docs/DEPLOYMENT.md) for detailed instructions.

---

## 💡 Architecture Decision Records (ADR)

Key decisions documented in design:

1. **Microservices from Day 1**: Independent scaling, deployment, and team ownership
2. **Event Sourcing**: Complete audit trail and replay capability
3. **Async Messaging**: Decoupled services, natural retry/failure handling
4. **Semantic Kernel**: Microsoft-native, composable, extensible
5. **React Frontend**: Fast iteration, large ecosystem, Fluent UI integration

---

## ❓ FAQ

**Q: Can I use LangGraph instead of Semantic Kernel?**
A: Yes, LangGraph can be integrated for complex workflow orchestration. Start with SK for MVP, add LG for advanced graph-based control.

**Q: How do I add a new agent?**
A: Copy AgentRequirements service structure, implement IAgentHandler interface, register in Orchestrator routing.

**Q: Where do I add guardrails?**
A: Quality Gate Agent validates all outputs. Add policy engine for compliance checks before approval gates.

**Q: How is cost calculated?**
A: Track token usage per agent in Application Insights. Implement usage quotas in orchestrator if needed.

---

## 📞 Support

For questions about:
- **Architecture**: See [main.bicep](./infrastructure/bicep/main.bicep) and service responsibilities
- **API**: Check [RunsController.cs](./backend/services/OrchestratorApi/Controllers/RunsController.cs)
- **Frontend**: Review [Dashboard.tsx](./frontend/src/pages/Dashboard.tsx)
- **Testing**: See [TESTING.md](./docs/TESTING.md) standards matrix

---

## 🎉 You're Ready!

The full agentic platform scaffolding is complete. Start with Phase 1 and build incrementally. Good luck! 🚀
