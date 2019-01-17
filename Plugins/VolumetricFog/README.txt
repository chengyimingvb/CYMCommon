**************************************
*        VOLUMETRIC FOG & MIST       *
* Created by Ramiro Oliva (Kronnect) * 
*            README FILE             *
**************************************


Updating from earlier versions
------------------------------

If you're updating from v7.5 or earlier, please note that this update introduces new options regarding transparency support.
Also make sure you uncheck unused shader features clicking on the "Shader Options" button at top of Volumetric Fog & Mist inspector.


How to use this asset
---------------------
Firstly, you should run the Demo Scenes provided to get an idea of the overall functionality. There're lot of use cases where you can use Volumetric Fog & Mist, from gorgeous thick animated smoke, to thin mist above grass, dense dust storms, ...
Later, you should read the documentation and experiment with the tool.

Hint: to quick start using the asset just add VolumetricFog script to your camera. It will show up in the Game view. Customize it using the custom inspector.


Demo Scenes
-----------
There're 10+ demo scenes, located in "Demos" folder. Just go there from Unity, open them and run it. Remember to remove the Demos folder from your project to reduce size.


Documentation/API reference
---------------------------
The PDF is located in the Documentation folder. It contains instructions on how to use this asset as well as a useful Frequent Asked Question section.


Support
-------
Please read the documentation PDF and browse/play with the demo scenes and sample source code included before contacting us for support :-)

* Support: contact@kronnect.me
* Website-Forum: http://kronnect.me
* Twitter: @KronnectGames


Future updates
--------------

All our assets follow an incremental development process by which a few beta releases are published on our support forum (kronnect.com).
We encourage you to signup and engage our forum. The forum is the primary support and feature discussions medium.

Of course, all updates of Volumetric Fog & Mist will be eventually available on the Asset Store.


Version history
---------------

v9.6 Current Release
- API: added SetFogOfWarAlpha with bounds option
- Added Inverted Geometry Mask option (in Shader Options)
- [Fix] Fixed regression bug with orthographic camera

v9.5.1 2018-DEC-14
- Added option to remove flickering of fog areas in large scenes due to floating point issues
- [Fix] Removed Unity 2018.3 warnings due to Prefab system changes

v9.5 2018-OCT-16
- Void area: new option to compute void within raymarching loop producing a better effect when viewing void area from inside the fog
- Point lights: new Inside Atten parameter to reduce screen burning effect when camera is inside point light sphere or collinear in fog plane
- [Fix] Fixed "Use XY Plane" setting on fog areas
- [Fix] Fixed profile lerp bug

v9.4 2018-SEP-12
- Removed the limit of 6 point lights
- Material integration
- Added Force Composition option - can be used to improve edges when MSAA is enabled
- Added custom parameter for new point lights check interval (defaults to 3 seconds)
- API: added public TrackPointLights(force) argument to force immediate update of point lights
- API: added GetFogOfWarAlpha method
- [Fix] Fixed rare black artifacts when using blend transparency and depth blur due to NaN pixels in the frame buffer

v9.3 2018-JUN-01
- Added volume mask feature (enable it in Shader Options)
- Added downsampling, fog void and light scattering options to profile scriptable object
- Added texture update interval parameter to inspector (reduce to make directional light influence update faster)
- Added WebGL compatibility
- [Fix] Fixed inspector UI refresh issue when using profiles
- [Fix] Fixed issue with a render texture not being correctly released when transparency blend is enabled (only Unity 5.5-5.6 versions)

v9.2 2018-MAY-07
- Added fallOff parameter to fog void areas with box shape
- Design changes to editor inspector
- Dynamic Fog & Mist updated to version 6 (Unity 2017.2+)
- Increased range for noise sparse to 2
- [Fix] Fixed "Copy Sun Color" bug
- [Fix] Some settings in inspector could not be changed just after setting a new legacy preset
- [Fix] Fixed texture binding warning on Metal
- [Fix] Fixed fog area inspector bug

v9.1 2018-APR-03
- Added support for multiple cameras and fog areas. Test demo scene 18 added.
- Sprite shaders now support GPU instancing
- Added shortcuts to create fog areas in GameObject -> Create Other menu
- Added inspector undo support
- [Fix] Creating a Dynamic Fog & Mist volume from the main menu creates a Volumetric Fog & Mist volume instead. Fixed.
- [Fix] Fixed point light contribution to fog when fog distance option is used
- [Fix] Fixed point light effect vertical displacement when baseline height option is used
- [Fix] Fixed inspector not showing certain sections if fog camera is disabled
- [Fix] Fixed Sprite fog shaders instancing issue in Unity 2017.3.0
- [Fix] Output alpha value is now preserved
- [Fix] Fixed random locations of fog of war not being restored

v9.0 2018-JAN-11
- Improved Point Light system with better performance and quality. New inspector options: inscattering, global intensity.

v8.2 2018-JAN-02
- VR: support for Single Pass Instanced
- Point Lights: new "Distance To" or pivot which specifies the reference for distance calculation (good for fog areas + point lights)
- For of war: optimized setting/restoring fog transparency functions
- Dynamic Fog & Mist package updated to version 5.1
- Fog Areas: new option to show the area extents in the Scene View (as Gizmos)
- Editor: fog of war settings are now visible in inspector (were hidden if this feature was disabled)

v8.1 2017-DEC-12
- Fog animation keeps pace and looks properly when disabling/enabling cameras or script
- New demo scenes (only Unity 2017.1+): "Mountain Flight" and "Demo Map Magic"
- Refactored transparency blend option which also resulted in 1 less shader keyword
- New compute depth option: tree billboards + transparent objects
- Added back jitter option to reduce banding on fog areas edges
- Removed code which sets a fixed default value to fog area radius when assigning the character property
- Dynamic Fog package updated to version 5.0
- [Fix] Fixed banding issue with multiple fog areas in Linear Color space
- [Fix] Fixed speed and turbulences not affecting fog areas

v8.0 2017-SEP-05
- Fog Profiles. Create or store persistent profiles. Assign them at editor or runtime. Sample profiles inside Resources/Profiles.
- Fog Volumes. They now also accept profiles to transition from one zone to another using all available parameters contained in target profile.
- Fog Areas. Improved rendering pipeline, reduced overhead and no more multiple script on the main camera. Improved rendering sorting order.
- New fog areas prefabs inside Resources/Prefabs.
- Added fog area sorting mode option.
- Revamped transparency system with new algorithm for transparent particles and support for standard Unity billboard trees
- Added Compute Depth with layer mask option
- Added Compute Depth Scope option (optimizes compute depth execution)
- Box Areas now support falloff parameter. Produces better randomized borders
- Added Layer Mask option to Sun Shadows
- New Noise Sparse and Final Multiplier options. More freedom/randomization for cloudy skies and fields.
- Added Follow Mode (XYZ vs XZ plane only) to Fog Area follow feature.

v7.5 Published 2017.07.31
- Added Light Diffusion slider under Light Scattering section
- [Fix] Fixed regression issue that causes banding

v7.4.2 Published 2017.07.25
- Improved performance of fog color changes
- Optimizations to Sun shadows system

v7.4.1 Published 2017.07.17
- [Fix] VR: Fixed Single Pass Stereo Rendering in Unity 2017.1
- Some shader tweaks

v7.4 Published 2017.07.11
- Fog of war: clear methods now allows specifying duration of clearance to produce a smoother effect
- Added Shadow Cancellation parameter to simulate volumetric lighting effect. New demo scene 15.
- Exposed internal Sun Shadows Bias parameter which helps avoid self-shadowing
- Some shader optimizations, also removed jitter parameter as banding artifacts have been reduced

v7.3.1 Published 2017.06.01
- Added option Lighting Model under Fog Colors section
- Sun Shadows: extended tree compatibility
- Some fixes and improvements with transparency pass. Added debug toggle into inspector.

v7.3 Published 2017.05.30
- Improved transparency support, new BlendPass mode compatible with VR
- Improved look of fog over transparent objects
- Fog volumes: added option to specify custom fog colors
- Optimized shader variant count
- [Fix] Fixed downsampling fog regression bug
- [Fix] Removed console warning with Deferred rendering path + improving fog transparency option

v7.2.3b Published 2017.04.12
- Updated Dynamic Fog & Mist to version 4.0

v7.2.3 Published 2017.03.30
- Minor tweaks in fog parameters ranges to give more flexibility
- [Fix] Changes in Volumetric Fog inspector does not mark the scene as dirty ("pending save")
- [Fix] Fixed jittering changes not reflecting changes in Editor mode
- Minor fixes for Unity 5.6

V7.2.2 Published 2017.03.20
- [Fix] Fixed camera projection issue with Single Pass Stereo Rendering and OpenVR SDK

V7.2.1 Published 2017.02.17
- [Fix] Fixed regression bug with distance fog parameter

V7.2 Published 2017.02.09
- Support for orthographic camera
- New turbulence feature. Enable it under new Fog Animation section.

V7.1.1 Published 2017.02.04
- [Fix] Fixed inspector error when showing Fog of War section
- [Fix] Fixed inverted fog issue on DX11+MSAA+Downsampling

V7.1 Published 2017.02.01
- New feature: Sun shadows
- Ability to render along XY plane
- Some internal fixes and improvements

V7.0.1 Published 2017.01.20
- Proper detection of multiple render targets support when using downsampling x2 or higher
- Removed usage of reserved keyword for PS4 platform
- Fixed issue with incorrect Unity version packages on the Asset Store
- Added debug mode toggle in inspector for checking correct fog rendering
- Added reminder to disable unwanted effects in Build Options to optimize build compilation

V7.0 Published 2017.01.9
- Improved edge correction algorithm
- Downsampling up to x8 allowed
- New Depth Blur effect
- Added Point Light Check interval parameter and optimized search routine
- Added Max Distance FallOff parameter to smoothly blend fog end with sky/background
- Fixed issue with provided sprite shaders related to shadows in deferred rendering path

V6.5 Published 2016.12.2
- Fog and sky haze speed can now change smoothly without glitches
- Minor internal improvements and fixes

V6.4 Published 2016.11.16
- Inverted fog void are deprecated - now called fog areas, have a specific section in inspector
- Added Build Options to remove some shader features easily
- Improved fog distance effect

V6.3 Published 2016.10.25
- Option to copy Sun color
- Improved performance when Sun is assigned
- Fixed Sun shafts bug

V6.2 Published 2016.10.12
- Dynamic Fog & Mist upgraded to v2.3
- Fixed sun shafts incorrect intensity at night

V6.1 Published 2016.10.03
- Dynamic Fog & Mist upgraded to v2.2

V6.0 Published 2016.09.23
- Ability to render more than one fog area (inverted void area) with proper culling support. New demo scene.
- More than one Volumetric Fog & Mist script can be added per camera

V5.5 Published 2016.09.20
- Improved point light support
- Updated Dynamic Fog & Mist to version 2
- Some shader optimizations

V5.4 Published 2016.08.30
- New jittering parameter in light shaft section (allows to create smooth rays with fewer samples)
- VR: experimental support for Single Pass Stereo Rendering (Unity 5.4)
- Fixes issues when changing fog settings on a disabled camera
- Fixed fog over transparent objects (DirectX)

V5.3 Published 2016.07.15
- New dithering slider in the inspector to allow finer customization of the dithering effect.
- New jittering parameter which increases the level of control for banding artifacts
- New speed/damping parameter for baseline relative to camera option

V5.2 Published on 2016.05.24
- New dithering algorithm that applies to sky haze which contributes to extra banding reduction
- Fog of War: new params to control delay and duration of automatic fog restore after setting alpha
- Improved included alternate sprites shaders (unlit & diffuse) to accept vertex colors.
- Ability to follow a character when using fog inverted mode.
- Fixed Fog would shift height under some circumstances

V5.1 Published on 2016.05.12
- Reduced banding at low density.
- Changed Dynamic Fog & Mist Advanced shader variant to use shader model 3.0

V5.0 Published on 2016.04.27
- New "Edge Improve" option to reduce fog bleeding/pixelization over geometry edges when downsampling is increased.
- New “Dithering” option to reduce banding artifacts.
- Inverted areas (spherical and boxed) are now accurate and works from any view angle.
- Dynamic Fog & Mist (included in package) updated to V1.7

V4.2 Published on 2016.04.19
- New demo scenes “High Clouds” and “MountainClimb”
- Improved area fog with better cloud scattering
- Can set custom colliders on Fog Volumes
- Fixed light scattering bug on DX platform when antialias is enabled

V4.1 Published on 2016.03.30
- Light scattering option (a.k.a. god rays, sun shafts or volumetric scattering)
- Improved performance when fog density is low
- Reorganized inspector settings in foldable sections
- Fixed sky haze noise sampling
- Fixed point light sampling scale

V4.0 Published on 2016.03.14
- Support for Point Light (up to 6 point lights) with auto tracking - another option for creating artistic effects. New Demo Scene.
- New Sprite materials & shaders (Sprite Fog Diffuse, Sprite Fog Unlit)
- Includes Dynamic Fog & Mist 1.5
- Can invert void areas
- Improved performance
- Fixes regarding Sun unassigment in inspector
- Integrated Dynamic Fog & Mist to avoid issues with shared resources

V3.3 Published on 2016.03.05
- Enhanced compatibility with sprite renderer

V3.2 Published on 2016.02.19
- New World Edge preset
- Compatibility with Gaia via Extension Manager
- Compatibility with Time of Day (assign Sun game object to Sun property in inspector)
- Ability to render either in front or behind transparent objects with a single click (inspector)
- Ability to assign a gameobject (character) to make the void area follow it automatically
- Ability to set the baseline of the fog automatically with Camera height.
- Button in inspector to unassign the Sun
- Improved preset auto configuration, now detects water level
- Improved falloff for distance fog when views from top or bottom
- Improved fog algorithm
- Compatibility with Render Texture (Demo Scene 3 included, check video below)
- Fixed issues with different height base lines

V3.1 Published on 2016.02.09
- Fog of War.

V3.0 Published on 2016.01.25
- Downsampling option to improve performance. Best results when fog is used as cloud layer.

V2.2 Published on 2016.01.22
- Support for boxed void areas

V2.1 Published on 2016.01.08
- Automatic light alignment with defined Sun

V2.0 Published on 2016.01.04
- Support for void areas
- Support for elevated fog & clouds

V1.2 Published on 2015.12.22
- Improved support for transparent objects

V1.1 Published on 2015.12.03
