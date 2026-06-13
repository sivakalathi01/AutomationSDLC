# Deployment Guide

## Prerequisites

- Azure Subscription
- Azure CLI
- GitHub Actions configured with Azure credentials
- Docker registry (Azure Container Registry)

## Environment Setup

### 1. Create Azure Resources

```bash
# Set variables
RG_NAME="rg-prod-agentic"
LOCATION="eastus"
ENVIRONMENT="prod"

# Create resource group
az group create --name $RG_NAME --location $LOCATION

# Deploy infrastructure
az deployment sub create \
  --template-file infrastructure/bicep/main.bicep \
  --parameters resourceGroupName=$RG_NAME environment=$ENVIRONMENT \
  --location $LOCATION
```

Preferred deployment pattern with environment parameter files:

```bash
az deployment sub create \
  --location eastus \
  --parameters infrastructure/bicep/main.dev.bicepparam

az deployment sub create \
  --location eastus2 \
  --parameters infrastructure/bicep/main.test.bicepparam

az deployment sub create \
  --location eastus \
  --parameters infrastructure/bicep/main.prod.bicepparam
```

Before production deployment, replace `REPLACE_WITH_SECURE_VALUE` with a secure parameter value source or pass the secure value at deployment time.

### 2. Configure Secrets

Key Vault secret naming convention for app configuration:
- `ConnectionStrings--ServiceBus`
- `ConnectionStrings--CosmosDb`
- `AzureOpenAI--Endpoint`
- `AzureOpenAI--ApiKey`
- `AzureOpenAI--DeploymentName`
- `Security--Jwt--Authority`
- `Security--Jwt--Audience`
- `ApplicationInsights--InstrumentationKey`

Use the provided runbook and sample manifest to seed Key Vault secrets:

```powershell
pwsh -File infrastructure/scripts/Seed-KeyVaultSecrets.ps1 \
  -VaultName kv-prod-agentic \
  -ManifestPath infrastructure/scripts/keyvault-secrets.sample.json
```

Files:
- `infrastructure/scripts/Seed-KeyVaultSecrets.ps1`
- `infrastructure/scripts/keyvault-secrets.sample.json`

Add the following secrets to GitHub:
- `AZURE_CLIENT_ID`: Service principal client ID
- `AZURE_CLIENT_SECRET`: Service principal secret
- `AZURE_TENANT_ID`: Azure tenant ID
- `ACR_URL`: Container Registry URL
- `ACR_USERNAME`: Registry username
- `ACR_PASSWORD`: Registry password
- `VITE_API_URL`: Frontend API URL
- `AZURE_STATIC_WEB_APPS_API_TOKEN`: SWA deployment token

### 3. Deploy Services

#### Manual Deployment

```bash
# Build images
docker build -f infrastructure/docker/Dockerfile.Orchestrator \
  -t $ACR_URL/orchestrator:latest .

# Push to registry
docker push $ACR_URL/orchestrator:latest

# Deploy to Container Apps
az containerapp create \
  --name orchestrator \
  --resource-group $RG_NAME \
  --image $ACR_URL/orchestrator:latest \
  --environment cae-prod-agentic
```

Preferred Phase 3 pattern:
- Deploy infrastructure from [infrastructure/bicep/main.bicep](infrastructure/bicep/main.bicep)
- Seed Key Vault secrets
- Let the orchestrator Container App consume settings through managed identity + Key Vault secret references

The root Bicep deployment now creates the orchestrator Container App and wires:
- user-assigned managed identity
- AcrPull access to ACR
- Key Vault secret references for runtime configuration

#### Automated Deployment via GitHub Actions

Push to main branch - workflows in `.github/workflows/` will automatically:
1. Build and test
2. Push images to ACR
3. Deploy to Container Apps

## Post-Deployment

### 1. Verify Services

```bash
# Check Container Apps
az containerapp list -g $RG_NAME

# Check logs
az containerapp logs show -n orchestrator -g $RG_NAME
```

### 2. Configure Monitoring

- Set up Application Insights alerts
- Configure Langfuse for agent tracing
- Create custom dashboards

### 3. Run Smoke Tests

```bash
# Test orchestrator health
curl https://orchestrator.example.com/health

# Test frontend
curl https://swa-prod-agentic.azurestaticapps.net
```

## Scaling

### Horizontal Scaling

```bash
# Update Container App replica configuration
az containerapp update \
  --name orchestrator \
  --resource-group $RG_NAME \
  --min-replicas 2 \
  --max-replicas 10
```

### Database Scaling

Cosmos DB scales automatically; adjust RU/s as needed:
```bash
az cosmosdb update \
  --name cosmos-prod-agentic \
  --resource-group $RG_NAME \
  --throughput 5000
```

## Rollback

### Container Apps

```bash
# Revert to previous revision
az containerapp revision list -n orchestrator -g $RG_NAME
az containerapp update \
  --name orchestrator \
  --resource-group $RG_NAME \
  --image $ACR_URL/orchestrator:previous-tag
```

## Troubleshooting

### Service won't start
- Check logs: `az containerapp logs show`
- Verify secrets in Key Vault
- Check resource limits
- Verify required non-development settings are present; the API now fails fast when critical secrets/configuration are missing

### Connection issues
- Verify Service Bus connection string
- Check Cosmos DB firewall rules
- Validate environment variables
- Validate Key Vault secret names use `--` in place of `:` for nested configuration keys
- Verify secrets were seeded with `az keyvault secret list --vault-name <vault-name>`
