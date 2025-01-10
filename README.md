# mARble-run

### The Authors:
Soroush Jahanzad, Edoardo Negri, Cyril Pomsar, Aaron Zürcher

Supervisor: Marcel Lancelle

### The Game:
Build intricate marble tracks in augmented reality, where your imagination knows no bounds. Watch as your marbles race through your designs, and battle for the first place.

### Report and Demo
<a href="https://github.com/EdoardoNegri/mARble-run/blob/edo/Final_Report.pdf">Report</a> | <a href="https://github.com/EdoardoNegri/mARble-run/blob/edo/Poster.pdf">Poster</a> | <a href="https://drive.google.com/file/d/1Ig_ZecCn03cI8onOf7Lej67ki8dpwyM1/view?usp=sharing">Demo</a>

(Fix Report and Demo Links)
### Description:
This project implements an interactive mixed-reality marble game designed for the Magic Leap 2 device. Users can create and manipulate virtual marble tracks in the air using the Magic Leap controller, offering a fun and immersive experience.

Key Features:
* Track Creation: Draw virtual tracks in 3D space with the controller, allowing marbles to run down dynamically created paths.
* Track Customization: Place special pre-made blocks like funnels, loopings, and other obstacles to enhance the marble's journey.
* Editing & Deleting: Easily modify or delete parts of the track in real-time, providing flexibility for creative play.
* Immersive Interaction: Use the Magic Leap 2's spatial awareness and controller gestures to interact with the track and marbles in an intuitive mixed-reality environment.

This project showcases the capabilities of spatial computing and brings a playful twist to mixed-reality experiences.


### Installation
First Install <a href="https://unity.com">Unity Hub</a> and open this repository as a unity project. (Unity Version 2022.3.48f1 was used)

For setting up the MagicLeap2 device with OpenXR in Unity please refer to either the official MagicLeap <a href="https://developer-docs.magicleap.cloud/docs/guides/unity-openxr/getting-started/openxr-unity-getting-started/#">documentation</a> or <a href="https://moodle-app2.let.ethz.ch/pluginfile.php/2109064/mod_resource/content/3/unty_tutorial.pdf">this guide</a> by Jonas Hein

Inside the project open the MainScene in the Project Explorer (Assets/Scenes/MainScene)

Then go to File->Build Settings...

Make sure to select: Android as "Platform, the MainScene in "Scenes in Build" and the Magic Leap device on "Run Device" then deploy with "Build and Run"









