variable "project_name" {
  type = string
}
variable "environment" {
  type = string
}
variable "resource_group_name" {
  type = string
}
variable "location" {
  type = string
}
variable "sku" {
  type    = string
  default = "B1"
}
variable "engine_url" {
  type = string
}
variable "redis_connection_string" {
  type = string
}
variable "app_insights_connection_string" {
  type    = string
  default = ""
}
variable "tags" {
  type    = map(string)
  default = {}
}
