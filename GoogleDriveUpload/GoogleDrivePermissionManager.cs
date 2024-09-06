using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;

namespace GoogleDriveUpload
{ 
    public class GoogleDrivePermissionManager
    {
        private readonly DriveService _service;

        public GoogleDrivePermissionManager(UserCredential credential)
        {
            _service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Drive API .NET Console App",
            });
        }

        // Método para otorgar permisos
        public Permission GrantPermission(string fileId, string emailAddress, string role)
        {
            var permission = new Permission()
            {
                Type = "user",
                Role = role,
                EmailAddress = emailAddress
            };

            var request = _service.Permissions.Create(permission, fileId);
            request.Fields = "id";
            return request.Execute();
        }

        // Método para revocar permisos
        public void RevokePermission(string fileId, string permissionId)
        {
            var request = _service.Permissions.Delete(fileId, permissionId);
            request.Execute();
        }

        // Método para listar los permisos actuales
        public IList<Permission> ListPermissions(string fileId)
        {
            var request = _service.Permissions.List(fileId);
            return request.Execute().Permissions;
        }

        // Método para gestionar permisos (agregar o quitar)
        public static void GestionarPermisos()
        {
            try
            {
                Console.Write("Ingresa el ID del archivo/carpeta en Google Drive: ");
                string fileId = Console.ReadLine();

                Console.Write("Ingresa el correo del usuario al que deseas otorgar/quitar permiso: ");
                string email = Console.ReadLine();

                Console.Write("Selecciona el rol (reader/writer/owner): ");
                string role = Console.ReadLine();

                Console.Write("Deseas agregar o quitar el permiso? (agregar/quitar): ");
                string accion = Console.ReadLine().ToLower();

                // Obtener las credenciales mediante la clase GoogleGetCredentials
                GoogleGetCredentials googleGetCredentials = new GoogleGetCredentials();
                UserCredential credential = googleGetCredentials.Get();

                GoogleDrivePermissionManager permissionManager = new GoogleDrivePermissionManager(credential);

                if (accion == "agregar")
                {
                    var permission = permissionManager.GrantPermission(fileId, email, role);
                    if (permission != null)
                    {
                        Console.WriteLine($"Permiso '{role}' otorgado a {email} con ID: {permission.Id}");
                    }
                }
                else if (accion == "quitar")
                {
                    // Obtener la lista de permisos y permitir seleccionar cuál eliminar
                    var permissions = permissionManager.ListPermissions(fileId);
                    Console.WriteLine("Permisos disponibles:");
                    foreach (var perm in permissions)
                    {
                        Console.WriteLine($"ID: {perm.Id}, Correo: {perm.EmailAddress}, Rol: {perm.Role}");
                    }

                    Console.Write("Ingresa el ID del permiso que deseas quitar: ");
                    string permissionId = Console.ReadLine();
                    permissionManager.RevokePermission(fileId, permissionId);
                    Console.WriteLine($"Permiso quitado.");
                }
                else
                {
                    Console.WriteLine("Acción no válida.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al gestionar permisos: {ex.Message}");
            }
        }
    }
}
