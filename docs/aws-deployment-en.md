# AWS Deployment Design

## Goal
This branch ports the backend deployment design from Azure Container Apps to AWS for an interview demo.

The first AWS version prioritizes a runnable, low-friction demo:
- API runs on ECS Fargate.
- API image is stored in Amazon ECR.
- Public access is through an Application Load Balancer.
- MongoDB runs as a sidecar container in the same ECS task.
- Logs go to CloudWatch Logs.

For production, replace the Mongo sidecar with Amazon DocumentDB or MongoDB Atlas.

## AWS Resource Mapping
- Azure Container Registry -> Amazon ECR
- Azure Container Apps -> Amazon ECS Fargate
- Azure Container Apps ingress -> Application Load Balancer
- Log Analytics -> CloudWatch Logs
- Mongo sidecar -> Mongo sidecar for demo, DocumentDB/Atlas for production
- Terraform azurerm provider -> Terraform aws provider

## Terraform Location
```text
infra/aws/
  main.tf
  variables.tf
  outputs.tf
  terraform.tfvars.example
```

## Local Docker Test
```powershell
docker compose -f infra/docker-compose.yml up -d
docker build -t sharegate-demo-api:aws-local -f src\ShareGateDemo.Api\Dockerfile .
docker rm -f sharegate-demo-api-aws-local
docker run -d --name sharegate-demo-api-aws-local -p 5070:8080 `
  -e ASPNETCORE_URLS=http://+:8080 `
  -e Mongo__ConnectionString="mongodb://host.docker.internal:27018" `
  -e Mongo__Database=sharegate_demo_aws_local `
  sharegate-demo-api:aws-local
```

Health check:
```powershell
Invoke-RestMethod -Uri http://localhost:5070/api/health
```

## AWS Deployment Flow
1. Configure AWS OIDC role for GitHub Actions.
2. Add GitHub secret `AWS_ROLE_TO_ASSUME`.
3. Push to `feat/aws-deploy` or run `deploy-aws` manually.
4. Workflow creates or updates ECR first.
5. Workflow builds and pushes the API image.
6. Terraform deploys ECS Fargate, ALB, networking, and logs.
7. Use `terraform output api_url` to get the endpoint.

## GitHub Actions Secrets
```text
AWS_ROLE_TO_ASSUME
```

## Manual Terraform Commands
```bash
cd infra/aws
terraform init
terraform apply -target=aws_ecr_repository.api
```

Build and push the image:
```bash
aws ecr get-login-password --region ca-central-1 | docker login --username AWS --password-stdin <account-id>.dkr.ecr.ca-central-1.amazonaws.com
docker build -t <ecr-repository-url>:v1 -f src/ShareGateDemo.Api/Dockerfile .
docker push <ecr-repository-url>:v1
```

Deploy everything:
```bash
terraform apply -var="api_image_tag=v1"
```

## Desktop Endpoint
After deployment, add the Terraform `api_url` output to `src/ShareGateDemo.Desktop/appsettings.json`:
```json
{
  "Name": "AWS",
  "Url": "http://<alb-dns-name>/"
}
```

## Production Follow-up
For a more production-like AWS design:
- Replace Mongo sidecar with DocumentDB or MongoDB Atlas.
- Store Mongo connection string in AWS Secrets Manager.
- Use private subnets for ECS tasks.
- Add NAT Gateway or VPC endpoints for private egress.
- Add ACM certificate and Route 53 custom domain.
- Add autoscaling policies for the ECS service.
