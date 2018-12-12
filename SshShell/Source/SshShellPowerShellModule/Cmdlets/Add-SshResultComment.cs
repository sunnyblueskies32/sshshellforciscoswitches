namespace C60.SshShellPowerShellModule.Cmdlets
{
    using System.Management.Automation;

    [Cmdlet(VerbsCommon.Add, "SshResultComment")]
    public class AddSshResultComment : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNull]
        [Alias("s")]
        public SshShell SshShell { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        [Alias("c")]
        public string Comment { get; set; }

        protected override void ProcessRecord()
        {
            this.SshShell.AddResultComment(this.Comment);
        }
    }
}
