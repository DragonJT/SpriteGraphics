namespace SpriteGraphics;

using GameEngine;

class Sprite {
    public Transform2D transform2D;
    public MainTexture mainTexture;
    public Color tint = new (0,0,0,0.4f);

    public Sprite(Vector2 position, float angleDegrees, Vector2 scale){
        transform2D = new Transform2D{
            position = position,
            angleDegrees = angleDegrees,
            scale = scale,
        };
        mainTexture = MainTexture.whiteTexture;
    }

    public void BeginPaint(){
        if(mainTexture == MainTexture.whiteTexture){
            mainTexture = new MainTexture((int)(transform2D.scale.x * 2), (int)(transform2D.scale.y * 2));
            mainTexture.Clear(Color255.Create(tint));
            mainTexture.UpdateData();
            tint = Color.White;
        }
    }

    public void DrawOnTexture(Vector2 viewportPosition){
        var mousePos = transform2D.GetLocalPositionFromWorld(viewportPosition);
        var x = (int)((mousePos.x + 1) * mainTexture.width * 0.5f);
        var y = (int)((mousePos.y + 1) * mainTexture.height * 0.5f);
        var w = 20;
        var h = 20;
        mainTexture!.SetPixelRect(x,y,x+w,y+h,Color255.Create(Color.Red));
        mainTexture!.UpdateData();
    }

    public void Draw(){
        Graphics.Draw(transform2D, mainTexture, tint);
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
        Graphics.Draw(Transform2D.CreateLine(a,b,borderSize), MainTexture.whiteTexture, color);
        Graphics.Draw(Transform2D.CreateLine(b,c,borderSize), MainTexture.whiteTexture, color);
        Graphics.Draw(Transform2D.CreateLine(c,d,borderSize), MainTexture.whiteTexture, color);
        Graphics.Draw(Transform2D.CreateLine(d,a,borderSize), MainTexture.whiteTexture, color);
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
class SpriteGraphics : Game{
    List<Sprite> sprites = [];
    Sprite? selected = null;
    Mode mode = Mode.Rect;
    Vector2 start;
    bool dragging;

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
            s.Draw();
        }
        if(mode == Mode.Edit){
            if(selected == null || !Handles.RectHandle(selected!.transform2D)){
                if(Input.GetButtonDown(Input.MOUSE_BUTTON_1)){
                    selected = GetSpriteAtPosition(Input.MousePosition);
                }
            }
            if(selected!=null && Input.GetKeyDown(Input.KEY_BACKSPACE)){
                sprites.Remove(selected);
            }
        }
        else if(mode == Mode.Paint){
            if(selected!=null){
                Handles.DrawBorder(selected.transform2D, Color.DarkCyan);
                if(Input.GetButton(Input.MOUSE_BUTTON_1)){
                    selected.DrawOnTexture(Input.MousePosition);
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
        if(Input.GetKeyDown(Input.KEY_P) && selected!=null){
            mode = Mode.Paint;
            selected.BeginPaint();
        }
        else if(Input.GetKeyDown(Input.KEY_E)){
            mode = Mode.Edit;
        }
        else if(Input.GetKeyDown(Input.KEY_R)){
            mode = Mode.Rect;
        }
    }

    public static void Main(){
        GameEngine.Create(new SpriteGraphics());
    }
}