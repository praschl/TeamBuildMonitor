# TeamBuildMonitor
## Motivation
When having a lot of TeamProjects in your TFS, it would be nice to have an overview of past and running builds over all TeamProjects.
However the TFS UI only shows one TeamProject at a time, making it tedious to check all builds of (e.g.) the last 12 hours.

## Solution
TeamBuildMonitor is a WPF application which runs in the system tray on windows. 
It monitors past and running builds on the TFS server over all TeamProjects and displays them in a nice and filterable list.
You can filter Buildname, TeamProject name, who requested the build, and how old it is.

<TODO> Image

## Issue
Only the Xaml-Builds are currently supported.
