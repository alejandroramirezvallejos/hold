namespace IMT_Reservas.Server.Application.Features.GrupoEquipo;

public class ComentarioEquipoDto
{
    public int Id { get; set; }
    public int IdGrupoEquipo { get; set; }
    public string CarnetUsuario { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}
