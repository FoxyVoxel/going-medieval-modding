## Creating and exporting assets 

### Unity Editor and project setup
* Download the Unity Editor version as specified [here](https://github.com/FoxyVoxel/going-medieval-modding/blob/main/ProjectSettings/ProjectVersion.txt)
    1. (Make sure you install Unity Windows Build Support)
* Clone [this](https://github.com/FoxyVoxel/going-medieval-modding.git) project
* Add project to Unity Hub and open it

### Making Your Asset Build
* After Unity loads, you should see the AddressableBuilder window. If not, you can open it manually from the “Going Medieval”  menu.
* Click the **Create New** button
* In the following popup give your mod a name and click **Create**

![alt_text](https://github.com/FoxyVoxel/going-medieval-modding/blob/main/ReadmeImages/image1.png "Creating new mod")

* You should be able to see your mod in the **Project Window **now, including empty folders. It’s advised that you don’t edit this directory manually.  

![alt_text](https://github.com/FoxyVoxel/going-medieval-modding/blob/main/ReadmeImages/image13.png "Project Window")

* Now you can copy assets to correct folders. 
    * .fbx files to **Mesh** 
    * Texture .png files to **Texture**
    * .png files used for UI to **Sprite**. For those, there’s an extra step. Set **Texture Type** to **Sprite (2D and UI)** and apply changes.
* Going Medieval uses **TMP Sprite Assets** to display icons in line with text. Open the “Going Medieval>TMP SpriteAsset Creator” window to create it. Select Mod Root Folder - in our example, that's “Assets/Mods/FVMod”; this selects all sprites from the **Sprite** folder.
* The window is now populated with a list. Modify the selection if needed and click **Create TMP SpriteAssets**.


![alt_text](https://github.com/FoxyVoxel/going-medieval-modding/blob/main/ReadmeImages/image11.png "Creating TextMeshPro SpriteAssets")

* Back in the **AddressableBuilder** window, make sure your mod is selected and click on the **Create Addressables** button.

![alt_text](https://github.com/FoxyVoxel/going-medieval-modding/blob/main/ReadmeImages/image3.png "Addressable Build")

* Open **Addressable Groups** window (Window>Asset Management>Addressables>Groups), and you should see your newly created mod group set as  (Default) with all assets in it and with all labels correctly applied.

![alt_text](https://github.com/FoxyVoxel/going-medieval-modding/blob/main/ReadmeImages/image7.png "Addressable Groups")

* Once again, in the **AddressableBuilder** window, check if your mod is selected and click on the **Build** button.
* After the build is done, asset packages should be placed in the mod’s **Exported** folder.  Copy its contents to your mod’s “Documents/Foxy Voxel/Going Medieval/Mods/Data/AddressableAssets/” and use the new assets in JSON files to show assets in the game. 

### Next steps

	TODO: Create Example Mod that can be checked in game
