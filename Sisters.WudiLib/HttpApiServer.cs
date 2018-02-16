using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;

namespace Sisters.WudiLib
{
    class HttpApiServer 
    {
        private HttpListener http;
        private Thread httpListenThread;

        private HttpApiCallback callback;
        private string host;
        private int port;

        public HttpApiServer(HttpApiCallback callback, string host = "127.0.0.1", int port = 8080)
        {
            this.callback = callback;
            this.host = host;
            this.port = port;
        }

        public void Run()
        {
            this.http = new HttpListener();
            this.http.Prefixes.Add(string.Format("http://{0}:{1}", host, port));
            this.http.Start();
            this.httpListenThread = new Thread(new ThreadStart(this.Listen));
        }

        private void Listen()
        {
            while (true)
            {
                HttpListenerContext context = this.http.GetContext();

                if (!this.callback.OnRequest(context))
                    continue;

                // TODO：处理上报数据
            }
        }
    }
}
