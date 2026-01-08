using System.Text.Json.Serialization;

namespace Order.Api.Models
{
    public record OrderCreatedEvent
    {
        [JsonPropertyName("pedidoId")]
        public Guid OrderId { get; set; }

        [JsonPropertyName("clienteId")]
        public Guid CustomerId { get; set; }

        [JsonPropertyName("valorTotal")]
        public decimal TotalValue { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "CRIANDO";

        [JsonPropertyName("dataCriacao")]
        public DateTime CreationDate { get; set; }
    }
}
