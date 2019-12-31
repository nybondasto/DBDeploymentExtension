using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace t1 
{
    public class DacHacFactory
    {
        public DacHacXml Build(string path)
        {
            return new DacHacXml(path);
        }
    }
}
