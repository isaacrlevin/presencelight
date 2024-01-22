### What is the best version to install?
It really depends on your workflow, for normal users, I would use the Microsoft Store as it least barrier to entry.

### How do I use my lights after installing PresenceLight?
PresenceLight "polls" Graph and Windows Theme data until you tell it not to. The easiest way to do this is either shutdown the app, or do a one-time sync to a custom color, which should stop any polling.

### Where is X light?
I would love PresenceLight to support EVERY smart light on the market, but I do not have the hardware to do that, I simply wrote a tool with the HW I have. If you want to add your own Smart Light brand, a PR is the fastest way. If you do, please follow the [Contributors Guide](CONTRIBUTING.md)

### This only runs on Windows, lame....
I am currently working on a cross-platform version using ASP.NET Core Blazor and Workers. I have been testing it on WSL2 as well as a RaspberryPi I have at home and it seems to be working well. I will release more info about that [here](https://github.com/isaacrlevin/PresenceLight/blob/master/worker-README.md)

