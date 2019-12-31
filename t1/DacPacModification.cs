using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace t1 
{
    public class DacPacModification
    {
        ModelChecksumWriter mcw;
        public DacPacModification()
        {
            string spath = @"C:\DATA\LSVSExt\LSVSExt\bin\Debug\LSVSExt.dacpac";
            mcw = new ModelChecksumWriter(spath);
        }
       
        public string RecalculateChecksum()
        {
            return mcw.FixChecksum();
        }

        
    }
}
