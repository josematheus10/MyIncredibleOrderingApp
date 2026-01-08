namespace Order.Api.Entities
{
    public class OrderEntity
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public decimal TotalValue { get; set; }
        public string Status { get; set; } = "CRIANDO";
        public DateTime CreationDate { get; set; }
    }
}
