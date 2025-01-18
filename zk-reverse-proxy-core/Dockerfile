FROM node:20-slim

WORKDIR /app

RUN addgroup --system --gid 1001 nodejs && \
    adduser --system --uid 1001 nodeuser && \
    chown -R nodeuser:nodejs /app

COPY package*.json ./
RUN apt-get update && \
    apt-get install -y --no-install-recommends python3 make g++ && \
    npm ci && \
    apt-get purge -y python3 make g++ && \
    apt-get autoremove -y && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

COPY --chown=nodeuser:nodejs . .

ENV NODE_ENV=production

USER nodeuser

ARG DEFAULT_PORT=80
ARG ZK_CONNECT_STRING=""
ENV PORT=$DEFAULT_PORT
ENV ZK_CONNECT_STRING=$ZK_CONNECT_STRING

ENTRYPOINT [ "node", "src/app.js" ]
CMD ["$ZK_CONNECT_STRING", "$PORT"]
