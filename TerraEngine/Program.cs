using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.IO;

namespace TerraEngine
{
    public class Program : GameWindow
    {
        private int _vertexArrayObject;
        private int _vertexBufferObject;
        private int _elementBufferObject;
        private int _shaderProgram;

        private Camera _camera;
        private bool _firstMove = true;
        private Vector2 _lastPos;

        private readonly float[] _vertices =
        {
            // positions          // colors
            // Base
            -0.5f, 0.0f, -0.5f,  1.0f, 0.0f, 0.0f,
             0.5f, 0.0f, -0.5f,  0.0f, 1.0f, 0.0f,
             0.5f, 0.0f,  0.5f,  0.0f, 0.0f, 1.0f,
            -0.5f, 0.0f,  0.5f,  1.0f, 1.0f, 0.0f,
            // Apex
             0.0f,  0.5f,  0.0f,  1.0f, 1.0f, 1.0f
        };

        private readonly uint[] _indices =
        {
            // Base
            0, 1, 2,
            2, 3, 0,
            // Sides
            0, 1, 4,
            1, 2, 4,
            2, 3, 4,
            3, 0, 4
        };

        public Program(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            // Enable depth testing
            GL.Enable(EnableCap.DepthTest);

            // Compile shaders and link program
            var vertexShaderSource = File.ReadAllText("Shaders/shader.vert");
            var fragmentShaderSource = File.ReadAllText("Shaders/shader.frag");

            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);
            CheckShaderCompilation(vertexShader);

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            CheckShaderCompilation(fragmentShader);

            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, vertexShader);
            GL.AttachShader(_shaderProgram, fragmentShader);
            GL.LinkProgram(_shaderProgram);
            CheckProgramLinking(_shaderProgram);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // Setup vertex data and buffers
            _vertexArrayObject = GL.GenVertexArray();
            _vertexBufferObject = GL.GenBuffer();
            _elementBufferObject = GL.GenBuffer();

            GL.BindVertexArray(_vertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            var positionLocation = GL.GetAttribLocation(_shaderProgram, "aPosition");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            var colorLocation = GL.GetAttribLocation(_shaderProgram, "aColor");
            GL.EnableVertexAttribArray(colorLocation);
            GL.VertexAttribPointer(colorLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            GL.ClearColor(Color4.CornflowerBlue);

            // Initialize camera
            _camera = new Camera(new Vector3(0.0f, 0.0f, 3.0f), Vector3.UnitY, -90.0f, 0.0f);

            CursorGrabbed = true;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_shaderProgram);

            Matrix4 model = Matrix4.Identity;
            Matrix4 view = _camera.GetViewMatrix();
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), Size.X / (float)Size.Y, 0.1f, 100.0f);

            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "model"), false, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "view"), false, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "projection"), false, ref projection);

            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (!IsFocused)
            {
                return;
            }

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            if (input.IsKeyDown(Keys.W))
            {
                _camera.ProcessKeyboard("FORWARD", (float)args.Time);
            }

            if (input.IsKeyDown(Keys.S))
            {
                _camera.ProcessKeyboard("BACKWARD", (float)args.Time);
            }

            if (input.IsKeyDown(Keys.A))
            {
                _camera.ProcessKeyboard("LEFT", (float)args.Time);
            }

            if (input.IsKeyDown(Keys.D))
            {
                _camera.ProcessKeyboard("RIGHT", (float)args.Time);
            }

            var mouse = MouseState;

            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                _camera.ProcessMouseMovement(deltaX, deltaY);
            }
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(_vertexBufferObject);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DeleteBuffer(_elementBufferObject);

            GL.BindVertexArray(0);
            GL.DeleteVertexArray(_vertexArrayObject);

            GL.UseProgram(0);
            GL.DeleteProgram(_shaderProgram);
        }

        private static void CheckShaderCompilation(int shader)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                var infoLog = GL.GetShaderInfoLog(shader);
                Console.WriteLine($"Error compiling shader: {infoLog}");
            }
        }

        private static void CheckProgramLinking(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                var infoLog = GL.GetProgramInfoLog(program);
                Console.WriteLine($"Error linking program: {infoLog}");
            }
        }

        public static void Main(string[] args)
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(1920, 1080),
                Title = "Terra-Engine"
            };

            using (var window = new Program(GameWindowSettings.Default, nativeWindowSettings))
            {
                window.Run();
            }
        }
    }
}
