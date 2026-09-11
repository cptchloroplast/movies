locals {
  secrets = {
    "TF_API_TOKEN" : var.TF_API_TOKEN,
    "CLOUDFLARE_API_TOKEN" : var.D1_CLOUDFLARE_API_TOKEN,
  }
  audience = "https://${var.github_repository}.${module.zone.name}"
}

module "zone" {
  source  = "app.terraform.io/okkema/zone/cloudflare"
  version = "~> 0.1"

  zone_id = var.cloudflare_zone_id
}

module "secrets" {
  for_each = local.secrets

  source  = "app.terraform.io/okkema/secret/github"
  version = "~> 0.2"

  repository = var.github_repository
  key        = each.key
  value      = each.value
}

module "database" {
  source  = "app.terraform.io/okkema/database/cloudflare"
  version = "~> 0.1"

  account_id = var.cloudflare_account_id
  name       = var.github_repository
}

module "worker" {
  source  = "app.terraform.io/okkema/worker/cloudflare"
  version = "~> 0.12"

  account_id          = var.cloudflare_account_id
  zone_id             = var.cloudflare_zone_id
  name                = var.github_repository
  hostnames           = [var.github_repository]
  content             = file(abspath("${path.module}/../dist/index.js"))
  compatibility_flags = toset(["nodejs_compat_v2"])
  secrets = [
    { name = "SENTRY_DSN", value = module.sentry.dsn },
    { name = "TMDB_TOKEN", value = var.TMDB_TOKEN },
  ]
  env_vars = [
    { name = "OAUTH_AUDIENCE", value = local.audience },
    { name = "OAUTH_TENANT", value = var.OAUTH_TENANT },
    { name = "HISTORY_URL", value = var.HISTORY_URL },
  ]
  databases = [
    { binding = "DB", id = module.database.id }
  ]

  depends_on = [module.database, module.sentry]
}

module "sentry" {
  source  = "app.terraform.io/okkema/project/sentry"
  version = "~> 0.4"

  github_organization = var.github_owner
  github_repository   = var.github_repository
}

module "server" {
  source  = "app.terraform.io/okkema/server/auth0"
  version = "~> 0.1"

  name       = var.github_repository
  identifier = local.audience
  scopes = {
    "read:movies" : "Read TMDB movies",
    "read:history" : "Read history",
  }
}
