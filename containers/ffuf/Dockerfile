FROM golang:1.23-alpine AS builder

ARG FFUF_VERSION=v2.1.0

RUN apk add --no-cache git && \
    go install github.com/ffuf/ffuf/v2@${FFUF_VERSION}

FROM alpine:3.20

COPY --from=builder /go/bin/ffuf /usr/local/bin/ffuf
COPY config/wordlists/ /opt/wordlists/

HEALTHCHECK NONE

ENTRYPOINT ["ffuf"]
CMD ["-w", "/opt/wordlists/common.txt", "-json", "-o", "/output/ffuf.json"]
