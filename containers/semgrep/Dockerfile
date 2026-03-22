FROM semgrep/semgrep:1.96.0

WORKDIR /src

HEALTHCHECK NONE

CMD ["semgrep", "--config=auto", "--json", "--output=/output/semgrep.json", "."]
