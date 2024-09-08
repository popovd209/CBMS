namespace Admin_CBMS.Views.Waiters.DTOs
{
    public class WaiterDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DisplayName => $"WAITER - {Name}";
    }
}
