#!/bin/sh
set -e

echo "[ollama] Starting server..."
/bin/ollama serve &
OLLAMA_PID=$!

echo "[ollama] Waiting for server to be ready..."
until wget -qO- http://localhost:11434/api/tags > /dev/null 2>&1; do
    sleep 1
done
echo "[ollama] Server is ready."

if [ -n "$OLLAMA_DEFAULT_MODEL" ]; then
    echo "[ollama] Pulling model: $OLLAMA_DEFAULT_MODEL"
    /bin/ollama pull "$OLLAMA_DEFAULT_MODEL"
    echo "[ollama] Model ready."
fi

wait $OLLAMA_PID
