# LiveSplit.SplitsBet [![Build Status](https://travis-ci.org/Gyoo/LiveSplit.SplitsBet.svg?branch=master)](https://travis-ci.org/Gyoo/LiveSplit.SplitsBet) [![Stories in Ready](https://badge.waffle.io/Gyoo/LiveSplit.SplitsBet.png?label=ready&title=Ready)](https://waffle.io/Gyoo/LiveSplit.SplitsBet)

More fun for your viewers!

Made by [@GyooRunsStuff](https://twitter.com/GyooRunsStuff)

Many thanks to [@CryZe107](https://twitter.com/CryZe107) and [@0x0ade](https://twitter.com/0x0ade)!

## Summary

SplitsBet is a plugin that adds a small bot to your chat (under your name). The goal of this bot is to allow viewers to guess what the time of your segments will be.
The closer they are to reality, the more points!

## How does it work?

### Commands

- `!start` __(Broadcaster only)__ Starts the bot. Viewers can bet whenever a message inviting them to do so appears. Moderators can be allowed to use it if the option is set in the settings.
- `!stop` __(Broadcaster only)__ Stops the bot. Viewers can't bet anymore. Moderators can be allowed to use it if the option is set in the settings.
- `!bet (hh:m)m:ss` Guess the time for the current split. Hours and first digit of minutes are optional (Example: 4:20 is a valid time).
- `!unbet` Cancels your bet for the current split. Watch out, there's a points penalty!
- __Special bets__: They are used for mini events in the middle of a run, for instance the Secret Slide in Mario 64 or Dampe's time attack in OoT. 3 subcommands are available :
  - `!specialbet start` __(Broadcaster/Mods only)__ Starts a special bet. It can be done anytime as long as the run is already started
  - `!specialbet (hh:m)m:ss` Guess the time for the special bet. You can't cancel a special bet
  - `!specialbet stop (hh:m)m:ss` __(Broadcaster/Mods only)__ Ends the special bet. Can be done anytime as long as the run is still going. Yhen you type this, you must input manually the time that has been done (i.e you do Dampe in 47 seconds, then you must type `!specialbet stop 47`
- `!checkbet` Verify your bet
- `!betcommands` Shows the available commands
- `!score` Shows your score during a run
- `!highscore` Shows the current highest score
- `!version` Shows the current version of SplitsBet in use

### Score system

The current score system works so:

- The closer you are to the time of the segment, the more points
- The amount of points you get is multiplicated by a coefficient (from 0 to 1) that decreases as the time goes on: Obviously, it'll be easier to guess a split if you bet near the end of it, thus you will have less points than someone who guessed it right at the very beginning of it.

These 2 parameters are managed with gaussians, which is, to me, the fairest way to calculate the score: it won't decrease too much if you're close enough, but will decrease kinda fast when you start being too far.
However, the scoring system is most likely to be changed sooner or later

### Long term

When the plugin is stable enough, I want to go to the next level by offering a web platform where all the scores will be centralized, for every stream that uses SplitsBet. This site will show stats and ranks for each broadcaster
and player, and possibly more!

## Download

Check the [releases page](https://github.com/Gyoo/LiveSplit.SplitsBet/releases) to get the latest version or take the risk and download a [development build](https://fezmod.tk/files/travis/splitsbet/)!

### Install plugin

Once you downloaded the plugin, unzip it in `"path/to/LiveSplit"/Components`

Then, add it to your layout:
```
Edit Layout ➞ "+" ➞ Control ➞ Splits Bet Bot
```
A window will show, asking for your Twitch credentials (__WARNING__: If you already linked your Twitch credentials to Livesplit, this window will not appear. The bot is ready to use), fill it and it's good to go! Don't forget to type !start in the chat otherwise the bets will be disabled. :smiley:

# Changelog

See complete changelog [here](https://gist.github.com/Gyoo/5ea00ea18a26419731fe)

## v0.5.2

### Fixes

- Disabled Splits Selection because it was making a mess.
- Fixed !unbet, one and for all
- Fixed delay shenanigans caused by Splits Selection

## v0.5.1

### Fixes

- Fixed critical misbehaviour when trying to stop or remove SplitsBet.

## v0.5

### Fixes

- Delay is now working completely
- Fixed !unbet, got buggy since last update
- Fixed wrong behaviour with the run starting offset
 
### Changes

- SplitsBet is now enabled by default on startup (no need to type !start anymore)
- Changed the scoring system slightly: Now, you will get points if you are in a 15% range around the segment time, no matter the length of the segment.

### Features

- Added Subsplits compatibility : if Subsplits is part of your layout, you can choose to enable bets only for parent splits or for every single split
- Added messages customization. Only the messages that don't designate a user are available for customization right now.
- Added Splits Selection : If you don't want people to bet on a certain split, unselect it in the settings. Currently buggy with SubSplits "parent splits only" feature.

### Known bugs

- As said just above, you cannot use Splits Selection and SubSplits at the same time because it's buggy. Only backup solution is to unselect subsplits in the splits selector (but it might not exactly be the behaviour you want). **Help greatly appreciated to fix that.**
