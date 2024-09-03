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
                // Autenticar al usuario utilizando GoogleAuthenticator
                GoogleAuthenticator authenticator = new GoogleAuthenticator();
                UserCredential credential = authenticator.Authenticate();

                if (credential == null)
                {
                    result.Correct = false;
                    result.Message = "Autenticación fallida.";
                    return result;
                }

                // Instanciar la clase Video para gestionar la subida del archivo a Google Drive
                Video videoManager = new Video(credential);

                // Subir y verificar el archivo en Google Drive
                var uploadResult = videoManager.UploadAndVerifyFile(fullPath);

                if (!uploadResult.Correct)
                {
                    return uploadResult; // Retornar el resultado si la subida falla
                }

                // Eliminar el archivo local después de haberlo subido exitosamente
                videoManager.DeleteLocalFile(fullPath);

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
