using System;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;

namespace GoogleDriveUpload
{
    public class RefreshCredentials
    {
        public static void UpdateAppConfigFromTxt(string configFilePath)
        {
            try
            {
                // Leer todo el contenido del archivo de configuración
                var lines = File.ReadAllLines(configFilePath);

                // Obtener la configuración actual
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                // Expresión regular para extraer las claves y valores del archivo de texto
                var regex = new Regex(@"<add key=""(?<key>[^""]+)"" value=""(?<value>[^""]*)""\s*/>");

                // Recorrer cada línea del archivo de configuración
                foreach (var line in lines)
                {
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        var key = match.Groups["key"].Value;
                        var value = match.Groups["value"].Value;

                        // Si la clave ya existe en app.config, actualizar su valor, sino agregar la nueva clave
                        if (config.AppSettings.Settings[key] != null)
                        {
                            config.AppSettings.Settings[key].Value = value;
                        }
                        else
                        {
                            config.AppSettings.Settings.Add(key, value);
                        }
                    }
                }

                // Guardar los cambios en el archivo de configuración
                config.Save(ConfigurationSaveMode.Modified);

                // Refrescar la sección para que los cambios se apliquen en la aplicación
                ConfigurationManager.RefreshSection("appSettings");

                Console.WriteLine("El archivo app.config ha sido actualizado exitosamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar el archivo app.config: {ex.Message}");
            }
        }
    }
}
