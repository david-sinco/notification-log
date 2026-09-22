// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import { fileURLToPath } from 'node:url';

const repoRoot = fileURLToPath(new URL('../../', import.meta.url)).replace(/[\\/]$/, '');

export default defineConfig({
    server: {
        port: Number(process.env.PORT) || 4321,
        host: true
    },
    integrations: [
        starlight({
            title: 'Llave',
            description: 'Guía de DDD, event sourcing y arquitectura sobre el código real de Llave.',
            defaultLocale: 'root',
            locales: {
                root: { label: 'Español', lang: 'es' }
            },
            customCss: ['./src/styles/custom.css'],
            sidebar: [
                { label: 'Proyecto', items: [{ autogenerate: { directory: 'proyecto' } }] },
                { label: 'Arquitectura', items: [{ autogenerate: { directory: 'arquitectura' } }] },
                { label: 'Patrones', items: [{ autogenerate: { directory: 'patrones' } }] },
                { label: 'OAuth 2.0', items: [{ autogenerate: { directory: 'oauth' } }] },
                { label: 'DDD', items: [{ autogenerate: { directory: 'ddd' } }] },
                { label: 'Event Sourcing', items: [{ autogenerate: { directory: 'event-sourcing' } }] }
            ]
        })
    ],
    vite: {
        resolve: {
            alias: { '@code': repoRoot }
        },
        server: {
            fs: { allow: [repoRoot] }
        }
    }
});
