using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace Tecsploit.POP3
{
    public class MessagePart
    {
        public NameValueCollection Headers { get; protected set; }

        public string ContentType { get; protected set; }
        public string MessageText { get; protected set; }

        public MessagePart(NameValueCollection headers, string messageText)
        {
            Headers = headers;
            ContentType = Headers["Content-Type"];
            MessageText = messageText;
        }
    }

}
