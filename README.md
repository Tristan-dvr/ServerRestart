## What the mod do?
The mod allows you to set a schedule for restarting your server and turn it off in a way that is convenient for you.
It safely disconnects all players and saves the world before stopping the server.

## How to launch server again?
If you use hosting services, then it is almost certain that hosting will start your server on its own as soon as it shuts down. If you keep the server on your own computer, you can start the server using a third-party program. 

## ServerCharacters
If you use ServerCharacters on your server, the mod can independently start maintenance mode a few minutes before the scheduled restart.
Keep in mind that the maintenance time will need to be configured for both mods.

You also can disable shut down option and restart the server with hosting services or your own.

## Console commands
These commands work only on the server. You can run them with mods that give remote access to the server console, for example [ValheimRcon](https://thunderstore.io/c/valheim/p/Tristan/ValheimRcon/) (`consoleCommand schedulerestart 600`) or [DiscordBot](https://thunderstore.io/c/valheim/p/RustyMods/DiscordBot/) (run commands from Discord).
- `schedulerestart <seconds>` — schedules a server restart the given number of seconds from now.
- `schedulerestart` — resets the restart to the next time from the configured schedule.

Commands are ignored while a restart is already in progress.

### Thanks for contributing
_madbomg_

# Contacts
If you have any questions / bug reports / suggestions for improvement or found incompatibility with another mod, feel free to contact me in discord `typedeff` or on GitHub
