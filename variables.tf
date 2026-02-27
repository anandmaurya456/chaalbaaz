# ============================================
# Chaalbaaz — Terraform Variables
# ============================================

variable "project_name" {
  description = "Project name used for all Azure resource naming"
  type        = string
  default     = "chaalbaaz"
}

variable "environment" {
  description = "Deployment environment"
  type        = string
  validation {
    condition     = contains(["dev", "prod"], var.environment)
    error_message = "Environment must be dev or prod."
  }
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "centralindia"
}

variable "backend_sku" {
  description = "App Service Plan SKU"
  type        = string
  default     = "B1"
}

variable "redis_sku" {
  description = "Azure Redis Cache SKU"
  type        = string
  default     = "Basic"
}

variable "redis_family" {
  type    = string
  default = "C"
}

variable "redis_capacity" {
  type    = number
  default = 0
}

variable "container_cpu" {
  description = "CPU cores for engine container"
  type        = number
  default     = 1
}

variable "container_memory" {
  description = "Memory in GB for engine container"
  type        = number
  default     = 2
}

variable "tags" {
  description = "Common tags for all resources"
  type        = map(string)
  default = {
    project     = "chaalbaaz"
    managed_by  = "terraform"
    repo        = "https://github.com/anandmaurya456/chaalbaaz"
  }
}
