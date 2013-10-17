//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Sspi
{
    /// <summary>
    /// MachineAccountCredential contains credential (DomainName, MachineName, AccountName, Password) 
    /// used by a machine.
    /// </summary>
    public class MachineAccountCredential : AccountCredential
    {
        /// <summary>
        /// Machine name
        /// </summary>
        private string machineName;


        /// <summary>
        /// Initialize an instance of NrpcClientCredential class.
        /// </summary>
        /// <param name="domainName">
        /// The domain name.
        /// </param>
        /// <param name="machineName">
        /// The account name. In
        /// Windows, all machine account names are the name of
        /// the machine with a $ (dollar sign) appended.
        /// </param>
        /// <param name="machinePassword">
        /// The password of the machine account.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when domainName, machineName or machinePassword is null.
        /// </exception>
        public MachineAccountCredential(string domainName, string machineName, string machinePassword) :
            base(domainName, machineName + '$', machinePassword)
        {
            if (domainName == null)
            {
                throw new ArgumentNullException("domainName");
            }
            if (machineName == null)
            {
                throw new ArgumentNullException("machineName");
            }
            if (machinePassword == null)
            {
                throw new ArgumentNullException("machinePassword");
            }

            this.machineName = machineName;
        }


        /// <summary>
        /// Machine name.
        /// </summary>
        public string MachineName
        {
            get
            {
                return machineName;
            }
        }
    }
}
