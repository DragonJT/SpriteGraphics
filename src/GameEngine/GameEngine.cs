namespace GameEngine;

static class Triangle {
    static float sign (Vector2 p1, Vector2 p2, Vector2 p3){
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    public static bool PointInTriangle (Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3){
        float d1, d2, d3;
        bool has_neg, has_pos;

        d1 = sign(pt, v1, v2);
        d2 = sign(pt, v2, v3);
        d3 = sign(pt, v3, v1);

        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }
}

public class Mesh2D {
    public List<Vector2> vertices = [];
    public List<uint> triangles = [];

    public bool Contains(Vector2 point) {
        for(var t=0;t<triangles.Count;t+=3){
            var a = vertices[(int)triangles[t]];
            var b = vertices[(int)triangles[t+1]];
            var c = vertices[(int)triangles[t+2]];
            if(Triangle.PointInTriangle(point,a,b,c)){
                return true;
            }
        }
        return false;
    }

    public Rect GetBounds(){
        var min = vertices[0];
        var max = vertices[0];
        for(var i = 1;i < vertices.Count;i++){
            if(vertices[i].x < min.x){
                min.x = vertices[i].x;
            }
            if(vertices[i].x > max.x){
                max.x = vertices[i].x;
            }
            if(vertices[i].y < min.y){
                min.y = vertices[i].y;
            }
            if(vertices[i].y > max.y){
                max.y = vertices[i].y;
            }
        }
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }

    public Vector2 Center => GetBounds().Center;

    public void Translate(Vector2 translate) {
        for(var i=0;i<vertices.Count;i++) {
            vertices[i] = vertices[i] + translate;
        }
    }

    void DrawPoly(List<Vector2> points){
        uint vertexID = (uint)vertices.Count;
        vertices.AddRange(points);
        for(uint i=2;i<points.Count;i++) {
            triangles.AddRange([vertexID, vertexID+i-1, vertexID+i]);
        }
    }

    public void DrawRect(Rect rect) {
        List<Vector2> points = [
            new Vector2(rect.x, rect.y),
            new Vector2(rect.x + rect.width, rect.y),
            new Vector2(rect.x + rect.width, rect.y + rect.height),
            new Vector2(rect.x, rect.y + rect.height)];
        DrawPoly(points);
    }

    void DrawEllipse(Rect rect, int count) {
        var points = new List<Vector2>();
        var delta = MathF.PI * 2f / count;
        var radians = 0f;
        for(var i=0;i<count;i++) {
            points.Add(rect.Center + new Vector2(MathF.Cos(radians) * rect.width/2f, MathF.Sin(radians) * rect.height/2f));
            radians += delta;
        }
        DrawPoly(points);
    }

    void DrawRectBorder(Rect rect, float border) {
        DrawRect(new Rect(rect.x, rect.y, rect.width, border));
        DrawRect(new Rect(rect.x, rect.y, border, rect.height));
        DrawRect(new Rect(rect.x, rect.y + rect.height - border, rect.width, border));
        DrawRect(new Rect(rect.x + rect.width - border, rect.y, border, rect.height));
    }

    public static Mesh2D Rect(Rect rect){
        var mesh = new Mesh2D();
        mesh.DrawRect(rect);
        return mesh;
    }

    public static Mesh2D Ellipse(Rect rect, int count){
        var mesh = new Mesh2D();
        mesh.DrawEllipse(rect, count);
        return mesh;
    }

    public static Mesh2D RectBorder(Rect rect, float border){
        var mesh = new Mesh2D();
        mesh.DrawRectBorder(rect, border);
        return mesh;
    }
}

public struct Rect(float x, float y, float width, float height){
    public float x = x;
    public float y = y;
    public float width = width;
    public float height = height;

    public Vector2 Fraction(Vector2 v){
        return new Vector2(JMath.InverseLerp(x, x+width, v.x), JMath.InverseLerp(y, y+height, v.y));
    }

    public Vector2 FromFraction(Vector2 f){
        return new Vector2(JMath.Lerp(x, x+width, f.x), JMath.Lerp(y, y+height, f.y));
    }

    public static Rect CreateFromCenterSize(Vector2 center, Vector2 size){
        var min = center - size/2f;
        return new Rect(min.x, min.y, size.x, size.y);
    }

    public static Rect CreateFromStartEnd(Vector2 start, Vector2 end){
        var center = (start + end) / 2f;
        var size = new Vector2(MathF.Abs(end.x - start.x), MathF.Abs(end.y - start.y));
        return CreateFromCenterSize(center, size);
    }

    public bool Contains(Vector2 point) {
        return point.x > x && point.y > y && point.x < x + width && point.y < y + height;
    }

    public Vector2 Center => new (x + width/2f, y + height/2f);
    public Vector2 Size => new Vector2(width, height);

    public override string ToString(){
        return "("+x+","+y+","+width+","+height+")";
    }
}


public struct Color255(byte r, byte g, byte b, byte a = 255){
    public byte r = r;
    public byte g = g;
    public byte b = b;
    public byte a = a;

    public static Color255 Create(Color color) {
        return new Color255((byte)(color.r*255), (byte)(color.g*255), (byte)(color.b*255));
    }
}

public struct Color(float r, float g, float b, float a = 1){
    public float r = r;
    public float g = g;
    public float b = b;
    public float a = a;

    public static Color Blue => new (0,0,1);
    public static Color LightCyan => new (0.3f,1,1);
    public static Color DarkCyan => new (0,0.3f,0.3f);
    public static Color Red => new (1,0,0);
    public static Color Yellow => new (1,1,0);
    public static Color Green => new (0,1,0);
    public static Color Black => new (0,0,0);
    public static Color White => new (1,1,1);
    public static Color Magenta => new (1,0,1);
    public static Color Orange => new (1,0.5f,0);

    public override string ToString() {
        return "("+r+","+g+","+b+","+a+")";
    }
}

public struct Vector2(float x, float y){
    public float x = x;
    public float y = y;
    public static Vector2 Zero => new (0,0);

    public float Length(){
        return MathF.Sqrt(x*x + y*y);
    }

    public Vector2 Normalized(){
        var length = Length();
        return new Vector2(x/length, y/length);
    }

    public static Vector2 operator -(Vector2 a, Vector2 b){
        return new Vector2(a.x - b.x, a.y - b.y);
    }

    public static Vector2 operator -(Vector2 v){
        return new Vector2(-v.x, -v.y);
    }

    public static Vector2 operator +(Vector2 a, Vector2 b){
        return new Vector2(a.x + b.x, a.y + b.y);
    }

    public static Vector2 operator *(Vector2 v, float f){
        return new Vector2(v.x * f, v.y * f);
    }

    public static Vector2 operator *(float f, Vector2 v){
        return new Vector2(v.x * f, v.y * f);
    }

    public static Vector2 operator /(Vector2 v, float f){
        return new Vector2(v.x / f, v.y / f);
    }

    public static Vector2 GetDirection(Vector2 a, Vector2 b){
        return (b-a).Normalized();
    }

    public Vector2 PerpendicularClockwise(){
        return new Vector2(y, -x);
    }

    public Vector2 PerpendicularCounterClockwise(){
        return new Vector2(-y, x);
    }

    public static Vector2 Lerp(Vector2 a, Vector2 b, float amount){
        return new Vector2(float.Lerp(a.x, b.x, amount), float.Lerp(a.y, b.y, amount));
    }

    public static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, float amount){
        return Lerp(Lerp(a,b,amount), Lerp(b,c,amount), amount);
    }

    public static float Dot(Vector2 a, Vector2 b){
        return a.x * b.x + a.y * b.y;
    }

    public Vector2 Abs(){
        return new Vector2(MathF.Abs(x), MathF.Abs(y));
    }

    public Vector2 Rotate(float angleDegrees){
        float angleRadians = angleDegrees * (float)Math.PI / 180.0f;
        float rotatedX = x * (float)Math.Cos(angleRadians) - y * (float)Math.Sin(angleRadians);
        float rotatedY = x * (float)Math.Sin(angleRadians) + y * (float)Math.Cos(angleRadians);
        return new(rotatedX, rotatedY);
    }

    public Vector2 Translate(Vector2 translate){
        return new Vector2(x + translate.x, y + translate.y);
    }

    public override string ToString(){
        return "("+x+","+y+")";
    }

    public static Vector2 GetDirection(float angleDegrees)
    {
        float angleRadians = angleDegrees * (float)Math.PI / 180.0f;
        return new Vector2((float)Math.Cos(angleRadians), (float)Math.Sin(angleRadians));
    }
}

public struct Vector2i(int x, int y){
    public int x = x;
    public int y = y;
}

public static class Screen{
    public static int width = 2000;
    public static int height = 1400;
}

public static class Input{
    public const int KEY_SPACE = 32;
    public const int KEY_APOSTROPHE = 39;  /* ' */
    public const int KEY_COMMA = 44;  /* , */
    public const int KEY_MINUS = 45;  /* - */
    public const int KEY_PERIOD = 46;  /* . */
    public const int KEY_SLASH = 47;  /* / */
    public const int KEY_0 = 48;
    public const int KEY_1 = 49;
    public const int KEY_2 = 50;
    public const int KEY_3 = 51;
    public const int KEY_4 = 52;
    public const int KEY_5 = 53;
    public const int KEY_6 = 54;
    public const int KEY_7 = 55;
    public const int KEY_8 = 56;
    public const int KEY_9 = 57;
    public const int KEY_SEMICOLON = 59;  /* ; */
    public const int KEY_EQUAL = 61;  /* = */
    public const int KEY_A = 65;
    public const int KEY_B = 66;
    public const int KEY_C = 67;
    public const int KEY_D = 68;
    public const int KEY_E = 69;
    public const int KEY_F = 70;
    public const int KEY_G = 71;
    public const int KEY_H = 72;
    public const int KEY_I = 73;
    public const int KEY_J = 74;
    public const int KEY_K = 75;
    public const int KEY_L = 76;
    public const int KEY_M = 77;
    public const int KEY_N = 78;
    public const int KEY_O = 79;
    public const int KEY_P = 80;
    public const int KEY_Q = 81;
    public const int KEY_R = 82;
    public const int KEY_S = 83;
    public const int KEY_T = 84;
    public const int KEY_U = 85;
    public const int KEY_V = 86;
    public const int KEY_W = 87;
    public const int KEY_X = 88;
    public const int KEY_Y = 89;
    public const int KEY_Z = 90;
    public const int KEY_LEFT_BRACKET = 91;  /* [ */
    public const int KEY_BACKSLASH = 92;  /* \ */
    public const int KEY_RIGHT_BRACKET = 93;  /* ] */
    public const int KEY_GRAVE_ACCENT = 96;  /* ` */
    public const int KEY_WORLD_1 = 161; /* non-US #1 */
    public const int KEY_WORLD_2 = 162; /* non-US #2 */

    /* Function keys */
    public const int KEY_ESCAPE = 256;
    public const int KEY_ENTER = 257;
    public const int KEY_TAB = 258;
    public const int KEY_BACKSPACE = 259;
    public const int KEY_INSERT = 260;
    public const int KEY_DELETE = 261;
    public const int KEY_RIGHT = 262;
    public const int KEY_LEFT = 263;
    public const int KEY_DOWN = 264;
    public const int KEY_UP = 265;
    public const int KEY_PAGE_UP = 266;
    public const int KEY_PAGE_DOWN = 267;
    public const int KEY_HOME = 268;
    public const int KEY_END = 269;
    public const int KEY_CAPS_LOCK = 280;
    public const int KEY_SCROLL_LOCK = 281;
    public const int KEY_NUM_LOCK = 282;
    public const int KEY_PRINT_SCREEN = 283;
    public const int KEY_PAUSE = 284;
    public const int KEY_F1 = 290;
    public const int KEY_F2 = 291;
    public const int KEY_F3 = 292;
    public const int KEY_F4 = 293;
    public const int KEY_F5 = 294;
    public const int KEY_F6 = 295;
    public const int KEY_F7 = 296;
    public const int KEY_F8 = 297;
    public const int KEY_F9 = 298;
    public const int KEY_F10 = 299;
    public const int KEY_F11 = 300;
    public const int KEY_F12 = 301;
    public const int KEY_F13 = 302;
    public const int KEY_F14 = 303;
    public const int KEY_F15 = 304;
    public const int KEY_F16 = 305;
    public const int KEY_F17 = 306;
    public const int KEY_F18 = 307;
    public const int KEY_F19 = 308;
    public const int KEY_F20 = 309;
    public const int KEY_F21 = 310;
    public const int KEY_F22 = 311;
    public const int KEY_F23 = 312;
    public const int KEY_F24 = 313;
    public const int KEY_F25 = 314;
    public const int KEY_KP_0 = 320;
    public const int KEY_KP_1 = 321;
    public const int KEY_KP_2 = 322;
    public const int KEY_KP_3 = 323;
    public const int KEY_KP_4 = 324;
    public const int KEY_KP_5 = 325;
    public const int KEY_KP_6 = 326;
    public const int KEY_KP_7 = 327;
    public const int KEY_KP_8 = 328;
    public const int KEY_KP_9 = 329;
    public const int KEY_KP_DECIMAL = 330;
    public const int KEY_KP_DIVIDE = 331;
    public const int KEY_KP_MULTIPLY = 332;
    public const int KEY_KP_SUBTRACT = 333;
    public const int KEY_KP_ADD = 334;
    public const int KEY_KP_ENTER = 335;
    public const int KEY_KP_EQUAL = 336;
    public const int KEY_LEFT_SHIFT = 340;
    public const int KEY_LEFT_CONTROL = 341;
    public const int KEY_LEFT_ALT = 342;
    public const int KEY_LEFT_SUPER = 343;
    public const int KEY_RIGHT_SHIFT = 344;
    public const int KEY_RIGHT_CONTROL = 345;
    public const int KEY_RIGHT_ALT = 346;
    public const int KEY_RIGHT_SUPER = 347;
    public const int KEY_MENU = 348;

    public const int RELEASE = 0;
    public const int PRESS = 1;
    public const int REPEAT = 2;
    
    public const int MOUSE_BUTTON_1 = 0;
    public const int MOUSE_BUTTON_2 = 1;
    public const int MOUSE_BUTTON_LEFT = MOUSE_BUTTON_1;
    public const int MOUSE_BUTTON_RIGHT = MOUSE_BUTTON_2;

    static List<int> keys = [];
    static List<int> keysDown = [];
    static List<int> keysUp = [];
    static List<int> buttons = [];
    static List<int> buttonsDown = [];
    static List<int> buttonsUp = [];
    static bool used = false;
    public static Vector2 MousePosition {get; private set;}
    public static Vector2 DeltaMousePosition {get; private set;}
    static string chars = "";

    public static string Chars => used ? "" : chars;

    public static bool GetKey(int key) {
        return !used && keys.Contains(key);
    }

    public static bool GetKeyDown(int key) {
        return !used && keysDown.Contains(key);
    }

    public static bool GetKeyUp(int key) {
        return !used && keysUp.Contains(key);
    }

    public static bool GetButton(int button) {
        return !used && buttons.Contains(button);
    }

    public static bool GetButtonDown(int button) {
        return !used && buttonsDown.Contains(button);
    }

    public static bool GetButtonUp(int button) {
        return !used && buttonsUp.Contains(button);
    }

    internal static void MouseButtonCallback(int button, int action, int mods){
        if(action == PRESS){
            buttons.Add(button);
            buttonsDown.Add(button);
        }
        else if(action == RELEASE){
            buttons.Remove(button);
            buttonsUp.Add(button);
        }
    }

    internal static void KeyCallback(int key, int scancode, int action, int mods){
        if(action == PRESS){
            keys.Add(key);
            keysDown.Add(key);
        }
        else if(action == RELEASE){
            keys.Remove(key);
            keysUp.Add(key);
        }
    }

    internal static void CharCallback(uint codepoint){
        chars += (char)codepoint;
    }

    internal static void Start() {
        MousePosition = GLFWHelper.GetCursorPosition();
    }

    internal static void EarlyUpdate() {
        var newMousePosition = GLFWHelper.GetCursorPosition();
        DeltaMousePosition = newMousePosition - MousePosition;
        MousePosition = newMousePosition;
    }

    internal static void LateUpdate() {
        buttonsDown.Clear();
        keysDown.Clear();
        buttonsUp.Clear();
        keysUp.Clear();
        used = false;
        chars = "";
    }

    public static void Use(){
        used = true;
    }
}

public static class Time {
    static double lastFrame;
    static double time;
    static System.Diagnostics.Stopwatch stopwatch = new();

    public static float DeltaTime => (float)(time - lastFrame);

    public static void Start(){
        lastFrame = 0;
        time = 0;
        stopwatch.Start();
    }

    public static void Update(){
        lastFrame = time;
        time = stopwatch.Elapsed.TotalSeconds;
    }
}

public static class JRandom {
    static Random rand = new();

    public static Vector2 GetVector2(Rect rect){
        return new Vector2(rect.x + rand.NextSingle() * rect.width, rect.y + rand.NextSingle() * rect.height);
    }

    public static Color GetColor(){
        return new Color(rand.NextSingle(), rand.NextSingle(), rand.NextSingle());
    }
}

public static class JMath {
    public const float DegreesToRadians = 0.0174533f;
    public const float RadiansToDegrees = 57.2958f;

    public static float Lerp(float start, float end, float t) {
        return start + (end - start) * t;
    }

    public static float InverseLerp(float start, float end, float value) {
        return (value - start) / (end - start);
    }

    public static float FindAngle(Vector2 a, Vector2 b){
        float dx = b.x - a.x;
        float dy = b.y - a.y;
        float angleInRadians = MathF.Atan2(dy, dx);
        float angleInDegrees = RadiansToDegrees * angleInRadians;
        return angleInDegrees;
    }
}

public abstract class Game {
    public virtual void Awake() {}
    public abstract void Draw();
    public virtual void KeyCallback(int key, int scancode, int action, int mods){}
    public virtual void WindowSizeCallback(int width, int height){}
    public virtual void CursorPosCallback(double xpos, double ypos){}
    public virtual void MouseButtonCallback(int button, int action, int mods){}
    public virtual void CharCallback(uint codepoint) {}
}

public static class GameEngine {
    internal static Buffer memory = new (5000);
    internal static IntPtr window;
    static Game? game;

    static void WindowSizeCallback(IntPtr window, int width, int height){
        Screen.width = width;
        Screen.height = height;
        game!.WindowSizeCallback(width, height);
    }

    static void KeyCallback(IntPtr window, int key, int scancode, int action, int mods){
        Input.KeyCallback(key, scancode, action, mods);
        game!.KeyCallback(key, scancode, action, mods);
    }

    static void MouseButtonCallback(IntPtr window, int button, int action, int mods) {
        Input.MouseButtonCallback(button, action, mods);
        game!.MouseButtonCallback(button, action, mods);
    }

    static void CursorPosCallback(IntPtr window, double xpos, double ypos){
        game!.CursorPosCallback(xpos, ypos);
    }

    static void CharCallback(IntPtr window, uint codepoint){
        Input.CharCallback(codepoint);
        game!.CharCallback(codepoint);
    }

    public static void Create(Game game){
        GameEngine.game = game;
        if(GLFW.glfwInit() == 0){
            throw new Exception("Can't init glfw");
        }
        window = GLFW.glfwCreateWindow(Screen.width, Screen.height, game.GetType().Name, IntPtr.Zero, IntPtr.Zero);
        GLFW.glfwMakeContextCurrent(window);
        GL.Init(GLFW.glfwGetProcAddress);
        
        Input.Start();
        GLFWHelper.SetKeyCallback(KeyCallback);
        GLFWHelper.SetMouseButtonCallback(MouseButtonCallback);
        GLFWHelper.SetWindowSizeCallback(WindowSizeCallback);
        GLFWHelper.SetCursorPosCallback(CursorPosCallback);
        GLFWHelper.SetCharCallback(CharCallback);

        Graphics.Init();
        Time.Start();
        game.Awake();

        while(GLFW.glfwWindowShouldClose(window) == 0) {
            memory.size = 0;
            Time.Update();
            Input.EarlyUpdate();
            game.Draw();
            Input.LateUpdate();

            GLFW.glfwSwapBuffers(window);
            GLFW.glfwPollEvents();
        }
    }
}