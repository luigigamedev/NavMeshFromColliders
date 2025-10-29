Unity’s NavMesh baking system only considers mesh renderers, which can lead to inaccurate or uneven results.

This tool prepares the scene for NavMesh baking by hiding existing mesh renderers and creating temporary mesh renderers based on the colliders in the scene. 
These temporary meshes can then be used to bake a more accurate NavMesh.

After baking, the tool restores the scene to its original state.
