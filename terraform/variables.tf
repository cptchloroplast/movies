variable "cloudflare_account_id" {}
variable "cloudflare_zone_id" {}
variable "github_owner" {}
variable "github_repository" {}

# Environment Variables
variable "D1_CLOUDFLARE_API_TOKEN" {
  sensitive = true
}
variable "TMDB_TOKEN" {
  sensitive = true
}
variable "HISTORY_URL" {}
variable "OAUTH_TENANT" {}

# GitHub Actions Secrets
variable "TF_API_TOKEN" {
  sensitive = true
}