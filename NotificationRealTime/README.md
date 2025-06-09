# Projeto MS Notifications - .NET 8 + Redis com Docker 

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

2. Execute o seguinte comando para iniciar o Redis:

```bash
  docker-compose up -d
```

- Acesse o back-end em: http://localhost:7035
- Acesse o front SSE em: sse.html?channel=channel1 -> para o canal ser channel1, é possivel mudar para outros 
- Acesse o front WebSocket em: websocket.html?channel=channel1 -> para o canal ser channel1, é possivel mudar para outros 

3. Parando a execução do projeto e removendo os containers

```bash
  docker-compose down
```
