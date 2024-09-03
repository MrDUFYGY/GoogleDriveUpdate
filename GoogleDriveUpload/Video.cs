using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GoogleDriveUpload
{
    public class Video
    {
        private static string ApplicationName = "Drive API .NET Console App";
        private UserCredential _credential;
        public Video(UserCredential credential)
        {
            _credential = credential;
        }


        public M_Result UploadAndVerifyFile(string fullPath)
        {
            var result = new M_Result();

            try
            {
                // Obtener el tamaño total del archivo antes de la subida
                long totalSize = new FileInfo(fullPath).Length;

                // Usar el modelo M_Path para descomponer la ruta completa
                M_Path pathInfo = new M_Path(fullPath);

                // Crear el servicio Drive API
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = _credential,
                    ApplicationName = ApplicationName,
                });

                // Verificar o crear la carpeta de la sucursal o estación
                string sucursalFolderId = CreateOrGetFolder(service, pathInfo.Sucursal.TrimEnd('\\'), null);

                // Verificar o crear la carpeta dentro de la sucursal con el nombre de la fecha
                string fechaFolderId = CreateOrGetFolder(service, pathInfo.Fecha.ToString("yyyy-MM-dd"), sucursalFolderId);

                // Verificar o crear la carpeta del canal dentro de la carpeta de la fecha
                string canalFolderId = CreateOrGetFolder(service, pathInfo.Canal.TrimEnd('\\'), fechaFolderId);

                // Subir el archivo al Drive dentro de la estructura Sucursal > Fecha > Canal
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = pathInfo.File,
                    Parents = new List<string> { canalFolderId }
                };

                FilesResource.CreateMediaUpload fileRequest;
                using (var stream = new FileStream(fullPath, FileMode.Open))
                {
                    fileRequest = service.Files.Create(fileMetadata, stream, "application/octet-stream");
                    fileRequest.Fields = "id";

                    IUploadProgress progress = null;
                    int maxRetries = 5;
                    int retries = 0;

                    while (retries < maxRetries)
                    {
                        Console.WriteLine($"Intento {retries + 1} de {maxRetries}...");
                        fileRequest.ProgressChanged += (IUploadProgress p) => FileRequest_ProgressChanged(p, totalSize);
                        progress = fileRequest.Upload();

                        if (progress.Status == UploadStatus.Completed)
                        {
                            var file = fileRequest.ResponseBody;
                            result.Correct = true;
                            result.Message = $"Archivo subido y validado correctamente: {file.Name} (ID: {file.Id}) [Intentos: {retries + 1}]";
                            Console.WriteLine(result.Message);
                            break;
                        }
                        else if (progress.Status == UploadStatus.Failed)
                        {
                            Console.WriteLine($"Error al subir el archivo {fullPath}, reintentando... ({retries + 1}/{maxRetries})");
                            retries++;
                        }
                    }

                    if (progress.Status != UploadStatus.Completed)
                    {
                        result.Correct = false;
                        result.Message = $"Error al subir el archivo: {fullPath} después de {maxRetries} intentos.";
                        Console.WriteLine(result.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = $"Ocurrió un error durante la operación: {ex.Message}";
            }

            return result;
        }

        private void FileRequest_ProgressChanged(IUploadProgress progress, long totalSize)
        {
            int progressBarLength = 50; // Longitud de la barra de progreso
            double percentage = (double)progress.BytesSent / totalSize;
            int progressBlocks = (int)(percentage * progressBarLength);

            Console.Write("\r[");
            Console.Write(new string('=', progressBlocks));
            Console.Write(new string(' ', progressBarLength - progressBlocks));
            Console.Write($"] {percentage:P0} ({progress.BytesSent / 1024} KB de {totalSize / 1024} KB)");

            if (progress.Status == UploadStatus.Completed)
            {
                Console.WriteLine("\nSubida completada.");
            }
            else if (progress.Status == UploadStatus.Failed)
            {
                Console.WriteLine("\nError durante la subida.");
            }
        }

        //public M_Result UploadAndVerifyFile(string fullPath)
        //{
        //    var result = new M_Result();

        //    try
        //    {
        //        // Usar el modelo M_Path para descomponer la ruta completa
        //        M_Path pathInfo = new M_Path(fullPath);

        //        // Crear el servicio Drive API
        //        var service = new DriveService(new BaseClientService.Initializer()
        //        {
        //            HttpClientInitializer = _credential,
        //            ApplicationName = ApplicationName,
        //        });

        //        // Verificar o crear la carpeta de la sucursal o estación
        //        string sucursalFolderId = CreateOrGetFolder(service, pathInfo.Sucursal.TrimEnd('\\'), null);

        //        // Verificar o crear la carpeta dentro de la sucursal con el nombre de la fecha
        //        string fechaFolderId = CreateOrGetFolder(service, pathInfo.Fecha.ToString("yyyy-MM-dd"), sucursalFolderId);

        //        // Verificar o crear la carpeta del canal dentro de la carpeta de la fecha
        //        string canalFolderId = CreateOrGetFolder(service, pathInfo.Canal.TrimEnd('\\'), fechaFolderId);

        //        // Subir el archivo al Drive dentro de la estructura Sucursal > Fecha > Canal
        //        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        //        {
        //            Name = pathInfo.File,
        //            Parents = new List<string> { canalFolderId }
        //        };

        //        FilesResource.CreateMediaUpload fileRequest;
        //        using (var stream = new FileStream(fullPath, FileMode.Open))
        //        {
        //            fileRequest = service.Files.Create(fileMetadata, stream, "application/octet-stream");
        //            fileRequest.Fields = "id";

        //            IUploadProgress progress = null;
        //            int maxRetries = 5;
        //            int retries = 0;

        //            while (retries < maxRetries)
        //            {
        //                progress = fileRequest.Upload();
        //                if (progress.Status == UploadStatus.Completed)
        //                {
        //                    var file = fileRequest.ResponseBody;
        //                    result.Correct = true;
        //                    result.Message = $"Archivo subido y validado correctamente: {file.Name} (ID: {file.Id}) [Intentos: {retries + 1}]";
        //                    Console.WriteLine(result.Message);
        //                    break;
        //                }
        //                else if (progress.Status == UploadStatus.Failed)
        //                {
        //                    Console.WriteLine($"Error al subir el archivo {fullPath}, reintentando... ({retries + 1}/{maxRetries})");
        //                    retries++;
        //                }
        //            }

        //            if (progress.Status != UploadStatus.Completed)
        //            {
        //                result.Correct = false;
        //                result.Message = $"Error al subir el archivo: {fullPath} después de {maxRetries} intentos.";
        //                Console.WriteLine(result.Message);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Correct = false;
        //        result.Message = $"Ocurrió un error durante la operación: {ex.Message}";
        //    }

        //    return result;
        //}

        //private void FileRequest_ProgressChanged(IUploadProgress progress)
        //{
        //    Console.WriteLine($"{progress.Status}: {progress.BytesSent} bytes enviados.");
        //}

        public bool VerifyFileUpload(DriveService service, string fileId)
        {
            try
            {
                var request = service.Files.Get(fileId);
                var file = request.Execute();
                return file != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verificando el archivo: {ex.Message}");
                return false;
            }
        }

        private string CreateOrGetFolder(DriveService service, string folderName, string parentFolderId)
        {
            // Buscar si la carpeta ya existe
            string existingFolderId = FindFolder(service, folderName, parentFolderId);
            if (!string.IsNullOrEmpty(existingFolderId))
            {
                Console.WriteLine($"La carpeta '{folderName}' ya existe con ID: {existingFolderId}");
                return existingFolderId;
            }

            // Crear la carpeta si no existe
            var folderMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = parentFolderId == null ? null : new List<string> { parentFolderId }
            };

            var folderRequest = service.Files.Create(folderMetadata);
            folderRequest.Fields = "id";
            var folder = folderRequest.Execute();

            Console.WriteLine($"Carpeta '{folderName}' creada con ID: {folder.Id}");
            return folder.Id;
        }

        private string FindFolder(DriveService service, string folderName, string parentFolderId)
        {
            var request = service.Files.List();
            request.Q = $"mimeType='application/vnd.google-apps.folder' and name='{folderName}'";
            if (!string.IsNullOrEmpty(parentFolderId))
            {
                request.Q += $" and '{parentFolderId}' in parents";
            }
            request.Spaces = "drive";
            request.Fields = "files(id, name)";
            request.PageSize = 10;

            var result = request.Execute();
            var folder = result.Files.FirstOrDefault();
            return folder?.Id;
        }

        public void DeleteLocalFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine($"El archivo local ha sido eliminado: {filePath}");
                }
                else
                {
                    Console.WriteLine($"El archivo local no existe: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al intentar eliminar el archivo local: {ex.Message}");
            }
        }
    }
}
