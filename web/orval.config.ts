import { defineConfig } from 'orval'

// Generates typed React Query hooks from the OpenAPI document that the .NET build writes to web/openapi.
export default defineConfig({
  songTheSame: {
    input: './openapi/openapi.json',
    output: {
      target: './src/shared/api/generated/endpoints.ts',
      schemas: './src/shared/api/generated/models',
      client: 'react-query',
      httpClient: 'fetch',
      mode: 'tags-split',
      clean: true,
      override: {
        mutator: {
          path: './src/shared/api/apiFetch.ts',
          name: 'apiFetch',
        },
      },
    },
  },
})
