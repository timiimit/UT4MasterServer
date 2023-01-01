FROM node:lts-alpine

WORKDIR /app
COPY package.json ./
RUN npm install
RUN npm install -g serve
COPY . ./
RUN chown -R node:node /app/node_modules
CMD ["npm", "run", "dev"]