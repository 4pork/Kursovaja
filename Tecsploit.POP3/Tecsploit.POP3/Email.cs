using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace Tecsploit.POP3
{
    public class Email
    {
        public NameValueCollection Headers { get; protected set; }

        public string ContentType { get; protected set; }
        public DateTime UtcDateTime { get; protected set; }
        public string From { get; protected set; }
        public string To { get; protected set; }
        public string Subject { get; protected set; }

        public Email(string emailText)
        {
            Headers = Util.ParseHeaders(emailText);

            ContentType = Headers["Content-Type"];
            From = Headers["From"];
            To = Headers["To"];
            Subject = Headers["Subject"];

            if (Headers["Date"] != null)
                try
                {
                    UtcDateTime =
                      Util.ConvertStrToUtcDateTime(Headers["Date"]);
                }
                catch (FormatException)
                {
                    UtcDateTime = DateTime.MinValue;
                }
            else
                UtcDateTime = DateTime.MinValue;
        }
    }

}
