import tailwindcss from '@tailwindcss/vite';
import siteConstants from './constants/site.constants';

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2024-11-01',
  devtools: { enabled: true },
  modules: ['@pinia/nuxt', '@vesp/nuxt-fontawesome', '@nuxtjs/mdc'],
  css: ['~/main.css'],
  app: {
    head: {
      title: siteConstants.title,
      link: [
        {
          rel: 'stylesheet',
          href: 'https://rsms.me/inter/inter.css'
        },
        {
          rel: 'apple-touch-icon',
          sizes: '180x180',
          href: '/apple-touch-icon.png'
        },
        {
          rel: 'icon',
          type: 'image/png',
          sizes: '32x32',
          href: '/favicon-32x32.png'
        },
        {
          rel: 'icon',
          type: 'image/png',
          sizes: '16x16',
          href: '/favicon-16x16.png'
        },
        {
          rel: 'manifest',
          href: '/site.webmanifest'
        }
      ]
    }
  },
  vite: {
    plugins: [tailwindcss()]
  },
  routeRules: {
    '/api/**': {
      proxy: `${import.meta.env.PROXY_URL}/api/**`
    }
  },
  fontawesome: {
    icons: {
      solid: [
        'download',
        'comments',
        'spinner',
        'circle-exclamation',
        'chevron-down',
        'clock',
        'circle-info'
      ]
    }
  },
  mdc: {
    headings: {
      anchorLinks: false
    }
  }
});
