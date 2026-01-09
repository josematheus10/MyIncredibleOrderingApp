# 🍺 A Incrível Api de pedidos do Zé

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=.net)
![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?style=flat-square&logo=docker)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-Messaging-FF6600?style=flat-square&logo=rabbitmq)

## 📋 Sobre o Projeto

Microserviço de pedidos que implementa o **padrão Outbox** para garantir a consistência entre o banco de dados relacional e o sistema de mensageria.
Me inspirei no projeto [Rinha de Backend](https://github.com/zanfranceschi/rinha-de-backend-2025) para criar esse projeto focado em demonstrar o padrão outbox com .NET 10, RabbitMQ e SqlServer.

### 🎯 Objetivo

A ideia principal é que todas as mudanças de estado do Order sejam registradas em uma tabela de eventos (outbox) dentro da mesma transação do banco de dados relacional. 
Posteriormente, um processo separado lê esses eventos da tabela outbox e os publica no sistema de mensageria (RabbitMQ).

## 🏗️ Arquitetura

- **Padrão Outbox**: Garantia de consistência eventual
- **Load Balancing**: 4 instâncias da API balanceadas com Nginx
- **Banco de Dados Relacional**: SQL Server para armazenamento transacional
- **RabbitMQ**: Sistema de mensageria assíncrona
- **.NET 10**: Framework principal
- **Docker**: Containerização e orquestração

### 🔄 Escalabilidade

A aplicação roda com **4 instâncias da API** (order.api.1 a order.api.4) balanceadas por um **Nginx** configurado como reverse proxy. Todas as instâncias compartilham o mesmo banco de dados SQL Server e o mesmo barramento de mensagens RabbitMQ.

## 💻 Recursos do Sistema

### Limites de Recursos por Serviço

O ambiente Docker está configurado com limites de recursos para garantir performance previsível:

| Serviço | Instâncias | CPU Limit | Memory Limit | CPU Reservation | Memory Reservation |
|---------|-----------|-----------|--------------|-----------------|-------------------|
| **nginx** | 1 | 0.25 vCPU | 128 MB | 0.1 vCPU | 64 MB |
| **order.api** | 4 | 0.25 vCPU cada | 256 MB cada | 0.125 vCPU cada | 128 MB cada |
| **sqlserver** | 1 | 2.0 vCPU | 5 GB | 1.0 vCPU | 2 GB |
| **rabbitmq** | 1 | 1.0 vCPU | 1.5 GB | 0.5 vCPU | 512 MB |

### 📊 Recursos Totais do Ambiente

| Recurso | Limite Total | Reserva Total |
|---------|-------------|---------------|
| **CPU** | 4.25 vCPU | 2.6 vCPU |
| **Memória** | 7.6 GB | 3.6 GB |

**Distribuição de Recursos:**
- **APIs (4x)**: 1.0 vCPU total / 1 GB RAM total
- **SQL Server**: 2.0 vCPU / 5 GB RAM (componente mais crítico)
- **RabbitMQ**: 1.0 vCPU / 1.5 GB RAM
- **Nginx**: 0.25 vCPU / 128 MB RAM (load balancer leve)

> **Nota:** Os limites de recursos são aplicados automaticamente pelo Docker Compose. SQL Server recebe mais recursos por ser o componente mais crítico para performance. As 4 instâncias da API compartilham a carga de requisições através do Nginx.

## 🚀 Como Executar

### Pré-requisitos

- Docker Desktop instalado
- .NET 10 SDK (para desenvolvimento local)
- **Mínimo recomendado**: 4 CPUs e 8 GB de RAM disponíveis para o Docker

### Executar a Aplicação

1. Clone o repositório:

    ```bash
    git clone https://github.com/josematheus10/MyIncredibleOrderingApp.git
    cd MyIncredibleOrderingApp
    ```

2. Inicie os containers:

    ```bash
    cd src/
    docker-compose up --build
    ```

3. Aguarde todos os serviços iniciarem (SQL Server e RabbitMQ possuem healthchecks)

4. Acesse a documentação da API:

    ```
    http://localhost:5000/swagger-ui.html
    ```

5. Acesse o painel de gerenciamento do RabbitMQ:

    ```
    http://localhost:15672
    Usuário: guest
    Senha: guest
    ```

### 🧪 Executar Testes de Carga

Para rodar o projeto de stress test:

```bash
cd src/Order.Api.ExtressTest
dotnet run
```

## 📊 Resultados de Testes de Carga

### Cenário de Teste

**Duração:** 3 minutos  
**Framework:** NBomber  
**Cenário:** create_order_scenario  
**Session ID:** 2026-01-09_00.56.18_session_6782789b

#### Estratégia de Carga

O teste utilizou uma abordagem progressiva de injeção de carga:

1. **Aquecimento** (15s): Ramping de 5 req/s
2. **Crescimento** (30s): Ramping para 50 req/s
3. **Estabilização** (60s): 50 req/s constante
4. **Pico** (15s): 250 req/s
5. **Recuperação** (30s): Redução para 50 req/s
6. **Stress Máximo** (30s): Ramping para 500 req/s

### Resultados Obtidos

#### Resumo Geral

| Métrica | Valor |
|---------|-------|
| **Total de Requisições** | 17.125 |
| **Requisições Bem-sucedidas** | 17.125 (100%) ✅ |
| **Requisições Falhadas** | 0 (0%) ✅ |
| **RPS Médio** | 95.14 req/s |
| **Data Transferida** | 0 MB |

#### Latência - Todas as Requisições

| Métrica | Latência (ms) |
|---------|---------------|
| **Mínima** | 4.69 ms |
| **Média** | 17.77 ms |
| **Máxima** | 3,595.73 ms |
| **Desvio Padrão** | 69.11 ms |
| **P50 (Mediana)** | 10.23 ms |
| **P75** | 16.48 ms |
| **P95** | 42.66 ms |
| **P99** | 102.40 ms |

### 📈 Análise de Performance

#### Pontos Positivos ✅

- **✨ Taxa de sucesso perfeita**: 100% de requisições processadas com sucesso
- **🚀 Latência excepcionalmente baixa**: 
  - P50: ~10ms (mediana muito baixa)
  - P75: ~16ms (75% das requisições em menos de 16ms)
  - P95: ~43ms (95% das requisições em menos de 43ms)
  - P99: ~102ms (99% das requisições em menos de 102ms)
- **📊 Throughput robusto**: Média consistente de 95.14 RPS durante 3 minutos de teste
- **💪 Alta resiliência**: Sistema permaneceu 100% estável mesmo em picos de 500 req/s
- **⚡ Baixa variabilidade**: Desvio padrão de apenas 69ms indica comportamento muito consistente
- **🎯 Performance previsível**: A grande maioria das requisições (99%) respondida em menos de 102ms


## 📁 Estrutura do Projeto

```
src/
├── Order.Api/                  # API principal
│   ├── Data/                   # Camada de dados
│   ├── Messaging/              # Integração com RabbitMQ
│   ├── Outbox/                 # Implementação do padrão Outbox
│   └── Services/               # Lógica de negócio
├── Order.Api.ExtressTest/      # Testes de carga
├── docker-compose.yml          # Orquestração de containers
└── nginx.conf                  # Configuração do load balancer
```

## 🛠️ Tecnologias Utilizadas

- **.NET 10**
- **Entity Framework Core**
- **RabbitMQ**
- **SQL Server 2022**
- **Nginx** (Load Balancer)
- **Docker & Docker Compose**
- **Swagger/OpenAPI**
- **NBomber** (Load Testing)

## ⚠️ Aviso Importante

> **Nota:** Desconsidere senhas fracas e outras vulnerabilidades de segurança. Este projeto é **apenas para fins educacionais** e de demonstração de como lidar com consistência eventual usando o padrão Outbox.

## 📚 Recursos Adicionais

- [Padrão Outbox](https://microservices.io/patterns/data/transactional-outbox.html)
- [Consistência Eventual](https://en.wikipedia.org/wiki/Eventual_consistency)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [NBomber Documentation](https://nbomber.com/)

---

Feito com ❤️ utilizando ajuda de IA 