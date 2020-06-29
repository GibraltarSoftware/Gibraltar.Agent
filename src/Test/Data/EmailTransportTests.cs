#region File Header
// /********************************************************************
//  * COPYRIGHT:
//  *    This software program is furnished to the user under license
//  *    by Gibraltar Software Inc, and use thereof is subject to applicable 
//  *    U.S. and international law. This software program may not be 
//  *    reproduced, transmitted, or disclosed to third parties, in 
//  *    whole or in part, in any form or by any manner, electronic or
//  *    mechanical, without the express written consent of Gibraltar Software Inc,
//  *    except to the extent provided for by applicable license.
//  *
//  *    Copyright © 2008 - 2015 by Gibraltar Software, Inc.  
//  *    All rights reserved.
//  *******************************************************************/
#endregion
#region File Header

/********************************************************************
 * COPYRIGHT:
 *    This software program is furnished to the user under license
 *    by Gibraltar Software, Inc, and use thereof is subject to applicable 
 *    U.S. and international law. This software program may not be 
 *    reproduced, transmitted, or disclosed to third parties, in 
 *    whole or in part, in any form or by any manner, electronic or
 *    mechanical, without the express written consent of Gibraltar Software, Inc,
 *    except to the extent provided for by applicable license.
 *
 *    Copyright © 2008 by Gibraltar Software, Inc.  All rights reserved.
 *******************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using Gibraltar.Monitor;
using Loupe.Extensibility.Data;
using NUnit.Framework;

#endregion File Header

namespace Gibraltar.Test.Data
{
    [Ignore("Requires email server configuration to test")]
    [TestFixture]
    public class EmailTransportTests
    {
        private const string EmailFromAddress = "No-reply@gibraltarsoftware.com";
        private const string EmailToAddress = "No-reply@gibraltarsoftware.com";
        private const string EmailOverrideServer = "provide.value.to.test";
        private const string EmailOverrideServerUser = "provide.value.to.test";
        private const string EmailOverrideServerPassword = "provide.value.to.test";
        
        [Test]
        public void BasicMessage()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                EmailMessage newMessage = new EmailMessage(EmailFromAddress);
                newMessage.AddAddress(EmailToAddress);
                newMessage.Subject = "NUnit:EmailTransportTests:BasicMessage";
                newMessage.Body = "This is an NUnit Email Transport Test message.";
                newMessage.Send();
            });
        }

        [Test]
        public void BasicMessageOverrideServer()
        {
            Assert.Throws<SmtpException>(() =>
            {
                EmailMessage newMessage = new EmailMessage(EmailFromAddress, EmailOverrideServer);
                newMessage.AddAddress(EmailToAddress);
                newMessage.Subject = "NUnit:EmailTransportTests:BasicMessageOverrideServer";
                newMessage.Body = "This is an NUnit Email Transport Test message.";
                newMessage.Send();
            });
        }

        [Test]
        public void BasicMessageOverrideServerAndUser()
        {
            EmailMessage newMessage = new EmailMessage(EmailFromAddress, EmailOverrideServer, EmailOverrideServerUser, EmailOverrideServerPassword);
            newMessage.AddAddress(EmailToAddress);
            newMessage.Subject = "NUnit:EmailTransportTests:BasicMessageOverrideServerAndUser";
            newMessage.Body = "This is an NUnit Email Transport Test message.";
            newMessage.Send();
        }

        [Test]
        public void BasicMessageWithAttachment()
        {
            //lets find us a test file.
            string tempFileNamePath = Path.GetTempFileName();

            try
            {
                using (StreamWriter testAttachment = File.CreateText(tempFileNamePath))
                {
                    testAttachment.WriteLine("NUnit:EmailTransportTests:BasicMessageWithAttachment");
                    testAttachment.WriteLine("This is a test file attachment to ensure that the email API is working.");
                }

                EmailMessage newMessage = new EmailMessage(EmailFromAddress, EmailOverrideServer, EmailOverrideServerUser, EmailOverrideServerPassword);
                newMessage.AddAddress(EmailToAddress);
                newMessage.Subject = "NUnit:EmailTransportTests:BasicMessageWithAttachment";
                newMessage.Body = "This is an NUnit Email Transport Test message that should have exactly 1 attachment";
                newMessage.AddAttachment(tempFileNamePath, "The First Attachment.txt");
                newMessage.Send();                
            }
            finally
            {
                File.Delete(tempFileNamePath);
            }
        }
    }
}