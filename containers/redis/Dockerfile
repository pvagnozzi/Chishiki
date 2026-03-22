FROM redis:7-alpine

COPY config/redis.conf /usr/local/etc/redis/redis.conf

HEALTHCHECK --interval=5s --timeout=3s --retries=5 \
  CMD redis-cli ping || exit 1

EXPOSE 6379

ENTRYPOINT ["redis-server", "/usr/local/etc/redis/redis.conf"]
