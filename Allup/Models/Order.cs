using System.ComponentModel.DataAnnotations;

namespace Allup.Models
{
    public class Order : BaseEntity
    {

        [DataType(DataType.PhoneNumber)]
        public string Number { get; set; }
        public string Address { get; set; }

        public decimal Subtotal { get; set; }

        public List<OrderItem> OrderItems { get; set; }
        public bool? Status { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

    }
    }
