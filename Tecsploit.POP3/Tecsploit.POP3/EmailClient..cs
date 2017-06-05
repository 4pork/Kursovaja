// Created By Lee Dyche for http://www.tecsploit.com
// Send any queries to lee@tecsploit.com
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using Tecsploit.POP3;

namespace GMailEmailReader
{
    public partial class EmailClient : Form
    {
        public EmailClient()
        {
            InitializeComponent();
        }


        private void buttonDownloadloadMail_Click(object sender, EventArgs e)
        {
            ReadMail();
        }

        public void ReadMail() {
            Pop3Client client = new Pop3Client(textBoxServer.Text, Convert.ToInt32(this.textBoxPOPPort.Text), textBoxEmail.Text, textBoxPassword.Text, true);
            client.Connect();
            int numMessages = client.GetEmailCount();

            List<Email> mailList = client.FetchEmailList(Convert.ToInt32(numericUpDownStartIndex.Value), Convert.ToInt32(numericUpDownNumMessgages.Value));
            
            int id = 0;
            List<MessagePart> messageParts;
            foreach (Email email in mailList) {
                
                textBoxOutput.Text += "Message to: " + email.To + Environment.NewLine;
                textBoxOutput.Text += "Message from: " + email.From + Environment.NewLine;
                textBoxOutput.Text += "Message subject: " + email.Subject + Environment.NewLine;
                textBoxOutput.Text += "Message Date: " + email.UtcDateTime.ToShortDateString() + Environment.NewLine;

                messageParts = client.FetchMessageParts(id);

                textBoxOutput.Text += "Body: " + Environment.NewLine;

                textBoxOutput.Text += Environment.NewLine;

                if (messageParts != null)
                {
                    foreach (MessagePart part in messageParts)
                    {
                        textBoxOutput.Text += part.MessageText;
                    }

                    textBoxOutput.Text += "Body: " + Environment.NewLine;
                }

                id++;
            }

            textBoxOutput.Text += "Number of messages: " + numMessages.ToString()+Environment.NewLine;
            client.Close();

        }

        private void buttonSend(object sender, EventArgs e)
        {
            try
            {
                Pop3Client client = new Pop3Client(textBoxSMTPServer.Text, Convert.ToInt32(this.textBoxSMTPPOrt.Text), textBoxEmail.Text, textBoxPassword.Text, true);
                bool r = client.SendMail(this.textBoxTo.Text, textBoxSubject.Text, textBoxBody.Text);

                if (r)
                {
                    MessageBox.Show("Message Sent!");
                }
                else {
                    MessageBox.Show("Could not send message! "+ client.LAST_ERROR);                
                }
            }
            catch (Exception exp) {
                MessageBox.Show("Error sending messages: " + exp.Message);
            }
        }

    }
}
