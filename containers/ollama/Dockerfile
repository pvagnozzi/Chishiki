FROM ollama/ollama:0.6.5

COPY scripts/entrypoint.sh /usr/local/bin/entrypoint.sh
RUN sed -i 's/\r//' /usr/local/bin/entrypoint.sh && chmod +x /usr/local/bin/entrypoint.sh

HEALTHCHECK --interval=10s --timeout=5s --start-period=30s --retries=5 \
  CMD wget -qO- http://localhost:11434/api/tags || exit 1

EXPOSE 11434

ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]
