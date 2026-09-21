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
            title: 'NotificationLog',
            description: 'Tutorial de DDD y arquitectura sobre un contexto de Notificaciones real.',
            defaultLocale: 'root',
            locales: {
                root: { label: 'Español', lang: 'es' }
            },
            customCss: ['./src/styles/custom.css'],
            sidebar: [
                { label: 'Proyecto', items: [{ autogenerate: { directory: 'proyecto' } }] },
                { label: 'DDD', items: [{ autogenerate: { directory: 'ddd' } }] },
                { label: 'Event Sourcing', items: [{ autogenerate: { directory: 'event-sourcing' } }] },
                { label: 'Arquitectura', items: [{ autogenerate: { directory: 'arquitectura' } }] },
                { label: 'Patrones', items: [{ autogenerate: { directory: 'patrones' } }] }
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
