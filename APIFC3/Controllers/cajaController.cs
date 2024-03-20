using APIFC3.Data;
using APIFC3.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

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
        [Authorize]
        public IEnumerable<Caja> Get()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Split(' ')[1];
            Debug.WriteLine("recibi el token = " + token);

            // Decodifica el token JWT
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            Debug.WriteLine("user id = " + userIdClaim);

            Thread.Sleep(5000);

            if (userIdClaim != null)
            {
                return _context.Cajas.Where(c => c.UserId == int.Parse(userIdClaim)).ToList();
            } else
            { 
                return Enumerable.Empty<Caja>(); 
            }
            
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
        [Authorize]
        public ActionResult Delete(int id) {
            //revisar este warning, si bien yo envio siempre un id correcto alguna mala llamada puede romper la api
            var caja = _context.Cajas.Find(id);

            //si quiero eliminar una caja debo eliminar a todos los movimientos
            if (caja != null)
            {
                IQueryable<Movimiento> movs = _context.Movimientos.Where(m => m.IdCaja == caja.Id);
                _context.Movimientos.RemoveRange(movs);
                _context.SaveChanges();
                _context.Cajas.Remove(caja);
                _context.SaveChanges();
                return Ok();
            } else
            {
                return NoContent();
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult NuevaCaja(CreacionCaja caja)
        {
            Debug.WriteLine("llegue a entrar a nueva caja");
            if (caja.caja.Saldo < 0)
            {
                return UnprocessableEntity("saldo_negativo");
            } else
            {
                int entidadesAfectadas = 0;
                try
                {
                    _context.Cajas.Add(caja.caja);
                    try
                    {
                        entidadesAfectadas += _context.SaveChanges();
                    }
                    catch (DbUpdateException ex)
                    {
                        Debug.WriteLine("capture la excepcion ->>>" + ex.Message);
                        var innerException = ex.InnerException;
                        if (innerException is SqlException sqlException)
                        {
                            // Aquí puedes realizar un manejo específico para los errores de SQL Server
                            if (sqlException.Number == 2627)
                            {
                                // Se violó una restricción de clave única
                                // Realizar el manejo correspondiente
                                Debug.WriteLine("Se violo una restriccion de clave unica");
                                return NoContent();
                            }
                        }
                    }

                    Movimiento m = new Movimiento();
                    m.Concepto = "Saldo inicial";
                    m.Valor = caja.caja.Saldo;
                    m.IdCaja = caja.caja.Id;
                    m.Fecha = caja.Fecha;
                    _context.Movimientos.Add(m);
                    entidadesAfectadas += _context.SaveChanges();
                    Debug.WriteLine(entidadesAfectadas);
                    return Ok();
                }
                catch (Exception e)
                {
                    string mensaje = "";
                    if (entidadesAfectadas == 0)
                        mensaje = "No se inserto ningun elemento";
                    else if (entidadesAfectadas == 1)
                        mensaje = "no se pudo insertar el movimiento inicial";

                    return StatusCode(500, "Error interno del servidor : " + mensaje);
                }

            }
            

        }

    }
}
