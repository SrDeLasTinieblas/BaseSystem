using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.Domain.Entities
{
    public class LogAuditoria
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; }
        public DateTime Fecha { get; set; }
        public string Tabla { get; set; }
        public string Accion { get; set; }
        public string ValorAnterior { get; set; }
        public string ValorNuevo { get; set; }
        public string DireccionIP { get; set; }
    }
}
