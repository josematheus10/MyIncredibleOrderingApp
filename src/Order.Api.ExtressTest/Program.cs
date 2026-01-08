using System;
using System.Diagnostics;
using System.IO;
using System.Net;
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
                BaseAddress = new Uri("http://localhost:5000")
            };

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
                        return Response.Ok(HttpStatusCode.Accepted);
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
                // Warm-up phase: 5 requests/sec for 15 seconds
                Simulation.RampingInject(rate: 5,
                                         interval: TimeSpan.FromSeconds(1),
                                         during: TimeSpan.FromSeconds(15)),

                // Gradual ramp-up: from 5 to 50 requests/sec over 30 seconds
                Simulation.RampingInject(rate: 50,
                                         interval: TimeSpan.FromSeconds(1),
                                         during: TimeSpan.FromSeconds(30)),

                // Sustained load: 50 requests/sec for 1 minute
                Simulation.Inject(rate: 50,
                                 interval: TimeSpan.FromSeconds(1),
                                 during: TimeSpan.FromMinutes(1)),

                // Spike test: 250 requests/sec for 15 seconds
                Simulation.Inject(rate: 250,
                                 interval: TimeSpan.FromSeconds(1),
                                 during: TimeSpan.FromSeconds(15)),

                // Recovery: back to 50 requests/sec for 30 seconds
                Simulation.Inject(rate: 50,
                                 interval: TimeSpan.FromSeconds(1),
                                 during: TimeSpan.FromSeconds(30)),

                // Stress test: gradual increase to 500 requests/sec
                Simulation.RampingInject(rate: 500,
                                         interval: TimeSpan.FromSeconds(1),
                                         during: TimeSpan.FromSeconds(30))
            );

            NBomberRunner
                .RegisterScenarios(scenario)
                .WithReportFileName("order-api-load-test")
                .WithReportFolder("reports")
                .Run();

            Console.WriteLine("\n✅ Teste concluído! Abrindo relatório...\n");

            var htmlReportPath = Path.Combine(Directory.GetCurrentDirectory(), "reports", "order-api-load-test.html");

            if (File.Exists(htmlReportPath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = htmlReportPath,
                        UseShellExecute = true
                    });
                    Console.WriteLine($"📊 Relatório aberto: {htmlReportPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao abrir relatório: {ex.Message}");
                    Console.WriteLine($"📁 Relatório salvo em: {htmlReportPath}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ Relatório HTML não encontrado em: {htmlReportPath}");
            }

            Console.WriteLine("\n👋 Encerrando aplicação...\n");
        }
    }
}