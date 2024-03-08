using APIFC3.Data;
using APIFC3.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;

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

        [HttpGet("{first}/{range}")]
        [Authorize]
        public ActionResult<getMovResult> getRange(int first, int range)
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Split(' ')[1];
            Debug.WriteLine("recibi el token = " + token);

            // Decodifica el token JWT
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            Debug.WriteLine("user id = " + userIdClaim);

            var movimientosPorUsuario = from movimiento in _context.Movimientos
                                        join caja in _context.Cajas on movimiento.IdCaja equals caja.Id
                                        where caja.UserId == int.Parse(userIdClaim)
                                        select movimiento;

            var elements = movimientosPorUsuario.OrderBy( u  => u.Fecha)
                                        .Skip(first-1)
                                        .Take(range)
                                        .ToList();

            getMovResult res = new getMovResult();
            if (elements.Any())
            {
                res.movs = elements;
                if (elements.OrderBy( e => e.Fecha).Last() == movimientosPorUsuario.OrderBy(e => e.Fecha).Last())
                {
                    res.siguiente = false;
                } else
                {
                    res.siguiente = true;   
                   
                }
                return res;
            } else
            {
                res.movs = Enumerable.Empty<Movimiento>();
                res.siguiente = false;
                return res;
            }

        }
        
        [HttpGet]
        [Authorize]
        public IEnumerable<Movimiento> Get()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Split(' ')[1];
            Debug.WriteLine("recibi el token = " + token);

            // Decodifica el token JWT
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            Debug.WriteLine("user id = " + userIdClaim);


            var movimientosPorUsuario = from movimiento in _context.Movimientos
                                        join caja in _context.Cajas on movimiento.IdCaja equals caja.Id
                                        where caja.UserId == int.Parse(userIdClaim)
                                        select movimiento;
            //Debug.WriteLine("empezo el delay");
            //Thread.Sleep(10000);
            //Debug.WriteLine("fin del delay");
            return movimientosPorUsuario;

        }

        [HttpPost]
        [Authorize]
        public IActionResult insertar(Movimiento movimiento)
        {
            var caja = _context.Cajas.FirstOrDefault( c => c.Id == movimiento.IdCaja);
            if (caja != null)
            {
                caja.Saldo = caja.Saldo + movimiento.Valor;
                _context.Movimientos.Add(movimiento);
                _context.SaveChanges();
                return Ok();
            } else
            {
                return BadRequest();
            }
            
        }

        [HttpDelete("{id}")]
        [Authorize]
        public ActionResult delete(int id)
        {
            var movToRemove = _context.Movimientos.FirstOrDefault( m => m.Id == id);
            if (movToRemove != null)
            {
                if (movToRemove.Concepto == "Saldo inicial")
                {
                    

                    var caja = _context.Cajas.FirstOrDefault(c => c.Id == movToRemove.IdCaja);
                    if (caja != null)
                    {
                        caja.Saldo -= movToRemove.Valor;
                        movToRemove.Valor = 0;
                        _context.SaveChanges();
                        return Ok();
                    }
                    return Conflict();


                } else
                {
                    var caja = _context.Cajas.FirstOrDefault(c => c.Id == movToRemove.IdCaja);
                    if (caja != null)
                    {
                        caja.Saldo = caja.Saldo - movToRemove.Valor;
                        _context.Movimientos.Remove(movToRemove);
                        _context.SaveChanges();
                        return Ok();
                    }
                    return Conflict();
                }
                
                
            } else
                return Conflict();
        }

    }
}
