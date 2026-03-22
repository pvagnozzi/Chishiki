FROM quay.io/keycloak/keycloak:26.0 AS builder

ENV KC_DB=postgres
ENV KC_HEALTH_ENABLED=true
ENV KC_METRICS_ENABLED=true

RUN /opt/keycloak/bin/kc.sh build

FROM quay.io/keycloak/keycloak:26.0

COPY --from=builder /opt/keycloak/ /opt/keycloak/
COPY themes/chishiki-login /opt/keycloak/themes/chishiki-login
COPY config/realm-export.json /opt/keycloak/data/import/realm-export.json

HEALTHCHECK --interval=10s --timeout=10s --start-period=90s --retries=5 \
  CMD curl -f http://localhost:8080/health/ready || exit 1

EXPOSE 8080

ENTRYPOINT ["/opt/keycloak/bin/kc.sh"]
CMD ["start-dev", "--import-realm"]
