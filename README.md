## General info
This program is a practical part of my thesis titled **Procedural creation of corridor networks in video games**

My thesis aims to study and describe existing methods used in procedural generation (especially for creating corridor networks) and propose a new approach to the problem. The outcome of the work is a computer
program made in Unity that enables users generate a 3D corridor network based on one of the following maze generation algorithms: Prim's algorithm, Kruskal's algorithm, Wilson's algorithm, recursive backtracker,
and the hunt and kill algorithm. These corridors are designed with an existing endless runner game in mind. The objective of the game is to travel from top to bottom in a relatively short time; therefore, corridors extending primarily in a vertical direction are preffered.
Additionally, the appearance of the created structure can be adjusted by changing the value of the provided parameters. These are: size, coordinates of start and finish points, number of main paths, branches
length, and placement of additional rooms. With the right adjustments, the generated corridors can be used in other game genres, not limited to endless runners.

<p align="center">
  <img src="https://github.com/ceidth/corridors_generation/assets/75451111/2d04ba32-9ded-4f00-84fd-900efb2f21a3" alt="Screenshot for the Corridors Generation project with Wilson's algorithm genereted"/>
</p>

## Generating corridor networks and implementation description
This Unity Project was made using a 2021.3.11f1 editor version. The Unity Editor is required to run it. Download it from [unity](https://unity.com/download).

The first action upon starting the program is selecting an algorithm. As mentioned above, user can choose one of the following maze generation algorithms: Prim's algorithm, Kruskal's algorithm, Wilson's algorithm, recursive backtracker, or the hunt and kill algorithm. A number of modifications were necessary in order to 3D generate corridor networks instead of a 2D maze. Firstly, each algorithm was adjusted to create a 3D maze rather than of a 2D one. Following that, further changes were implemented to reduce the number of generated fields, specific to each algorithm. Please review each algorithm's implementation in [this file](MazeGenerator3D.cs).
Mouse controls are used for camera movement: right-click to rotate the generated structure and scroll to zoom in and out.

<p align="center">
  <img src="https://github.com/ceidth/corridors_generation/assets/75451111/0a01f01a-76cc-4850-93b0-6f8800686553" alt="Screenshot for the Corridors Generation project with no algorithm chosen"/>
</p>

After selecting the algorithm, a parameter menu is displayed on the right side of the screen. The number of available parameters depends on the chosen algorithm. For algorithm that generate corridor networks with numerous branches, the option to display the branches as transparent is useful. This makes it possible to see the main path.

<p align="center">
  <img src="https://github.com/ceidth/corridors_generation/assets/75451111/10308b30-72e5-4c02-b389-60c845a2879b" alt="Screenshot for the Corridors Generation project with Kruskal's algorithm chosen but not generated"/>
</p>

When the **generate** button is clicked, the generated corridor network is displayed in the center. The color legend can be found in the top left corner.

<p align="center">
  <img src="https://github.com/ceidth/corridors_generation/assets/75451111/c6f765ac-a3ee-45cb-8228-8a4b22fcd7e5" alt="Screenshot for the Corridors Generation project with Kruskal's algorithm generated"/>
</p>

In the bottom left corner the **generate rooms** panel can be found. Rooms are displayed in blue and can be generated either anywhere or specifically on the main path and/or on the ends of branches. Rooms won't be generated next to each other or the start and finish points. The maximum number of rooms, displayed next to the corresponding slider, is calculated based on the generated structure. Please review the room generation impementation in [this file](MazeSolver.cs). Here you'll also find the implementation for finding the main path, which utilizes a recursive solver to locate the path from the start to the finish point of a corridor network.

<p align="center">
  <img src="https://github.com/ceidth/corridors_generation/assets/75451111/f438034e-2130-4138-bb36-474515232ce2" alt="Screenshot for the Corridors Generation project with Kruskal's algorithm and rooms generated"/>
</p>
