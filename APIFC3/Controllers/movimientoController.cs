using APIFC3.Data;
using APIFC3.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

                 Thread.Sleep(5000);







            getMovResult res = new getMovResult();
            res.cantidadMovs = movimientosPorUsuario.Count();
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

        private (bool , string) validacionMovimiento(Movimiento movimiento)
        {
            Debug.WriteLine("");
            Debug.WriteLine("numerocomodecimal = " + movimiento.Valor);
            Debug.WriteLine("");
            string numeroComoCadena = movimiento.Valor.ToString();
            Debug.WriteLine("");
            Debug.WriteLine("numerocomocadena = " + numeroComoCadena);
            Debug.WriteLine("");
            string[] partes = numeroComoCadena.Split(',');
            if (partes.Length == 1) {
                //tiene solo parte entera
                if ((partes[0][0] != '-' && partes[0].Length > 8) || (partes[0][0] == '-' && partes[0].Length > 9))
                {
                    return (false, "overflow_valor_parte_entera");
                } 
            }
            else if (partes.Length == 2)
            {
                if ((partes[0][0] != '-' && partes[0].Length > 8) || (partes[0][0] == '-' && partes[0].Length > 9))
                {
                    return (false, "overflow_valor_parte_entera");
                }
                if (partes[1].Length > 2)
                {
                    return (false, "overflow_valor_parte_decimal");
                }
            } else
            {
                return (false, "valor_invalido");
            }

            //llegue aca con valor valido
            //si bien en la base de datos pueso insertar 200 decidi no cambiarlo aca ya que es un proyecto personal
            //si fuera una aplicacion real debo remapear el entity cambiando el tamaño en la base de datos
            if (movimiento.Concepto.Length > 50)
            {
                return (false, "concepto_overflow");
            }

            return (true, "");
        }

        [HttpPost]
        [Authorize]
        public IActionResult insertar(Movimiento movimiento)
        {
            (bool movimientoValido, string mensajeError) = validacionMovimiento(movimiento);
            if (movimientoValido)
            {
                var caja = _context.Cajas.FirstOrDefault(c => c.Id == movimiento.IdCaja);
                if (caja != null)
                {
                    decimal nuevoSaldo = caja.Saldo + movimiento.Valor;

                    if (nuevoSaldo > 99999999.99m)
                    {
                        return UnprocessableEntity("saldo_maximo_excedido");
                        //error
                    } else if (nuevoSaldo < -99999999.99m)
                    {
                        return UnprocessableEntity("saldo_minimo_excedido");
                    }
                    {
                        caja.Saldo = nuevoSaldo;
                        _context.Movimientos.Add(movimiento);
                        _context.SaveChanges();
                        return Ok();
                    }

                    
                }
                else
                {
                    //por ahora asumo que siempre llega una llave foranea valida, para algun caso extremo deberia tratarlo
                    return BadRequest();
                }
            } else
            {
                return UnprocessableEntity(mensajeError);
            }
            
            
        }

        [HttpDelete("{id}")]
        [Authorize]
        public ActionResult delete(int id)
        {
            var movToRemove = _context.Movimientos.FirstOrDefault(m => m.Id == id);
            if (movToRemove != null)
            {
                var caja = _context.Cajas.FirstOrDefault(c => c.Id == movToRemove.IdCaja);


                if (caja != null)
                {

                    decimal saldoResultante = caja.Saldo - movToRemove.Valor;

                    if (saldoResultante > 99999999.99m || saldoResultante < -99999999.99m)
                    {
                        return UnprocessableEntity("saldo_caja_inconsistente");
                    }
                    else
                    {
                        if (movToRemove.Concepto == "Saldo inicial")
                        {
                            caja.Saldo -= movToRemove.Valor;
                            movToRemove.Valor = 0;
                            _context.SaveChanges();
                            return Ok();
                        }
                        else
                        {
                            caja.Saldo = caja.Saldo - movToRemove.Valor;
                            _context.Movimientos.Remove(movToRemove);
                            _context.SaveChanges();
                            return Ok();
                        }
                    }
                }
                else
                {
                    return Conflict("Movimiento sin caja asociada");
                }
            } else
            {
                return Conflict();
            }
        }

    }
}
