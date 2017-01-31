var ws = require("nodejs-websocket")
var fsp = require('fs-promise')
 
var B_START = "\033[1m"
var B_END = "\033[0m"

var meshFile, triangleMeshFile, transformsFile;
var meshStr, triangleStr, transformsStr;

if (process.argv.length != 2 && process.argv.length != 5) {
  console.log(
`usage: node app.js [mesh markers_mesh marker_transforms]
       ${B_START}mesh${B_END}: the mesh obj file
       ${B_START}markers_mesh${B_END}: the mesh obj file that represents marker positions
       ${B_START}marker_transforms${B_END}: the multimarker config file`)
  return;
} else if (process.argv.length == 2) {
  meshFile = "example_data/debug_mesh.obj"
  triangleMeshFile = "example_data/debug_triangles_mesh.obj"
  transformsFile = "example_data/debug_transforms.dat"
} else if (process.argv.length == 5) {
  meshFile = process.argv[2];
  triangleMeshFile = process.argv[3];
  transformsFile = process.argv[4];
}

fsp.readFile(meshFile)
  .then(function(data){
    meshStr = data
    return fsp.readFile(triangleMeshFile)
  })
  .then(function(data){
    triangleStr = data
    return fsp.readFile(transformsFile)
  })
  .then(function(data){
    transformsStr = data
    openServer()
  }).catch(error => console.log(error))

var openServer = function () {
  ws.createServer(function (conn) {
    console.log("new connection")
    conn.sendText(`mesh${meshStr}`)
    conn.sendText(`markers${transformsStr}`)
    conn.sendText(`triangles${triangleStr}`)
  }).listen(3110)

  console.log('server running')
}

