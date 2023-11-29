using Raylib_cs;

public class PruebasRayLib
{
    public static void Main()
    {
        int screenWidth = 1240;
        int screenHeight = 720;
        Raylib.InitWindow(screenWidth, screenHeight, "Galaga-Raylib");

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();



            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}