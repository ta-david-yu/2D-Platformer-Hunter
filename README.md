# 2D Platformer Hunter 
![image](https://github.com/ta-david-yu/2D-Platformer-Hunter/blob/master/platformer-preview.gif)  
[Features video links here](https://youtu.be/wnalr3_RULU)  
[I've also made a short level with it](https://youtu.be/Sj-WD9qeFmc)  
Implementation of a raycast-based 2D platformer controller in Unity.  
Extended from [Sebastian Lague's Creating a 2D Platformer series](https://youtu.be/MbWK8bCAU2w?list=PLFt_AvWsXl0f0hqURlhyIoAabKPgRsqjz.) with more customizable options and better modularization.

# Requirement
Unity 2018.3.8f1 or later

# How to install
* Add this package through Unity Package Manager with this link: https://github.com/ta-david-yu/2D-Platformer-Hunter.git

# Design

The code structure is based on a model that I call Input-Controller-Motor model. Each controller consists of three modules: Input, Controller and Motor.
Each module can be replaced with user-customized module to achieve various gameplay mechanics.
* **Input** represents the brain of a controller. The brain can be player's input or an AI. Waypoint navigation for moving platform is also a type of Input module.
* **Controller** represents the body of a controller. The body decides what a character can do, such as, double jumpping, dasing.
* **Motor** represents the physics law of a controller. For example, a character motor collides with obstacles; a platform motor can carry other motors or transforms.

Any other behaviours that do not belong to these three modules should instead be implemented in a different components and listen to events sent by three main modules.
For instance, a sprite animation controller that changes sprite when a character jumps should subscribe to OnJump event of the CharacterController.

# Features

* Variable Jump Height
* On-Slope Movement
* Air Jump
* Wall Jump
* Climbing Area - Ladder/rope climbing behaviour
* Restricted Climbing Area - Users can separate the actual movable area in climbing state with the trigger area. Controller will be smoothly interpolated from triggered position to restricted area when entering climbing state.
* One-Way Platform - Add the OneWayPlatform component to the platform which you want it to be One-Way Platform
* Moving Platform - Including a node editor for editing waypoints.
* Dash - User is able to customize dash modules that can be applied to a controller.
* Jump Input Buffering - The jump input will be buffered for a period of time when the character controller is still in the air.
* Coyote Time - Also known as grace period jumping. It allows players to register jump input in a small period of time even after moving off ledges.
* Ledge Grabbing
* Wall Climbing - You can configure whether your controller slides on wall or climbs on walls (just like how [Knuckles](https://youtu.be/-qOVqxn-JVA) does)  

# Documentation
* The collision layer of the raycaster on a controller should be different from the layer of the gameobject itself, otherwise you fly :D
* More to be added :D

# Materials
rvros - Animated Pixel Adventurer
https://rvros.itch.io/animated-pixel-hero
