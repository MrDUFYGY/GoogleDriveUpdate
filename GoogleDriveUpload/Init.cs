using Google.Apis.Auth.OAuth2;
using System;

namespace GoogleDriveUpload
{
    public class Init
    {
        public static M_Result ProcessFileUpload(string fullPath)
        {
            var result = new M_Result();
            try
            {
                // Instanciar el autenticador de Google y autenticar al usuario
                GoogleAuthenticator authenticator = new GoogleAuthenticator();
                UserCredential credential = authenticator.Authenticate();

                if (credential == null)
                {
                    result.Correct = false;
                    result.Message = "Autenticación fallida.";
                    return result;
                }

                // Instanciar el gestor de Google Drive con las credenciales autenticadas
                VideoUpload videoUpload = new VideoUpload(credential);

                // Subir y verificar el archivo en la ruta especificada
                var uploadResult = videoUpload.UploadAndVerifyFile(fullPath);

                if (!uploadResult.Correct)
                {
                    return uploadResult;  // Retornar si la subida falla
                }

                // Eliminar el archivo local después de subirlo
                videoUpload.DeleteLocalFile(fullPath);

                result.Correct = true;
                result.Message = "Se completó la ejecución de todos los módulos correctamente.";
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = "Ha ocurrido un error. ERROR: " + ex.Message;
                result.Error = ex;
            }
            return result;
        }

    }
}
