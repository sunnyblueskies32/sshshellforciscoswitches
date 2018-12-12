namespace C60.SshShellPowerShellModule.Cmdlets
{
    using System;
    using System.Management.Automation;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security;

    [Cmdlet(VerbsCommon.New, "SshSession", DefaultParameterSetName = "PasswordAuthentication")]
    [OutputType(typeof(SshShell))]
    public class NewSshSession : PSCmdlet
    {
        private int port = 22;
        
        private int connectionTimeout = 10;

        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        [Alias("h", "Host", "Computer", "IPAddress")]
        public string SshHost { get; set; }

        [Parameter(Position = 1, ParameterSetName = "PasswordAuthentication", Mandatory = true)]
        [Parameter(Position = 1, ParameterSetName = "PrivateKeyAuthentication", Mandatory = true)]
        [ValidateNotNullOrEmpty]
        [Alias("u")]
        public string User { get; set; }

        [Parameter(
                    Position = 2,
                    ParameterSetName = "PasswordAuthentication",
                    Mandatory = true)]
        [Alias("p")]
        public string Password { get; set; }

        [Parameter(
            Position = 2,
            ParameterSetName = "PrivateKeyAuthentication",
            Mandatory = true)]
        [ValidateNotNullOrEmpty]
        [Alias("k", "KeyFile", "PrivateKey")]
        public string PrivateKeyFile { get; set; }

        [Parameter(
            Position = 3,
            ParameterSetName = "PrivateKeyAuthentication",
            Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Passphrase { get; set; }

        [Parameter(
            Position = 1,
            ParameterSetName = "CredentialAuthentication",
            Mandatory = true)]
        [Alias("c")]
        [ValidateNotNull]
        public PSCredential Credential { get; set; }

        [Parameter]
        [Alias("po")]
        public int Port 
        {
            get
            {
                return this.port;
            }
            
            set
            {
                this.port = value;
            }
        }

        [Parameter]
        [Alias("ct")]
        public int ConnectionTimeout
        {
            get
            {
                return this.connectionTimeout;
            }

            set
            {
                this.connectionTimeout = value;
            }
        }

        protected override void BeginProcessing()
        {
            SshShell sshShell = null;
            switch (this.ParameterSetName)
            {
                case "PasswordAuthentication":
                    sshShell = new SshShell(this.SshHost, this.User, this.Port);
                    sshShell.SetPasswordAuthentication(this.Password);
                    break;

                case "PrivateKeyAuthentication":
                    sshShell = new SshShell(this.SshHost, this.User, this.Port);
                    sshShell.SetPrivateKeyAuthentication(this.PrivateKeyFile, this.Passphrase);
                    break;

                case "CredentialAuthentication":
                    sshShell = new SshShell(this.SshHost, this.Credential.UserName, this.Port);
                    sshShell.SetPasswordAuthentication(this.Credential.GetNetworkCredential().Password);
                    break;
            }

            sshShell.ConnectionTimeout = this.ConnectionTimeout;
            sshShell.Connect();
            this.WriteObject(sshShell);
        }
    }
}
