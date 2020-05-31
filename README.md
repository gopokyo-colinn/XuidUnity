# <img src="https://user-images.githubusercontent.com/20549024/76211417-8616d780-6249-11ea-8e94-2f337664f595.gif" width=25> XdUnityUI - AdobeXD to Unity UI

![introduction](https://user-images.githubusercontent.com/20549024/76156453-0f40e800-613e-11ea-9923-59554aceae3c.gif)

## Language.

- [日本語](README_JP.md)
- [English](README.md)

## Overview

- Convert AdobeXD artboards to Unity UI Prefab.

## Installation

- To download and install
  1. https://github.com/itouh2-i0plus/XdUnityUI/releases
  1. click "▶Assets" of the latest version to download XdUnityUI.unitypackage.
  1. import the XdUnityUI.unitypackage into Unity.
  The /Assets/I0plus/XdUnityUI folder will be created.
  1. install the AdobeXD plugin
     - Double-click on /Assets/I0plus/XdUnityUI-ForAdobeXD/XdUnityUIExport.xdx.
- When cloning from a Git repository
  1. clone a Git repository
        - https://github.com/itouh2-i0plus/XdUnityUI
          - We are using LFS, and some Git clients require configuration.
  1. open (clone folder)/UnityProject in Unity.
        - Assets/I0plus/XdUnityUI is the plugin folder.
        - It's currently a Unity2019.3, UniversalRenderPipeline project.
  1. install the AdobeXD plugin
        - Double-click on /Assets/I0plus/XdUnityUI-ForAdobeXD/XdUnityUIExport.xdx.

## Quick Start

1. open the Adobe XD sample.
    - It's in /Assets/I0plus/XdUnityUI-ForAdobeXD/samples.xd.

2. AdobeXD export
    1. click on the artboard name and select the artboard.
    1. click on "XdUnityUI export plugin" in the plugin menu.
    1. "Folder" is the destination of the output folder.
    1. click on "Export" to start outputting. 

<img src="https://user-images.githubusercontent.com/20549024/76756957-0bf6cd80-67ca-11ea-9504-7ef273613a36.gif" width="640" />

3. Unity conversion
    - Unity Menu > Assets > XdUnityUI > Specify Folder Import
    - Specifies the same folder as the export.
    - The created Prefab will be placed in Assets/I0plus/CreatedPrefabs.
    - The created UI images are placed in Assets/I0plus/CreatedSprites.
      - UI images are sliced.

<img src="https://user-images.githubusercontent.com/20549024/76759838-d3f28900-67cf-11ea-9721-31c221cfe63a.gif" width="640" />

1. place the Prefab under the Canvas.

<img src="https://user-images.githubusercontent.com/20549024/76759902-f5ec0b80-67cf-11ea-9dd5-5ca556222c40.gif" width="640" />

## Samples

### Dots Scrollbar/ Horizontal layout scroll

|AdobeXD|Unity
|---|---|
| <img src="https://user-images.githubusercontent.com/20549024/76518275-27986600-64a2-11ea-9292-e3d058bf2468.png" width = "300" height = " auto " /> | <img src="https://user-images.githubusercontent.com/20549024/76518784-12700700-64a3-11ea-9407-9bca8449b070.gif" width="500" height="auto " /> |

### Image switching button

|AdobeXD|Unity
|---|---|
| <img src="https://user-images.githubusercontent.com/20549024/76143694-f47a5f00-60bc-11ea-885f-ec28c2f61454.PNG" width="300" height="auto"/> | <img src="https://user-images.githubusercontent.com/20549024/76143630-5dada280-60bc-11ea-8121-45d24aa03601.gif" width="500" height="auto"/> |

### Dialogue

|AdobeXD|Unity
|---|---|
| <img src="https://user-images.githubusercontent.com/20549024/76143695-f5ab8c00-60bc-11ea-8cdb-8002f8765cd9.PNG" width="300" height="auto"/> | <img src="https://user-images.githubusercontent.com/20549024/76143631-5e463900-60bc-11ea-9698-975443962aa5.gif" width="500" height="auto"/> |

### Vertical scrolling list

|AdobeXD|Unity
|---|---|
| <img src="https://user-images.githubusercontent.com/20549024/76143701-f6dcb900-60bc-11ea-98bd-0d49d8a6d77d.png" width="300" height="auto"/> | <img src="https://user-images.githubusercontent.com/20549024/76143628-5d150c00-60bc-11ea-9a93-9541c55e8190.gif" width="500" height="auto"/> | <img src="https://user-images.githubusercontent.com/20549024/76143628-5d150c00-60bc-11ea-9a93-9541c55e8190.gif" width="500" height="auto"/>

### Character Input

|AdobeXD|Unity
|---|---|
| <img src="https://user-images.githubusercontent.com/20549024/76143696-f5ab8c00-60bc-11ea-9d8f-9acf72188b8e.PNG" width="300" height="auto"/> | <img src="https://user-images.githubusercontent.com/20549024/76143624-5ab2b200-60bc-11ea-820a-b25755c6433c.gif" width="500" height="auto"/> | <img src="https://user-images.githubusercontent.com/20549024/76143624-5ab2b200-60bc-11ea-820a-b25755c6433c.gif" width="500" height="auto"/>

### Toggle/radio button

|AdobeXD|Unity
|---|---|
| <img src="https://user-images.githubusercontent.com/20549024/76143700-f6dcb900-60bc-11ea-9ea3-4af279919544.PNG" width="300" height="auto"/> | <img src="https://user-images.githubusercontent.com/20549024/76143627-5c7c7580-60bc-11ea-8a86-ae4890365330.gif" width="500" height="auto"/> | <img src="https://user-images.githubusercontent.com/20549024/76143627-5c7c7580-60bc-11ea-8a86-ae4890365330.gif" width="500" height="auto"/>

### Scroll.

|AdobeXD|Unity
|---|---|
| <img src="https://user-images.githubusercontent.com/20549024/76143697-f6442280-60bc-11ea-9214-00ca42075c22.PNG" width="300" height="auto"/> | <img src="https://user-images.githubusercontent.com/20549024/76143626-5be3df00-60bc-11ea-8236-0838d1e575b0.gif" width="500" height="auto"/> | <img src="https://user-images.githubusercontent.com/20549024/76143626-5be3df00-60bc-11ea-8236-0838d1e575b0.gif" width="500" height="auto"/>

### Text Window

|AdobeXD|Unity
|---|---|
| <img src="https://user-images.githubusercontent.com/20549024/76143702-f7754f80-60bc-11ea-8c02-9cf9b46b77f2.PNG" width="300" height="auto"/> | <img src="https://user-images.githubusercontent.com/20549024/76143629-5dada280-60bc-11ea-80d5-541f8d97317a.gif" width="500" height="auto"/> | <img src="https://user-images.githubusercontent.com/20549024/76143702-f7754f80-60bc-11ea-8c02-9cf9b46b77f2.PNG" width="500" height="auto"/>

## ChangeLog

### [v0.9] - 2020-05-31
- XD,Unity: Responsive resizing information is now more accurate. 
- Unity: Import by specifying a folder.

### [v0.8] - 2020-03-16
- XD: Fixed to output a selection.
- Unity: fixed to work with Unity2018.
- Unity: fixed to work with Unity2017.
- Unity: deleted the asmdef file.

<details><summary>Details</summary><div>
### [v0.7.2] - 2020-03-13
- Testing the DotsScrollbar
- Fixing mask processing

### [v0.5] - 2020-03-07

- Maintain InputField conversion.
- README_JP.md Sample images added

### [v0.4] - 2020-03-04

- README.md Englishization
- XD plugins English support

### [v0.3.2] - 2020-03-03

- Sample Modifications
- README.md Revisions and additions
- Corrected XdPlugin/main.js comments

### [v0.3.1] - 2020-03-02

- TextMeshPro sample added and explanation corrected.
- Add Button Sample
- Toggle samples added
- README.md Fix.

### [v0.3] - 2020-03-01

- Creating a unitypackage
- How to install from unitypackage
</div></details>.

## Operating conditions

- It's developed on Windows.
  - It is currently inadequately tested on a Mac.
- We're developing in Unity2019.3.
- AdobeXD has been tested with the latest version.

## About conversion

### Overview

- The conversion rules are applied to Adobe XD layer names.
  - The conversion rules are defined by the CSS description.
  - The output is a JSON file and an image file.
  - The output image will be sliced unless otherwise specified.
- Writing the output file to the Unity project, XdUnityUI/Import folder, will perform the conversion process in Unity.
- Prefab and Sprite will be output to the specified folder.

### Rule Description

- The name of the AdobeXD layer determines how it will function on Unity.

#### image

- example

  ```
  image.
  window-image
  icon-image
  ```

- Explanation.
  - If a layer or group layer has a name like the above, it will generate an image of the combined image of that layer and its child layers, and will be given an Image component in Unity.
- caution
  - Child layers are also treated as images, so they will not be converted.

#### button

- example
  ```
  button
  start-button
  back-button
  ```
- Explanation.
  - The button component is granted in Unity.
- caution
  - A child layer needs an image layer.

#### text

- example

  ```
  text
  title-text
  name-text
  ```

- Explanation.
  - The Text component is also attached in Unity by giving the text layer a name like the above.
- caution
  - The font used by AdobeXD must exist in the Unity project, in Assets/I0plus/XdUnityUI/Fonts/ below, in .ttf or .otf.
  - There is a design difference between AdobeXD and Unity. (e.g., Kerning).

#### textmp

- example

  ```
  textmp
  title-textmp
  name-textmp
  ```

- Explanation.
  - The TextMeshPro component is also attached in Unity by giving the text layer a name like the above.
- caution
  - You need TMP_PRESENT in Project Settings > Player > Scripting Define Symbols.
  - TextMeshPro font assets are required for fonts used by Adobe XD in Unity projects, such as Assets/I0plus/XdUnityUI/Fonts/.
     - Example: If you are using the Open Sans font Regular in AdobeXD
        - TextMeshPro font, look for the file name "Open Sans-Regular SDF.set".
  - There is a design difference between AdobeXD and Unity. (e.g., Kerning).

#### .viewport-layout-y

- Explanation.

  - Scrollable vertical layout.
  - Refer to the samples.xd VerticalListSample artboard. (with scroll bar).
  - <img src="https://user-images.githubusercontent.com/20549024/75763061-ff608700-5d7e-11ea-985c-88feb3a2de70.png" width="30%">
  - A postscript is planned.

#### scrollbar
- A postscript is planned.

#### toggle.
- A postscript is planned.

## When a problem arises

### AdobeXD plugin running.

#### Exporting images fails.

- cause
  - It may be a problem on Adobe XD. It is under investigation.

### Unity conversion running.

#### Converting fails when trying to handle characters.

- cause
  - There may be no font.
- countermeasure
  - It will output the name of the font file you tried to find in the Console but couldn't find.
  - Rename the font file, if possible, and copy it to the XdUnityUI/Fonts directory (the directory where the "So_XdUnityUIFonts file is located).

#### TextMeshPro error is printed.

- cause
  - Scripting Define Symbols does not have TMP_PRESENT.

- countermeasure
  - Install the TextMeshPro package.
  - Add TMP_PRESENT to Project Settings > Player > Scripting Define Symbols.
     
### Conversion Results

#### No need for the image component of the artboard background

- cause
  - The artboard background has been set up.
- countermeasure
  - Select the art board and uncheck "Appearance" > "Paint".

## To use it better.

### Original conversion rules

- Editing the conversion rules CSS
  - To change the XDX file
     1. rename XdUnityUIExport.xdx to XdUnityUIExport.zip.
     1. unzip and edit the index.css file.
     1. zip compression again, change the extension to xdx.
     1. reinstall the plugin.
  - When expanding to the Adobe XD development folder
     1. uninstall the XdUnityUI export plugin.
     1. rename XdUnityUIExport.xdx to XdUnityUIExport.zip.
     Unzip and copy the folder to the AdobeXD development folder (go to Plugins > Development Version > Show Development Folder).
     1. edit index.css.
     1. reload the plugin. (AdobeXD plugin menu>development version>reload).
- Conversion rule CSS description
  - A postscript is planned.
- Per-artboard conversion rules
  - A postscript is planned.

### Add your own components when converting

- A postscript is planned.

### 9Slice.

- A postscript is planned.

## Future Plans

- Target.
  - Enable UI design in AdobeXD until release.
- merit
  - Hold the final version in the hands of the designer.
  - You can use CCLibrary to integrate with various tools.
- Issue.
  - The work done on Prefab (adding components and adjusting parameters in Unity) will disappear when Prefab is overwritten in conversion.
     - countermeasure
        - Copy and use.
        - In Prefab Variant, evacuate additional work.
          - A name change would cause the work to disappear.
          - There seems to be some work left in the Variant file (under investigation to see if it can be restored).
  - 9Slice.
     - in support of
  - Component State
     - https://helpx.adobe.com/jp/xd/help/create-component-states.html
     - When it becomes possible to obtain it through the AdobeXD Plugin API, we will support it.
- the others
  - under investigation
     - Can it be converted to UXML?
     - Is it possible to create a UI for DOTS mode?

## License.

- XdUnityUI is free (MIT license).
- It's open source on GitHub.
     - https://github.com/itouh2-i0plus/XdUnityUI
- The image license used in the sample
  - "Vector graphic for Fishdom Playrix" by Daria Volkova is licensed under [CC BY-NC-ND 4.0](https://creativecommons.org/licenses/by-nc-nd/4.0/?ref=ccsearch&atype=rich)
  - https://www.behance.net/gallery/10239443/Vector-graphic-for-Fishdom-Playrix

## Support Forum

- https://forum.unity.com/threads/xdunityui-adobexd-to-unity.843730/.

## Acknowledgements.

- @kyubuns (https://kyubuns.dev)
- Baum2 (https://github.com/kyubuns/Baum2)

### Thank you so much for your help.
