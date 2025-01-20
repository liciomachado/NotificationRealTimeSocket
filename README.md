# Projeto MS Notifications - .NET 8 + Postgres com Docker 

Este é um projeto .NET que utiliza Docker para facilitar o ambiente de desenvolvimento e execução. Utiliza Docker Compose para orquestrar a aplicação juntamente com Redis Pub/sub para envio das notificações aos ouvintes.

## Pré-requisitos

- Docker Engine: [Instalação do Docker](https://docs.docker.com/get-docker/)
- Docker Compose: [Instalação do Docker Compose](https://docs.docker.com/compose/install/)

## Como executar

1. Clone este repositório:

 ```bash
   git clone https://github.com/liciomachado/NotificationRealTimeSocket.git
   cd NotificationRealTimeSocket
  ```

2. Execute o seguinte comando para iniciar o projeto junto com o Redis:

```bash
  docker-compose up -d
```
Isso iniciará os contêineres Docker em segundo plano (-d para detached mode), incluindo a aplicação .NET e o banco de dados PostgreSQL.

Acesse o back-end em: http://localhost:7035
Acesse o front SSE em: sse.html
Acesse o front WebSocket em: websocket.html

3. Parando a execução do projeto e removendo os containers

```bash
  docker-compose down
```
