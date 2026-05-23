# Build Stage 1

FROM node:22-alpine AS build
WORKDIR /app

RUN corepack enable && corepack prepare pnpm@11.2.2 --activate

# Copy package.json and lockfile
COPY . ./

# Install dependencies
RUN pnpm install --frozen-lockfile

# Copy the entire project

# Build the project
RUN pnpm run build

# Build Stage 2

FROM node:22-alpine
WORKDIR /app

# Only `.output` folder is needed from the build stage
COPY --from=build /app/.output/ ./

# Change the port and host
ENV PORT=3000
ENV HOST=0.0.0.0

EXPOSE 3000

CMD ["node", "/app/server/index.mjs"]