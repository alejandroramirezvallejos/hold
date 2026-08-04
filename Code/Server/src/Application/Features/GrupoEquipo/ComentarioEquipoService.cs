using Ardalis.Result;
using IMT_Reservas.Server.Infrastructure.Repositories.Implementations;

namespace IMT_Reservas.Server.Application.Features.GrupoEquipo;

public class ComentarioEquipoService
{
    private const int MaxContenidoLength = 1024;
    private readonly ComentarioEquipoRepository _repository;

    public ComentarioEquipoService(ComentarioEquipoRepository repository) => _repository = repository;

    public async Task<Result<List<ComentarioEquipoDto>>> GetByGrupo(int grupoId)
    {
        if (!await _repository.GrupoExiste(grupoId))
            return Result<List<ComentarioEquipoDto>>.NotFound();

        return Result<List<ComentarioEquipoDto>>.Success(await _repository.GetByGrupo(grupoId));
    }

    public async Task<Result<ComentarioEquipoDto>> Create(
        int grupoId,
        string carnet,
        CrearComentarioEquipoDto? dto
    )
    {
        var contenido = dto?.Contenido?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(carnet))
            return Result<ComentarioEquipoDto>.Error("Usuario requerido");
        if (string.IsNullOrWhiteSpace(contenido))
            return Result<ComentarioEquipoDto>.Error("Comentario requerido");
        if (contenido.Length > MaxContenidoLength)
            return Result<ComentarioEquipoDto>.Error("Comentario máximo 1024 caracteres");
        if (!await _repository.GrupoExiste(grupoId))
            return Result<ComentarioEquipoDto>.NotFound();

        return Result<ComentarioEquipoDto>.Created(
            await _repository.Add(grupoId, carnet, contenido)
        );
    }
}
