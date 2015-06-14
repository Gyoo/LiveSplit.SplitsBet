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

##v0.4

###Fixes

- Fixed error when there is no highscore in the middle of a run
- Fixed bugs with the scores when unsplitting then resplitting
- Changed settings management to prevent settings from being completely reset from a version to another
- Fixed a bug related to loading new splits without reloading SplitsBet

###Features

- Added `!version` to see which version is running
- Added setting to choose which comparison is shown in the chat at the beginning of a split (either Best segment, average segment, best split times segment, PB segment, or none)
- Added setting to set a delay between your split and the messages showing in the chat. This is done to prevent spoiling the run because of the stream delay


###Misc
- Refactoring of some code (you won't see that as a user)
- Changed the phrasing of the bot
- Changed the time coefficient formula, because users felt like it's more rewarding to bet fast than to bet accurate. From now on, the coef will start decreasing at around 30% of the time of the best segment, and at the latest (aka 75% of the time of the best segment) the coef will be around 0.5, which means you'll only earn 50% of what you would've earned if you bet at the beginning.

###Known Bugs

- If your segment time is shorter than the delay you set, SplitsBet will be completely rekt until you reset your run. This is mostly because the current method used for the delay is a bit dirty, but hey it does the job. This will be refactored at some point.
