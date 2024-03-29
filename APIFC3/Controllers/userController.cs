using APIFC3.Data;
using APIFC3.Data.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace APIFC3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class userController : ControllerBase
    {
        private readonly FlujoCajaContext _context;
        
        public userController(FlujoCajaContext context) {
            _context = context;
        }


        [HttpPost]
        public IActionResult Create(Usuario usuario)
        {
            if (!(_context.Usuarios.FirstOrDefault(u => u.UserName == usuario.UserName) == null))
            {
                var errorResponse = new
                {
                    error = new
                    {
                          message = "El usuario ya existe",
                        code = "user_already_exist"
                    }
                };
                //el usuario ya existe, manejar esto, ver si conflict es la mejor opcion
                return Conflict(errorResponse);
            }
            else
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(usuario.Password, BCrypt.Net.BCrypt.GenerateSalt());
                usuario.Password = hashedPassword;
                _context.Usuarios.Add(usuario);

                try
                {
                    _context.SaveChanges();
                } catch (Exception ex)
                {
                    Console.WriteLine($"Se produjo una excepción: {ex.Message}");
                    return BadRequest(ex);
                }
                

                var tokenHandler = new JwtSecurityTokenHandler();
                var ByteKey = Encoding.UTF8.GetBytes(Constantes.key);
                var TokenDes = new SecurityTokenDescriptor
                {
                    Subject = new System.Security.Claims.ClaimsIdentity(new Claim[]
                    {
                            new Claim("userId", usuario.UserId.ToString()),
                            new Claim(ClaimTypes.Name, usuario.UserName)
                    }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(ByteKey), SecurityAlgorithms.HmacSha256Signature)
                };

                var tkn = tokenHandler.CreateToken(TokenDes);

                return Ok(tokenHandler.WriteToken(tkn));
            }
        }

        [HttpDelete]
        [Authorize]
        public IActionResult Delete()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Split(' ')[1];

            // Decodifica el token JWT
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            if (userId != null)
            {
                try
                {
                    int userIdInt = Int32.Parse(userId);
                    
                    //meter try catch aca
                    try
                    {
                        var cajasDelUsuario = from caja in _context.Cajas
                                              where caja.UserId == userIdInt
                                              select caja;

                        var movimientosDelUsuario = from caja in cajasDelUsuario
                                                    join movimiento in _context.Movimientos
                                                    on caja.Id equals movimiento.IdCaja
                                                    select movimiento;

                        _context.Movimientos.RemoveRange(movimientosDelUsuario);
                        _context.Cajas.RemoveRange(cajasDelUsuario);
                        _context.Usuarios.RemoveRange(_context.Usuarios.Where(u => u.UserId == userIdInt));
                        _context.SaveChanges();
                        return Ok();

                    } catch(Exception ex)
                    {
                        Console.WriteLine($"Se produjo una excepción: {ex.Message} - No se pudo eliminar el usuario");
                        return BadRequest(ex);
                    }
                }
                catch {
                    //se convirtio mal el string que venia como id en el token
                    return UnprocessableEntity();
                }
                
            } else
            {
                return UnprocessableEntity();
                //error en el token
            }

        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Login(Usuario usuario)
        {

            string user = usuario.UserName;
            string password = usuario.Password;

            //intento buscar una entidad con ese username
            var userinDB = _context.Usuarios.FirstOrDefault(u => u.UserName == user);

            if (userinDB != null) {
                
                //encripto la password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
                string passDb = userinDB.Password;

                /**
                 * Se ingresa primero la password recibida sin encriptar y luego el hash guardado en la base de datos
                 */
                bool contrasenaValida = BCrypt.Net.BCrypt.Verify(password, passDb);


                if (contrasenaValida)
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var ByteKey = Encoding.UTF8.GetBytes(Constantes.key);
                    var TokenDes = new SecurityTokenDescriptor
                    {
                        Subject = new System.Security.Claims.ClaimsIdentity(new Claim[]
                        {
                            new Claim("userId", userinDB.UserId.ToString()),
                            new Claim(ClaimTypes.Name, userinDB.UserName)
                        }),
                        Expires = DateTime.UtcNow.AddDays(1),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(ByteKey), SecurityAlgorithms.HmacSha256Signature)
                    };

                    var tkn = tokenHandler.CreateToken(TokenDes);

                    return Ok(tokenHandler.WriteToken(tkn));

                }else
                {
                    Debug.WriteLine("datos incorrectos");
                    return Unauthorized("password");
                    
                }

            } else
            {
                //unauthorized por nombre de usuario incorrecto
                return Unauthorized("user");
            }
        }
    }
    
}
