FROM aquasec/trivy:0.60.0

COPY config/trivy.yaml /etc/trivy/trivy.yaml

HEALTHCHECK NONE

CMD ["fs", "--config", "/etc/trivy/trivy.yaml", "/src"]
