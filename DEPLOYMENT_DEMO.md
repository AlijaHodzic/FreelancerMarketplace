# Demo Deployment

This project is set up for a demo deployment with:

- `frontend` on Vercel
- `backend` on Render

## 1. Deploy the backend to Render

Create a new Render Web Service from this repository and point it to:

- Root Directory: `backend/Freelance.Api`
- Runtime: `.NET`

Render can also read the included [render.yaml](/C:/Users/User/Desktop/Freelance%20web/FreelanceMarketplace/render.yaml).

Important environment variables:

- `ASPNETCORE_ENVIRONMENT=Production`
- `ALLOW_DESTRUCTIVE_DB_RESET=true`
- `Cors__AllowedOrigins=https://your-vercel-project.vercel.app`
- `Jwt__Issuer=Freelance.Api`
- `Jwt__Audience=Freelance.Client`
- `Jwt__Key=<strong-random-secret>`

For a simple demo, SQLite is used with a persistent disk.

Health check:

- `/health`

## 2. Configure the frontend for Vercel

Deploy the `frontend` folder as the Vercel project root.

Recommended settings:

- Root Directory: `frontend`
- Build Command: `npm run build`

The frontend now uses:

- local development: `http://localhost:5005/api`
- production/demo: `/api`

That means Vercel must proxy `/api/*` to your Render backend.

## 3. Add the Vercel API rewrite

After Render gives you a public URL like:

- `https://your-render-service.onrender.com`

copy [vercel.json.example](/C:/Users/User/Desktop/Freelance%20web/FreelanceMarketplace/frontend/vercel.json.example) to `frontend/vercel.json` and replace the placeholder URL.

Example:

```json
{
  "rewrites": [
    {
      "source": "/api/(.*)",
      "destination": "https://your-render-service.onrender.com/api/$1"
    }
  ]
}
```

## 4. Demo accounts

Use these seeded demo logins:

- Client: `hello@novacommerce.demo` / `Pass1234`
- Freelancer: `alex.rivera@freelancehub.demo` / `Pass1234`

## Notes

- This setup is good for demo/review flows.
- For long-term production, replace SQLite with Postgres.
- Once the first deploy succeeds, you can set `ALLOW_DESTRUCTIVE_DB_RESET=false` to avoid accidental schema resets.
