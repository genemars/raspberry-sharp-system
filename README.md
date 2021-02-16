Raspberry# System
=================

See the **[Raspberry\# System Wiki](raspberry-sharp-system/wiki)** for full documentation and samples.

Introduction
------------
Raspberry# System is a .NET/Mono Library for Raspberry Pi. This project is an initiative of the [Raspberry#](http://www.raspberry-sharp.org) Community.

Current release is an early public release. Some features may not have been extensively tested.
Raspberry# System currently supports high resolution timer, as well as Raspberry Pi system board identification.

Features
--------

### Raspberry.System
Raspberry.System provides utilities for Raspberry Pi boards, while using .NET concepts, syntax and case.
You can easily add a reference to it in your Visual Studio projects using the **[Raspberry.System Nuget](https://www.nuget.org/packages/Raspberry.System)**.

It currently support the following features:
+ Automatic detection of various Raspberry Pi revisions, as of December 2020, **Raspberry Pi model 1 through 4 hardware (including compute modules and zero)**
+ High resolution (about 1µs) timers


Compile
-------
+ Clone the code to your system:  sudo git clone https://github.com/bkenobi/raspberry-sharp-system.git
+ change owner/group to pi (or whatever you log on as):  sudo chown -R pi **development directory**;  sudo chgrp -R pi **development directory**
+ install mono-devel:  sudo apt install mono-devel
+ Compile Test.Board:  msbuild Test.Board.csproj
+ run Test.Board.exe to confirm the code runs:  mono Test.Board.exe
