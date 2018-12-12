namespace C60.SshShellPowerShellModule
{
    using System;
    using System.Text.RegularExpressions;
    using Renci.SshNet;

    /// <summary>
    /// Provide the core functionality of create, connect and send commands to a shell session.
    /// </summary>
    public class SshShell : IDisposable
    {
        private const string CommentPrefix = "/// ";
        private const int DefaultConnectionTimeout = 10;
        private SshClient sshClient;
        private AuthenticationMethod authenticationMethod;
        private ShellStream shellStream;
        private ShellStreamManager shellStreamManager;
        private ShellResult shellResult;
        private bool isConnected;

        public SshShell(string host, string user, int port = 22)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException("host");
            }

            if (string.IsNullOrEmpty(user))
            {
                throw new ArgumentNullException("user");
            }

            this.Host = host;
            this.Port = port;
            this.User = user;
            this.ConnectionTimeout = DefaultConnectionTimeout;

            this.shellResult = new ShellResult();
        }

        public string Host { get; private set; }

        public int Port { get; private set; }

        public string User { get; private set; }

        public int ConnectionTimeout { get; set; }

        public bool IsConnected
        {
            get
            {
                return this.isConnected && this.sshClient.IsConnected;
            }

            private set
            {
                this.isConnected = value;
            }
        }

        public string AllResult
        {
            get
            {
                return this.shellResult.AllResult;
            }
        }

        public string LastResult
        {
            get
            {
                return this.shellResult.LastResult;
            }
        }

        public string LastResultLine
        {
            get
            {
                return this.shellResult.LastResultLine;
            }
        }

        public string LastResultWithoutFirstLastLines
        {
            get
            {
                return this.shellResult.LastResultWithoutFirstLastLines;
            }
        }

        public void SetPasswordAuthentication(string password)
        {
            this.authenticationMethod = new PasswordAuthenticationMethod(this.User, password);
        }

        public void SetPrivateKeyAuthentication(string fileName, string passphrase = null)
        {
            this.authenticationMethod = new PrivateKeyAuthenticationMethod(this.User, new PrivateKeyFile(fileName, passphrase));
        }

        public void Connect()
        {
            if (this.authenticationMethod == null)
            {
                throw new Exception("No authentication method provided");
            }

            ConnectionInfo connectionInfo = new ConnectionInfo(this.Host, this.Port, this.User, this.authenticationMethod);
            connectionInfo.Timeout = TimeSpan.FromSeconds(this.ConnectionTimeout);
            this.sshClient = new SshClient(connectionInfo);

            try
            {
                this.sshClient.Connect();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Failed to connect to {0} with user {1}. {2}.", this.Host, this.User, ex.Message), ex);
            }

            try
            {
                //this.shellStream = this.sshClient.CreateShellStream("C60SshShellPS", 0, 0, 0, 0, 0);
                this.shellStream = this.sshClient.CreateShellStream("C60SshShellPS", 0, 0, 0, 0, 1000);

                this.shellStreamManager = new ShellStreamManager(this.shellStream);
                this.shellStreamManager.WaitForResponse();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("The host {0} doesn't support Shell stream. {1}.", this.Host, ex.Message), ex);
            }

            this.IsConnected = true;
            this.shellResult.AddResult(this.shellStreamManager.ReadStreamWhileInputAvailable());
        }

        public void Execute(string command, int timeoutMilliseconds = 0, Regex breakOnMatch = null, Regex waitUnlimitedOn = null, bool cleanInvalidCharacter = true)
        {
            if (!this.IsConnected)
            {
                throw new Exception("Not Connected");
            }

            // Clear any existing information from the stream
            this.shellResult.AddResult(this.shellStreamManager.ReadStreamWhileInputAvailable(0), cleanInvalidCharacter);

            if (!string.IsNullOrEmpty(command))
            {
                this.shellStreamManager.WriteLineAndWaitForResponse(command);
                //this.shellStreamManager.WriteLineAndWaitForResponse("N0tP@$$sw0rd!");
                //this.shellStreamManager.JustWrite(command);
                this.shellResult.AddResult(
                                            this.shellStreamManager.ReadStreamWhileInputAvailable(
                                                                                                    TimeSpan.FromMilliseconds(timeoutMilliseconds), 
                                                                                                    breakOnMatch, 
                                                                                                    waitUnlimitedOn), 
                                            cleanInvalidCharacter);
               
            }
        }

        /// <summary>
        /// Add a comment to the session result. The comment will be added with the fix prefix "/// "
        /// </summary>
        public void AddResultComment(string comment)
        {
            if (!string.IsNullOrEmpty(comment))
            {
                this.shellResult.AddResult(CommentPrefix + comment);
            }
        }

        /// <summary>
        /// Clear all the results from the current session.
        /// </summary>
        public void ClearResult()
        {
            this.shellResult.ClearResult();
        }

        public void Close()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        void IDisposable.Dispose()
        {
            this.Close();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.authenticationMethod != null && this.authenticationMethod is IDisposable)
                {
                    ((IDisposable)this.authenticationMethod).Dispose();
                }

                if (this.shellStream != null)
                {
                    this.shellStream.Dispose();
                }

                if (this.sshClient != null)
                {
                    this.sshClient.Disconnect();
                    this.sshClient.Dispose();
                }
            }
        }
    }
}
