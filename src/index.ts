import { SentryWorker } from "@okkema/worker/sentry"
import { API, AuthBindings, authorize, AuthVariables } from "@okkema/worker/api"

type Environment = {
  SENTRY_DSN: string
  TMDB_TOKEN: string
  HISTORY_URL: string
  DEBUG: string
  REDIRECT: string
} & AuthBindings

type Variables = AuthVariables

const BASE_URL_V3 = "https://api.themoviedb.org/3"

export default SentryWorker<Environment>({
  fetch(req, env, ctx) {
    const app = API<Environment, Variables>({
      tokenUrl: `https://${env.OAUTH_TENANT}/oauth/token`,
      scopes: {
        "read:movies": "Read TMDB movies",
        "read:history": "Read history"
      },
      options: {
        docs_url: null
      }
    })

    async function handler(c) {
      const url = new URL(c.req.url)
      c.res = await fetch(`${BASE_URL_V3}${url.pathname}${url.search}`, {
        headers: {
          "Authorization": `Bearer ${env.TMDB_TOKEN}`
        }
      })
    }

    app.get("/movie/*", authorize("read:movies"), handler)
    app.get("/search/movie", authorize("read:movies"), handler)

    async function historyHandler(c) {
      const url = new URL(c.req.url)
      c.res = await fetch(`${env.HISTORY_URL}${url.pathname}${url.search}`, {
        headers: {
          "Authorization": c.req.header("Authorization")!
        }
      })
    }

    app.get("/history/*", authorize("read:history"), historyHandler)
    
    if (new URL(req.url).pathname === "/") 
      return new Response("Redirecting...", { 
        status: 301, 
        headers: {
          Location: env.REDIRECT,
        }
      })
    return app.fetch(req as any, env, ctx) as any
  }
})
