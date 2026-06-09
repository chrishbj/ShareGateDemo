variable "name_prefix" {
  type        = string
  description = "Prefix used for AWS resources."
  default     = "sharegate-demo"
}

variable "aws_region" {
  type        = string
  description = "AWS region."
  default     = "ca-central-1"
}

variable "api_image_name" {
  type        = string
  description = "Container image name for the API."
  default     = "sharegate-demo-api"
}

variable "api_image_tag" {
  type        = string
  description = "Container image tag for the API."
  default     = "v1"
}

variable "mongo_database" {
  type        = string
  description = "MongoDB database name."
  default     = "sharegate_demo"
}

variable "tags" {
  type        = map(string)
  description = "Tags applied to AWS resources."
  default = {
    Project = "ShareGateDemo"
    Purpose = "InterviewDemo"
  }
}
