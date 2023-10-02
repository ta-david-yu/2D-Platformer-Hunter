![image](https://github.com/ta-david-yu/2D-Platformer-Hunter/blob/master/platformer-preview.gif)  
# 2D Platformer Hunter 
Implementation of a raycast-based 2D platformer controller in Unity.  
Extended from [Sebastian Lague's Creating a 2D Platformer series](https://youtu.be/MbWK8bCAU2w?list=PLFt_AvWsXl0f0hqURlhyIoAabKPgRsqjz.) with more customizable options and better modularization.

<details open="open">
  <summary>Table of Contents</summary>
  <ol>
    <li>
        <a href="#description">Description</a>
    </li>
    <li>
		<a href="#installation">Installation</a>
    </li>
    <li>
        <a href="#design">Design</a>
    </li>
    <li>
        <a href="#features">Features</a>
    </li>
      <li>
        <a href="#misc-notes">Misc Notes</a>
    </li>
    <li>
        <a href="#credits">Credits</a>
    </li>
    <li>
        <a href="#license">License</a>
    </li>
  </ol>
</details>

## Description
- 2D Platformer Hunter is a 2D platforming controller that uses raycast-based collision, extended from [Sebastian Lague's "Creating a 2D Platformer" series](https://youtu.be/MbWK8bCAU2w?list=PLFt_AvWsXl0f0hqURlhyIoAabKPgRsqjz.) featuring more customization and modularity.
- [Demo Video](https://youtu.be/wnalr3_RULU)
- [Example Tutorial Level](https://youtu.be/Sj-WD9qeFmc)

## Installation
- Ensure you have the latest version of the [Unity Real-Time Development Platform](https://unity.com/download).
- Navigate to Unity's top menu tab, select Window > Package Manager, and install this package through the following link: https://github.com/ta-david-yu/2D-Platformer-Hunter.git

## Design

The code structure is based on a Input-Controller-Motor model. Each controller consists of three individual components: Input, Controller and Motor.

Each module can be replaced with user-customized module to achieve various gameplay mechanics.
* The **Input** serves as the brain of the controller. The brain can be a preprogrammed AI system or be player-controlled. Input modules also support waypoint navigation for moving platforms.
* The **Controller** represents the body of the controller. The body represents the actions a given character can perform. This includes regular movement, jumping, double-jumping, and dashing.
* The **Motor** controls the physics acting on the controller. For example, a motor for a given character can collide with obstacles in a level. A motor for a platform or other level object can carry other motors and transforms.

Other behaviours that do not belong to these three modules should instead be implemented in a different external components and set to listen to events sent out by one of these three main modules.

For instance, a sprite animation controller that swaps sprites when a character performs a jump should be set to listen to the OnJump event of the CharacterController object.

## Features

### Jumping
  - Jump Input Buffering
    - An inputted jump will be held and buffered for a period of time until the character has an actionable jump again, to improve responsivenes.
  - Coyote Time
    - Also known colloquially as "Grace Period Jumping". If a jump input is registered very shortly after a player character moves off a jumpable surface, the jump still occurs despite technically being mid-air.
  - Variable Jump Height depending on held button.
  - Wall Jump
  - Air Jump
### Climbing
  - Editable climbing area
    - Supports climbing on designated areas such as ladders and ropes. Climbing and non-climbing areas are distinguished by an area trigger. The controller will smoothly interpolate its position from the area trigger onto  the climbable area.
  - Ledge Grabbing
  - Wall Climbing
    - Configurable behavior depending on whether you prefer your controller to slide down walls or climb up them.
### Other
  - Dash modules that can be applied to controllers to alter dashing behavior.
  - Smooth movement on slopes
  - One-Way Platform component that horizontally translates a platform in one direction.
  - Moving Platform component that includes a node editor for waypoint editing.    

## Misc Notes
- The raycaster collision layer must be on different layer than the GameObject, to ensure gravity scaling and prevent unintended behavior.

## Credits
- [Character Sprites](https://rvros.itch.io/animated-pixel-hero)

## License
- Licensed under the [MIT License](LICENSE), meaning it is freely editable as long as the original copyright notice and disclaimers are included.
