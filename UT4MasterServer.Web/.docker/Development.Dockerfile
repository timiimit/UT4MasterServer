FROM node:lts-alpine as build-stage

WORKDIR /app/

COPY package*.json /app/
COPY tsconfig*.json /app/

RUN npm ci
COPY ./ /app/
RUN npm run build-dev

FROM nginxinc/nginx-unprivileged

COPY --from=build-stage /app/dist/ /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf

ENTRYPOINT ["nginx", "-g", "daemon off;"]
EXPOSE 80
