FROM anchore/grype:v0.88.0

COPY config/grype.yaml /etc/grype/grype.yaml

HEALTHCHECK NONE

CMD ["dir:/src", "--config", "/etc/grype/grype.yaml"]
