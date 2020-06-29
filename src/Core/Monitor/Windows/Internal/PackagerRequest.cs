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
using Gibraltar.Data;

#endregion File Header

namespace Gibraltar.Monitor.Windows.Internal
{
    internal class PackagerRequest
    {
        /// <summary>
        /// Create a packager request restricted to the current application.
        /// </summary>
        public PackagerRequest()
            : this(Log.SessionSummary.Product, Log.SessionSummary.Application)
        {
        }

        /// <summary>
        /// Create a new packager request for the specified product and application name
        /// </summary>
        /// <param name="productName">The product name to restrict the package to</param>
        /// <param name="applicationName">Optional.  The application name to restrict the product to.</param>
        public PackagerRequest(string productName, string applicationName)
        {
            //product name can't be null.
            if (string.IsNullOrEmpty(productName))
                throw new ArgumentNullException("A product name must be specified");

            ProductName = productName;

            ApplicationName = applicationName;
        }

        public bool AllowEmail { get; set; }
        public bool AllowServer { get; set; }
        public bool AllowFile  { get; set; }
        public bool AllowRemovableMedia { get; set; }

        public PackageTransport Transport { get; set; }

        public string DestinationEmailAddress { get; set; }

        public string FromEmailAddress { get; set; }

        public string FileNamePath { get; set; }

        public SessionCriteria Criteria { get; set; }

        public string ProductName { get; private set; }

        public string ApplicationName { get; private set; }

        public string EmailServer { get; set; }
        public int ServerPort { get; set; }
        public bool UseSsl { get; set; }
        public string EmailServerUser { get; set; }
        public string EmailServerPassword { get; set; }

        /// <summary>
        /// True if the product and application configured for the request cover the running application
        /// </summary>
        public bool CoversCurrentApplication
        {
            get
            {
                bool returnVal = true;

                if (ProductName.Equals(Log.SessionSummary.Product, StringComparison.OrdinalIgnoreCase) == false)
                {
                    returnVal = false; //product not our product, no way we're covered.
                }
                else if ((string.IsNullOrEmpty(ApplicationName) == false )
                    && (ApplicationName.Equals(Log.SessionSummary.Application, StringComparison.OrdinalIgnoreCase) == false))
                {
                    returnVal = false; //application is set but not our app.
                }

                return returnVal;
            }
        }
    }

    internal enum PackageTransport
    {
        File = 0,
        RemovableMedia = 1,
        Email = 2,
        Server = 3
    }
}
