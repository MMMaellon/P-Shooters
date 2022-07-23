# MMMaellon's P-shooters

This is a prefab that allows anyone to easily and quickly set up particle shooter (P-shooter) guns for VRChat worlds.

It provides a very opinonated solution out of the box because it was built with setup speed in mind.
However, it also leaves a lot of room for customization and it should be easy to integrate new types of guns, new game mechanics, and new features like scoreboards and spectator cameras.
This prefab was designed to help build games with instantaneous audio and visual feedback and responsive controls, two things that are often missing from VRChat games.
I implore you to also build your game with those goals in mind.
If you need help with any of this, contact me at @MMMaellon on Twitter or MMMaellon#2053 on Discord.

## Setup Instructions

0) Download the latest unitypackage here: <https://github.com/MMMaellon/P-Shooters/releases>
1) Navigate to the '__PREFABS__' folder and drag the prefab called '__GUN MANAGER__' into your scene.
2) Read the instructions on the inspector of the '__Gun Manager__' object.
3) Repeatedly and passionately mash the setup buttons on the '__Gun Manager__' object any time you make a change to anything.

## Dependencies

This prefab relies on UdonSharp and CyanPlayerObjectPool (included in the unitypackage)
I used VRChat's Creator Companion to create all my projects, so you'll need the version of UdonSharp that the creator companion gives you

## Notes

- The example scene references 3d models that weren't included in the unity package, so it may not render correctly
- I left room in the code to add upgrades to weapons and players, but did not implement any yet

## CREDITS

FakeLight and Filamented Shaders from Silent (@Silent0264 on Twitter)
Scope Shader from migero <https://migero.booth.pm/>
CyanPlayerObjectPool from CyanLaser (@CyanLaser on Twitter)
Sound Effects from FreeSound.org and JetDog8808 (@jetdog8808_dev on Twitter)
Temp Gun Model from Smooth_Octopus (@nataliedesign on Twitter)
