## Introduction

kclog is key logger, intended to track the number of keys pressed per day. It tries not to be a malicious keylogger, but it *is* a keylogger, meaning it can log sensitive information, including passwords, and so it not recommended to run in any circumstances where such information may be available.

kclog does attempt to mitigate key logging risk - the timing of keys is logged with a minute precision and saved to the database in a random order in each timing event.

## Usage

Run kclog.exe, which will immediately start listening for keyboard and mouse events. Events are stored in memory and the saved to an SQLite database (specified by `DbPath`) every `PulseTime` milliseconds.

## Download

Compiled downloads are not available.

## Compiling

To clone and run this application, you'll need [Git](https://git-scm.com) and [.NET](https://dotnet.microsoft.com/) installed on your computer. From your command line:

```
# Clone this repository
$ git clone https://github.com/btigi/mmp

# Go into the repository
$ cd src

# Build  the app
$ dotnet build
```

## Configuration

Configuration values are stored in appsettings.json

  `DbPath` - The full path to the database file, e.g. C:\keys.db

  `DbPassword` - The password for encryption of the database

  `PulseTime` - The number of milliseconds between save events, e.g 30000
  
## Additional

This repository includes a copy of the GlobalHooks [repository](https://github.com/Indieteur/GlobalHooks) upgraded to NET8.

## Licencing

- kclog is licenced under CC BY-NC-ND 4.0 https://creativecommons.org/licenses/by-nc/4.0/ Full licence details are available in licence.md
- kclog relies on code by [Indieteur](https://github.com/Indieteur/GlobalHooks) under the [CC0 1.0 Universal licence](https://creativecommons.org/publicdomain/zero/1.0/)
- kclog uses [SQLitePCL.raw](https://github.com/ericsink/SQLitePCL.raw) under the Apache 2.0 licence, for SQLite database encryption.