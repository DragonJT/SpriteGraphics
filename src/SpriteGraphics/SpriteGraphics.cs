namespace SpriteGraphics;

using GameEngine;

class Sprite {
    public Transform2D transform2D;
    public MainTexture? mainTexture;

    public Sprite Clone(){
        var sprite = new Sprite(transform2D.position, transform2D.angleDegrees, transform2D.scale){
            mainTexture = mainTexture
        };
        return sprite;
    }

    public Sprite(Vector2 position, float angleDegrees, Vector2 scale){
        transform2D = new Transform2D{
            position = position,
            angleDegrees = angleDegrees,
            scale = scale,
        };
    }

    public void DrawOnTexture(Vector2 viewportPosition, Brush brush){
        mainTexture ??= new MainTexture((int)(transform2D.scale.x * 2), (int)(transform2D.scale.y * 2));
        var mousePos = transform2D.GetLocalPositionFromWorld(viewportPosition);
        var x = (int)((mousePos.x + 1) * mainTexture.width * 0.5f);
        var y = (int)((mousePos.y + 1) * mainTexture.height * 0.5f);
        if(brush.brushShape == BrushShape.Square){
            mainTexture!.SetPixelRect(x-brush.radius,y-brush.radius,x+brush.radius,y+brush.radius,brush.color);
        }
        else if(brush.brushShape == BrushShape.Circle){
            mainTexture!.SetPixelCircle(x-brush.radius,y-brush.radius,x+brush.radius,y+brush.radius,brush.color);
        }
        mainTexture!.UpdateData();
    }

    public void Draw(bool isSelected){
        if(mainTexture == null || isSelected){
            Graphics.Draw(transform2D, MainTexture.whiteTexture, new Color(0,0,0,0.4f));
        }
        if(mainTexture!=null){
            Graphics.Draw(transform2D, mainTexture, Color.White);
        }
    }
}

static class Handles{
    static string? id;
    const int posHandleSize = 25;
    const int rotHandleSize = 20;
    const int borderSize = 5;
    static MainTexture rotationHandleTexture = GetCircleHandleTexture(rotHandleSize, borderSize);
    static MainTexture positionHandleTexture = GetSquareHandleTexture(posHandleSize, borderSize);
    static Vector2 startHandleOppositePosition;
    static float lastAngle;

    static MainTexture GetSquareHandleTexture(int size, int border){
        var tex = new MainTexture(size, size);
        tex.Clear(new Color255(255,255,255,255));
        tex.SetPixelRect(border, border, size - border, size - border, new Color255(255,255,255,50));
        tex.UpdateData();
        return tex;
    }

    static MainTexture GetCircleHandleTexture(int size, int border){
        var tex = new MainTexture(size, size);
        tex.SetPixelCircle(0, 0, size, size, new Color255(255,255,255,255));
        tex.SetPixelCircle(border, border, size - border, size - border, new Color255(255,255,255,50));
        tex.UpdateData();
        return tex;
    }

    static void PositionHandle(Transform2D transform2D, Vector2 handleLocal, ref bool used){
        var name = "Position"+handleLocal.x+handleLocal.y;
        var handleTransform2D = new Transform2D(
            transform2D.GetWorldPositionFromLocal(handleLocal), 
            transform2D.angleDegrees,
            new Vector2(posHandleSize * 0.5f, posHandleSize * 0.5f)
        );
        if(used){
            Graphics.Draw(handleTransform2D, positionHandleTexture, Color.LightCyan);
            return;
        }
        if(Input.GetButtonDown(Input.MOUSE_BUTTON_1) && handleTransform2D.Contains(Input.MousePosition)) {
            startHandleOppositePosition = transform2D.GetWorldPositionFromLocal(-handleLocal);
            id = name;
            used = true;
        }
        else if(Input.GetButtonUp(Input.MOUSE_BUTTON_1) && id == name){
            id = null;
            used = true;
        }
        else if(Input.GetButton(Input.MOUSE_BUTTON_1) && id == name){
            var handlePos = Input.MousePosition;
            var center = (startHandleOppositePosition + handlePos) * 0.5f;
            var oppositePositionX = transform2D.GetWorldPositionFromLocal(new Vector2(-handleLocal.x, handleLocal.y));
            var oppositePositionY = transform2D.GetWorldPositionFromLocal(new Vector2(handleLocal.x, -handleLocal.y));
            var absTransform = new Transform2D(transform2D.position, transform2D.angleDegrees, transform2D.scale.Abs());
            var dir = absTransform.GetLocalPositionFromWorld(handlePos);
            var dirx = (dir.x * handleLocal.x) > 0 ? 1 : -1;
            var diry = (dir.y * handleLocal.y) > 0 ? 1 : -1;
            var sizex = (handlePos - oppositePositionX).Length() * 0.5f * dirx;
            var sizey = (handlePos - oppositePositionY).Length() * 0.5f * diry;
            transform2D.position = center;
            transform2D.scale = new Vector2(sizex, sizey);
            used = true;
        }
        Graphics.Draw(handleTransform2D, positionHandleTexture, used ? Color.Orange : Color.LightCyan);
    }

    static void RotationHandle(Transform2D transform2D, Vector2 handleLocal, ref bool used){
        var name = "Rotation"+handleLocal.x+handleLocal.y;
        var rotationHandleTransform = new Transform2D(
            transform2D.GetWorldPositionFromLocal(handleLocal), 
            transform2D.angleDegrees, 
            new Vector2(rotHandleSize * 0.5f, rotHandleSize * 0.5f)
        );
        if(used){
            Graphics.Draw(rotationHandleTransform, rotationHandleTexture, Color.LightCyan);
            return;
        }
        if(!used){
            if(Input.GetButtonDown(Input.MOUSE_BUTTON_1) && rotationHandleTransform.Contains(Input.MousePosition)){
                id = name;
                lastAngle = JMath.FindAngle(transform2D.position, Input.MousePosition);
                used = true;
            }
            else if(Input.GetButtonUp(Input.MOUSE_BUTTON_1) && id == name){
                id = null;
                used = true;
            }
            else if(Input.GetButton(Input.MOUSE_BUTTON_1) && id == name){
                var newAngle = JMath.FindAngle(transform2D.position, Input.MousePosition);
                transform2D.angleDegrees += newAngle - lastAngle;
                lastAngle = newAngle;
                used = true;
            }
        }
        Graphics.Draw(rotationHandleTransform, rotationHandleTexture, used ? Color.Orange : Color.LightCyan);
    }

    public static void DrawBorder(Transform2D transform2D, Color color){
        var a = transform2D.GetWorldPositionFromLocal(new Vector2(-1,-1));
        var b = transform2D.GetWorldPositionFromLocal(new Vector2(1,-1));
        var c = transform2D.GetWorldPositionFromLocal(new Vector2(1,1));
        var d = transform2D.GetWorldPositionFromLocal(new Vector2(-1,1));
        Graphics.Draw(Transform2D.Line(a,b,borderSize), MainTexture.whiteTexture, color);
        Graphics.Draw(Transform2D.Line(b,c,borderSize), MainTexture.whiteTexture, color);
        Graphics.Draw(Transform2D.Line(c,d,borderSize), MainTexture.whiteTexture, color);
        Graphics.Draw(Transform2D.Line(d,a,borderSize), MainTexture.whiteTexture, color);
    }

    public static bool RectHandle(Transform2D transform2D){
        var used = false;
        DrawBorder(transform2D, Color.DarkCyan);
        PositionHandle(transform2D, new Vector2(-1,-1), ref used);
        PositionHandle(transform2D, new Vector2(1,-1), ref used);           
        PositionHandle(transform2D, new Vector2(1,1), ref used);
        PositionHandle(transform2D, new Vector2(-1,1), ref used);
        RotationHandle(transform2D, new Vector2(0,1), ref used);
        RotationHandle(transform2D, new Vector2(1,0), ref used);
        RotationHandle(transform2D, new Vector2(0,-1), ref used);
        RotationHandle(transform2D, new Vector2(-1,0), ref used);
        if(!used){
            if(Input.GetButtonDown(Input.MOUSE_BUTTON_1) && transform2D.Contains(Input.MousePosition)){
                id = "Position";
                used = true;
            }
            else if(Input.GetButtonUp(Input.MOUSE_BUTTON_1) && id == "Position"){
                id = null;
                used = true;
            }
            else if(Input.GetButton(Input.MOUSE_BUTTON_1) && id == "Position"){
                transform2D.position += Input.DeltaMousePosition;
                used = true;
            }
        }
        return used;
    }
}

enum Mode { Edit, Rect, Paint}
enum BrushShape { Square, Circle }

class Brush {
    public BrushShape brushShape = BrushShape.Square;
    public int radius = 10;
    public Color255 color = new (40,200,40,255);
}

class SpriteGraphics : Game{
    List<Sprite> sprites = [];
    Sprite? selected = null;
    Mode mode = Mode.Rect;
    Brush brush = new();
    Vector2 start;
    bool dragging;
    MainTexture? colorPallette;

    public override void Awake()
    {
        colorPallette = new MainTexture(3,1);
        colorPallette.SetPixel(0,0,new Color255(40,200,40,255));
        colorPallette.SetPixel(1,0,new Color255(20,100,20,255));
        colorPallette.SetPixel(2,0,new Color255(200,255,100,255));
        colorPallette.UpdateData();
    }

    Sprite? GetSpriteAtPosition(Vector2 position){
        for(var i=sprites.Count-1;i>=0;i--){
            if(sprites[i].transform2D.Contains(position)){
                return sprites[i];
            }
        }
        return null;
    }

    public override void Draw(){
        Graphics.Clear(Color.Blue);
        
        foreach(var s in sprites){
            s.Draw(s == selected);
        }
        var colorPalletteRect = new Rect(0,0,200,50);
        Graphics.Draw(Transform2D.Rect(colorPalletteRect), colorPallette!, Color.White);
        if(Input.GetButtonDown(Input.MOUSE_BUTTON_1) && colorPalletteRect.Contains(Input.MousePosition)){
            brush.color = colorPallette!.GetColor(colorPalletteRect.Fraction(Input.MousePosition));
        }
        else{
            if(mode == Mode.Edit){
                if(selected == null || !Handles.RectHandle(selected!.transform2D)){
                    if(Input.GetButtonDown(Input.MOUSE_BUTTON_1)){
                        selected = GetSpriteAtPosition(Input.MousePosition);
                    }
                }
                if(Input.GetKeyDown(Input.KEY_D) && selected!=null){
                    var duplicated = selected.Clone();
                    sprites.Add(duplicated);
                    selected = duplicated;
                }
            }
            else if(mode == Mode.Paint){
                if(selected!=null){
                    Handles.DrawBorder(selected.transform2D, Color.DarkCyan);
                    if(Input.GetButton(Input.MOUSE_BUTTON_1)){
                        selected.DrawOnTexture(Input.MousePosition, brush);
                    }
                }
                
            }
            else if(mode == Mode.Rect){
                if(Input.GetButtonDown(Input.MOUSE_BUTTON_1)){
                    start = Input.MousePosition;
                    sprites.Add(new Sprite(start, 0, Vector2.Zero));
                    dragging = true;
                }
                else if(dragging && Input.GetButtonUp(Input.MOUSE_BUTTON_1)){
                    dragging = false;
                }
                else if(dragging && Input.GetButton(Input.MOUSE_BUTTON_1)){
                    var sprite = sprites[^1];
                    sprite.transform2D.position = (start + Input.MousePosition)/2f;
                    sprite.transform2D.scale = (Input.MousePosition - start).Abs()/2f;
                    selected = sprite;
                }
            }
        }
        if(Input.GetKeyDown(Input.KEY_P) && selected!=null){
            mode = Mode.Paint;
        }
        if(Input.GetKeyDown(Input.KEY_E)){
            mode = Mode.Edit;
        }
        if(Input.GetKeyDown(Input.KEY_R)){
            mode = Mode.Rect;
        }
        if(selected!=null && Input.GetKeyDown(Input.KEY_BACKSPACE)){
            sprites.Remove(selected);
            selected = null;
        }

        if(Input.GetKeyDown(Input.KEY_COMMA)){
            brush.brushShape = BrushShape.Square;
        }
        if(Input.GetKeyDown(Input.KEY_PERIOD)){
            brush.brushShape = BrushShape.Circle;
        }
        if(Input.GetKey(Input.KEY_0)){
            brush.radius = 5;
        }
        if(Input.GetKey(Input.KEY_1)){
            brush.radius = 10;
        }
        if(Input.GetKey(Input.KEY_2)){
            brush.radius = 20;
        }
        if(Input.GetKey(Input.KEY_3)){
            brush.radius = 40;
        }
        if(Input.GetKey(Input.KEY_4)){
            brush.radius = 80;
        }
    }

    public static void Main(){
        GameEngine.Create(new SpriteGraphics());
    }
}