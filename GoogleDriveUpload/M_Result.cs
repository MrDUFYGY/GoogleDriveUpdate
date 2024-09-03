using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDriveUpload
{
    public class M_Result
    {
        public bool Correct { get; set; }
        public string Message { get; set; }
        public Exception Error { get; set; }
        public object Object { get; set; }

    }
}
