using System;

using System.Collections.Generic;

namespace GoogleDriveUpload
{
    public class Installed
    {
        public string ClientId { get; set; }
        public string ProjectId { get; set; } 
        public string AuthUri { get; set; } 
        public string TokenUri { get; set; } 
        public string AuthProviderX509CertUrl { get; set; }
        public string ClientSecret { get; set; }
        public List<string> RedirectUris { get; set; } 
    }

    public class Credentials
    {
        public Installed Installed { get; set; } = new Installed();
    }
}
