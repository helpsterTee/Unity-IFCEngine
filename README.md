# Unity-IFCEngine
Import scripts for IFC files.

### Features working:
* Geometry
* Project scale in Milli, Deci and Centimeters
* Assign materials according to material definitions in Unity component
* Get georeference latitude, longitude, elevation from IFC file

### Feature ToDo:
* Read IFC materials and create Unity ones accordingly
* Read IFC properties and make them usable

Note: Implementation is based on my IfcEngineWrapper project, which is currently not available as OSS, and IFCEngine http://rdf.bg/ifc-engine-dll.html?page=products
Please see the license terms of IFCEngine DLL before trying to use this project.

## Installation
1. Create a new Unity *2017.2* project
2. Import CielaSpike's ThreadNinja from the Unity Asset Store: https://www.assetstore.unity3d.com/en/#!/content/15717
3. Download this repository as ZIP file and extract it to the project root OR
3. Download a release package from "Releases" and import it using Assets -> Import Package -> Custom Package
4. If you've used a release package, copy the contents of IFCImporter/IFCDefines to the Unity project root directory
5. Open the IFCImporter/ExampleImport map to see how it's used!

## Usage
The import scripts basically consist of the files ImportIFC.cs and MaterialAssignment.cs.
1. Create an empty game object
2. Attach the ImportIFC component to the game object
3. Attach the MaterialAssignment component to the game object
4. Assign the MaterialAssignment component to the ImportIFC component
5. Change the length of the material array and add defines for "IFCClass"->"Material" (case sensitive)

![MaterialAssign](https://i.imgur.com/OsT74AE.jpg)

6. Create your own scripting class, calling the component and using the Init() and ImportFile() function
7. Use the callback function to do further stuff, when the import is finished.

![Calling](https://i.imgur.com/JtTtow6.jpg)

8. **Note:** Objects created in play mode are not persistant to the game world. However, prefabs will be created upon initial import, so you can then drag and drop them in editor (or write your own serialization method). This may be needed to change texture scaling because of auto-generated UV coordinates.

![Texture](https://i.imgur.com/ZVixMbw.jpg)

9. Import some IFC files!

10. Access common IFC properties via the IFCVariables component attached to the child gameobjects
![properties](https://i.imgur.com/7u8V3cK.png)

## Release

Be sure to include the IFC2X3-Settings.xml +.exp and IFC4-Settings.xml + .exp, IFCEngine.dll and IfcEngineWrapper.dll into your binary directory!

![Import](https://i.imgur.com/MrLfS99.jpg)

## Contributing
Create a pull request. Only pull requests corresponding to an issue will be considered, so please create an issue accordingly to the feature or bug you're submitting code for, if there isn't any.
