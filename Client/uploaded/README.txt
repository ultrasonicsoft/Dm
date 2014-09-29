Note: all xml file need to have write permission.
1. filelist.xml
- put your OCH user/password: <ftpuser>[your username]</ftpuser><ftppass>[your password]</ftppass>
- specify the locations that you will look up the files to upload: 
Ex. <location name="Music">/var/www/uploaded/files/music</location>
+ name is the title which will be displayed on the tab for that location
+ you can specify as many locations as you want (Note: you have to set write permission for all files and folders in these location, include new folders)
That's all configurations with filelist.xml. When you run the app, filelist.xml will list all the folders to upload and the files in these folders. 
The uploaded folder will be stored in finish.xml.
If you want to start from beginning, just delete these two files, and run the app. It will generate a filelist.xml and you're back to step 1.

2. config.php
- you put the cookie here.

3. categories.xml
- put the categories in <categories></categories> tags. For ex:

<?xml version="1.0" encoding="ISO-8859-1"?>

<categories>

  <types name="Movies">

    <item>Comedy</item>

    <item>Thriller</item>

  </types>
  
  <types name="Musics">

    <item>Pop</item>

  </types>

</categories>

4. informations.xml
- put the informations in <informations></informations> tags. For ex:

<?xml version="1.0" encoding="ISO-8859-1"?>

<informations>

  <item>720 HD</item>

  <item>1080 HD</item>

</informations>
