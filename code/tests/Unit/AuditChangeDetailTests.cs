using System.Text.Json;
using FluentAssertions;
using IMT_Reservas.Server.Application.Features.AuditLog;
using IMT_Reservas.Server.Application.Features.Usuario;
using IMT_Reservas.Server.Application.Features.Equipo;

namespace IMT_Reservas.Tests.Unit;

[TestFixture]
internal class AuditChangeDetailTests
{
    [Test]
    public void Build_RecordsVisibleValuesAndProtectsPersonalData()
    {
        var previous = new UsuarioDto
        {
            Nombre = "Ana",
            Email = "anterior@ucb.edu.bo",
            ImagenFirma = [1, 2],
            Rol = "estudiante",
        };
        var current = new UsuarioDto
        {
            Nombre = "Andrea",
            Email = "nuevo@ucb.edu.bo",
            ImagenFirma = [3, 4],
            Rol = "docente",
        };

        var detail = AuditChangeDetail.Build(previous, current);
        using var json = JsonDocument.Parse(detail!);
        var changes = json.RootElement.GetProperty("cambios").EnumerateArray().ToList();

        changes.Should().Contain(change =>
            change.GetProperty("campo").GetString() == "Nombre"
            && change.GetProperty("anterior").GetString() == "Ana"
            && change.GetProperty("nuevo").GetString() == "Andrea"
            && !change.GetProperty("protegido").GetBoolean()
        );
        changes.Should().Contain(change =>
            change.GetProperty("campo").GetString() == "Email"
            && change.GetProperty("protegido").GetBoolean()
            && change.GetProperty("anterior").ValueKind == JsonValueKind.Null
            && change.GetProperty("nuevo").ValueKind == JsonValueKind.Null
        );
        changes.Should().Contain(change =>
            change.GetProperty("campo").GetString() == "Firma"
            && change.GetProperty("protegido").GetBoolean()
        );
        detail.Should().NotContain("anterior@ucb.edu.bo");
        detail.Should().NotContain("nuevo@ucb.edu.bo");
    }

    [Test]
    public void BuildCreated_RecordsVisibleSnapshotWithoutSensitiveValues()
    {
        var detail = AuditChangeDetail.BuildCreated(new UsuarioDto
        {
            Nombre = "Ana",
            Email = "ana@ucb.edu.bo",
            ImagenFirma = [1, 2],
            Rol = "docente",
        });

        using var json = JsonDocument.Parse(detail!);
        var changes = json.RootElement.GetProperty("cambios").EnumerateArray().ToList();

        changes.Should().Contain(change =>
            change.GetProperty("campo").GetString() == "Nombre"
            && change.GetProperty("nuevo").GetString() == "Ana"
        );
        changes.Should().Contain(change =>
            change.GetProperty("campo").GetString() == "Email"
            && change.GetProperty("protegido").GetBoolean()
        );
        detail.Should().NotContain("ana@ucb.edu.bo");
    }

    [Test]
    public void BuildDeleted_RecordsOnlyPreviousVisibleValues()
    {
        var detail = AuditChangeDetail.BuildDeleted(new UsuarioDto
        {
            Nombre = "Ana",
            Rol = "docente",
        });

        using var json = JsonDocument.Parse(detail!);
        var name = json.RootElement.GetProperty("cambios").EnumerateArray()
            .Single(change => change.GetProperty("campo").GetString() == "Nombre");

        name.GetProperty("anterior").GetString().Should().Be("Ana");
        name.GetProperty("nuevo").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Test]
    public void Build_UsesRelatedNamesInsteadOfInternalIdentifiers()
    {
        var detail = AuditChangeDetail.Build(
            new EquipoDto { IdAmbiente = 1, Ubicacion = "Laboratorio A" },
            new EquipoDto { IdAmbiente = 2, Ubicacion = "Laboratorio B" }
        );

        using var json = JsonDocument.Parse(detail!);
        var changes = json.RootElement.GetProperty("cambios").EnumerateArray().ToList();

        changes.Should().ContainSingle(change =>
            change.GetProperty("campo").GetString() == "Ubicacion"
            && change.GetProperty("anterior").GetString() == "Laboratorio A"
            && change.GetProperty("nuevo").GetString() == "Laboratorio B"
        );
        changes.Should().NotContain(change => change.GetProperty("campo").GetString() == "Ambiente");
    }
}
