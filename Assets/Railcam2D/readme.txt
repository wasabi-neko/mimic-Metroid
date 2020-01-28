Railcam2D -- Version 1.0.0 -- 01 Nov 2017
Copyright (c) 2017 Jonathan Madelaine



### USER GUIDE ###

http://railcam2d.com/user-guide/index.html



### SUPPORT ###

For bug reporting or to ask a question, please use one of the following:

http://railcam2d.com/support/contact.html
support@railcam2d.com



### QUICK START ###

Railcam 2D is designed to be an intuitive plug and play asset. The only file that needs to be manually added to your scene is the Railcam2DCore component file (Railcam2DCore.cs) located in the root directory. All other files are implemented through this component.

To control camera movement with Railcam 2D, simply drag and drop the Railcam2DCore.cs file onto the camera object you want to manipulate. This will add a Railcam2DCore component to your camera.

Railcam 2D moves the camera by following a target. To give Railcam 2D a target to follow, assign a value to the Target variable in the Railcam2DCore component editor. This will usually be a player-controlled object.

For more information see the USER GUIDE.



### BASICS ###

The main benefit of Railcam 2D revolves around its ability to control camera movement through the implementation of Rails and Triggers. This section is a step by step guide that teaches you how to create both a basic Rail and Trigger component and have them influence camera movement in a scene.

After following this walkthrough, you should be more familiar with the Asset's UI and be able to successfully implement Railcam 2D in a project.


Moving the Camera


Add a Railcam2DCore component to the camera

The Railcam2DCore component is the heart of Railcam 2D. It is responsible for camera movement and connects all other components of the Asset.

Assign a Target in the Railcam2DCore editor

To move the camera, the Core requires a Target object to follow. This is usually a player-controlled object.

Enter Play Mode

Enter Play Mode and see that the camera now follows the Target object.

Exit Play Mode

The Core also provides buttons that add Manager components to the camera object. These components allow Rail and Trigger components to be added to the scene.


Adding a Rail


Click "Enable" next to "Rail Editing" in the Railcam2DCore editor

This will add a RailManager component to the camera object and enable Scene View editing for Rails. The RailManager component provides an interface to organise and edit the camera's Rail components.

Click "Add New Rail" in the RailManager editor

This will add a Rail object with a Rail component to the scene as a child of the camera object. Rails are paths through a scene that the camera can move along.

Click "Rail 0" in the RailManager editor

This will enable editing of Rail 0's values in the RailManager.

Select "Rail 0" in the "Start On Rail" dropdown of the Railcam2DCore editor

Now that Rail 0 has been added to the scene, it can be used by the Core component to calculate camera position. To connect the camera to Rail 0, select "Rail 0" in the "Start On Rail" dropdown selector of the Railcam2DCore component editor.

Enter Play Mode

Enter Play Mode and see that the camera follows the target object, but is limited to the path of Rail 0.

Exit Play Mode

This is an example of using a single Rail in a scene. To use multiple Rails and to allow the camera to connect and disconnect from Rails during runtime, you can use Trigger components.


Adding a Trigger


Click "Enable" next to "Trigger Editing" in the Railcam2DCore editor

This will add a TriggerManager component to the camera object and enable Scene View editing for Triggers. The TriggerManager component provides an interface to organise and edit the camera's Trigger components.

Click "Add New Trigger" in the TriggerManager editor

This will add a Trigger object with a Trigger component to the scene as a child of the camera object. Triggers provide a way to attach and detach the camera from Rails during runtime. This means that multiple Rails can be used in a scene, and allows for much more control of camera movement.

Click "Trigger 0" in the TriggerManager editor

This will enable editing of Trigger 0's values in the TriggerManager.

Select "Connect To Selected Rail" in the "Event" dropdown of the TriggerManager editor

A Trigger's Event is what happens when the Trigger detects its Target.

Select "Rail 0" in the "Selected Rail" dropdown of the TriggerManager editor

This Trigger will now connect the camera to Rail 0 when the Trigger detects its Target.

Select "-" in the "Start On Rail" dropdown of the Railcam2DCore editor

This will prevent the camera from starting on a Rail. This is required in order to see the effect of Trigger 0 (which will attach the camera to Rail 0).

Enter Play Mode

See that the camera now follows the Target object freely, but if the Target passes through Trigger 0, the camera becomes connected and is limited to Rail 0.

Exit Play Mode

Triggers provide a way to connect and disconnect the camera from Rails at specific locations in the scene, meaning multiple Rails and Triggers provide greater control of camera movement.



### VERSION LOG ###

1.0.0 -- 01 Nov 2017
- First release


