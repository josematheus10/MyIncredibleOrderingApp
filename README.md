# Zé Incredible Ordering Api

Order.Api/
│
├─ Controllers/          
│   └─ OrdersController.cs       # Endpoints da API
│
├─ Models/                     # DTOs (entrada/saída)
│   └─ CreateOrderRequest.cs
│

│
├─ Services/                  
│   ├─ OrderService.cs          # Lógica de negócio
│ 
├─ Messaging/                  # RabbitMQ
│   ├─ OrderCreatedPublisher.cs # Publica eventos Order
├─ Data/                        
│   ├─ AppDbContext.cs          # DbContext do EF Core
│   ├─ OrderRepository.cs        # Operações de CRUD do User
│   └─ Entities/                # Entidades do banco
│       └─ Order.cs