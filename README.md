# Projeto MS Notifications - .NET 8 + Postgres com Docker 

Este � um projeto .NET que utiliza Docker para facilitar o ambiente de desenvolvimento e execu��o. Utiliza Docker Compose para orquestrar a aplica��o juntamente com um banco de dados PostgreSQL.

## Pr�-requisitos

- Docker Engine: [Instala��o do Docker](https://docs.docker.com/get-docker/)
- Docker Compose: [Instala��o do Docker Compose](https://docs.docker.com/compose/install/)

## Como executar

1. Clone este reposit�rio:

 ```bash
   git clone https://github.com/liciomachado/NotificationRealTimeSocket.git
   cd NotificationRealTimeSocket
  ```

2. Execute o seguinte comando para iniciar o projeto junto com o Redis:

```bash
  docker-compose up -d
```
Isso iniciar� os cont�ineres Docker em segundo plano (-d para detached mode), incluindo a aplica��o .NET e o banco de dados PostgreSQL.

Acesse o back-end em: http://localhost:7035
Acesse o front SSE em: sse.html
Acesse o front WebSocket em: websocket.html

3. Parando a execu��o do projeto e removendo os containers

```bash
  docker-compose down
```
