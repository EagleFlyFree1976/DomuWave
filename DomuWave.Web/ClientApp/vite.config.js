import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import vueDevTools from 'vite-plugin-vue-devtools'
 

const buildVersion = Date.now()

function htmlVersionPlugin() {
  return {
    name: 'html-version',
    transformIndexHtml(html) {
      return html
        .replace(/assets\/js\/app\.js/g, `assets/js/app.js?v=${buildVersion}`)
        .replace(/assets\/css\/style\.css/g, `assets/css/style.css?v=${buildVersion}`)
    }
  }
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueJsx(),
    vueDevTools(),
    htmlVersionPlugin(),
  ],
  build: {
    rollupOptions: {
      output: {
        // Nomi fissi — il cache busting è gestito via query string nell'index.html
        entryFileNames: `assets/js/app.js`,
        chunkFileNames: `assets/js/[name].js`,
        assetFileNames: (assetInfo) => {
          if (assetInfo.name && assetInfo.name.endsWith('.css')) {
            return 'assets/css/style.css';
          }
          return 'assets/[name][extname]';
        }
      }
    },
    emptyOutDir: true
  },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  define: {
    // Assicurati che la variabile NODE_ENV sia corretta
    'process.env.NODE_ENV': JSON.stringify(process.env.NODE_ENV || 'development'),
  },
})
