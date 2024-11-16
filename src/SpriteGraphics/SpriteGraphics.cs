namespace SpriteGraphics;
using GameEngine;

class Sprite {
    public Transform2D transform2D;
    public MainTexture mainTexture;

    public Sprite(Vector2 position, float angleDegrees, Vector2 scale){
        transform2D = new Transform2D{
            position = position,
            angleDegrees = angleDegrees,
            scale = scale,
        };
        mainTexture = new MainTexture((int)(scale.x*2), (int)(scale.y*2));
        mainTexture.Clear(new Color255(0,0,0,100));
        mainTexture.UpdateData();
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
        Graphics.Draw(transform2D, mainTexture, Color.White);
    }
}

static class Handles{
    static string? id;
    const int posHandleSize = 25;
    const int rotHandleSize = 35;
    const int borderSize = 5;
    static MainTexture rotationHandleTexture = GetCircleHandleTexture(rotHandleSize, borderSize);
    static MainTexture positionHandleTexture = GetSquareHandleTexture(posHandleSize, borderSize);
    static Vector2 startHandleOppositePosition;

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

    static void PositionHandle(string name, Transform2D transform2D, Vector2 handleLocal, ref bool used){
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

    static void RotationHandle(Transform2D transform2D, ref bool used){
        var rotationHandleTransform = new Transform2D(
            transform2D.position, 
            transform2D.angleDegrees, 
            new Vector2(rotHandleSize * 0.5f, rotHandleSize * 0.5f)
        );
        if(used){
            Graphics.Draw(rotationHandleTransform, rotationHandleTexture, Color.LightCyan);
            return;
        }
        if(!used){
            if(Input.GetButtonDown(Input.MOUSE_BUTTON_1) && rotationHandleTransform.Contains(Input.MousePosition)){
                id = "Rotation";
                used = true;
            }
            else if(Input.GetButtonUp(Input.MOUSE_BUTTON_1) && id == "Rotation"){
                id = null;
                used = true;
            }
            else if(Input.GetButton(Input.MOUSE_BUTTON_1) && id == "Rotation"){
                transform2D.angleDegrees += Input.DeltaMousePosition.x * 0.2f;
                used = true;
            }
        }
        Graphics.Draw(rotationHandleTransform, rotationHandleTexture, used ? Color.Orange : Color.LightCyan);
    }

    public static bool RectHandle(Transform2D transform2D){
        var used = false;
        var a = transform2D.GetWorldPositionFromLocal(new Vector2(-1,-1));
        var b = transform2D.GetWorldPositionFromLocal(new Vector2(1,-1));
        var c = transform2D.GetWorldPositionFromLocal(new Vector2(1,1));
        var d = transform2D.GetWorldPositionFromLocal(new Vector2(-1,1));
        Graphics.Draw(Transform2D.CreateLine(a,b,borderSize), MainTexture.whiteTexture, Color.Black);
        Graphics.Draw(Transform2D.CreateLine(b,c,borderSize), MainTexture.whiteTexture, Color.Black);
        Graphics.Draw(Transform2D.CreateLine(c,d,borderSize), MainTexture.whiteTexture, Color.Black);
        Graphics.Draw(Transform2D.CreateLine(d,a,borderSize), MainTexture.whiteTexture, Color.Black);
        PositionHandle("TopLeft", transform2D, new Vector2(-1,-1), ref used);
        PositionHandle("TopRight", transform2D, new Vector2(1,-1), ref used);           
        PositionHandle("BottomRight", transform2D, new Vector2(1,1), ref used);
        PositionHandle("BottomLeft", transform2D, new Vector2(-1,1), ref used);
        RotationHandle(transform2D, ref used);
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

class SpriteGraphics : Game{
    List<Sprite> sprites = [];
    Sprite? selected;

    public override void Awake(){
        sprites.Add(new Sprite(new Vector2(800, 400), 30, new Vector2(250, 250)));
        sprites.Add(new Sprite(new Vector2(400, 1200), 45, new Vector2(200, 300)));
        sprites.Add(new Sprite(new Vector2(500, 500), 35, new Vector2(300, 300)));
        selected = sprites[2];
    }

    public override void Draw(){
        Graphics.Clear(Color.Blue);
        
        foreach(var s in sprites){
            s.Draw();
        }
        if(!Handles.RectHandle(selected!.transform2D)){
            if (Input.GetButton(Input.MOUSE_BUTTON_1)){
                foreach(var s in sprites){
                    s.DrawOnTexture(Input.MousePosition);
                }
            }
        }
    }

    public static void Main(){
        GameEngine.Create(new SpriteGraphics());
    }
}