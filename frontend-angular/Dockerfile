FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginx:1.27-alpine
WORKDIR /usr/share/nginx/html

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/frontend-angular/browser/ ./
COPY docker-entrypoint.sh /docker-entrypoint.d/40-runtime-config.sh
RUN chmod +x /docker-entrypoint.d/40-runtime-config.sh

ENV API_BASE_URL=/api \
    FIREBASE_API_KEY= \
    FIREBASE_AUTH_DOMAIN= \
    FIREBASE_PROJECT_ID= \
    FIREBASE_APP_ID=

EXPOSE 80
