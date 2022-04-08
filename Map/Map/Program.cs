using System;
using System.Collections.Generic;


namespace AdventureMap
{
	internal class Program
	{
		//Set width and height for the map
		const int width = 100;
		const int height = 30;

		const char forest = 'T';
		const char borderCornerUpperLeft = '\u250F';
		const char borderCornerUpperRight = '\u2513';
		const char borderCornerLowerRight = '\u251B';
		const char borderCornerLowerLeft = '\u2517';
		const char borderHorizontal = '\u2500';
		const char borderVertical = '\u00A6';
		const char road = '#';
		const char curveVertical = '|';
		const char curveLeft = '/';
		const char curveRight = '\\';
		const char bridgeRailing = '=';
		const char bridge = '#';
		const char leftTurrent = '[';
		const char rightTurrent = ']';
		const string mapTitle = "ADVENTURE MAP";

		static char curveTile;
		static int riverCrossingX;
		static int riverCrossingY;
		static int wallCrossingX;
		static int wallCrossingY;

		static char[,] map;
		static List<int> riverPos = new List<int>();

		static void Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.UTF8;

			GenerateMap(width, height);
			DrawMap();
		}

		static void GenerateMap(int width, int height)
		{
			//Initiate 2D array of char and fill it with empty char
			map = new char[width, height];

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					map[x, y] = ' ';
				}
			}
			GenerateForest();
			GenerateMainRoad();
			GenerateRiver();
			GenerateBridge();
			GenerateSmallRoad();
			GenerateWall();
			GenerateTurrent();
			GenerateBoarder();
			GenerateMapTitle();
		}

		static void GenerateForest()
		{
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					//First two columns should allways be forest tiles
					if (x == 1 || x == 2)
					{
						SetTile(x, y, forest);
						continue;
					}
					//Randomly generate trees with lesser frequensy the further to the right of the map we are
					if (0 == GetRandom(x))
					{
						SetTile(x, y, forest);
					}
				}
			}
		}

		static void GenerateMainRoad()
		{
			//The road across the map starts at middle of the map from left to right
			int startY = height / 2;

			//Set bounderies for how far up and down the road can be generated
			int minY = height / 5;
			int maxY = height * 4 / 5;

			//Will determine if the road goes up, down or straight
			int direction;

			//First road tile should start just after the left border of map
			SetTile(1, startY, road);
			for (int x = 2; x < width; x++)
			{
				direction = GetRandom(10);
				//Get a 10% chans of the road going either up or down
				if (direction == 0)
				{
					startY--;
				}
				else if (direction == 1)
				{
					startY++;
				}
				SetTile(x, ClampInt(minY, maxY, startY), road);
			}
		}

		static void GenerateCurve(double xStartPosProcent, int randomizeValue, bool isRiverCurve, out int crossingX, out int crossingY, out char tile)
		{
			//Starting position of curved tiles where xStartPosProcent is a double between 0 and 1 representing procential of distance from left
			int startX = (int)((double)width * xStartPosProcent);
			int direction;
			//Used to determine where the curved tiles intersect with road
			crossingX = 0;
			crossingY = 0;
			//Used to set tile to be generated based on which direction the curve turns
			tile = ' ';
			for (int y = 1; y < height; y++)
			{
				//Set tile as straight at beginning of each iteration before evaluating if curve turns
				tile = curveVertical;
				direction = GetRandom(randomizeValue);
				if (direction == 0)
				{
					startX--;
					tile = curveLeft;
				}
				else if (direction == 1)
				{
					startX++;
					tile = curveRight;
				}

				if (map[startX - 1, y] == road || map[startX, y] == road || map[startX + 1, y] == road)
				{
					crossingX = startX;
					crossingY = y;
				}
				if (isRiverCurve)
				{
					//Ad direction of river curve to a List so a road can trace the path of the curve 
					riverPos.Add(startX);
					CurveStep(startX, y, 3);
				}
				else
				{
					CurveStep(startX, y, 2);
				}
			}
		}

		static void CurveStep(int xPos, int yPos, int steps)
		{
			//Sets how wide(x-steps) the curve is
			for (int x = xPos; x < xPos + steps; x++)
			{
				SetTile(x, yPos, curveTile);
			}
		}

		static void GenerateRiver()
		{
			GenerateCurve(0.75, 6, true, out riverCrossingX, out riverCrossingY, out curveTile);
		}

		static void GenerateWall()
		{
			GenerateCurve(0.25, 6, false, out wallCrossingX, out wallCrossingY, out curveTile);
		}

		static void GenerateTurrent()
		{
			//Place turrent tower next to crossing with road
			SetTile(wallCrossingX, wallCrossingY - 1, leftTurrent);
			SetTile(wallCrossingX + 1, wallCrossingY - 1, rightTurrent);

			SetTile(wallCrossingX, wallCrossingY, road);
			SetTile(wallCrossingX + 1, wallCrossingY, road);

			//Place turrent tower next to crossing with road
			SetTile(wallCrossingX, wallCrossingY + 1, leftTurrent);
			SetTile(wallCrossingX + 1, wallCrossingY + 1, rightTurrent);
		}

		static void GenerateBridge()
		{
			for (int x = riverCrossingX - 2; x <= riverCrossingX + 5; x++)
			{
				SetTile(x, riverCrossingY - 1, bridgeRailing);
				SetTile(x, riverCrossingY, bridge);
				SetTile(x, riverCrossingY + 1, bridgeRailing);
			}
			//Place road tiles along beginning and end of bridge so the transition from bridge to road looks smoother

			SetTile(riverCrossingX - 3, riverCrossingY - 1, road);
			SetTile(riverCrossingX - 3, riverCrossingY, road);
			SetTile(riverCrossingX - 3, riverCrossingY + 1, road);

			SetTile(riverCrossingX + 6, riverCrossingY - 1, road);
			SetTile(riverCrossingX + 6, riverCrossingY, road);
			SetTile(riverCrossingX + 6, riverCrossingY + 1, road);
		}

		static void GenerateSmallRoad()
		{
			//The small road traces the path of the river
			int x = riverCrossingY;
			for (int y = riverCrossingY + 1; y < height; y++)
			{
				if (x <= 0)
				{
					x = 0;
				}
				if (x >= riverPos.Count - 1)
				{
					x = riverPos.Count - 1;
				}
				SetTile(riverPos[x] - 4, y, road);
				x++;
			}
		}

		static void GenerateBoarder()
		{
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (y == 0 || y == height - 1)
					{
						map[x, y] = borderHorizontal;
						continue;
					}

					if (x == 0 || x == width - 1)
					{
						map[x, y] = borderVertical;
					}
				}
			}

			map[0, 0] = borderCornerUpperLeft;
			map[width - 1, 0] = borderCornerUpperRight;
			map[width - 1, height - 1] = borderCornerLowerRight;
			map[0, height - 1] = borderCornerLowerLeft;
		}

		static void GenerateMapTitle()
		{
			//Print map title at top middle of map
			int titleXPos = (width - mapTitle.Length) / 2;
			int titleLetterIndex = 0;
			//Print each letter of string one at a time
			for (int x = titleXPos; x < titleXPos + mapTitle.Length; x++)
			{
				SetTile(x, 1, mapTitle[titleLetterIndex]);
				titleLetterIndex++;
			}
		}

		static void SetTile(int xPos, int yPos, char tileSymbol)
		{
			//Add tile to map array, clamping so it doesn't pertrude its edges
			map[ClampInt(1, width, xPos), ClampInt(1, height - 1, yPos)] = tileSymbol;
		}

		static void DrawMap()
		{
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					Console.BackgroundColor = ConsoleColor.Green; //Grass tiles
					if (map[x, y] == forest)
					{
						Console.BackgroundColor = ConsoleColor.DarkGreen;
						Console.ForegroundColor = ConsoleColor.Black;
					}
					if (map[x, y] == road)
					{
						Console.BackgroundColor = ConsoleColor.Yellow;
						Console.ForegroundColor = ConsoleColor.DarkGray;
					}
					if (map[x, y] == curveVertical || map[x, y] == curveLeft || map[x, y] == curveRight)
					{
						if (x > width / 2)
						{
							//River tiles 
							Console.BackgroundColor = ConsoleColor.DarkBlue;
							Console.ForegroundColor = ConsoleColor.Blue;
						}
						else
						{
							//Wall tiles
							Console.BackgroundColor = ConsoleColor.Gray;
							Console.ForegroundColor = ConsoleColor.Black;
						}
					}
					if (map[x, y] == bridgeRailing || map[x, y] == leftTurrent || map[x, y] == rightTurrent)
					{
						Console.BackgroundColor = ConsoleColor.Gray;
						Console.ForegroundColor = ConsoleColor.Black;
					}
					if (map[x, y] == borderHorizontal || map[x, y] == borderVertical || map[x, y] == borderCornerUpperLeft || map[x, y] == borderCornerUpperRight || map[x, y] == borderCornerLowerRight || map[x, y] == borderCornerLowerLeft)
					{
						Console.BackgroundColor = ConsoleColor.Black;
						Console.ForegroundColor = ConsoleColor.DarkYellow;
					}
					if (y == 1 && x >= (width - mapTitle.Length) / 2 && x < width - (width - mapTitle.Length) / 2 - 1)
					{
						Console.BackgroundColor = ConsoleColor.Black;
						Console.ForegroundColor = ConsoleColor.DarkYellow;
					}
					Console.Write(map[x, y]);
				}
				Console.WriteLine("");
			}
		}

		static int ClampInt(int min, int max, int valueToClamp)
		{
			if (valueToClamp < min)
			{
				return min;
			}

			if (valueToClamp >= max)
			{
				return max - 1;
			}

			return valueToClamp;
		}

		static int GetRandom(int max)
		{
			Random random = new Random();
			int result = random.Next(max);
			return result;
		}
	}
}
