using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.IO;
using System.Threading;

public class GoogleGetCredentials
{
    private static string[] Scopes =
    {
        DriveService.Scope.DriveFile,
        DriveService.Scope.Drive
    };

    private static string ApplicationName = "Drive API .NET Console App";

    public void GetCredentials()
    {
        try
        {
            string credentialsPath = "credentials.json";  // Asegúrate de que este archivo esté al mismo nivel que Program.cs
            string configFolderPath = "Config";  // Carpeta donde se guardará el archivo de configuración
            string configFilePath = Path.Combine(configFolderPath, "GoogleDriveConfig.txt");
            string tokenFolderPath = "GoogleTokens";  // Carpeta donde se guardarán los tokens

            // Verificar si las carpetas de configuración y tokens existen, si no, crearlas
            if (!Directory.Exists(configFolderPath))
            {
                Directory.CreateDirectory(configFolderPath);
            }

            if (!Directory.Exists(tokenFolderPath))
            {
                Directory.CreateDirectory(tokenFolderPath);
            }

            // Cargar los datos de credentials.json
            JObject credentialsJson = JObject.Parse(File.ReadAllText(credentialsPath));
            var installedSection = credentialsJson["installed"];

            ClientSecrets clientSecrets = installedSection.ToObject<ClientSecrets>();

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = clientSecrets,
                Scopes = Scopes,
                DataStore = new FileDataStore(tokenFolderPath, true)
            });

            // Realizar la autenticación y obtener los tokens
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets,
                Scopes,
                "user",
                System.Threading.CancellationToken.None,
                new FileDataStore(tokenFolderPath, true)).Result;

            // Generar archivo de configuración en texto plano
            GenerateConfigFile(clientSecrets, credential.Token, installedSection, configFilePath);

            // Verificar si el archivo de configuración fue generado exitosamente
            if (File.Exists(configFilePath))
            {
                // Eliminar los archivos de tokens en la carpeta GoogleTokens
                DeleteTokenFiles(tokenFolderPath);
            }

            Console.WriteLine("El proceso ha finalizado. Los tokens y archivos de configuración se han guardado correctamente.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error durante la autenticación: {ex.Message}");
        }
    }

    private void GenerateConfigFile(ClientSecrets clientSecrets, TokenResponse tokenResponse, JToken installedSection, string configFilePath)
    {
        try
        {
            var configData = $@"
            <add key=""ClientId"" value=""{clientSecrets.ClientId}"" />
            <add key=""ClientSecret"" value=""{clientSecrets.ClientSecret}"" />
            <add key=""AccessToken"" value=""{tokenResponse.AccessToken}"" />
            <add key=""RefreshToken"" value=""{tokenResponse.RefreshToken}"" />
            <add key=""ExpiresInSeconds"" value=""666666666"" />
            <add key=""Scope"" value=""{tokenResponse.Scope}"" />
            <add key=""TokenIssuedUtc"" value=""{tokenResponse.IssuedUtc}"" />
            <add key=""TokenType"" value=""{tokenResponse.TokenType}"" />
            <add key=""TokenUri"" value=""{installedSection["token_uri"]}"" />
            <add key=""AuthUri"" value=""{installedSection["auth_uri"]}"" />
            <add key=""AuthProviderX509CertUrl"" value=""{installedSection["auth_provider_x509_cert_url"]}"" />
            <add key=""RedirectUris"" value=""{installedSection["redirect_uris"].First}"" />
            <add key=""IdToken"" value=""{tokenResponse.IdToken}"" />
            <add key=""ProjectId"" value=""{installedSection["project_id"]}"" />
            ";

            // Escribir el archivo de configuración
            File.WriteAllText(configFilePath, configData);

            Console.WriteLine($"Archivo de configuración generado en: {configFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al generar el archivo de configuración: {ex.Message}");
        }
    }

    public UserCredential Get()
    {
        try
        {
            string[] Scopes = { DriveService.Scope.DriveFile, DriveService.Scope.Drive };
            string credPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempToken");

            if (!Directory.Exists(credPath))
            {
                Directory.CreateDirectory(credPath);
            }

            // Autenticar al usuario
            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = ConfigurationManager.AppSettings["ClientId"],
                    ClientSecret = ConfigurationManager.AppSettings["ClientSecret"]
                },
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)
            ).Result;

            return credential;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error durante la autenticación: {ex.Message}");
            return null; // Devolver null si hay un error
        }
    }

    private void DeleteTokenFiles(string tokenFolderPath)
    {
        try
        {
            // Obtener todos los archivos en la carpeta de tokens
            var tokenFiles = Directory.GetFiles(tokenFolderPath);

            // Eliminar cada archivo de token
            foreach (var file in tokenFiles)
            {
                File.Delete(file);
            }

            Console.WriteLine("Los archivos de token han sido eliminados, pero la carpeta se ha conservado.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar los archivos de token: {ex.Message}");
        }
    }
}
