# DC Lethal Company Mod
This is a mod I created for my friends, it includes a few different tweaks.


In short, custom videos are in the `TV_Videos` folder in the mod's folder (the folder containing the DLL).

Custom songs for the boombox are in the `AudioClips` and `Boombox_AudioClips` folder. **The `Boombox_AudioClips` are only played on the boombox**.\
Custom songs for the record player are in the `AudioClips` and `RecordPlayer_AudioClips` folder. **The `RecordPlayer_AudioClips` are only played on the record player**.\
Audio files in the `AudioClips` folder are played on **both** the boombox and record player.

Only one video format is currently supported: `*.mp4` (well, I don't know if it's just that format, but I only load in files that end with `*.mp4`, so I don't know, take a chance).

Here's the supported audio formats (and their file extension):
- MPEG (`.mp3`)
- WAV (`.wav`)
- OGGVORBIS (`.ogg`)
- AIFF (`.aif` OR `.aiff`)


## Known issues:
1) If you miss the quota you will retain your items but ONLY in game OR until the game auto saves (you play one more round).


---



## (Missing) Quota Tweaks
The days to meet the quota are quite short, and I suffer from "skill issues." So ...

### Adjusting the days to reach the quota
Use the `DaysToMeetQuota` setting to set the number of days you have to reach the quota.\
Cannot be less than 1. Can be as high as you want, just note The Company only buys at a 100% rate on the final day...

### Not loosing your items after missing the quota
**BUG NOTE:** while you _will_ keep your items, it doesn't save them correctly. So, if you miss the quota you will retain your items but ONLY in game OR until the game auto saves (you play one more round). **You have been warned.**

You can use these settings to keep your ship purchases after you miss the quota. The do what they sound like they do.\
`ResetSuits`: whether your suits are removed or not.\
`ResetFurniturePurchases`: whether the cosmetic items are removed or not (tables, tv, record player, fishie).\
`ResetShipUpgrades`: whether the teleporter, reverse teleporter or loud horn are removed.\
`ResetFurniturePositions`: whether the cosmetic ship item (and upgrades) positions are reset. If true, purchases items (things that are not defaults) are placed into storage unless you also set them to be removed.\
`DoNotResetDefaultFurniturePlacements`: if the default furniture items (terminal, beds, cabinet, closet) positions are reset.


---

## Everyone Died Tweaks
Okay, your team sucks (skill issue). Well, now you can:

### Keeping your scrap after everyone dies
Use `ScrapProtection` to adjust what happens when everyone dies:
- `None` if you want to loose all your scrap if everyone dies
- `RandomChance` for some of the scrap to randomly disappear (dependent on `ScrapProtectionChance`)
- `All` to keep all scrap


If you select `RandomChance` for your `ScrapProtection`, then `ScrapProtectionChance` is the % chance that a given scrap will be kept.\
That is, if you have 5 scrap, `ScrapProtectionChance` is `.5` and everyone dies, each scrap _individually_ has a 50% chance of being kept.\
Use `1` for a 100% chance (you keep everything), `0` for a 0% chance (you loose everything), or something in between.

If you want some kind of penalty for scrap you keep after everyone dies, use `ScrapValueModifierForAllPlayersDead` to specify the percentage change in scrap that is kept.\
Setting `.75` for this means all scrap keeps 75% of it's value and keeps 25% of it.\
You cannot set a value greater than `1` (an increase of scrap value) or lower than `0` (below 0 would just flip the value between a negative and non-negative value.).



### \[Planned\] Reducing the token deductions from dead players
This has not been implemented yet :/.


---


## Video Tweaks
You can play custom videos on the television! Place `.mp4` videos into the `TV_Videos` folder next to the mod's DLL file and they'll be automatically loaded in on game start.

You do have to enable custom videos in the configuration. Under `Video` you'll see the config option `TelevisionVideoMode`, set this to `OnlyCustomVideos` to enable custom videos. `NoCustomVideos` will use the game's videos.\
If you enable custom videos but do not have any custom videos to play, the TV will not turn on.


## Audio Tweaks
Sometimes the custom videos/songs you add are extremely loud. Luckily you can turn the volume down with the `-` key and up with the `+` key.\
This will only update the volume of the item you're looking at, unless you hold **right alt** at the same time (which updates _all_ item's volume).\

All keys are customizable via the configuration file. See a list of valid keys here: https://docs.unity3d.com/Packages/com.unity.inputsystem@0.2/api/UnityEngine.InputSystem.Key.html or in the code [here](DCMod/KeyToString.cs).

You can adjust the maximum volume `MaxVolume` between 0 (off) and 1 (full volume). (0.5 is half, 0.25 for quarter and so on).\
Use `DefaultVolume` to adjust the startup volume of all video/music emitters.\
`VolumeIncrements` is used to adjust how much the volume changes when you press the volume up/down key.


You can play custom songs on the boombox and record player!\
There's three "modes" to the custom music for both the record player and boombox:
- `NoCustomMusic`: only the default game music is used
- `InterweaveCustomAudio`: both the game's music and your custom songs are played.
- `OnlyCustomAudio`: no game music is used, only your custom songs are played.

These are set via the `RecordPlayerAudioMode` and `BoomboxAudioMode` respectively.\
You can allow the boombox to keep playing music even if you pocket it (change items) via the `StopBoomboxIfPocketed` config option.

Lastly, you can choose whether the songs are stream from disk or not via `SteamCustomAudioFromStorage`. It _might_ improve performance, but it will limit playback capabilities (the same song cannot be played twice).


---


# Building
If you want to build this mod yourself, all you have to do is clone the repo, run `Set game path.ps1` and open+build the solution.\
The `Set game path.ps1` script will search/ask for the Lethal Company game files path. This way the project can load in the dependencies correctly as well as copy the mod to your plugins folder after being built.