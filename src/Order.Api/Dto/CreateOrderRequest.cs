using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Order.Api.Dto
{
    public record CreateOrderRequest
    {
        [Required(ErrorMessage = "Id do cliente é obrigatório")]
        [JsonPropertyName("clienteId")]
        public Guid CustomerId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor total tem que ser maior que zero")]
        [JsonPropertyName("valorTotal")]
        public decimal TotalValue { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; } = "CRIANDO";

        [JsonPropertyName("dataCriacao")]
        public DateTime? CreationDate { get; set; } = DateTime.Now;
    }
}
