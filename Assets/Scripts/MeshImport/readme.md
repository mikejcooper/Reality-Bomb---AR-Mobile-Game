# MeshImport

To load meshes during the game, go to the `dev_server~` directory. This is hidden in the Unity GUI, so you'll need to find it in your file manager / terminal. From there, run `npm install` to install the required Node JS modules and then `node app.js` to run the mesh server. Now when you run the game you should see a mesh get imported when it runs.

## Important
It's important that you run the dev server before you run anything from inside the Unity editor. Both the server and Unity script will try to bind to a specific port but the OS won't allow this. The Unity script will detect when the server is already bound to the port and then assume the server is on localhost and carry on as normal. It doesn't work the other way round :)