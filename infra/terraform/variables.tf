variable "name_prefix" {
  type        = string
  description = "Prefix used for Azure resources."
  default     = "sharegate-demo"
}

variable "location" {
  type        = string
  description = "Azure region."
  default     = "canadacentral"
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