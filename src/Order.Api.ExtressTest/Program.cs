using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NBomber.CSharp;

namespace Order.Api.ExtressTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:65404")
            };

            bool continuar = true;
            
            while (continuar)
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════╗");
                Console.WriteLine("║   TESTE DE CARGA - ORDER API           ║");
                Console.WriteLine("╚════════════════════════════════════════╝");
                Console.WriteLine();
                Console.WriteLine("1 - Executar teste de carga");
                Console.WriteLine("2 - Sair");
                Console.WriteLine();
                Console.Write("Escolha uma opção: ");
                
                var opcao = Console.ReadLine();
                
                switch (opcao)
                {
                    case "1":
                        ExecutarTeste(httpClient);
                        break;
                    case "2":
                        continuar = false;
                        Console.WriteLine("\nEncerrando...");
                        break;
                    default:
                        Console.WriteLine("\n❌ Opção inválida! Pressione qualquer tecla para continuar...");
                        Console.ReadKey();
                        break;
                }
            }

            httpClient.Dispose();
        }

        static void ExecutarTeste(HttpClient httpClient)
        {
            Console.WriteLine("\n🚀 Iniciando teste de carga...\n");
            
            var scenario = Scenario.Create("create_order_scenario", async context =>
            {
                var random = new Random();
                
                var orderRequest = new
                {
                    clienteId = Guid.NewGuid(),
                    valorTotal = Math.Round((decimal)(random.NextDouble() * 1000 + 1), 2),
                };

                var jsonContent = JsonSerializer.Serialize(orderRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                try
                {
                    var response = await httpClient.PostAsync("/Orders", content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        return Response.Ok();
                    }
                    else
                    {
                        return Response.Fail();
                    }
                }
                catch (Exception)
                {
                    return Response.Fail();
                }
            })
            .WithoutWarmUp()
            .WithLoadSimulations(
                Simulation.Inject(rate: 300,
                                  interval: TimeSpan.FromSeconds(1),
                                  during: TimeSpan.FromMinutes(2))
            );

            NBomberRunner
                .RegisterScenarios(scenario)
                .WithReportFileName("order-api-load-test")
                .WithReportFolder("reports")
                .Run();
            
            Console.WriteLine("\n✅ Teste de carga finalizado! Verifique o relatório detalhado na pasta './reports'");
            Console.WriteLine("\nPressione qualquer tecla para voltar ao menu...");
            Console.ReadKey();
        }
    }
}