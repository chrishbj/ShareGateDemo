output "ecr_repository_url" {
  description = "ECR repository URL for the API image."
  value       = aws_ecr_repository.api.repository_url
}

output "load_balancer_dns_name" {
  description = "Public DNS name of the Application Load Balancer."
  value       = aws_lb.api.dns_name
}

output "api_url" {
  description = "Public API base URL."
  value       = "http://${aws_lb.api.dns_name}/"
}
