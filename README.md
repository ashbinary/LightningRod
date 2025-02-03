# LightningRod
LightningRod is a randomizer for most aspects of Splatoon 3.

![LightningRod](https://raw.githubusercontent.com/ashbinary/LightningRod/refs/heads/main/assets/preview.png)

## IMPORTANT NOTE
**I am NOT responsible for any bans caused by LightningRod. This is meant to be used in an environment disconnected from Nintendo's servers, and using it online will almost surely cause a ban. By downloading this, you, the user, are taking full responsibilty if banned.**

**Due to the hyper-softcoded nature of this program, stability is not guaranteed. In the case of a crash, simply re-randomize the game, and maybe toggle off a few of the more severe options.**

## Requirements
- A dump of the game, in either a dumped romFS, .XCI, or .NSP format.
- A working prod.keys (usually on the latest version of the Switch OS).
- [.NET Runtime 9.0.1](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
  
## Usage
The prod.keys file should be placed in the `C:/Users/[username]/.switch` folder. (This is not required if you are using a romFS dump of the game to load it.)

Load in the .NSPs/.XCIs/romFS dumps on the left side of the program. Once the basegame is loaded in, the program will unlock the available options.

Configurations can be saved and shared between others, and will keep the randomizer's seed, which can be used to generate the exact randomization while not having to transfer files to others.

Pressing `Load Game Data` will setup all the files you've currently inputted. Inputting any new files (except configurations) will not be used unless you press it again.

When ready, press the `Randomize + Save` button. The user will be prompted with a directory to save the data in, and when the directory is saved, the program will 'stop responding'. It is simply working on generating the game's files. When the LightningRod.log file appears in the directory, the game has completed generating and the files can be used as an Atmosphère mod.

## Credits

[Shadów](https://x.com/shadowninja108) - Created the BYML library used in the program, and was the biggest inspiration in completing the project as a whole.

[AeonSake](https://aeonsake.com/) - Created the SARC & MSBT libraries used in LightningRod.

[DiamCreeper23](https://bsky.app/profile/diam.bsky.social) - Main pre-release playtester.
