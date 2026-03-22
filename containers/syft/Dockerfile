FROM anchore/syft:v1.20.0

COPY config/syft.yaml /etc/syft/syft.yaml

HEALTHCHECK NONE

CMD ["scan", "/src", "--config", "/etc/syft/syft.yaml"]
