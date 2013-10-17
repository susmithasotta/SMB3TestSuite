﻿//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------


using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Sspi
{
    /// <summary>
    /// Credential with account information.
    /// </summary>
    public class AccountCredential : ICredential
    {
        /// <summary>
        /// domain of account.
        /// </summary>
        private string domain;

        /// <summary>
        /// account
        /// </summary>
        private string account;

        /// <summary>
        /// Password of account.
        /// </summary>
        private string accountPassword;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="accountName">Account name</param>
        /// <param name="password">User password</param>
        public AccountCredential(string domainName, string accountName, string password)
        {
            this.domain = domainName;
            this.account = accountName;
            this.accountPassword = password;
        }


        /// <summary>
        /// Account name
        /// </summary>
        public string AccountName
        {
            get
            {
                return this.account;
            }
        }


        /// <summary>
        /// Domain name
        /// </summary>
        public string DomainName
        {
            get
            {
                return this.domain;
            }
        }


        /// <summary>
        /// Account's password
        /// </summary>
        public string Password
        {
            get
            {
                return this.accountPassword;
            }
        }
    }
}
