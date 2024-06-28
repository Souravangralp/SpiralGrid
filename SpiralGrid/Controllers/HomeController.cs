using Microsoft.AspNetCore.Mvc;
using SpiralGrid.Models;
using System.Diagnostics;

namespace SpiralGrid.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Action method for the Index page.
        /// </summary>
        /// <param name="size">Optional size parameter for the spiral grid.</param>
        /// <param name="targets">List of target numbers to find intersected numbers.</param>
        /// <returns>Returns a view populated with a SpiralGridViewModel.</returns>
        public IActionResult Index(int? size, List<int> targets)
        {
            // Initialize the view model for the Spiral Grid
            SpiralGridViewModel model = new SpiralGridViewModel();

            // Generate the spiral grid with the specified size or default to 10 if size is null
            model.Grid = GetSpiralGrid(size ?? 10);

            // Set the list of target numbers in the model
            model.Targets = targets;

            // Find intersected numbers for each target number and add to the model
            foreach (var target in targets)
            {
                model.IntersectedNumbers.Add(FindIntersectedNumbers(target, size ?? 10, model.Grid));
            }

            // Return the populated model to the view
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Generates a spiral grid of integers with numbers arranged in a spiral pattern.
        /// </summary>
        /// <param name="size">The size of the grid (assumed to be square).</param>
        /// <returns>A 2D array representing the spiral grid filled with integers.</returns>
        public int[,] GetSpiralGrid(int size)
        {
            // Initialize the size of the grid
            int n = size;
            int[,] grid = new int[n, n]; // Create a n x n grid

            // Start filling the grid with the largest number
            int num = n * n;

            // Define the directions for movement: right, down, left, up
            int[,] coordinates = { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } };
            int directionIndex = 0; // Start with moving right
            int row = 0, col = 0; // Start position at the top-left corner

            // Fill the grid in a spiral pattern
            for (int i = 0; i < n * n; i++)
            {
                grid[row, col] = num; // Assign the current number to the grid cell
                num--; // Decrement the number for the next cell

                // Calculate the next position based on the current direction
                int nextRow = row + coordinates[directionIndex, 0];
                int nextCol = col + coordinates[directionIndex, 1];

                // Check if the next position is within bounds and not yet filled
                if (0 <= nextRow && nextRow < n && 0 <= nextCol && nextCol < n && grid[nextRow, nextCol] == 0)
                {
                    // If valid, move to the next position
                    row = nextRow;
                    col = nextCol;
                }
                else
                {
                    // If not valid, change the direction (right -> down -> left -> up -> right)
                    directionIndex = (directionIndex + 1) % 4;
                    // Move to the next position in the new direction
                    row += coordinates[directionIndex, 0];
                    col += coordinates[directionIndex, 1];
                }
            }

            // Return the filled spiral grid
            return grid;
        }


        /// <summary>
        /// Finds numbers intersected by a line from the minimum value coordinates to the specified number m in a 2D grid.
        /// </summary>
        /// <param name="m">The target number to find in the grid.</param>
        /// <param name="size">The size of the grid (assumed to be square).</param>
        /// <param name="grid">The 2D array representing the grid.</param>
        /// <returns>A list of numbers intersected by the line from the minimum value coordinates to m.</returns>
        /// <exception cref="ArgumentException">Thrown when the number m is not found in the grid.</exception>
        public static List<int> FindIntersectedNumbers(int m, int size, int[,] grid)
        {
            // Initialize the target coordinates for the number m
            int targetRow = -1, targetCol = -1;

            // Search for the number m in the grid to find its coordinates
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (grid[i, j] == m)
                    {
                        targetRow = i;
                        targetCol = j;
                        break; // Exit the inner loop if the number is found
                    }
                }

                if (targetRow != -1)
                    break; // Exit the outer loop if the number is found
            }

            // If the number m is not found in the grid, throw an exception
            if (targetRow == -1 || targetCol == -1)
            {
                throw new ArgumentException("The number is not found in the grid.");
            }

            // Find the coordinates of the minimum value in the grid
            (int, int) originCoordinates = FindCoordinatesOfMinValue(size, grid);

            // Bresenham's line algorithm to find intersected numbers
            List<int> intersectedNumbers = new List<int>();
            int x1 = originCoordinates.Item1, y1 = originCoordinates.Item2; // Starting coordinates
            int x2 = targetRow, y2 = targetCol; // Ending coordinates (coordinates of m)

            int dx = Math.Abs(x2 - x1); // Difference in x-coordinates
            int dy = Math.Abs(y2 - y1); // Difference in y-coordinates

            int sx = x1 < x2 ? 1 : -1; // Step direction for x
            int sy = y1 < y2 ? 1 : -1; // Step direction for y

            int value = dx - dy; // Initial value term

            // Iterate until the line from (x1, y1) to (x2, y2) is complete
            while (true)
            {
                // Add the current grid value to the list of intersected numbers
                intersectedNumbers.Add(grid[x1, y1]);

                // If the line endpoint is reached, exit the loop
                if (x1 == x2 && y1 == y2)
                    break;

                // Calculate the value term for the next step
                int e2 = 2 * value;

                // Adjust the value term and coordinates based on the Bresenham algorithm
                if (e2 > -dy)
                {
                    value -= dy;
                    x1 += sx; // Move in the x direction
                }
                if (e2 < dx)
                {
                    value += dx;
                    y1 += sy; // Move in the y direction
                }
            }

            // Return the list of intersected numbers
            return intersectedNumbers;
        }

        /// <summary>
        /// Finds the coordinates of the minimum value in a 2D grid of integers.
        /// </summary>
        /// <param name="size">The size of the grid (assumed to be square).</param>
        /// <param name="grid">The 2D array representing the grid.</param>
        /// <returns>A tuple containing the row and column indices of the minimum value.</returns>
        public static (int, int) FindCoordinatesOfMinValue(int size, int[,] grid)
        {
            // Initialize variables to track the minimum value and its coordinates
            int minValue = int.MaxValue;
            int minRow = -1, minCol = -1;

            // Iterate through each cell in the grid
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    // Check if the current cell value is smaller than the current minimum
                    if (grid[i, j] < minValue)
                    {
                        // Update the minimum value and its coordinates
                        minValue = grid[i, j];
                        minRow = i;
                        minCol = j;
                    }
                }
            }

            // Return the coordinates of the minimum value
            return (minRow, minCol);
        }
    }
}
