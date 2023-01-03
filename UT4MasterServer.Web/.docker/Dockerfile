FROM node:lts-alpine

# set the working directory
WORKDIR /app

# install app dependencies
COPY package.json ./

RUN npm install
RUN npm install -g serve

# Bundle app source
COPY . ./

RUN chown -R node:node /app/node_modules

# start app
CMD ["npm", "run", "dev"]