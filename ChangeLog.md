# Change log

V1.4
--
- Nalin:
	- Check to see if we are running on the .NET 4.5 framework.
	- Better update handling for situations where a user has both the Steam version and the standard version installed.
	- Fixed creation of registry keys related to Melder One-Click Installs Meldii opens a UAC prompt to escalate its privileges.
	- Fixed a crash as reported by MadWarrior.

- Qowface 
	- Close any open flyouts when opening a new one.

- FreakByte
	- Fixed Meldii.Statics.GetFirefallInstallPath.

- Arkii:
	- Added an option to check for patch on start or not.
	- Restarts as admin when updating.
	- Updated the mod check to test if any file would override an installed file.
	- Restores backed up files to the correct locations.
	- Added option to launch Firefall with Steam.
	- Added option to go to an addons installed folder on right click.


V1.3
--
- Nalin:
	- Fixed a crash if the Firefall patch servers where offline.
	- Firefall Update Handling, If a new Firefall version is detected, prompt the user to open the launcher and download it.
	- Made crash logs more informative.
	- Remove mods if we launch the patcher to update.
	
- Added the ability to change the theme used in Meldii.
- Not supported addons no longer count towards the number of addons to update display.
- Fixed an issue where a mod could uninstall default UI components, ooops.
- Remove commands are no longer ignored.

V1.2
--
- Nalin: Added direct download support for addons.
- Varixai: Fixed some typos.

V1.1
--
- Nalin: Implemented the ability to launch Firefall through Meldii.
- Nalin: Handle full URLs in the forum info generator


V1.0
--
Public Release