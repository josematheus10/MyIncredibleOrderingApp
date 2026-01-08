# 🍺 Zé Incredible Ordering API

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=.net)
![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?style=flat-square&logo=docker)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-Messaging-FF6600?style=flat-square&logo=rabbitmq)

## 📋 Sobre o Projeto

Microserviço de pedidos que implementa o **padrão Outbox** para garantir a consistência entre o banco de dados relacional e o sistema de mensageria.
Me insperie no projeto Rinha de backend para criar esse projeto focado em demonstrar o padrão outbox com .NET 10, RabbitMQ e Docker.
Não consegui atingir numero tão altos de requisições por segundo, mas consegui bons resultados em testes de carga.

### 🎯 Objetivo

A ideia principal é que todas as mudanças de estado do pedido sejam registradas em uma tabela de eventos (outbox) dentro da mesma transação do banco de dados relacional. Posteriormente, um processo separado lê esses eventos da tabela outbox e os publica no sistema de mensageria (RabbitMQ).

## 🏗️ Arquitetura

- **Padrão Outbox**: Garantia de consistência eventual
- **Banco de Dados Relacional**: Armazenamento transacional
- **RabbitMQ**: Sistema de mensageria assíncrona
- **.NET 10**: Framework principal
- **Docker**: Containerização

## 💻 Recursos do Sistema

### Limites de Recursos

O ambiente Docker está configurado com limites de recursos para garantir performance previsível:

| Serviço | CPU Limit | Memory Limit | CPU Reservation | Memory Reservation |
|---------|-----------|--------------|-----------------|-------------------|
| **order.api** | 1.0 vCPU | 2 GB | 0.5 vCPU | 1 GB |
| **sqlserver** | 2.0 vCPU | 4 GB | 1.0 vCPU | 2 GB |
| **rabbitmq** | 1.0 vCPU | 2 GB | 0.5 vCPU | 512 MB |
| **TOTAL** | **4.0 vCPU** | **8 GB** | **2.0 vCPU** | **3.5 GB** |

> **Nota:** Os limites de recursos são aplicados automaticamente pelo Docker Compose. SQL Server recebe mais recursos por ser o componente mais crítico para performance.

## 🚀 Como Executar

### Pré-requisitos

- Docker Desktop instalado
- .NET 10 SDK (para desenvolvimento local)

### Executar a Aplicação

1. Clone o repositório:

    ```bash
    git clone https://github.com/josematheus10/MyIncredibleOrderingApp.git
    cd MyIncredibleOrderingApp
    ```

2. Inicie os containers:

    ```bash
    cd src
    docker-compose up --build
    ```

3. Acesse a documentação da API:

    ```
    http://localhost:5000/swagger-ui.html
    ```

### 🧪 Executar Testes de Carga

Para rodar o projeto de stress test:

```bash
cd src/Order.Api.StressTest
dotnet run
```

## 📁 Estrutura do Projeto

```
src/
├── Order.Api/                  # API principal
│   ├── Data/                   # Camada de dados
│   ├── Messaging/              # Integração com RabbitMQ
│   ├── Outbox/                 # Implementação do padrão Outbox
│   └── Services/               # Lógica de negócio
├── Order.Api.StressTest/       # Testes de carga
└── docker-compose.yml          # Orquestração de containers
```

## 🛠️ Tecnologias Utilizadas

- **.NET 10**
- **Entity Framework Core**
- **RabbitMQ**
- **SQL Server**
- **Docker & Docker Compose**
- **Swagger/OpenAPI**

## ⚠️ Aviso Importante

> **Nota:** Desconsidere senhas fracas e outras vulnerabilidades de segurança. Este projeto é **apenas para fins educacionais** e de demonstração de como lidar com consistência eventual usando o padrão Outbox.

## 📚 Recursos Adicionais

- [Padrão Outbox](https://microservices.io/patterns/data/transactional-outbox.html)
- [Consistência Eventual](https://en.wikipedia.org/wiki/Eventual_consistency)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)

## 📝 Licença

Este projeto é para fins educacionais.

---

Feito com ❤️ utilizando IA como auxiliar, como por exemplo a criação do README.md