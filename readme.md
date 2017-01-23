Rippy
=====

A GUI for converting songs from flac to MP3 and creating private torrents for pass the headphones.

Download the latest version from [the releases page](https://github.com/flare561/Rippy/releases/)

Usage
-----

This program uses MP3 Tag and dBpoweramp, the locations for these are currently defined in settings.json

If settings.json does not exist for you, run the program once, then close it and ensure the settings are correct.

In the future, the settings will be managed in the GUI, so check back for updates.

Select a folder containing your flac files using the browse at the top right. Ensure the metadata is correct using the Set Metadata button which will open MP3 Tag in the flac folder.

Artist, Year, and Album will be filled using metadata if possible. Manually set Medium, Publisher, and Catalog Number using the textboxes, and modify artist, year and album if necessary.

Ensure that the folder preview looks correct, and select a tracker. Then select which transcodes you would like using the checkboxes at the bottom.

If flac is selected, the flac files will be copied to an output folder located in your transcodes directory and a torrent will be created for them.

When you're finished press create, and your files will be transcoded, and a torrent created. 

###Notes:

This is very much a work in progress! If you find any bugs or have any feature requests, please let me know about them [on the issues page.](https://github.com/flare561/Rippy/issues)

There are a couple outstanding issues, so check if yours is already present before creating a new issue.