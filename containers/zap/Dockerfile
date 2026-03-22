FROM ghcr.io/zaproxy/zaproxy:stable

EXPOSE 8090

HEALTHCHECK --interval=15s --timeout=5s --start-period=60s --retries=5 \
  CMD curl -s "http://localhost:8090/JSON/core/view/version/" || exit 1

CMD ["-daemon", \
     "-port", "8090", \
     "-host", "0.0.0.0", \
     "-config", "api.addrs.addr.name=.*", \
     "-config", "api.addrs.addr.regex=true", \
     "-config", "api.disablekey=true"]
