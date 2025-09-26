import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  server: {
    port: 3301,
    https: {
      key: '../certs/localhost.key',
      cert: '../certs/localhost.crt'
    }
  },
  plugins: [react()],
})
