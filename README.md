# TLabVehiclePhysics
Open Source WheelCollider for Unity. pacejka based wheel logic and utility for creating simple tire models.

## Support
[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/tlabaltoh)

## Screenshot
<table>
    <caption>GamePlay Overview</caption>
    <tr>
        <td><img src="Media/overview.png" width="512" /></td>
    </tr>
</table>

<details>

<summary>Example parameters used in this project</summary>
<table>
    <caption>Pacejka</caption>
    <tr>
        <td><img src="Media/pacejka.png" width="530" /></td>
        <td><img src="Media/pacejka-lateral.png" width="530" /></td>
        <td><img src="Media/pacejka-longitudinal.png" width="530" /></td>
    </tr>
</table>
<table>
    <caption>Downforce and Torque Curve with LUT</caption>
    <tr>
        <td><img src="Media/lut-downforce-max.png" width="530" /></td>
        <td><img src="Media/lut-downforce-min.png" width="530" /></td>
    </tr>
    <tr>
        <td><img src="Media/multi-lut-downforce.png" width="530" /></td>
        <td><img src="Media/multi-lut-torque-curve.png" width="530" /></td>
    </tr>
</table>
</details>

## Getting Started
### Prerequisites
- 2022.3.19f1
- Universal Rendering Pipeline (URP)

### Installing
Clone the repository to any directory with the following command  
```
git clone https://github.com/TLabAltoh/TLabVehiclePhysics.git

cd TLabVehiclePhysics

git submodule upadte --init
```

### WheelColiderSource
#### How to play
##### Car Operation
- Left / Right Arrow: Handle
- Up Arrow: Accelerator
- Down Arrow: Brake
- Q: Shift Up
- E: Shift Down
- C: Clutch
##### Camera Operation
- ASDW: Camera Rotation
- Z: Switch Camera (Pov / Follow)

## Reference
- [Randomation-Vehicle-Physics](https://github.com/JustInvoke/Randomation-Vehicle-Physics)
- [Unity5-WheelCollider](https://github.com/unity-car-tutorials/Unity5-WheelColliderSource)

