using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;

namespace IMT_Reservas.Server.Infrastructure.Config;

public static class DataProtectionConfiguration
{
    public static void Configure(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment
    )
    {
        var builder = services.AddDataProtection().SetApplicationName("UCBHold");
        var keysPath = configuration["DataProtection:KeysPath"];
        if (string.IsNullOrWhiteSpace(keysPath))
            return;

        builder.PersistKeysToFileSystem(new DirectoryInfo(keysPath));

        var certificatePath = configuration["DataProtection:CertificatePath"];
        var passwordFile = configuration["DataProtection:CertificatePasswordFile"];
        if (string.IsNullOrWhiteSpace(certificatePath) || string.IsNullOrWhiteSpace(passwordFile))
        {
            if (environment.IsProduction())
                throw new InvalidOperationException(
                    "A Data Protection certificate and password file are required in production."
                );
            return;
        }

        if (!File.Exists(certificatePath) || !File.Exists(passwordFile))
            throw new InvalidOperationException("Data Protection secret files were not found.");

        var password = File.ReadAllText(passwordFile).Trim();
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("The Data Protection certificate password is empty.");

        var certificate = new X509Certificate2(
            certificatePath,
            password,
            X509KeyStorageFlags.EphemeralKeySet
        );
        if (!certificate.HasPrivateKey)
            throw new InvalidOperationException("The Data Protection certificate has no private key.");

        builder.ProtectKeysWithCertificate(certificate);
    }
}
