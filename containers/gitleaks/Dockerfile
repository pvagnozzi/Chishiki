FROM zricethezav/gitleaks:v8.23.3

COPY config/gitleaks.toml /etc/gitleaks/gitleaks.toml

HEALTHCHECK NONE

CMD ["detect", "--source", "/path", "--config", "/etc/gitleaks/gitleaks.toml", \
     "--report-format", "json", "--report-path", "/output/gitleaks.json", "--exit-code", "0"]
