﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Gibraltar.Server.Client
{
    [Serializable]
    public class GibraltarRateLimitException : GibraltarException
    {
        public GibraltarRateLimitException(string message, TimeSpan? delay)
            :base(message)
        {
            Timestamp = DateTimeOffset.Now;
            Delay = delay;

            if (delay.HasValue)
            {
                RetryAfter = Timestamp.Add(delay.Value);
            }
            else
            {
                RetryAfter = Timestamp.AddSeconds(1);
            }
        }

        public DateTimeOffset Timestamp { get; private set; }

        public DateTimeOffset RetryAfter { get; private set; }

        /// <summary>
        /// The number of seconds to delay before retrying
        /// </summary>
        public TimeSpan? Delay { get; private set; }
    }
}
