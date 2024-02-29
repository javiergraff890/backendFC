﻿using APIFC3.Data;
using APIFC3.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
        public ActionResult Delete(int id, int idUsuario) {
            //revisar este warning, si bien yo envio siempre un id correcto alguna mala llamada puede romper la api
            var caja = _context.Cajas.Find(id);

            //si quiero eliminar una caja debo eliminar a todos los movimientos
            if (caja != null)
            {
                
                Movimiento m = new Movimiento();
                m.Concepto = "Eliminacion de caja";
                m.Valor = -caja.Saldo;
                m.IdCaja = caja.Id;
                //m.UserId = idUsuario;
                _context.Movimientos.Add(m);
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
        public ActionResult NuevaCaja(Caja caja)
        {
            Debug.WriteLine("llegue a entrar");
            int entidadesAfectadas = 0;
            try
            {
                _context.Cajas.Add(caja);
                try
                {
                    entidadesAfectadas += _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    Debug.WriteLine("capture la excepcion ->>>"+ex.Message);
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
                m.Valor = caja.Saldo;
                m.IdCaja = caja.Id;
                _context.Movimientos.Add(m);
                entidadesAfectadas += _context.SaveChanges();
                Debug.WriteLine(entidadesAfectadas);
                return Ok();
            } catch(Exception e)
            {
                string mensaje ="";
                if (entidadesAfectadas == 0)
                    mensaje = "No se inserto ningun elemento";
                else if (entidadesAfectadas == 1)
                    mensaje = "no se pudo insertar el movimiento inicial";
                
                return StatusCode(500, "Error interno del servidor : "+mensaje);
            }
            
            

        }
    }
}
