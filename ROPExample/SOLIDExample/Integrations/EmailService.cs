using SOLIDExample.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOLIDExample.Integrations
{
    public class EmailService : IEmailService
    {
        public void EmailClient(string emailAddress, string template)
        {
            //Look at how easy it is to send an email to a client
        }
    }
}
