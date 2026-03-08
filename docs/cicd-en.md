# CI/CD (GitHub Actions)

## Workflows
- `ci.yml`: Build validation for every push/PR.
- `deploy-azure.yml`: Deploy on push to `main`.

## Required Secrets
Set these in **GitHub Repo Settings → Secrets and variables → Actions**:
- `AZURE_CREDENTIALS`: Service principal JSON for `azure/login`.
- `AZURE_SUBSCRIPTION_ID`: Azure subscription ID (used by Terraform import).
- `TF_STATE_RG`: Resource group for Terraform state.
- `TF_STATE_STORAGE`: Storage account for Terraform state.
- `TF_STATE_CONTAINER`: Storage container for Terraform state.
- `TF_STATE_KEY`: State file name.

## CI (ci.yml)
Key steps:
- `actions/checkout@v4`
- `actions/setup-dotnet@v4`
- `dotnet build`

## Deploy (deploy-azure.yml)
Key steps (high-level):
1. **Azure Login**  
   Uses `AZURE_CREDENTIALS` for `azure/login@v2`.

2. **Terraform Init**  
   Uses remote state in Azure Storage:
   ```bash
   terraform init \
     -backend-config="resource_group_name=$TF_STATE_RG" \
     -backend-config="storage_account_name=$TF_STATE_STORAGE" \
     -backend-config="container_name=$TF_STATE_CONTAINER" \
     -backend-config="key=$TF_STATE_KEY"
   ```

3. **Import Existing Resources (if needed)**  
   Prevents “already exists” failures when resources were created earlier.
   ```bash
   terraform import azurerm_resource_group.rg "/subscriptions/$AZURE_SUBSCRIPTION_ID/resourceGroups/sharegate-demo-rg"
   ```

4. **Base Infra Apply**  
   Applies core resources:
   ```bash
   terraform apply -auto-approve \
     -target="random_string.suffix" \
     -target="azurerm_resource_group.rg" \
     -target="azurerm_log_analytics_workspace.law" \
     -target="azurerm_container_registry.acr" \
     -target="azurerm_container_app_environment.env"
   ```

5. **Build & Push Image**  
   ```bash
   docker build -t $ACR_LOGIN_SERVER/sharegate-demo-api:$IMAGE_TAG -f src/ShareGateDemo.Api/Dockerfile .
   docker push $ACR_LOGIN_SERVER/sharegate-demo-api:$IMAGE_TAG
   ```

6. **Full Apply**  
   Updates Container App with the new image:
   ```bash
   terraform apply -auto-approve -var="api_image_tag=$IMAGE_TAG"
   ```

