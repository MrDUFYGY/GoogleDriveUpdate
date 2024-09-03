using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Threading;

namespace GoogleDriveUpload
{
    public class GoogleAuthenticator
    {
        private static readonly string[] Scopes =
        {
            DriveService.Scope.DriveFile,
            Oauth2Service.Scope.UserinfoProfile,
            Oauth2Service.Scope.UserinfoEmail
        };

        private static readonly string ApplicationName = "Drive API .NET Console App";

        public UserCredential Authenticate()
        {
            try
            {
                string credPath = Path.Combine(Path.GetTempPath(), "tokenFolder");

                // Crear o actualizar el archivo de token con la información de app.config
                var tokenResponse = UpdateTokenResponse(credPath);

                // Crear un GoogleAuthorizationCodeFlow para manejar la autenticación
                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = ConfigurationManager.AppSettings["ClientId"],
                        ClientSecret = ConfigurationManager.AppSettings["ClientSecret"]
                    },
                    Scopes = Scopes,
                    DataStore = new FileDataStore(credPath, true)
                });

                // Crear el UserCredential usando el TokenResponse existente
                var credential = new UserCredential(flow, "user", tokenResponse);

                // Verificar si el token está obsoleto y refrescarlo si es necesario
                if (credential.Token.IsStale)
                {
                    bool success = credential.RefreshTokenAsync(CancellationToken.None).Result;
                    if (!success)
                    {
                        Console.WriteLine("No se pudo refrescar el token de acceso. Reautenticando...");
                        // Aquí puedes forzar la reautenticación si el refresh falla.
                        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                            new ClientSecrets
                            {
                                ClientId = ConfigurationManager.AppSettings["ClientId"],
                                ClientSecret = ConfigurationManager.AppSettings["ClientSecret"]
                            },
                            Scopes,
                            "user",
                            CancellationToken.None,
                            new FileDataStore(credPath, true)).Result;
                    }
                }

                // Obtener la información del perfil del usuario autenticado
                var email = GetAuthenticatedUserEmail(credential);
                Console.WriteLine($"Autenticado con la cuenta: {email}");

                return credential;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante la autenticación: {ex.Message}");
                return null;  // Devuelve null si ocurre un error durante la autenticación
            }
        }

        private TokenResponse UpdateTokenResponse(string credPath)
        {
            try
            {
                // Ruta completa del archivo de token
                string tokenFilePath = Path.Combine(credPath, "Google.Apis.Auth.OAuth2.Responses.TokenResponse-user");

                TokenResponse tokenResponse;

                if (File.Exists(tokenFilePath))
                {
                    // Leer el archivo de token existente
                    var json = File.ReadAllText(tokenFilePath);
                    tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(json);
                }
                else
                {
                    tokenResponse = new TokenResponse();
                }

                // Actualizar las propiedades del token según la información de app.config
                tokenResponse.AccessToken = ConfigurationManager.AppSettings["AccessToken"];
                tokenResponse.RefreshToken = ConfigurationManager.AppSettings["RefreshToken"];
                tokenResponse.TokenType = "Bearer";
                tokenResponse.ExpiresInSeconds = Convert.ToInt32(ConfigurationManager.AppSettings["ExpiresInSeconds"]);
                tokenResponse.Scope = ConfigurationManager.AppSettings["Scope"];

                // Si la fecha de emisión es mínima o pasada, se reestablece para mantener el token vigente
                tokenResponse.IssuedUtc = DateTime.UtcNow;

                // Serializar el objeto TokenResponse actualizado a JSON
                var updatedJson = JsonConvert.SerializeObject(tokenResponse, Formatting.Indented);

                // Sobrescribir el archivo de token con la nueva información
                //File.WriteAllText(tokenFilePath, updatedJson);

                Console.WriteLine("El archivo de token ha sido actualizado correctamente.");

                return tokenResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar el archivo de token: {ex.Message}");
                throw; // Relanza la excepción para que sea manejada en Authenticate
            }
        }

        private string GetAuthenticatedUserEmail(UserCredential credential)
        {
            try
            {
                var oauth2Service = new Oauth2Service(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                var userInfo = oauth2Service.Userinfo.Get().Execute();
                return userInfo.Email ?? "Correo no disponible";
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error al obtener el correo electrónico: {ex.Message}");
                return "Correo no disponible";
            }
        }
    }
}
