# sshshellforciscoswitches
SSHShell for Cisco Switches


This repository contains the some fixes to the original SSHShell available at

https://gallery.technet.microsoft.com/scriptcenter/SSH-Module-that-use-shell-42d918d5

Fix details:
1) the original repo at above link has problems with sending the "enable" command to the switch after login where the TextWriter.WriteLine in the C# code adds '\r\n' to the end of string "enable" making the switch/router to take the '\n' as enable secret and results in "access denied"

It is fixed by changing TextWriter.WriteLine to TextWriter.Write with "enable" followed by '\r'.

Usage/Installation:

1) if planning to use the SshShell module here in a powershell script then just download and put in Powershell\Modules folder

for e.g C:\Program Files\WindowsPowershell\Modules\SshShell

And the SshShell module can be imported in powershell as below (taken from above link)

Import-Module SshShell 
 
$elevatedPrompt = "#.$" 

$configPrompt = "\(config\)#.$" 

$objectPrompt = "object\)#.$" 
 
$s = New-SshSession -SshHost $asaIP -User $user -Password $password 

Send-SshCommand $s "enable" -Expect "Password:" 

Send-SshCommand $s "$elevatedPassword" -Expect $elevatedPrompt 
 
Send-SshCommand $s "show run object id $objectId" -Expect $elevatedPrompt 
     
if ($s.LastResult -match "does not exist") { 
 
 Send-SshCommand $s "conf t" -Expect $configPrompt 
 
 Send-SshCommand $s "object network $objectId" -Expect $objectPrompt 
 
 Send-SshCommand $s "description $description" -Expect $objectPrompt 
 
 Send-SshCommand $s "host $hostIP" -Expect $objectPrompt 
 
 Send-SshCommand $s "end" -Expect $elevatedPrompt 
  
  Send-SshCommand $s "write mem" -Expect "[OK]" -WaitUnlimitedOn "configuration\.\.\.|Cryptochecksum|copied" 

} 

 
Close-SshSession $s

2) It is written in Programming Language: C#

if you are planning to modify or rebuild the repo present in Source directory:

after building the repository, C60.SshShellPowerShellModule.dll is generated in obj/debug which needs to be copied to for e.g C:\Program 

Files\WindowsPowershell\Modules\SshShell folder and restart the powershell IDE/ISE for the changes to take effect

