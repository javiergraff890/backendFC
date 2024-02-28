using APIFC3.Data;
using APIFC3.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIFC3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class cajaController : ControllerBase
    {
        private readonly FlujoCajaContext _context;

        public cajaController(FlujoCajaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IEnumerable<Caja> Get()
        {
            return _context.Cajas.ToList();
        }

        [HttpGet("{id}")]
        [Authorize]
        public ActionResult<Caja> GetById(int id)
        {
            var caja = _context.Cajas.Find(id);
            
            if (caja is null)
                return NotFound();
            return Ok(caja);
        }

        [HttpDelete("{id}")] 
        public ActionResult Delete(int id) {
            //revisar este warning, si bien yo envio siempre un id correcto alguna mala llamada puede romper la api
            var caja = _context.Cajas.Find(id);

            if (caja != null)
            {
                _context.Cajas.Remove(caja);
                _context.SaveChanges();
                return Ok();
            } else
            {
                return NoContent();
            }
                 
        }
    }
}
