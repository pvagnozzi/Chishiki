FROM pgvector/pgvector:pg17

COPY init/ /docker-entrypoint-initdb.d/
RUN find /docker-entrypoint-initdb.d/ -name "*.sh" -exec sed -i 's/\r//' {} +

HEALTHCHECK --interval=5s --timeout=5s --start-period=20s --retries=5 \
  CMD pg_isready -U ${POSTGRES_USER:-postgres}

EXPOSE 5432
