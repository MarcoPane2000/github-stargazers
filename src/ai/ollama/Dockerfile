FROM ollama/ollama:latest

RUN ollama serve & \
    sleep 5 && \
    ollama pull phi3 && \
    ollama pull nomic-embed-text

EXPOSE 11434
ENTRYPOINT ["ollama", "serve"]