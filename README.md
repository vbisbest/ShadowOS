# ShadowOS
Android application security tool

Testing mobile applications for security vulnerabilities is a difficult job.  There are many target areas to analyze and much of it is manual.   ShadowOS is a tool to help security testers evaluate mobile applications for vulnerabilities.   It is a custom created Android OS that intercepts events and displays them in a console. The captured events are relevant to a mobile pen test and makes in depth app analysis easier.  

The code modifications are located in the Libraries and Runtime areas of the OS.  This gives ShadowOS the ability to capture information without affecting the application or need for wrappers/injections.
![Console Capture](Images/aos.png)

ShadowOS captures the following application events:

* File System - All reads and writes to the file system
* Local Database Access - select, insert, update statements and parameters
* Device Internet Requests - This includes HTTP and HTTPS traffic.  Since this is a custom OS, the HTTPS traffic is captured before encryption.

Another advantage with ShadowOS is that it will defeat any kind of root detection since the OS appears to be a valid Android OS.

# Emulator
ShadowOS is runs in an emulator for quick and easy testing of applications. The image is based on Android 9.0 Pie.

# Output
ShadowOS logs all events to the adb console.   Each event will have a tag of "ShadowOS" and the type of event with relevant information. Below you can see how events where captured while exercising the OWASP GoatDroid application.

![Console Capture](Images/shadowos2.png)

Tools can also be created for viewing the data.  ShadowOS Monitor is an example.  Double click on events to open files from the device or open a SQLite databse.  See ShadowOSMonitor folder for details. (Here you can see we discovered non-encrypted username and password stored on the device)
![Tool Capture](Images/shadowos3.png)

# Usage
To implement ShadowOS yourself, you will need to pull down AOSP version 9.0.  Instructions can be found here https://source.android.com/setup/build/building

Once the OS is pulled down, you can overwrite the source files from the "Modified Files" folder.  Here are the paths:

* working_directory/external/okhttp/okhttp/src/main/java/com/squareup/okhttp/internal/http/HttpEngine.java
* working_directory⁩/libcore/⁨ojluni/src/⁨main/⁨java/⁨java/⁨io⁩/fileoutputstream.java
* working_directory⁩/libcore/⁨ojluni/src/⁨main/⁨java/⁨java/⁨io⁩/fileinputstream.java
* working_directory/frameworks/base/core/java/android/database/sqlite/SQLiteDatabase.java

Once you have ShadowOS built, you can use logcat to see the captured application events.  Here is a command to filter on ShadowOS events:
* ./adb logcat *:s "ShadowOS"

# To-Do
* Create precompiled ShadowOS image for download.   
* Look for more areas to capture application activity and behavior
* Expand on ShadowOS Monitor with more features
