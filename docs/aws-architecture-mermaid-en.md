# AWS Deployment Relationship Diagram

This diagram shows how the GitHub repository, CI/CD workflow, Terraform state, AWS compute, container registry, logs, and client entry points relate to each other in the AWS demo deployment.

```mermaid
flowchart LR
    developer[Developer / Interview Demo] -->|git push| github[GitHub Repo\nchrishbj/ShareGateDemo]

    github -->|push to feat/aws-deploy| gha[GitHub Actions\ndeploy-aws workflow]
    gha -->|OIDC assume role| iam[AWS IAM Role\nsharegate-demo-github-actions]

    gha -->|terraform init / apply| tf[Terraform\ninfra/aws]
    tf -->|remote state| s3[S3 Terraform State Bucket\nsharegate-demo-tfstate-...]
    tf -->|state locking| ddb[DynamoDB Lock Table\nsharegate-demo-tf-locks]

    gha -->|docker build & push| ecr[ECR Repository\nsharegate-demo-api]
    tf -->|provisions / updates| ecs[ECS Fargate Service\nsharegate-demo-qjex0i-api]
    ecr -->|API image| ecs

    ecs -->|runs task definition| task[ECS Task\napi container + mongo sidecar]
    task --> api[ASP.NET Core API\nPort 8080]
    task --> mongo[MongoDB Container\nSidecar demo database]
    api -->|read/write jobs| mongo

    tf -->|provisions| alb[Application Load Balancer\nsharegate-demo-qjex0i-alb]
    alb -->|routes HTTP /api/*| api

    user[WPF Desktop App / Browser / curl] -->|HTTP endpoint| alb

    task -->|container logs| cw[CloudWatch Logs\n/ecs/sharegate-demo-qjex0i-api]

    subgraph awsAccount["AWS Account 598886663126 / ca-central-1"]
        iam
        s3
        ddb
        ecr
        ecs
        task
        api
        mongo
        alb
        cw
    end
```

## Public Endpoint

Base URL:

```text
http://sharegate-demo-qjex0i-alb-1455212926.ca-central-1.elb.amazonaws.com
```

Health check:

```text
http://sharegate-demo-qjex0i-alb-1455212926.ca-central-1.elb.amazonaws.com/api/health
```

## Console Locations

- Region: `Canada (Central) ca-central-1`
- ECS cluster: `sharegate-demo-qjex0i-cluster`
- ECS service: `sharegate-demo-qjex0i-api`
- Load balancer: `sharegate-demo-qjex0i-alb`
- ECR repository: `sharegate-demo-api`
- CloudWatch log group: `/ecs/sharegate-demo-qjex0i-api`
- Terraform state bucket: `sharegate-demo-tfstate-598886663126-ca-central-1`
- Terraform lock table: `sharegate-demo-tf-locks`

## Demo Architecture Note

The MongoDB container is intentionally deployed as a sidecar in the same ECS task for a lightweight interview demo. This keeps the deployment self-contained and cheap, but it also means data is local to the running task. For production, the data layer should move to a managed/shared service such as Amazon DocumentDB, MongoDB Atlas, or another persistent database service.

