using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceSapProcedure.Repositories
{
    interface IRepository
    {
        void setup(string fullpath);
        void normalize();
        void execute();
    }
}
