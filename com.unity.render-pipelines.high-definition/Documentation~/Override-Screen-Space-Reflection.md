# Screen Space Reflection

The **Screen Space Reflection** (SSR) override is a High Definition Render Pipeline (HDRP) feature that uses the depth and color buffer of the screen to calculate reflections. For information about how screen space refraction works in HDRP, see the [Screen space refraction documentation](Reflection-in-HDRP.html#ScreenSpaceReflection).

## Using Screen Space Reflection

To use SSR in your Scene, you must enable it for your Cameras. In the Inspector for your [HDRP Asset](HDRP-Asset.html), go to the **Default Frame Settings > Lighting > Reflections** section and enable the **Screen Space Reflection** checkbox.

HDRP uses the [Volume](Volumes.html) framework to calculate SSR, so to enable and modify SSR properties, you must add a **Screen Space Reflection** override to a [Volume](Volumes.html) in your Scene. To add **Screen Space Reflection** to a Volume:

1. In the Scene or Hierarchy view, select the GameObject that contains the Volume component to view it in the Inspector.
2. In the Inspector, navigate to **Add Override > Lighting** and click **Screen Space Reflection**. 
   HDRP now calculates SSR for any Camera this Volume affects.

## Properties

![](Images/Override-ScreenSpaceReflection1.png)

| **Property**                  | **Description**                                              |
| ----------------------------- | ------------------------------------------------------------ |
| **Screen Edge Fade Distance** | Use the slider to control the distance at which HDRP fades out screen space reflections when the destination of the ray is near the boundaries of the screen. Increase this value to increase the distance from the screen edge at which HDRP fades out screen space reflections for a ray destination. |
| **Max Number of Ray Steps**   | Sets the maximum number of iterations that the algorithm can execute before it stops trying to find an intersection with a Mesh. For example, if you set the number of iterations to 1000 and the algorithm only needs 10 to find an intersection, the algorithm terminates after 10 iterations. If you set this value too low, the algorithm may terminate too early and abruptly stop reflections. |
| **Object Thickness**          | Use the slider to control the thickness of the GameObjects on screen. Because the SSR algorithm can not distinguish thin GameObjects from thick ones, this property helps trace rays behind GameObjects. The algorithm applies this property to every GameObject uniformly. |
| **Min Smoothness**            | Use the slider to set the minimum amount of surface smoothness at which HDRP performs SSR tracing. Lower values result in HDRP performing SSR tracing for less smooth GameObjects. |
| **Smoothness Fade Start**     | Use the slider to set the smoothness value at which SSR reflections begin to fade out. Lower values result in HDRP fading out SSR reflections for less smooth GameObjects |
| **Reflect Sky**               | Enable the checkbox to make SSR handle sky reflection. Disable this checkbox to make rays that intersect with the skybox always use the local Reflection Probe. |