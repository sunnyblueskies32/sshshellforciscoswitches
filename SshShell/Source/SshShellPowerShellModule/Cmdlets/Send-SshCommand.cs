namespace C60.SshShellPowerShellModule.Cmdlets
{
    using System;
    using System.Management.Automation;
    using System.Text.RegularExpressions;

    [Cmdlet(VerbsCommunications.Send, "SshCommand")]
    [OutputType(typeof(string))]
    public class SendSshCommand : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNull]
        [Alias("s")]
        public SshShell SshShell { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        [Alias("c")]
        public string Command { get; set; }

        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true)]
        [Alias("w")]
        public int WaitMillisecondsForOutput { get; set; }

        [Parameter(Position = 3, ValueFromPipelineByPropertyName = true)]
        [Alias("e")]
        public string Expect { get; set; }

        [Parameter(Position = 4, ValueFromPipelineByPropertyName = true)]
        [Alias("wu")]
        public string WaitUnlimitedOn { get; set; }

        [Parameter]
        public SwitchParameter IgnoreInvalidCharacters { get; set; }
        
        protected override void BeginProcessing()
        {
            if (!this.SshShell.IsConnected)
            {
                this.ThrowTerminatingError(new ErrorRecord(new ArgumentException("SshShell is not connected"), "-1", ErrorCategory.OperationStopped, this.SshShell));
            }
        }

        protected override void ProcessRecord()
        {
            Regex expect = ParseRegex(this.Expect);
            Regex waitUnlimitedOn = ParseRegex(this.WaitUnlimitedOn);
            //Console.WriteLine("Parse Record {0}",Command.Length.ToString());

            this.SshShell.Execute(this.Command.Trim(), this.WaitMillisecondsForOutput, expect, waitUnlimitedOn, !this.IgnoreInvalidCharacters);
            if (expect != null && !expect.IsMatch(this.SshShell.LastResult))
            {
                this.ThrowTerminatingError(new ErrorRecord(new Exception(string.Format("Result not match expected pattern '{1}'.\nCommand result:\n{0}\n", this.SshShell.LastResult, this.Expect)), "-2", ErrorCategory.InvalidResult, this));
            }

            this.WriteObject(this.SshShell.LastResult);
            //this.WriteObject("this is new thing!");
        }

        private static Regex ParseRegex(string regex)
        {
            Regex result = null;
            if (!string.IsNullOrEmpty(regex))
            {
                try
                {
                    result = new Regex(regex);
                }
                catch (ArgumentException ex)
                {
                    throw new Exception(string.Format("Invalid pattern '{0}'.\nException message: {1}", regex, ex.Message), ex);
                }
            }

            return result;
        }
    }
}
