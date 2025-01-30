import tailwindcss from '@tailwindcss/vite';

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2024-11-01',
  devtools: { enabled: true },
  modules: ['@pinia/nuxt', '@vesp/nuxt-fontawesome', '@nuxtjs/mdc'],
  css: ['~/assets/main.css'],
  app: {
    head: {
      link: [
        {
          rel: 'stylesheet',
          href: 'https://rsms.me/inter/inter.css'
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
      solid: ['download', 'comments', 'spinner']
    }
  },
  mdc: {
    headings: {
      anchorLinks: false
    }
  }
});
