# Tonemapping

Tonemapping is the process of remapping HDR values of an image in a range suitable to display on screen.

## Using Tonemapping

**Tonemapping** uses the [Volume](Volumes.html) framework, so to enable and modify **Tonemapping** properties, you must add a **Tonemapping** override to a [Volume](Volumes.html) in your Scene. To add **Tonemapping** to a Volume:

1. In the Scene or Hierarchy view, select the GameObject that contains the Volume component to view it in the Inspector.
2. In the Inspector, navigate to **Add Override > Post-processing** and click on **Tonemapping**. HDRP now applies **Tonemapping** to any Camera this Volume affects.

## Properties

![](Images/Post-processingTonemapping1.png)

| **Property**          | **Description**                                              |
| --------------------- | ------------------------------------------------------------ |
| **Mode**              | Use the drop-down to select a tonemapping algorithm to use for color grading. The options are:**None**: Use this option if you do not want to apply tonemapping.**Neutral**: Use this option if you only want range-remapping with minimal impact on color hue & saturation. It is generally a great starting point for extensive color grading.**ACES**: Use this option to apply a close approximation of the reference ACES tonemapper for a more filmic look. It is more contrasted than Neutral and has an effect on actual color hue & saturation. Note that if you use this tonemapper all the grading operations will be done in the ACES color spaces for optimal precision and results.**Custom**: Use this option if you want to specify the tonemapping settings yourself. Selecting this mode exposes properties that allow you to customize the tonemapping curve. |
| **Toe Strength**      | Use the slider to set the strength of the transition between the curve's toe and the curve's mid-section. A value of 0 results in no transition and a value of 1 results in a very hard transition.To expose this property, select **Custom** from the **Mode** drop-down. |
| **Toe Length**        | Use the slider to set the length of the curve's toe. Higher values result in longer toes and therefore contain more of the dynamic range..To expose this property, select **Custom** from the **Mode** drop-down. |
| **Shoulder Strength** | Use the slider to set the strength of the transition between the curve's midsection and the curve's shoulder. A value of 0 results in no transition and a value of 1 results in a very hard transition.To expose this property, select **Custom** from the **Mode** drop-down. |
| **Shoulder Length**   | Set the amount of f-stops to add to the dynamic range of the curve. This is how much of the highlights that the curve takes into account.To expose this property, select **Custom** from the **Mode** drop-down. |
| **Shoulder Angle**    | Use the slider to set how much overshoot to add to the curve's shoulder.To expose this property, select **Custom** from the **Mode** drop-down. |
| **Gamma**             | Set a gamma correction to the entire curve.To expose this property, select **Custom** from the **Mode** drop-down. |