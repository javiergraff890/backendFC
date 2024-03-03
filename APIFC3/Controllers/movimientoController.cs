using APIFC3.Data;
using APIFC3.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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


            return movimientosPorUsuario;
        }

        [HttpPost]
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

    }
}
