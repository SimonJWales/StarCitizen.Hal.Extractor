# Star Citizen Hal Extractor

APP FOR DECOMPRESSING, EXTRACTING AND CONVERTING STAR CITIZEN GAME DATA FOR USE IN EXTERNAL APPLICATIONS.

THIS IS AN UNOFFICIAL STAR CITIZEN FAN SITE, THAT IS NOT AFFILIATED WITH CLOUD IMPERIUM GAMES OR ROBERTS SPACE INDUSTRIES.
ALL CONTENT IN THIS REPO NOT AUTHORED BY THE OWNER OF THE REPO IS THE PROPERTY OF THE RESPECTIVE OWNER.
```
This application is based on the efforts of Peter Dolkens (unp4k).
Full credit goes to him for creating the orignal code bases that this application utilises 
and re-implements. See below for full information about his and other inspiring Star Citizen projects.
```
## So what is it?
The Star Citizen HAL Extractor (HAL X for short) is a windows application that can extract game data from the `.p4k` game file of Star Citizen and output xml, graphic and any other data within the p4k data file, for use in external programs.

## Development Installation
* Fork this repo if you want to play around with the code (and improve it)

## General Installation (standalone version)
* Download the latest MSIX windows installer version file from: [HAL X LATEST](https://sc-hal.com/hal-x/latest)
* Run the file and install to your device.

## How to (standalone version)
* Open a Windows Command prompt, Terminal, Bash or whatever is your favourite command-line shell
* CD into the location of the `StarCitizen.Builder.exe`
* Run the exe with or without arguments (see below for which arguments are available)

## Why do we need this?
While [unp4k](https://github.com/dolkensp/unp4k) is terrific at getting the data out from the `Data.p4k` file and outputing user friendly objects, it does not seem to be updated or maintained.

I wanted to create an easy to use web app that could give me useful information about where I could buy a particular item or what items that I could put in a specific vehicle - which became [sc-hal.com](https://sc-hal.com). Without the extraction and conversion tools the website would not be able to be updated, so I created the HAL Extractor to solve my problem. Maybe it can help you too. It is one of the applications used to create [sc-hal.com](https://sc-hal.com/) and it is offered here free in the hope it helps you :)

A complete extraction  (using default XML values) can take between 2 and 10 minutes (depending on your system). Around 1 minute to extract the files from the .p4k and the rest of the time converting the binary cryXML files to readable formats.

---

# Project links
[HAL X Latest version](https://sc-hal.com/hal-x/latest)

[unp4k](https://github.com/dolkensp/unp4k)

---

![made-by-the-community-white-200](https://user-images.githubusercontent.com/44800187/210419931-50f91abd-6fc1-4135-bd62-0d84c496a5bc.png)
