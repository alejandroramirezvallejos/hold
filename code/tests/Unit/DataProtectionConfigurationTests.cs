using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using IMT_Reservas.Server.Infrastructure.Config;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace IMT_Reservas.Tests.Unit;

[TestFixture]
internal sealed class DataProtectionConfigurationTests
{
    [Test]
    public void Configure_ProductionPersistentKeysWithoutCertificate_Throws()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["DataProtection:KeysPath"] = Path.GetTempPath(),
                }
            )
            .Build();
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(item => item.EnvironmentName).Returns(Environments.Production);

        var action = () =>
            DataProtectionConfiguration.Configure(services, configuration, environment.Object);

        action.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Configure_WithoutPersistentPath_UsesFrameworkDefaults()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(item => item.EnvironmentName).Returns(Environments.Production);

        var action = () =>
            DataProtectionConfiguration.Configure(services, configuration, environment.Object);

        action.Should().NotThrow();
    }

    [Test]
    public void Configure_WithCertificate_EncryptsPersistedKeyMaterial()
    {
        var directory = Directory.CreateTempSubdirectory("ucbhold-dp-");
        try
        {
            const string password = "test-certificate-password";
            var certificatePath = Path.Combine(directory.FullName, "protection.pfx");
            var passwordPath = Path.Combine(directory.FullName, "password.txt");
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest(
                "CN=UCB Hold Test",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1
            );
            using var certificate = request.CreateSelfSigned(
                DateTimeOffset.UtcNow.AddMinutes(-1),
                DateTimeOffset.UtcNow.AddDays(1)
            );
            File.WriteAllBytes(certificatePath, certificate.Export(X509ContentType.Pfx, password));
            File.WriteAllText(passwordPath, password);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["DataProtection:KeysPath"] = directory.FullName,
                        ["DataProtection:CertificatePath"] = certificatePath,
                        ["DataProtection:CertificatePasswordFile"] = passwordPath,
                    }
                )
                .Build();
            var environment = new Mock<IWebHostEnvironment>();
            environment.SetupGet(item => item.EnvironmentName).Returns(Environments.Production);
            var services = new ServiceCollection();

            DataProtectionConfiguration.Configure(services, configuration, environment.Object);
            using var provider = services.BuildServiceProvider();
            provider.GetRequiredService<IDataProtectionProvider>()
                .CreateProtector("test")
                .Protect("sensitive-value");

            var keyXml = File.ReadAllText(Directory.GetFiles(directory.FullName, "key-*.xml").Single());
            keyXml.Should().Contain("encryptedSecret");
            keyXml.Should().NotContain("sensitive-value");
        }
        finally
        {
            directory.Delete(true);
        }
    }
}
