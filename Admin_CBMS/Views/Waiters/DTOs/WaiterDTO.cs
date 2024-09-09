namespace Admin_CBMS.Views.Waiters.DTOs
{
    public class WaiterDTO
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PersonalPin { get; set; }
        public DateTime? ContractDate { get; set; }
        public string DisplayName => $"WAITER - {Name}";
    }
}
