FROM qdrant/qdrant:v1.13.1

COPY config/config.yaml /qdrant/config/production.yaml

HEALTHCHECK --interval=5s --timeout=5s --start-period=10s --retries=5 \
  CMD wget -qO- http://localhost:6333/healthz || exit 1

EXPOSE 6333
EXPOSE 6334
