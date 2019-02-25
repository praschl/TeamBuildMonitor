# TeamBuildMonitor
## Motivation
When having a lot of TeamProjects in your TFS, it would be nice to have an overview of past and running builds over all TeamProjects.
However the TFS UI only shows one TeamProject at a time, making it tedious to check all builds of (e.g.) the last 12 hours.
Also, notification when a build starts or stops would be nice, but theres nothing in TFS that does that by default.

## Solution
TeamBuildMonitor is a WPF application which runs in the system tray on windows. 
It monitors past and running builds on the TFS server over all TeamProjects and displays them in a nice and filterable list.
When a build starts or changes its status, you will also get a small notification in the bottom right of the screen.
You can filter Buildname, TeamProject name, who requested the build, and how old it is.

[TODO: Image]

## Issue
Only the Xaml-Builds are currently supported.
