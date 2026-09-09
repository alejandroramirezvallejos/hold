using FluentAssertions;
using IMT_Reservas.Server.Application.Features.AuditLog;
using IMT_Reservas.Server.Core.Entities;
using IMT_Reservas.Server.Infrastructure.Config;
using IMT_Reservas.Server.Infrastructure.Repositories.Implementations;
using IMT_Reservas.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace IMT_Reservas.Tests.Integration;

[TestFixture]
internal class AuditLogRepositoryTests : ServiceTest<AuditLogRepository>
{
    protected override AuditLogRepository CreateService(ApplicationDbContext db) => new(db);

    [Test]
    public async Task GetFiltered_FiltersByActorActionAndDateBeforeLimitingResults()
    {
        var now = DateTime.UtcNow;
        Db.AuditLogs.AddRange(
            BuildLog("Rechazar", "Fernando Terrazas", "12890061", now),
            BuildLog("Aprobar", "Fernando Terrazas", "12890061", now),
            BuildLog("Rechazar", "Otra Persona", "5555555", now)
        );
        await Db.SaveChangesAsync();

        var result = await Sut.GetFiltered(
            "Prestamo",
            "fernando",
            "Rechazar",
            now.AddMinutes(-1),
            now.AddMinutes(1)
        );

        result.Should().ContainSingle();
        result.Single().AdminCarnet.Should().Be("12890061");
        result.Single().Accion.Should().Be("Rechazar");
    }

    [Test]
    public async Task AuditService_StoresTheCurrentActorsFullName()
    {
        Db.Usuarios.Add(new Usuario
        {
            Carnet = "12890061",
            Nombre = "Fernando",
            ApellidoPaterno = "Terrazas",
            ApellidoMaterno = "Llanos",
        });
        await Db.SaveChangesAsync();
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity([new Claim("sub", "12890061")], "test")
            ),
        };
        var service = new AuditLogService(
            Sut,
            new HttpContextAccessor { HttpContext = context },
            new UsuarioReadRepository(Db)
        );

        await service.Log(AuditAccion.Editar, "Equipo", "15");

        Db.AuditLogs.Single().AdminNombre.Should().Be("Fernando Terrazas Llanos");
    }

    private static AuditLog BuildLog(
        string action,
        string actorName,
        string actorCarnet,
        DateTime timestamp
    ) =>
        new()
        {
            Accion = action,
            Entidad = "Prestamo",
            EntidadId = "1",
            AdminNombre = actorName,
            AdminCarnet = actorCarnet,
            Timestamp = timestamp,
            EstadoEliminado = false,
        };
}
