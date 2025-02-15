# Vignette

In Photography, vignetting is the term for the darkening and/or desaturating towards the edges of an image compared to the center. In real life, thick or stacked filters, secondary lenses, and improper lens hoods are usually the cause of this effect. You can use vignetting to draw focus to the center of an image.

## Using Vignette

**Vignette** uses the [Volume](Volumes.html) framework, so to enable and modify **Vignette** properties, you must add a **Vignette** override to a [Volume](Volumes.html) in your Scene. To add **Vignette** to a Volume:

1. In the Scene or Hierarchy view, select the GameObject that contains the Volume component to view it in the Inspector.
2. In the Inspector, navigate to **Add Override > Post-processing** and click on **Vignette**. HDRP now applies **Vignette** to any Camera this Volume affects.

## Properties

![](Images/Post-processingVignette1.png)

| **Property**   | **Description**                                              |
| -------------- | ------------------------------------------------------------ |
| **Mode**       | Use the drop-down to select the vignette display mode. **Procedural**: Select this mode to expose properties that control the position, shape, and intensity of the vignette. **Masked**: Select this mode to use a texture mask to create a custom vignette effect. Use this mode to achieve irregular vignetting effects. |
| **Color**      | Use the color picker to set the color of the vignette.       |
| **Center**     | Set the vignette center point. For reference, the screen center is [0.5, 0.5]. To expose this property, select **Procedural** from the **Mode** drop-down. |
| **Intensity**  | Use the slider to set the strength of the vignette effect. To expose this property, select **Procedural** from the **Mode** drop-down. |
| **Smoothness** | Use the slider to set the smoothness of the vignette borders. To expose this property, select **Procedural** from the **Mode** drop-down. |
| **Roundness**  | Use the slider to set the roundness of the vignette. Higher values result in a more round vignette. To expose this property, select **Procedural** from the **Mode** drop-down. |
| **Rounded**    | Enable the checkbox to make the vignette perfectly round. Disable the checkbox to make the vignette match the shape on the current aspect ratio. To expose this property, select **Procedural** from the **Mode** drop-down. |
| **Mask**       | Assign a Texture to use as the vignette. This should be a black and white mask. To expose this property, select **Masked** from the **Mode** drop-down. |
| **Opacity**    | Use the slider to set the opacity of the mask vignette. To expose this property, select **Masked** from the **Mode** drop-down. |