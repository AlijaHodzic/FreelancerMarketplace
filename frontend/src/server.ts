import {
  AngularNodeAppEngine,
  createNodeRequestHandler,
  isMainModule,
  writeResponseToNodeResponse,
} from '@angular/ssr/node';
import express from 'express';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const serverDistFolder = dirname(fileURLToPath(import.meta.url));
const browserDistFolder = resolve(serverDistFolder, '../browser');

const app = express();
const angularApp = new AngularNodeAppEngine();
const backendApiBaseUrl = process.env['API_PROXY_TARGET'] || 'http://localhost:5005';
const bodylessMethods = new Set(['GET', 'HEAD']);

/**
 * Proxy API requests during SSR/prerender so production builds can still
 * resolve marketplace data when the frontend uses a relative /api path.
 */
app.use('/api/**', async (req, res) => {
  try {
    const targetUrl = new URL(req.originalUrl, backendApiBaseUrl);
    const requestBody =
      bodylessMethods.has(req.method.toUpperCase()) || !req.readable
        ? undefined
        : await new Promise<Buffer>((resolve, reject) => {
            const chunks: Buffer[] = [];

            req.on('data', (chunk) => chunks.push(Buffer.isBuffer(chunk) ? chunk : Buffer.from(chunk)));
            req.on('end', () => resolve(Buffer.concat(chunks)));
            req.on('error', reject);
          });

    const response = await fetch(targetUrl, {
      method: req.method,
      headers: {
        accept: req.headers.accept ?? 'application/json',
        'content-type': req.headers['content-type'] ?? 'application/json',
      },
      body: requestBody,
    });

    const body = await response.text();
    res.status(response.status);
    response.headers.forEach((value, key) => res.setHeader(key, value));
    res.send(body);
  } catch (error) {
    console.error('API proxy failed:', error);
    res.status(502).json({ message: 'API proxy failed during SSR.' });
  }
});

/**
 * Serve static files from /browser
 */
app.use(
  express.static(browserDistFolder, {
    maxAge: '1y',
    index: false,
    redirect: false,
  }),
);

/**
 * Handle all other requests by rendering the Angular application.
 */
app.use('/**', (req, res, next) => {
  angularApp
    .handle(req)
    .then((response) =>
      response ? writeResponseToNodeResponse(response, res) : next(),
    )
    .catch(next);
});

/**
 * Start the server if this module is the main entry point.
 * The server listens on the port defined by the `PORT` environment variable, or defaults to 4000.
 */
if (isMainModule(import.meta.url)) {
  const port = process.env['PORT'] || 4000;
  app.listen(port, () => {
    console.log(`Node Express server listening on http://localhost:${port}`);
  });
}

/**
 * Request handler used by the Angular CLI (for dev-server and during build) or Firebase Cloud Functions.
 */
export const reqHandler = createNodeRequestHandler(app);
