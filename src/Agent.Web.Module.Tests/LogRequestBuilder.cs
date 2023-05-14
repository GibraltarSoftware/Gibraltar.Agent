using System;
using System.Collections.Generic;
using ExpectedObjects;
using Gibraltar.Agent.Web.Module.Models;

namespace Loupe.Agent.Web.Module.Tests
{
    public class LogRequestBuilder
    {
        private ClientDetails stdClientDetails = new ClientDetails
        {
            Description = "Firefox 37.0 32-bit on Windows 8.1 64-bit",
            Layout = "Gecko",
            Name = "Firefox",
            UserAgentString = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0",
            Version = "37.0",
            OS = new ClientOS
            {
                Architecture = 64,
                Family = "Windows",
                Version = "8.1"
            },
            Size = new ClientDimensions
            {
                Height = 873,
                Width = 1102
            }
        };

        private LogRequest _currentRequest;
        private LogRequest Request
        {
            get { return _currentRequest ?? (_currentRequest = new LogRequest()); }
        }

        public LogRequestBuilder AddSession(string sessionId = null)
        {
            Request.Session = new ClientSession();
            Request.Session.CurrentAgentSessionId = sessionId;
            return this;
        }

        public LogRequestBuilder AddClientDetails()
        {
            if (Request.Session == null)
            {
                throw new NullReferenceException("No session created to associate client details with");
            }

            Request.Session.Client = stdClientDetails;
            return this;
        }

        public LogRequestBuilder AddMessage(LogMessage message)
        {
            if (Request.LogMessages == null)
            {
                Request.LogMessages = new List<LogMessage>();
            }

            Request.LogMessages.Add(message);

            return this;
        }

        public LogRequest Build()
        {
            return Request;
        }
    }
}