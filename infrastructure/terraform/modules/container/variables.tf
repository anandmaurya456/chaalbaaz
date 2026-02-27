variable "project_name" { type = string }
variable "environment" { type = string }
variable "resource_group_name" { type = string }
variable "location" { type = string }
variable "cpu" { type = number; default = 1 }
variable "memory" { type = number; default = 2 }
variable "stockfish_depth" { type = number; default = 20 }
variable "stockfish_threads" { type = number; default = 2 }
variable "redis_url" { type = string; sensitive = true }
variable "tags" { type = map(string); default = {} }
