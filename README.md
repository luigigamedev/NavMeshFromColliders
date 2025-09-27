Mesh renderers are often detailed with specific artifacts.
Since NavMesh baking is done based on the mesh renderers, it can generate a jerky and imprecise NavMesh.

This small editor tool will prepare the scene for NavMesh baking by deactivating all the mesh renderers and generating simple ones based on the actual colliders, with the intention of improving the precision of the generated NavMesh.

## Demo
**Quickly switch back and forth from bake mode to normal mode**

![switch-back-n-forth](https://user-images.githubusercontent.com/1649627/236692973-fae38a89-eb3b-4ac2-a9eb-7fe19640749f.gif)

**Bake the NavMesh**

![bake](https://user-images.githubusercontent.com/1649627/236693021-b305c93d-a44b-4fa3-964e-d624eb46a23c.gif)

**Comparing the results**

![comparing](https://user-images.githubusercontent.com/1649627/236693585-d32929df-dbd2-48d7-9ca8-74dafd9b73ba.png)
