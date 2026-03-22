FROM sonarqube:lts-community

EXPOSE 9000

VOLUME ["/opt/sonarqube/data", "/opt/sonarqube/logs", "/opt/sonarqube/extensions"]

HEALTHCHECK --interval=30s --timeout=10s --start-period=120s --retries=5 \
  CMD wget -qO- http://localhost:9000/api/system/status | grep -q '"status":"UP"' || exit 1
