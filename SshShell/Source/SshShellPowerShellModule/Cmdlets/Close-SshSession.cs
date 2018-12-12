namespace C60.SshShellPowerShellModule.Cmdlets
{
    using System.Management.Automation;

    [Cmdlet(VerbsCommon.Close, "SshSession")]
    public class CloseSshSession : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [ValidateNotNull]
        [Alias("s")]
        public SshShell SshShell { get; set; }

        protected override void BeginProcessing()
        {
            this.SshShell.Close();
        }
    }
}
