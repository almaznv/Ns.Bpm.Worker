using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ns.BpmOnline.Worker
{
    public interface IExecutor
    {
        void Execute(byte[] data);
    }
}
