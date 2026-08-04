using IMT_Reservas.Server.Application.Features.GrupoEquipo;
using IMT_Reservas.Server.Core.Entities;
using IMT_Reservas.Server.Infrastructure.Config;
using Microsoft.EntityFrameworkCore;

namespace IMT_Reservas.Server.Infrastructure.Repositories.Implementations;

public class ComentarioEquipoRepository
{
    private readonly ApplicationDbContext _db;

    public ComentarioEquipoRepository(ApplicationDbContext db) => _db = db;

    public async Task<bool> GrupoExiste(int grupoId) =>
        await _db.GruposEquipos.AsNoTracking().AnyAsync(grupo => grupo.Id == grupoId);

    public async Task<List<ComentarioEquipoDto>> GetByGrupo(int grupoId) =>
        await QueryComentarios()
            .Where(comentario => comentario.IdGrupoEquipo == grupoId)
            .OrderByDescending(comentario => comentario.FechaCreacion)
            .ToListAsync();

    public async Task<ComentarioEquipoDto> Add(int grupoId, string carnet, string contenido)
    {
        var comentario = new ComentarioEquipo
        {
            IdGrupoEquipo = grupoId,
            CarnetUsuario = carnet,
            Contenido = contenido,
            FechaCreacion = DateTime.UtcNow,
        };

        _db.ComentariosEquipos.Add(comentario);
        await _db.SaveChangesAsync();

        return await QueryComentarios().FirstAsync(item => item.Id == comentario.Id);
    }

    private IQueryable<ComentarioEquipoDto> QueryComentarios() =>
        from comentario in _db.ComentariosEquipos.AsNoTracking()
        join usuario in _db.Usuarios.AsNoTracking()
            on comentario.CarnetUsuario equals usuario.Carnet
        where !comentario.EstadoEliminado
        select new ComentarioEquipoDto
        {
            Id = comentario.Id,
            IdGrupoEquipo = comentario.IdGrupoEquipo,
            CarnetUsuario = comentario.CarnetUsuario,
            NombreUsuario = (usuario.Nombre + " " + usuario.ApellidoPaterno).Trim(),
            Contenido = comentario.Contenido,
            FechaCreacion = comentario.FechaCreacion,
        };
}
