Push-Location .\DickClubMod\

function New-Link {
    param(
        [string]$Destination
    )
    New-Item -Path Lib -ItemType SymbolicLink -Value $Destination | Out-Null
    Write-Host "Linked .\DickClubMod\Lib\ <---> $Destination"

    # Writes the game's path for the post-build copy script
    (Join-Path -Path $Destination -ChildPath "..\..\") | Set-Content -Path "..\GamePath.txt"
}


try {
    if (Test-Path -Path Lib -Type Container) {
        $choicePrompt = "Delete/Override the existing `"Lib`" folder?"
        $yes = New-Object System.Management.Automation.Host.ChoiceDescription "&Yes", "Override the folder"
        $no = New-Object System.Management.Automation.Host.ChoiceDescription "&No", "No, cancel, exit, get me outta here"
        $options = [System.Management.Automation.Host.ChoiceDescription[]]($yes, $no)
        $result = $host.ui.PromptForChoice("Existing Lib Folder Found", $choicePrompt, $options, 1)

        if ($result -eq 0) {
            Remove-Item Lib -Force
        } else {
            return;
        }
    }

    Write-Host ""
    Write-Host ""

    # This is an attempt to find the game files for you
    $likelyGamePath = "Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\"
    # Get all drives
    $driveLetters = Get-Volume | Where-Object {
        $null -ne $_.DriveLetter -and ($_.DriveType -eq 'Fixed' -or $_.DriveType -eq 'Removable')
    } | Select-Object -ExpandProperty DriveLetter

    foreach ($driveLetter in $driveLetters) {
        $fullPath = Join-Path -Path "$($driveLetter):\" -ChildPath $likelyGamePath
        if (Test-Path -Path $fullPath -PathType Container) { # Found on this drive
            Write-Host "Found game flies at $fullPath"

            $choicePrompt = "Is this the correct path for Lethal Company?"
            $yes = New-Object System.Management.Automation.Host.ChoiceDescription "&Yes", "Use this this path"
            $no = New-Object System.Management.Automation.Host.ChoiceDescription "&No", "Do not use this path"
            $options = [System.Management.Automation.Host.ChoiceDescription[]]($yes, $no)
            $result = $host.ui.PromptForChoice("Game Folder Found.", $choicePrompt, $options, 0)

            if ($result -eq 0) {
                Write-Host ""
                Write-Host ""
                New-Link -Destination $fullPath
                return;
            }
        }
    }

    # Browse for the game's folder
    Add-Type -AssemblyName System.Windows.Forms
    $folderDialog = New-Object System.Windows.Forms.FolderBrowserDialog
    #$folderDialog.Title = "Please select Lethal Company's 'Managed' folder inside the Data folder"
    $folderDialog.Description = "Select .\Lethal Company_Data\Managed\"
    $folderDialog.ShowNewFolderButton = $false
    $result = $folderDialog.ShowDialog()

    if ($result -eq [System.Windows.Forms.DialogResult]::OK) {
        Write-Host "Selected `"$($folderDialog.SelectedPath)`""
        # Check if the selected path ends with "Managed"
        if ($($folderDialog.SelectedPath) -notlike "*\Managed") {
            $choicePrompt = "Hmm, this isn't the `"Managed`" folder... Is this the correct path for the Lethal Company game files?"
            $yes = New-Object System.Management.Automation.Host.ChoiceDescription "&Yes", "Use this this path"
            $no = New-Object System.Management.Automation.Host.ChoiceDescription "&No", "Do not use this path"
            $options = [System.Management.Automation.Host.ChoiceDescription[]]($yes, $no)
            $result = $host.ui.PromptForChoice("Game Folder Found.", $choicePrompt, $options, 1)

            if ($result -eq 1) {
                return;
            }
        }

        $selectedFolderPath = $folderDialog.SelectedPath
        New-Link -Destination $selectedFolderPath
    } else {
        Write-Host "Canceled. You'll new to run `"mklink /D Lib <path to Lethal Company_Data\Managed\>`" in order to build."
        Write-Host "Or.. you can fix the dependency locations yourself."
    }
} finally {
    Pop-Location
}