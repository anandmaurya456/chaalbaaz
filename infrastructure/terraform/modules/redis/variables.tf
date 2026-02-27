variable "project_name" { type = string }
variable "environment" { type = string }
variable "resource_group_name" { type = string }
variable "location" { type = string }
variable "sku" { type = string; default = "Basic" }
variable "family" { type = string; default = "C" }
variable "capacity" { type = number; default = 0 }
variable "tags" { type = map(string); default = {} }
