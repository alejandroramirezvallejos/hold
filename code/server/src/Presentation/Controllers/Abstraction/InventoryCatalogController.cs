using IMT_Reservas.Server.Application.Abstraction;
using IMT_Reservas.Server.Application.Features.Inventario;
using IMT_Reservas.Server.Core.Abstraction;
using IMT_Reservas.Server.Infrastructure.Repositories.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMT_Reservas.Server.Presentation.Controllers.Abstraction;

public abstract class InventoryCatalogController<TEntity>(
    Service<TEntity, Repository<TEntity, CatalogoInventarioDto>, CatalogoInventarioDto> service
) : Controller
    where TEntity : Entity
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => ToResponse(await service.GetAll());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id) => ToResponse(await service.Get(id));

    [Authorize(Roles = "administrador")]
    [HttpPost]
    public async Task<IActionResult> Create(CatalogoInventarioDto dto)
    {
        var result = await service.Create(dto);
        return ToCreatedResponse(result, nameof(Get), new { id = result.Value?.Id });
    }

    [Authorize(Roles = "administrador")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CatalogoInventarioDto dto) =>
        ToResponse(await service.Update(id, dto));

    [Authorize(Roles = "administrador")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) =>
        ToDeleteResponse(await service.Delete(id));
}
