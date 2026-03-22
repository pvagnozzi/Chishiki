FROM owasp/dependency-check:latest

WORKDIR /src

HEALTHCHECK NONE

CMD ["--scan", "/src", "--format", "JSON", "--out", "/output", "--enableRetired"]
