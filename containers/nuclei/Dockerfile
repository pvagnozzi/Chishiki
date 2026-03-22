FROM projectdiscovery/nuclei:v3.3.9

COPY config/nuclei.yaml /etc/nuclei/nuclei.yaml

HEALTHCHECK NONE

CMD ["-config", "/etc/nuclei/nuclei.yaml"]
