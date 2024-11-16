
namespace GameEngine;
using System.Runtime.InteropServices;
using System.Numerics;

static class Kernel32{
    [DllImport("kernel32.dll")]
    public static extern void RtlZeroMemory(IntPtr dst, UIntPtr length);
}

delegate void WindowSizeCallbackDelegate(IntPtr window, int width, int height);
delegate void CursorPosCallbackDelegate(IntPtr window, double xpos, double ypos);
delegate void MouseButtonCallbackDelegate(IntPtr window, int button, int action, int mods);
delegate void KeyCallbackDelegate(IntPtr window, int key, int scancode, int action, int mods);
delegate void CharCallbackDelegate(IntPtr window, uint codepoint);

static class GLFW{
    private const string DllFilePath = @"glfw3.dll";

    public const int GLFW_OPENGL_API = 0x00030001;
    public const int GLFW_OPENGL_ES_API = 0x00030002;

    public const int GLFW_CLIENT_API = 0x00022001;
    public const int GLFW_CONTEXT_VERSION_MAJOR = 0x00022002;
    public const int GLFW_CONTEXT_VERSION_MINOR = 0x00022003;

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static int glfwInit();

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static IntPtr glfwCreateWindow(int width, int height, string title, IntPtr monitor, IntPtr share);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwMakeContextCurrent(IntPtr window);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static int glfwWindowShouldClose(IntPtr window);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwPollEvents();

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static IntPtr glfwGetProcAddress (string procname);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwWindowHint (int hint, int value);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwSwapBuffers (IntPtr window);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwGetWindowSize(IntPtr window, IntPtr width, IntPtr height);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwSetMouseButtonCallback(IntPtr window, IntPtr mouseButtonCallback);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwSetCursorPosCallback(IntPtr window, IntPtr cursorPosCallback);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwSetWindowSizeCallback(IntPtr window, IntPtr windowSizeCallback);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwGetCursorPos(IntPtr window, IntPtr xpos, IntPtr ypos);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwSetKeyCallback(IntPtr window, IntPtr keyCallback);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwSetCharCallback(IntPtr window, IntPtr charCallback);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwGetKey(IntPtr window, int key);
}

static class GLFWHelper {
    public static void SetMouseButtonCallback(MouseButtonCallbackDelegate mouseButtonCallbackDelegate){
        var ptr = Marshal.GetFunctionPointerForDelegate(mouseButtonCallbackDelegate);
        GLFW.glfwSetMouseButtonCallback(GameEngine.window, ptr);
    }

    public static void SetCursorPosCallback(CursorPosCallbackDelegate cursorPosCallbackDelegate){
        var ptr = Marshal.GetFunctionPointerForDelegate(cursorPosCallbackDelegate);
        GLFW.glfwSetCursorPosCallback(GameEngine.window, ptr);
    }

    public static void SetKeyCallback(KeyCallbackDelegate keyCallbackDelegate){
        var ptr = Marshal.GetFunctionPointerForDelegate(keyCallbackDelegate);
        GLFW.glfwSetKeyCallback(GameEngine.window, ptr);
    }

    public static void SetWindowSizeCallback(WindowSizeCallbackDelegate windowSizeCallbackDelegate){
        var ptr = Marshal.GetFunctionPointerForDelegate(windowSizeCallbackDelegate);
        GLFW.glfwSetWindowSizeCallback(GameEngine.window, ptr);
    }

    public static void SetCharCallback(CharCallbackDelegate charCallbackDelegate){
        var ptr = Marshal.GetFunctionPointerForDelegate(charCallbackDelegate);
        GLFW.glfwSetCharCallback(GameEngine.window, ptr);
    }

    public static Vector2 GetCursorPosition(){
        var xptr = GameEngine.memory.Allocate(8);
        var yptr = GameEngine.memory.Allocate(8);
        GLFW.glfwGetCursorPos(GameEngine.window, xptr, yptr);
        return new Vector2((float)Marshal.PtrToStructure<double>(xptr), (float)Marshal.PtrToStructure<double>(yptr));
    }
}

class Buffer(int maxSize){
    public readonly IntPtr ptr = Marshal.AllocHGlobal(maxSize);
    public readonly int maxSize = maxSize;
    public int size = 0;

    public void SetAllDataToZero(){
        Kernel32.RtlZeroMemory(ptr, (uint)maxSize);
    }

    public byte[] GetBytes(int id, int size){
        byte[] managedArray = new byte[size];
        Marshal.Copy(ptr + id, managedArray, 0, size);
        return managedArray;
    }

    public void SetBytes(int id, byte[] bytes){
        Marshal.Copy(bytes, 0, ptr + id, bytes.Length);
    }

    public IntPtr AddBytes(byte[] bytes){
        var ptr = Allocate(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        return ptr;
    }

    public IntPtr AddFloatArray(float[] array){
        return AddBytes(array.SelectMany(BitConverter.GetBytes).ToArray());
    }

    public IntPtr AddString(string @string){
        byte[] bytes = @string.Select(c=>(byte)c).ToArray();
        return AddBytes(bytes);
    }

    public IntPtr AddStringArray(string[] strings){
        IntPtr[] ptrs = strings.Select(AddString).ToArray();
        return AddBytes(ptrs.SelectMany(i=>BitConverter.GetBytes(i)).ToArray());
    }

    public IntPtr AddIntArray(int[] array){
        return AddBytes(array.SelectMany(BitConverter.GetBytes).ToArray());
    }

    public IntPtr AddUintArray(uint[] array){
        return AddBytes(array.SelectMany(BitConverter.GetBytes).ToArray());
    }

    public IntPtr Allocate(int size){
        if(this.size + size > maxSize){
            throw new Exception("Buffer not large enough...");
        }
        var ptr = this.ptr + this.size;
        this.size += size;
        return ptr;
    }

    public void FreeAll(){
        Marshal.FreeHGlobal(ptr);
    }
}

static class GLHelper{
    public static uint GenVertexArray(){
        var ptr = GameEngine.memory.Allocate(4);
        GL.glGenVertexArrays(1, ptr);
        return Marshal.PtrToStructure<uint>(ptr);
    }

    public static uint GenBuffer(){
        var ptr = GameEngine.memory.Allocate(4);
        GL.glGenBuffers(1, ptr);
        return Marshal.PtrToStructure<uint>(ptr);
    }

    public static uint GenTexture(){
        var ptr = GameEngine.memory.Allocate(4);
        GL.glGenTextures(1, ptr);
        return Marshal.PtrToStructure<uint>(ptr);
    }

    public static void UniformColor(uint shaderProgram, string name, Color color){
        var namePtr = GameEngine.memory.AddString(name+'\0');
        var location = GL.glGetUniformLocation(shaderProgram, namePtr);
        GL.glUniform4f(location, color.r, color.g, color.b, color.a);
    }

    public static void UniformMatrix4fv(uint shaderProgram, string name, Matrix4x4 matrix){
        var namePtr = GameEngine.memory.AddString(name+'\0');
        var location = GL.glGetUniformLocation(shaderProgram, namePtr);
        var matrixPtr = GameEngine.memory.AddFloatArray([
            matrix.M11, matrix.M12, matrix.M13, matrix.M14, 
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34, 
            matrix.M41, matrix.M42, matrix.M43, matrix.M44,
        ]);
        GL.glUniformMatrix4fv(location, 1, 0, matrixPtr);
    }

    public static void UniformInt(uint shaderProgram, string name, int i){
        var namePtr = GameEngine.memory.AddString(name+'\0');
        var location = GL.glGetUniformLocation(shaderProgram, namePtr);
        GL.glUniform1i(location, i);
    }
}

static class Shader{

    static uint CreateShader(string source, uint type){
        var shader = GL.glCreateShader(type);
        GL.glShaderSource(shader, 1, GameEngine.memory.AddStringArray([source]), 0);
        GL.glCompileShader(shader);
        var success = GameEngine.memory.Allocate(4);
        GL.glGetShaderiv(shader, GL.GL_COMPILE_STATUS, success);
        if(Marshal.PtrToStructure<int>(success) == 0){
            var infoLog = GameEngine.memory.Allocate(512);
            GL.glGetShaderInfoLog(shader, 512, 0, infoLog);
            throw new Exception(Marshal.PtrToStringAnsi(infoLog));
        }
        return shader;
    }

    public static uint CreateShaderProgram(string vertexSource, string fragmentSource){
        var vertexShader = CreateShader(vertexSource, GL.GL_VERTEX_SHADER);
        var fragmentShader = CreateShader(fragmentSource, GL.GL_FRAGMENT_SHADER);
        var program = GL.glCreateProgram();
        GL.glAttachShader(program, vertexShader);
        GL.glAttachShader(program, fragmentShader);
        GL.glLinkProgram(program);
        var success = GameEngine.memory.Allocate(4);
        GL.glGetProgramiv(program, GL.GL_LINK_STATUS, success);
        if(Marshal.PtrToStructure<int>(success) == 0) {
            var infoLog = GameEngine.memory.Allocate(512);
            GL.glGetProgramInfoLog(program, 512, 0, infoLog);
            throw new Exception(Marshal.PtrToStringAnsi(infoLog));
        }
        GL.glDeleteShader(vertexShader);
        GL.glDeleteShader(fragmentShader);  
        return program;
    }
}

public class MainTexture {
    public static readonly MainTexture whiteTexture = GetWhiteTexture();
    public readonly int width;
    public readonly int height;
    byte[] bytes;
    Buffer data;
    uint id;

    static MainTexture GetWhiteTexture(){
        var tex = new MainTexture(1,1);
        tex.SetPixel(0,0,new Color255(255,255,255,255));
        tex.UpdateData();
        return tex;
    }

    public Color255 GetColor(Vector2 fraction){
        var x = (int)(fraction.x * width);
        var y = (int)(fraction.y * height);
        var start = (x + y*width) * 4;
        return new Color255(bytes[start + 0], bytes[start + 1], bytes[start + 2], bytes[start + 3]);
    }

    public void SetPixel(int x, int y, Color255 color){
        var start = (x + y*width) * 4;
        bytes[start + 0] = color.r;
        bytes[start + 1] = color.g;
        bytes[start + 2] = color.b;
        bytes[start + 3] = color.a;
    }

    public void SetPixelRect(int x, int y, int xmax, int ymax, Color255 color){
        if(x<0){
            x=0;
        }
        if(y<0){
            y=0;
        }
        if(xmax>=width){
            xmax = width-1;
        }
        if(ymax>=height){
            ymax = height-1;
        }
        for(var xi=x;xi<xmax;xi++){
            for(var yi=y;yi<ymax;yi++){
                SetPixel(xi,yi,color);
            }
        }
    }

    public void SetPixelCircle(int x, int y, int xmax, int ymax, Color255 color){
        var center = new Vector2((x + xmax)/2f, (y + ymax)/2f);
        var radius = (xmax - x)/2f;
        if(x<0){
            x=0;
        }
        if(y<0){
            y=0;
        }
        if(xmax>=width){
            xmax = width-1;
        }
        if(ymax>=height){
            ymax = height-1;
        }
        for(var xi=x;xi<xmax;xi++){
            for(var yi=y;yi<ymax;yi++){
                if((center - new Vector2(xi,yi)).Length() < radius){
                    SetPixel(xi,yi,color);
                }
            }
        }
    }

    public void Clear(Color255 color){
        SetPixelRect(0, 0, width, height, color);
    }

    public MainTexture(int width, int height){
        id = GLHelper.GenTexture();
        this.width = width;
        this.height = height;
        bytes = new byte[width * height * 4];
        data = new Buffer(width * height * 4);
    }

    public void UpdateData(){
        data.SetBytes(0, bytes);
        GL.glBindTexture(GL.GL_TEXTURE_2D, id);

        GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, (int)GL.GL_REPEAT);	
        GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, (int)GL.GL_REPEAT);
        GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, (int)GL.GL_NEAREST);
        GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, (int)GL.GL_NEAREST);

        GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, (int)GL.GL_RGBA, width, height, 0, GL.GL_RGBA, GL.GL_UNSIGNED_BYTE, data.ptr);
    }

    public void Bind0(){
        GL.glActiveTexture(GL.GL_TEXTURE0);
        GL.glBindTexture(GL.GL_TEXTURE_2D, id);
    }
}

public class Transform2D {
    public Vector2 position;
    public float angleDegrees;
    public Vector2 scale;

    public Transform2D(){}

    public Transform2D(Vector2 position, float angleDegrees, Vector2 scale){
        this.position = position;
        this.angleDegrees = angleDegrees;
        this.scale = scale;
    }

    public static Transform2D Line(Vector2 a, Vector2 b, float width){
        return new Transform2D(
            (a + b) / 2f,
            JMath.FindAngle(a, b),
            new Vector2((b-a).Length() * 0.5f, width * 0.5f)
        );
    }

    public static Transform2D Rect(Rect rect){
        return new Transform2D(rect.Center, 0, rect.Size/2f);
    }

    public Matrix4x4 GetMatrix(){
        var cam = Matrix4x4.CreateOrthographicOffCenter(0, Screen.width, Screen.height, 0, -1, 1);
        var model = Matrix4x4.CreateScale(scale.x, scale.y, 1) * 
            Matrix4x4.CreateRotationZ(angleDegrees * JMath.DegreesToRadians) *
            Matrix4x4.CreateTranslation(position.x, position.y, 0);
        return model * cam;
    }

    public Vector2 GetLocalPositionFromWorld(Vector2 screenPosition){
        if(Matrix4x4.Invert(GetMatrix(), out Matrix4x4 inverseMatrix)){
            var viewportPosition = new Vector2(screenPosition.x / Screen.width * 2 - 1, (Screen.height - screenPosition.y) / Screen.height * 2 - 1);
            var result = Vector4.Transform(new Vector4(viewportPosition.x, viewportPosition.y, 0, 1), inverseMatrix);
            return new Vector2(result.X/result.W, result.Y/result.W);
        }
        throw new Exception("Error cant invert matrix");
    }

    public bool Contains(Vector2 screenPosition){
        var transformPosition = GetLocalPositionFromWorld(screenPosition);
        return transformPosition.x >= -1 && transformPosition.x <= 1 && transformPosition.y >= -1 && transformPosition.y <= 1;
    }

    public Vector2 GetWorldPositionFromLocal(Vector2 localPosition){
        var matrix = Matrix4x4.CreateScale(scale.x, scale.y, 1) * 
            Matrix4x4.CreateRotationZ(angleDegrees * JMath.DegreesToRadians) *
            Matrix4x4.CreateTranslation(position.x, position.y, 0);
        var result = Vector4.Transform(new Vector4(localPosition.x, localPosition.y, 0, 1), matrix);
        return new Vector2(result.X/result.W, result.Y/result.W);
    }
}

public static class Graphics {
    static uint vao;
    static uint vbo;
    static uint ebo;
    static uint shaderProgram;

    internal static void Init(){
        vao = GLHelper.GenVertexArray();
        vbo = GLHelper.GenBuffer();
        ebo = GLHelper.GenBuffer();
        
        string vertexSource = @"#version 330 core
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTexCoord;

uniform mat4 matrix;

out vec2 TexCoord;

void main()
{
    gl_Position = matrix * vec4(aPos, 0.0, 1.0);
    TexCoord = aTexCoord;
}  "+"\0";

        string fragmentSource = @"#version 330 core
out vec4 FragColor;
  
in vec2 TexCoord;

uniform sampler2D ourTexture;
uniform vec4 tint;

void main()
{
    vec4 tex = texture(ourTexture, TexCoord);
    FragColor = tex * tint;
} "+"\0";
        shaderProgram = Shader.CreateShaderProgram(vertexSource, fragmentSource);

        float[] vertices = [-1,-1,0,0,1,-1,1,0,1,1,1,1,-1,1,0,1];
        uint[] indices = [0,1,2,0,2,3];
        var verticesPtr = GameEngine.memory.AddFloatArray(vertices);
        var indicesPtr = GameEngine.memory.AddUintArray(indices);
        GL.glBindVertexArray(vao);
        GL.glBindBuffer(GL.GL_ARRAY_BUFFER, vbo);
        GL.glBufferData(GL.GL_ARRAY_BUFFER, vertices.Length * 4, verticesPtr, GL.GL_STATIC_DRAW);

        GL.glBindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, ebo);
        GL.glBufferData(GL.GL_ELEMENT_ARRAY_BUFFER, indices.Length * 4, indicesPtr, GL.GL_STATIC_DRAW);

        GL.glVertexAttribPointer(0, 2, GL.GL_FLOAT, GL.GL_FALSE, 4 * sizeof(float), 0);
        GL.glEnableVertexAttribArray(0);

        GL.glVertexAttribPointer(1, 2, GL.GL_FLOAT, GL.GL_FALSE, 4 * sizeof(float), 2 * sizeof(float));
        GL.glEnableVertexAttribArray(1);

        GL.glBlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);
        GL.glEnable(GL.GL_BLEND);
        GL.glUseProgram(shaderProgram);
    }

    public static void Clear(Color color){
        GL.glClearColor(color.r, color.g, color.b, color.a);
        GL.glClear(GL.GL_COLOR_BUFFER_BIT);
        GL.glViewport(0,0,Screen.width, Screen.height);
    }

    public static void Draw(Transform2D transform2D, MainTexture texture, Color tint){
        texture.Bind0();
        GLHelper.UniformInt(shaderProgram, "outTexture", 0);
        GLHelper.UniformColor(shaderProgram, "tint", tint);
        GLHelper.UniformMatrix4fv(shaderProgram, "matrix", transform2D.GetMatrix());
        GL.glDrawElements(GL.GL_TRIANGLES, 6, GL.GL_UNSIGNED_INT, 0);
    }
}

