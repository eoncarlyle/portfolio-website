FROM registry.fedoraproject.org/fedora-minimal:39

# Install Node.js 20 and development tools
RUN microdnf -y install nodejs nodejs-devel npm python3 make gcc gcc-c++ && \
    microdnf clean all

WORKDIR /app

# Add non-root user
RUN useradd -r -u 1001 -g 0 nodeuser && \
    chown -R nodeuser:0 /app

COPY package*.json ./
RUN npm ci && \
    # Keep build dependencies since they're minimal in Fedora
    microdnf clean all

COPY --chown=nodeuser:0 . .
RUN npm run build

ENV NODE_ENV=production
USER 1001

ARG DEFAULT_PORT=80
ARG ZK_CONNECT_STRING=""
ENV PORT=$DEFAULT_PORT
ENV ZK_CONNECT_STRING=$ZK_CONNECT_STRING

ENTRYPOINT [ "node", "dist/index.js" ]
CMD ["$ZK_CONNECT_STRING", "$PORT"]
