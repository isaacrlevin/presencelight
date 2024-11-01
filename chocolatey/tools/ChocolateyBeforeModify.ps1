# Make sure to kill any presencelight processes before attempting an
# uninstall or upgrade of PresenceLight
Get-Process presencelight* -ErrorAction SilentlyContinue | Stop-Process