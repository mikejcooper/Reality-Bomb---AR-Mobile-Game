var keypress = require('keypress');
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

var meshFile, chullVerticesFile, boundaryVerticesFile, transformsFile;
var meshStr, chullVerticesStr, boundaryVerticesStr, transformsStr;

keypress(process.stdin);

if (process.argv.length != 2 && process.argv.length != 5) {
  console.log(
`usage: node app.js [mesh vertices marker_transforms]
       ${B_START}mesh${B_END}: the mesh obj file
       ${B_START}chull_vertices${B_END}: a newline seperated list of vertices defining the convex hull
       ${B_START}boundary_vertices${B_END}: a newline seperated list of vertices defining the boundary (derived from convex hull)
       ${B_START}marker_transforms${B_END}: the multimarker config file`)
  return;
} else if (process.argv.length == 2) {
  meshFile = "example_data/mesh.obj"
  chullVerticesFile = "example_data/chull_vertices.txt"
  boundaryVerticesFile = "example_data/boundary_vertices.txt"
  transformsFile = "example_data/transforms.dat"
} else if (process.argv.length == 5) {
  meshFile = process.argv[2];
  chullVerticesFile = process.argv[3];
  boundaryVerticesFile = process.argv[4];
  transformsFile = process.argv[5];
}

var loadData = function (callback) {
  fsp.readFile(meshFile)
    .then(function(data){
      meshStr = data
      return fsp.readFile(chullVerticesFile)
    })
    .then(function(data){
      chullVerticesStr = data
      return fsp.readFile(boundaryVerticesFile)
    })
    .then(function(data){
      boundaryVerticesStr = data
      return fsp.readFile(transformsFile)
    })
    .then(function(data){
      transformsStr = data
      console.log('loaded data');
      if (callback) callback()
    }).catch(error => console.log(error))
}

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
    conn.sendText(`chull_vertices${chullVerticesStr}`)
    conn.sendText(`boundary_vertices${boundaryVerticesStr}`)
  }).listen(TRANSFER_PORT)

  console.log('server running')
}

loadData(openServer);

// listen for the "keypress" event
process.stdin.on('keypress', function (ch, key) {
  if (key && key.ctrl && key.name == 'r') {
    loadData()
  } else if (key && key.ctrl && key.name == 'c') {
    process.stdin.pause();
    process.exit();
  }
})

process.stdin.setRawMode(true);
process.stdin.resume();

