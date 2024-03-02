using APIFC3.Data;
using Microsoft.AspNetCore.Mvc;

namespace APIFC3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class movimientoController : ControllerBase
    {
        private readonly FlujoCajaContext _context;

        public movimientoController(FlujoCajaContext context)
        {
            _context = context;
        }

    }
}
