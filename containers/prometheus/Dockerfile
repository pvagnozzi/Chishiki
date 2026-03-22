FROM prom/prometheus:v2.54.0

COPY config/prometheus.yml /etc/prometheus/prometheus.yml

EXPOSE 9090

CMD [ \
  "--config.file=/etc/prometheus/prometheus.yml", \
  "--storage.tsdb.path=/prometheus", \
  "--storage.tsdb.retention.time=15d", \
  "--web.console.libraries=/usr/share/prometheus/console_libraries", \
  "--web.console.templates=/usr/share/prometheus/consoles", \
  "--web.enable-lifecycle" \
]
