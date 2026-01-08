# Zé Incredible Ordering Api

Order.Api/
│
├─ Controllers/          
│   └─ OrdersController.cs       # Endpoints da API
│
├─ Models/                     # DTOs (entrada/saída)
│   └─ CreateOrderRequest.cs
│
├─ Entities/                   # Entidades do banco
│   └─ Order.cs
│
├─ Services/                  
│   ├─ OrderService.cs          # Lógica de negócio
│   └─ RabbitMqService.cs      # Publicação/consumo RabbitMQ
│
├─ Data/                        
│   ├─ AppDbContext.cs         # DbContext do EF Core
│   └─ UserRepository.cs       # Operações de CRUD