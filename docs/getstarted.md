# Get Started

Download the current version from the table below:
| Tool | System | Download | 
| --- | --- | --- | 
| Command Line Tool | Windows | [busgs_cli_0.0.3_beta.zip](busgs_cli_0.0.3_beta.zip) | 
| GUI Tool | Windows | [busgs_uwp_0.0.3_beta.zip](busgs_uwp_0.0.3_beta.zip) | 

The archives contain a directory `busgs` with all files required
for execution. Unzip the archive to a folder on your computer from where
you wish to start the program.

<!-- TODO: add web-assembly link -->
Or use the web-assembly under: [busgs-web]()

> _Note_: Currently the web-assembly app only works with _Chrome_ based browsers 
> such as _Google Chrome_ or _Microsoft Edge_. _FireFox_ technically also
> works but with some strange behaviors.

## Command Line Tool

You can use _PowerShell_ or _Command Prompt_ (or any other terminal) to execute 
the program.
Change into the folder where the `Busgs.exe` is located. 
You can now execute by 
```powershell
.\Busgs -h  # PowerShell
Busgs -h    # Command Prompt
```
This prints the help screen.

### Alias for PowerShell

Best practice for easy use is to set an alias. 
This way the tool can be executed from any folder.
In _PowerShell_ call the command:
```powershell
Set-Alias -Name Busgs C:\temp\busgs\Busgs.exe
```
Make sure to change the file path to where `Busgs.exe` is located on your computer,
for this example we assumed `C:\temp\busgs\Busgs.exe`.
> __Note:__ This alias is not permanent. That means once you closed _PowerShell_ 
> you have to set the alias again.

### Permanent Alias
To make the alias _permanent_ add the command from above to your _PowerShell_ 
profile, e.g. `$PROFILE.CurrentUserCurrentHost`.
Then the alias is available each time you open _PowerShell_.
Please read the 
[online instructions](https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_profiles?view=powershell-5.1) 
on how to edit or create profiles.

For impatient readers, the short instructions are:

- Execute from _PowerShell_

        notepad $PROFILE.CurrentUserCurrentHost
    You might be asked if the file should be created, if it does not exist. Please confirm that.

- At the end of the file add a new line with the command from above.

        Set-Alias -Name Busgs C:\temp\busgs\Busgs.exe
    Save the file and close _Notepad_.


- Close and restart _PowerShell_. Now you can call the tool by `Busgs` from any 
  folder and in any new session of _PowerShell_. 
