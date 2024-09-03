using System;
using System.IO;

namespace GoogleDriveUpload
{
    class M_Path
    {
        public string FullPath { get; set; }
        public string BasePath { get; set; }
        public DateTime Fecha { get; set; }
        public string Sucursal { get; set; }
        public string Canal { get; set; }
        public string File { get; set; }


        // Constructor que toma la ruta completa y la descompone en sus partes
        public M_Path(string fullPath)
        {
            FullPath = fullPath;
            ParseFullPath();
        }

        private void ParseFullPath()
        {
            if (string.IsNullOrEmpty(FullPath))
            {

                throw new ArgumentException("El FullPath no puede estar vacío");
            }

            // Obtener el nombre del archivo
            File = Path.GetFileName(FullPath);

            // Obtener la ruta base (hasta la fecha)
            BasePath = Path.GetDirectoryName(FullPath);
            if (BasePath != null)
            {
                string[] parts = BasePath.Split(Path.DirectorySeparatorChar);

                // Asumimos que la estructura es siempre la misma
                if (parts.Length >= 4)
                {
                    Sucursal = parts[parts.Length - 3];
                    Canal = parts[parts.Length - 1];
                    BasePath = string.Join(Path.DirectorySeparatorChar.ToString(), parts, 0, parts.Length - 4);
                    Fecha = DateTime.ParseExact(parts[parts.Length - 2], "yyyy-MM-dd", null);
                }
            }
        }

    }
}

