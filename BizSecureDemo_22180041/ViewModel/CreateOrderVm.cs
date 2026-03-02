using System.ComponentModel.DataAnnotations;

namespace BizSecureDemo_22180041.ViewModel
{
    public class CreateOrderVm
    {
        [Required, MaxLength(80)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }
    }
}
