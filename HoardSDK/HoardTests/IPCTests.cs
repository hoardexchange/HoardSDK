using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using HoardIPC;

namespace HoardTests
{
    public class IPCTests
    {
        [Fact]
        public void Test1()
        {
            PipeClient pc = new PipeClient();
            //pc.Initialize();

            
            //pc.Shutdown();
            Assert.Equal(4, 4);
        }
    }
}
