namespace C60.SshShellPowerShellModule.Cmdlets
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    
    [Cmdlet(VerbsCommon.Enter, "SshSession")]
    public class EnterSshSession : PSCmdlet
    {
        private string exitCommand = "EXIT";

        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [ValidateNotNull]
        [Alias("s")]
        public SshShell SshShell { get; set; }

        [Parameter(Position = 1)]
        [Alias("e")]
        public string ExitCommand 
        { 
            get
            {
                return this.exitCommand;
            } 

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.exitCommand = value.ToUpperInvariant();
                }
            }
        }

        protected override void BeginProcessing()
        {
            this.CheckConnection();
            this.WriteObject(this.SshShell.AllResult);
            while (true)
            {
                string prompt = string.IsNullOrEmpty(this.SshShell.LastResultLine) ? string.Empty : string.Format("'{0}'", this.SshShell.LastResultLine); 
                string command = this.InvokeCommand.InvokeScript("Read-Host " + prompt).FirstOrDefault().ToString().Trim();
                if (command.ToUpperInvariant() == this.ExitCommand)
                {
                    break;
                }

                this.CheckConnection();
                this.SshShell.Execute(command);
                this.WriteObject(this.SshShell.LastResultWithoutFirstLastLines);
            }
        }

        private void CheckConnection()
        {
            if (!this.SshShell.IsConnected)
            {
                this.ThrowTerminatingError(new ErrorRecord(new ArgumentException("SshShell is not connected"), "-1", ErrorCategory.OperationStopped, this.SshShell));
            }
        }
    }
}
