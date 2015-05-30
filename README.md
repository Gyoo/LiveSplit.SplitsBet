# LiveSplit.SplitsBet [![Build Status](https://travis-ci.org/Gyoo/LiveSplit.SplitsBet.svg?branch=master)](https://travis-ci.org/Gyoo/LiveSplit.SplitsBet)

More fun for your viewers !

## Summary

SplitsBet is a plugin that adds a small bot to your chat (under your name). The goal of this bot is to allow viewers to guess what the time of your segments will be.
The closer they are to reality, the more points !

## How does it work ?

### Commands

- !start __(Broadcaster only)__ : Starts the bot. Viewers can bet whenever a message inviting them to do so appears
- !stop __(Broadcaster only)__ : Stops the bot. Viewers can't bet anymore.
- !bet (hh:m)m:ss : Guess the time for the current split. Hours and first digit of minutes are optional (Example : 4:20 is a valid time). You can't change your bet after you set it.
- !unbet : Cancels your bet for the current split. Watch out, there's a points penalty !
- !checkbet : Verify your bet
- !betcommands : Shows the available commands
- !score : Shows your score during a run
- !highscore : Shows the current highest score

### Score system

The current score system works so :

- The closer you are to the time of the segment, the more points
- The amount of points you get is multiplicated by a coefficient (from 0 to 1) that decreases as the time goes on : Obviously, it'll be easier to guess a split if you bet near the end of it, thus you will have less points than someone who guessed it right at the very beginning of it.

These 2 parameters are managed with gaussians, which is, to me, the fairest way to calculate the score : it won't decrease too much if you're close enough, but will decrease kinda fast when you start being too far.
However, the scoring system is most likely to be changed sooner or later

### Long term

When the plugin is stable enough, I want to go to the next level by offering a web platform where all the scores will be centralized, for every stream that uses SplitsBet. This site will show stats and ranks for each broadcaster
and player, and possibly more !

## Download

Check the [releases page](https://github.com/Gyoo/LiveSplit.SplitsBet/releases) to get the latest version or take the risk and download a [development build](https://fezmod.tk/files/travis/splitsbet/) !

### Install plugin

Once you downloaded the plugin, unzip it in <LiveSplit folder>/Components
Then, add it to your layout : Edit Layout/"+"/Control/Splits Bet Bot
A window will show, asking for your Twitch credentials, fill it and it's good to go ! Don't forget to type !start in the chat otherwise the bets will be disabled :)
