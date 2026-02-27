variable "project_name" { type = string }
variable "environment" { type = string }
variable "resource_group_name" { type = string }
variable "location" { type = string }
variable "sku" { type = string; default = "Free_F1" }
variable "capacity" { type = number; default = 1 }
variable "tags" { type = map(string); default = {} }
