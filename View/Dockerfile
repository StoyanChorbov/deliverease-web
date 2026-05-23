ARG NODE_VERSION=22.13.1
FROM node:${NODE_VERSION}-slim AS base
ENV NODE_ENV=development
WORKDIR /app

# Build
FROM base AS build
WORKDIR /app
COPY View/package*.json ./
RUN npm install

FROM base
COPY --from=build /app/node_modules /app/node_modules
COPY View/. /app/
CMD ["npm", "run", "start"]