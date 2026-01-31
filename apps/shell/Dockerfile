# Stage 1: Build
FROM node:20-alpine AS build
RUN corepack enable && corepack prepare pnpm@9 --activate
WORKDIR /app

# Copy workspace config and all package.json files for dependency resolution
COPY package.json pnpm-workspace.yaml pnpm-lock.yaml ./
COPY apps/shell/package.json apps/shell/
COPY apps/admin/package.json apps/admin/
COPY apps/client/package.json apps/client/
COPY packages/ui-kit/package.json packages/ui-kit/
COPY packages/api-client/package.json packages/api-client/
COPY packages/shared-types/package.json packages/shared-types/
COPY e2e/package.json e2e/

# Install all workspace dependencies
RUN pnpm install --frozen-lockfile

# Copy workspace packages (shared libraries used by shell)
COPY packages/ packages/

# Copy shell source
COPY apps/shell/ apps/shell/

# Build args for Vite env vars (baked in at build time)
ARG VITE_API_BASE_URL=http://localhost:5000
ARG VITE_KEYCLOAK_URL=http://localhost:8080
ARG VITE_KEYCLOAK_REALM=dc-platform
ARG VITE_KEYCLOAK_CLIENT_ID=dc-platform-admin

ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
ENV VITE_KEYCLOAK_URL=$VITE_KEYCLOAK_URL
ENV VITE_KEYCLOAK_REALM=$VITE_KEYCLOAK_REALM
ENV VITE_KEYCLOAK_CLIENT_ID=$VITE_KEYCLOAK_CLIENT_ID

RUN cd apps/shell && pnpm exec vite build

# Stage 2: Serve
FROM nginx:alpine
COPY --from=build /app/apps/shell/dist /usr/share/nginx/html
COPY infrastructure/nginx/spa.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
