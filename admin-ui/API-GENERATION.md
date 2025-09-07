# API Generation from Swagger

This project automatically generates TypeScript API clients and DTOs from the backend Swagger specification.

## Overview

The API generation system uses OpenAPI Generator to create type-safe TypeScript clients from your backend's Swagger/OpenAPI specification. This ensures your frontend stays in sync with backend API changes.

## Prerequisites

1. **Backend must be running** on `http://localhost:5046`
2. **Swagger endpoint** must be accessible at `http://localhost:5046/swagger/v1/swagger.json`

## Usage

### Generate API Client Once

```bash
npm run generate:api
```

This will:
- Check if the backend is running
- Download the Swagger specification
- Generate TypeScript API client and DTOs
- Create an index file for easy imports
- Update .gitignore to exclude generated files

### Watch Mode (Development)

```bash
npm run generate:api:watch
```

This will watch for changes in the backend API project and automatically regenerate the client when changes are detected.

### Automatic Generation

The API client is automatically generated before each build:

```bash
npm run build  # Runs generate:api first
```

## Generated Files

The generated files are placed in `src/api/generated/`:

```
src/api/generated/
├── apis/           # API client classes
├── models/         # TypeScript interfaces/DTOs
├── base/           # Base configuration and types
└── index.ts        # Main export file
```

## Importing Generated APIs

```typescript
// Import specific APIs
import { RulesApi, UsersApi } from '@/api/generated';

// Import DTOs/models
import { Rule, CreateRule, UpdateRule } from '@/api/generated';

// Import everything
import * as ApiClient from '@/api/generated';
```

## Configuration

The generation is configured via `openapi-generator-config.json`. Key settings:

- **Generator**: `typescript-axios` (uses Axios for HTTP requests)
- **Output**: `./src/api/generated`
- **Separate Models**: Models and APIs are in separate folders
- **Naming**: camelCase for properties, PascalCase for types
- **Enums**: String enums with UPPERCASE naming

## Integration with Existing Code

The generated API client can be integrated with your existing axios configuration:

```typescript
import { Configuration } from '@/api/generated';
import apiClient from './client'; // Your existing axios instance

// Configure the generated client to use your axios instance
const config = new Configuration({
  basePath: 'http://localhost:5046/api',
  accessToken: () => localStorage.getItem('auth_token') || '',
});

// Use the generated APIs
const rulesApi = new RulesApi(config, undefined, apiClient);
```

## Troubleshooting

### Backend Not Running

If you see "Backend is not running or not accessible", start your backend:

```bash
dotnet run --project src/GamificationEngine.Api
```

### Generation Fails

1. Ensure the backend is running and accessible
2. Check that the Swagger endpoint returns valid JSON
3. Verify the OpenAPI Generator CLI is installed: `npm list @openapitools/openapi-generator-cli`

### Type Conflicts

If you have naming conflicts between generated and existing types:

1. Use namespace imports: `import { Rule as GeneratedRule } from '@/api/generated'`
2. Update your existing types to match the generated ones
3. Consider removing manual type definitions in favor of generated ones

## Best Practices

1. **Don't edit generated files** - they will be overwritten on next generation
2. **Use the generated types** instead of manually defining DTOs
3. **Run generation regularly** during development to stay in sync
4. **Commit generated files** only if your team doesn't have the backend running locally
5. **Use watch mode** during active development

## CI/CD Integration

For CI/CD pipelines, ensure the backend is running before generating APIs:

```yaml
# Example GitHub Actions step
- name: Start Backend
  run: dotnet run --project src/GamificationEngine.Api &
  
- name: Wait for Backend
  run: npx wait-on http://localhost:5046/swagger/v1/swagger.json
  
- name: Generate API Client
  run: npm run generate:api
```
