namespace E_Commerce_BackendAPI.Model
{
    public class BaseEntity
    {
        public int Id { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }

    }
}
