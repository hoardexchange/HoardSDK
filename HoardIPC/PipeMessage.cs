using System.Collections.Generic;

namespace HoardIPC
{
    public class SingleCommand
    {
        public string command;
        public string arg;
    }

    public class PipeMessage
    {
        static public int MessageChunkSize = 256;

        public IList<SingleCommand> commands = new List<SingleCommand>();
    }
}
