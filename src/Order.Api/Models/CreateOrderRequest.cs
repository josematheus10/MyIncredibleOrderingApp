using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Order.Api.Models
{
    public record CreateOrderRequest
    {
        [Required(ErrorMessage = "O campo: clienteId é um Guid obrigatório")]
        [JsonPropertyName("clienteId")]
        public Guid CustomerId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O campo: valorTotal tem que ser maior que zero")]
        [JsonPropertyName("valorTotal")]
        public decimal TotalValue { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; } = "CRIANDO";

        [JsonPropertyName("dataCriacao")]
        public DateTime? CreationDate { get; set; } = DateTime.Now;
    }
}
