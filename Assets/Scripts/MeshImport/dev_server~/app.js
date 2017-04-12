var ws = require("nodejs-websocket")
var fsp = require('fs-promise')
var dgram = require('dgram');
var dgramClient = dgram.createSocket("udp4");
var ip = require('ip');

var UDP_BROADCAST_PORT = 3110;
var TRANSFER_PORT = 3111;
var UDP_PAYLOAD = "RealityBomb";
var B_START = "\033[1m"
var B_END = "\033[0m"

var meshFile, verticesFile, transformsFile;
var meshStr, verticesStr, transformsStr;

if (process.argv.length != 2 && process.argv.length != 5) {
  console.log(
`usage: node app.js [mesh vertices marker_transforms]
       ${B_START}mesh${B_END}: the mesh obj file
       ${B_START}vertices${B_END}: the mesh obj file that represents marker positions
       ${B_START}marker_transforms${B_END}: the multimarker config file`)
  return;
} else if (process.argv.length == 2) {
  meshFile = "example_data/debug_mesh.obj"
  verticesFile = "example_data/debug_vertices.txt"
  transformsFile = "example_data/debug_transforms.dat"
} else if (process.argv.length == 5) {
  meshFile = process.argv[2];
  verticesFile = process.argv[3];
  transformsFile = process.argv[4];
}

fsp.readFile(meshFile)
  .then(function(data){
    meshStr = data
    return fsp.readFile(verticesFile)
  })
  .then(function(data){
    verticesStr = data
    return fsp.readFile(transformsFile)
  })
  .then(function(data){
    transformsStr = data
    openServer()
  }).catch(error => console.log(error))

var sendBroadcast = function () {
  console.log(`Broadcasting to ${ip.subnet(ip.address(), '255.255.255.0').broadcastAddress}`);
  dgramClient.send(UDP_PAYLOAD, 0, UDP_PAYLOAD.length, UDP_BROADCAST_PORT, ip.subnet(ip.address(), '255.255.255.0').broadcastAddress);
}

var openServer = function () {

  dgramClient.on('listening', function(){
      dgramClient.setBroadcast(true);
      sendBroadcast();
      setInterval(sendBroadcast, 5000);
      // dgramClient.close();
  });

  dgramClient.bind(UDP_BROADCAST_PORT);

  ws.createServer(function (conn) {
    console.log("new connection")
    conn.sendText(`mesh${meshStr}`)
    conn.sendText(`markers${transformsStr}`)
    conn.sendText(`vertices${verticesStr}`)
  }).listen(TRANSFER_PORT)

  console.log('server running')
}

