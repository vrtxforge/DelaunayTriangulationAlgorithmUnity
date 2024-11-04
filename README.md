# Delaunay Triangulation in Unity 6

## Preview Demo
![Delaunay Triangulation Demo](https://github.com/vrtxforge/DelaunayTriangulationAlgorithm/blob/master/Previews/delaunay-preview.gif)

This project implements Delaunay triangulation in Unity 6 using a coroutine-based simulation. It visualizes the triangulation process step-by-step, allowing for customizations through Unity Inspector parameters. This can be useful for understanding the mechanics of triangulation and is suitable for educational and experimental purposes in 2D computational geometry.

## Important Notes
- **In Development**: This project is still in development. Features may change, and improvements are ongoing.
- **Boundary Limitations**: The simulation is currently limited to a perfect square boundary (or any simple shape without concaves). Future versions may support more complex shapes.
- **Basic Visualization**: The Delaunay triangulation visualization is basic and primarily serves educational purposes. Further enhancements may be added to improve visual clarity and user interaction.

## Features
- Generates a set of random points within specified bounds and triangulates them.
- Visualizes the triangulation process in real time.
- Provides controls over simulation speed, boundary size, and point count.
- Displays the final triangulation result at the end of the simulation.

## Requirements
- Unity 6 (may work with later versions).

## Installation
1. Download or clone this repository.
2. Open the project in Unity or create a new Unity project and add the script files to the `Assets` folder.

## Usage
Attach the `DelaunayTriangulation` script to a GameObject in your Unity scene. Set the simulation parameters in the Unity Inspector as follows:

### Parameters
- **minBound**: Minimum bound for random point generation within the simulation area.
- **maxBound**: Maximum bound for random point generation within the simulation area.
- **simulationSpeed**: Speed at which the simulation progresses (lower values make the visualization slower).
- **maxPointCount**: Maximum number of random points generated within the simulation area.
- **superTriangleScale**: Scale factor for the super triangle used to enclose all points during triangulation.

### Classes
- **DelaunayTriangulation**: Main class controlling the Delaunay triangulation simulation and visualization.
- **Triangle**: Represents a triangle in the triangulation, with methods to check if a point lies within its circumcircle or if it contains a specific point.
- **Edge**: Represents an edge between two points with equality checks for undirected edges.

## Simulation Steps
1. **Generate Super Triangle**: A large triangle that encloses all randomly generated points is created.
2. **Random Point Generation**: Points are randomly generated within the specified bounds.
3. **Triangulation Process**: Each point is added to the triangulation, updating the triangles in real time.
4. **Finalization**: Triangles connected to the vertices of the super triangle are removed to leave only the triangulation of the generated points.

## Example Usage
1. Attach the script to an empty GameObject.
2. Adjust parameters in the Inspector as desired.
3. Play the scene to see the triangulation process in action.
4. The final triangulated mesh is displayed in red once the simulation is complete.

## Code Structure
- **Start()**: Initializes the simulation, creates bounds, and starts the triangulation coroutine.
- **GenerateSuperTriangle()**: Creates the initial super triangle to enclose all points.
- **TriangulateCoroutine()**: Manages the step-by-step triangulation, updating the display after each point insertion.
- **InsertPoint()**: Handles point insertion and triangle adjustment based on Delaunay conditions.
- **DrawTriangles()**: Visualizes triangles in the scene using `Debug.DrawLine`.
- **OnDrawGizmos()**: Displays points as red spheres in the Scene view.

## Example Inspector Settings
```plaintext
Main Simulation Parameter
- Min Bound: -5
- Max Bound: 5
- Simulation Speed: 0.2

Max Point Count: 15
Super Triangle Scale: 3
