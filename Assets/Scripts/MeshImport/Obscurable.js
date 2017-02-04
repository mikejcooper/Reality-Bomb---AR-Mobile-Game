#pragma strict

function Start () {
	var renders = GetComponentsInChildren(Renderer);
	for (var rendr : Renderer in renders){
		rendr.material.renderQueue = 2002; // set their renderQueue
	}
}
