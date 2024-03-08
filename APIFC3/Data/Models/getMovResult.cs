namespace APIFC3.Data.Models
{
    public partial class getMovResult
    {
        public IEnumerable<Movimiento> movs {  get; set; }
        public bool siguiente { get; set; }
    }
}
