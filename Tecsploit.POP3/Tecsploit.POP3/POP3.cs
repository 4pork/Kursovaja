using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net.Security;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Mail;

namespace Tecsploit.POP3
{
    public class Pop3Client : IDisposable
    {

        public StreamWriter Writer { get; protected set; }
        public StreamReader Reader { get; protected set; }

        public string Host { get; protected set; }
        public int Port { get; protected set; }
        public string Email { get; protected set; }
        public string Password { get; protected set; }
        public bool IsSecure { get; protected set; }

        private bool Disposed = false;

        public TcpClient Client { get; protected set; }
        public Stream ClientStream { get; protected set; }


        public string LAST_ERROR { get; set; }

        public Pop3Client(string host, int port, string email, string password) : this(host, port, email, password, false)
        {
        }

        public Pop3Client(string host, int port, string email, string password, bool secure)
        {
            Host = host;
            Port = port;
            Email = email;
            Password = password;
            IsSecure = secure;
        }

        public void Connect()
        {
            if (Client == null)
                Client = new TcpClient();

            if (!Client.Connected)
                Client.Connect(Host, Port);

            if (IsSecure)
            {
                SslStream secureStream = new SslStream(Client.GetStream());
                
                secureStream.AuthenticateAsClient(Host);
                ClientStream = secureStream;
                secureStream = null;
            }
            else
                ClientStream = Client.GetStream();

            Writer = new StreamWriter(ClientStream);
            Reader = new StreamReader(ClientStream);

            ReadLine();
            Login();
        }

        public int GetEmailCount()
        {
            int count = 0;
            string response = SendCommand("STAT");

            if (IsResponseOk(response))
            {
                string[] arr = response.Substring(4).Split(' ');
                count = Convert.ToInt32(arr[0]);
            }
            else
                count = -1;

            return count;
        }

        public Email FetchEmail(int emailId)
        {
            if (IsResponseOk(SendCommand("TOP " + emailId + " 0")))
                return new Email(ReadLines());
            else
                return null;
        }

        public List<Email> FetchEmailList(int start, int count)
        {
            List<Email> emails = new List<Email>(count);

            for (int i = start; i < (start + count); i++)
            {
                Email email = FetchEmail(i);

                if (email != null)
                    emails.Add(email);
            }

            return emails;
        }

        public List<MessagePart> FetchMessageParts(int emailId)
        {
            if (IsResponseOk(SendCommand("RETR " + emailId)))
                return Util.ParseMessageParts(ReadLines());

            return null;
        }


        public void Close()
        {
            if (Client != null)
            {
                if (Client.Connected)
                    Logout();

                Client.Close();
                Client = null;
            }

            if (ClientStream != null)
            {
                ClientStream.Close();
                ClientStream = null;
            }

            if (Writer != null)
            {
                Writer.Close();
                Writer = null;
            }

            if (Reader != null)
            {
                Reader.Close();
                Reader = null;
            }

            Disposed = true;
        }

        public void Dispose()
        {
            if (!Disposed)
                Close();
        }

        protected void Login()
        {
            if (!IsResponseOk(SendCommand("USER " + Email)) ||
              !IsResponseOk(SendCommand("PASS " + Password)))
                throw new Exception("User/password not accepted");
        }

        protected void Logout()
        {
            SendCommand("RSET");
        }

        protected string SendCommand(string cmdtext)
        {
            Writer.WriteLine(cmdtext);
            Writer.Flush();

            return ReadLine();
        }

        protected string ReadLine()
        {
            return Reader.ReadLine() + "\r\n";
        }

        protected string ReadLines()
        {
            StringBuilder b = new StringBuilder();

            while (true)
            {
                string temp = ReadLine();

                if (temp == ".\r\n" || temp.IndexOf("-ERR") != -1)
                    break;

                b.Append(temp);
            }

            return b.ToString();
        }

        protected static bool IsResponseOk(string response)
        {
            if (response.StartsWith("+OK"))
                return true;
            if (response.StartsWith("-ERR"))
                return false;

            throw new Exception("Cannot understand server response: " +
              response);
        }

        public bool SendMail(string to, string subject, string message)
        {
            try
            {
                NetworkCredential loginInfo = new NetworkCredential(Email, Password);
                MailMessage msg = new MailMessage();
                msg.From = new MailAddress(Email);
                msg.To.Add(new MailAddress(to));
                msg.Subject = subject;
                msg.Body = message;
                msg.IsBodyHtml = true;
                SmtpClient client = new SmtpClient(Host);
                client.Port = Port;
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = loginInfo;
                client.Send(msg);

                return true;
            }
            catch (Exception exp)
            {
                LAST_ERROR = exp.Message;
                return false;
            }

        }
    }
}

