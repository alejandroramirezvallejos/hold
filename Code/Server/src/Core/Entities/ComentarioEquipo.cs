using System.ComponentModel.DataAnnotations.Schema;
using IMT_Reservas.Server.Core.Abstraction;

namespace IMT_Reservas.Server.Core.Entities;

public class ComentarioEquipo : Entity
{
    public int IdGrupoEquipo { get; set; }
    public string CarnetUsuario { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [ForeignKey("IdGrupoEquipo")]
    public GrupoEquipo? GrupoEquipo { get; set; }

    [ForeignKey("CarnetUsuario")]
    public Usuario? Usuario { get; set; }
}
