# 2D Platformer Controller 
![image](https://github.com/ta-david-yu/2D-Platformer-Hunter/blob/master/platformer-preview.gif)  
![Video](https://youtu.be/wnalr3_RULU)  
Implementation of a raycast-based 2D platformer controller in Unity.  
Extended from [Sebastian Lague's Creating a 2D Platformer series](https://youtu.be/MbWK8bCAU2w?list=PLFt_AvWsXl0f0hqURlhyIoAabKPgRsqjz.) with more customizable options and better modularization.

# Requirement
Unity 2018.3.7f1 or later

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
* Climbing Area - ladder/rope climbing behaviour
* Restricted Climbing Area - Users can separate the actual movable area in climbing state with the trigger area. Controller will be smoothly interpolated from triggered position to restricted area when entering climbing state.
* One-Way Platform
* Moving Platform - Including a node editor for editing waypoints.
* Dash - User is able to customize dash modules that can be applied to a controller. A dash module describes how a controller moves during a dash action. It can either be a dodging movement or a teleport action.
* Jump Input Buffering - The jump input will be buffered for a period of time when the character controller is still in the air. Once the controller hits the ground, the buffered jump will be executed.
* Coyote Time - Also known as grace period jumping. It allows players to register jump input in a small period of time even after moving off ledges.

# Documentation
* To be added :D

# Maybe, probably, will be, future to-do list:

Most of these features are not necessary for a 2D platformer in my opinion.

| Feature   | Description |
| --------- | ------- |
| Custom Action Module (WIP) | A new "Action" state in character controller. It will be more flexible than Dash Module. Probably with an action editor that can edit frame events / curve, and maybe a hitbox editor. |
| Ability System (WIP)  | A RPG ability system. |

Features not related to platformer but important

| Feature   | Description |
| --------- | ------- |
| Using UPM | Pack this project into Unity Package managed by new Unity Package Manager. [note](https://gist.github.com/LotteMakesStuff/6e02e0ea303030517a071a1c81eb016e) |

*[UPM]: Unity Package Manager

# Materials
rvros - Animated Pixel Adventurer
https://rvros.itch.io/animated-pixel-hero
