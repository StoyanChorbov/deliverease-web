# Use small image for smaller size
ARG NODE_VERSION=22
FROM node:${NODE_VERSION}-alpine AS build

# Setup environment
#ENV PNPM_HOME="/pnpm"
#ENV PATH="${PNPM_HOME}:${PATH}"
RUN corepack enable

# Copy package config files to the image and install dependencies
WORKDIR /app
COPY View/package.json View/pnpm-lock.yaml ./
RUN pnpm i

# Copy and build the application
COPY View/. ./
RUN pnpm run build

# Copy the compiled application to a new image
FROM node:${NODE_VERSION}-alpine
WORKDIR /app
COPY --from=build /app/.output ./.output

# Build the application
ENV NODE_ENV=production
ENV HOST=0.0.0.0
ENV PORT=3000

# Start the application
EXPOSE 3000
CMD ["node", "/app/server/index.mjs"]