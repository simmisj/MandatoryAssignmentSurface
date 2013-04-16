using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace MandatoryAssignmentSurface
{
    class IncommingConnection 
    {

        TcpListener socket;
        
        public IncommingConnection(TcpListener socket)
        {
            this.socket = socket;
            socket.Start();
            Thread newThread = new Thread(new ThreadStart(run));
            newThread.Start();
        }

        public void run() 
        {
            
        }
    }
}
