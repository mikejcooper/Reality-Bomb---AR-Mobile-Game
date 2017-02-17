# Pass The Bomb / Reality Bomb
Repository for uni games project.

## Environment

### Dev Server
You need to make sure you're running the node development server in the background when running on your machine. It emulates the server which will transfer meshes to the clients. The readme on how to setup and run the server in [Assets/Scripts/MeshImport/](Assets/Scripts/MeshImport/readme.md).

### Build Scripts
There are a few build scripts available in the Unity toolbar up top under the heading "Builds". You can use these to build and launch the different TV/client apps quickly.

On MacOS if you want to launch more than one instance of an app you can right click it, "show package contents" and then run the executable in Contents/MacOS as many times as you need.

## Conventions
To make code more readable and easier to maintain we use a few coding conventions:

- [MSDN C# conventions](https://msdn.microsoft.com/en-us/library/ff926074.aspx)
- [PascalCase for public member variables](https://msdn.microsoft.com/en-us/library/ms229043(v=vs.100).aspx)
- [_leadingUnderscore for private member variables](http://softwareengineering.stackexchange.com/a/209533)
- [camelCase for local variables](https://msdn.microsoft.com/en-us/library/ms229043(v=vs.100).aspx)
- MVC pattern using [events and Delegates](https://msdn.microsoft.com/en-us/library/17sde2xt(v=vs.90).aspx)
- So don't be afraid to use one or two singleton controllers in a scene. There's a good example [here](http://gamedev.stackexchange.com/a/116010/25627) of how to implement singletons in Unity
## Build Scripts

