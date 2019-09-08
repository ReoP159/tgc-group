using Microsoft.DirectX.DirectInput;
using System.Drawing;
using System.IO;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer m�s ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar el modelo que instancia GameForm <see cref="Form.GameForm.InitGraphics()" />
    ///     line 97.
    /// </summary>
    public class GameModel : TgcExample
    {
        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        /// <param name="mediaDir">Ruta donde esta la carpeta con los assets</param>
        /// <param name="shadersDir">Ruta donde esta la carpeta con los shaders</param>
        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

        //Caja que se muestra en el ejemplo.
        private TGCBox Box1 { get; set; }
        private TGCBox Box2 { get; set; }
        private TGCBox Box3 { get; set; }

        //Mesh de TgcLogo.
        private TgcMesh Mesh { get; set; }

        //Boleano para ver si dibujamos el boundingbox
        private bool BoundingBox { get; set; }

        private TgcMesh ship;
        private float side_speed = 70F;
        private float forward_speed = 30F;


        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqu� todo el c�digo de inicializaci�n: cargar modelos, texturas, estructuras de optimizaci�n, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        ///     Borrar el codigo ejemplo no utilizado.
        /// </summary>
        public override void Init()
        {

            var loader = new TgcSceneLoader();
            var center = TGCVector3.Empty;

            //Setting up de ttoda la scene
            var destFolder = MediaDir + "Escenario";

            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }

            //var scene = 


            ship = loader.loadSceneFromFile(MediaDir + "StarWars-YWing\\StarWars-YWing-TgcScene.xml").Meshes[0];
            ship.Rotation += new TGCVector3(0, FastMath.PI_HALF, 0);
            ship.Position = new TGCVector3(0, 0, 0);
            ship.Transform = TGCMatrix.Scaling(TGCVector3.One) * TGCMatrix.RotationYawPitchRoll(ship.Rotation.Y, ship.Rotation.X, ship.Rotation.Z) * TGCMatrix.Translation(ship.Position);

            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;

            //Textura de la carperta Media. Game.Default es un archivo de configuracion (Game.settings) util para poner cosas.
            //Pueden abrir el Game.settings que se ubica dentro de nuestro proyecto para configurar.
            var pathTexturaCaja = MediaDir + "StarWars-ATAT\\Textures\\BlackMetalTexture.jpg";

            //Cargamos una textura, tener en cuenta que cargar una textura significa crear una copia en memoria.
            //Es importante cargar texturas en Init, si se hace en el render loop podemos tener grandes problemas si instanciamos muchas.
            var texture = TgcTexture.createTexture(pathTexturaCaja);
            
            //Creamos una caja 3D ubicada de dimensiones (5, 10, 5) y la textura como color.
            var size_lados = new TGCVector3(10, 100, 160);
            var size_suelo = new TGCVector3(100, 10, 160);
            //Construimos una caja seg�n los par�metros, por defecto la misma se crea con centro en el origen y se recomienda as� para facilitar las transformaciones.
            Box1 = TGCBox.fromSize(size_lados, texture);
            Box1.Transform = TGCMatrix.Translation(new TGCVector3(50, 0, -80));
            Box2 = TGCBox.fromSize(size_lados, texture);
            Box2.Transform = TGCMatrix.Translation(new TGCVector3(-50, 0, -80));
            Box3 = TGCBox.fromSize(size_suelo, texture);
            Box3.Transform = TGCMatrix.Translation(new TGCVector3(0, -50, -80));

            ship.Scale = new TGCVector3(0.7f, 0.7f, 0.7f);

            //Suelen utilizarse objetos que manejan el comportamiento de la camara.
            //Lo que en realidad necesitamos gr�ficamente es una matriz de View.
            //El framework maneja una c�mara est�tica, pero debe ser inicializada.
            //Posici�n de la camara.
            var cameraPosition = new TGCVector3(0, 0, 125);
            //Quiero que la camara mire hacia el origen (0,0,0).
            var lookAt = TGCVector3.Empty;
            //Configuro donde esta la posicion de la camara y hacia donde mira.
            Camara.SetCamera(cameraPosition, lookAt);
            //Internamente el framework construye la matriz de view con estos dos vectores.
            //Luego en nuestro juego tendremos que crear una c�mara que cambie la matriz de view con variables como movimientos o animaciones de escenas.

        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la l�gica de computo del modelo, as� como tambi�n verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        public override void Update()
        {
            PreUpdate();

            var side_movement = TGCVector3.Empty;
            var forward_movement = TGCVector3.Empty;

            //Capturar Input teclado

            if (Input.keyPressed(Key.F))
            {
                BoundingBox = !BoundingBox;
            }

            if (Input.keyDown(Key.S))
            {
                side_movement.Y = -1;
            }

            if (Input.keyDown(Key.W))
            {
                side_movement.Y = 1;
            }

            if (Input.keyDown(Key.D))
            {
                side_movement.X = -1;
            }

            if (Input.keyDown(Key.A))
            {
                side_movement.X = 1;
            }

            if (Input.keyDown(Key.Space))
            {
                forward_movement.Z = -1;
            }

            side_movement *= side_speed * ElapsedTime;
            forward_movement *= forward_speed * ElapsedTime;
            ship.Move(side_movement+forward_movement);

            //Para seguir a la nave
            var dis_camara_nave = new TGCVector3(0, 10, 125);
            Camara.SetCamera(ship.Position + dis_camara_nave, ship.Position);
            

            PostUpdate();
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqu� todo el c�digo referido al renderizado.
        ///     Borrar todo lo que no haga falta.
        /// </summary>
        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones seg�n nuestra conveniencia.
            PreRender();

            ship.Render();
            //Dibuja un texto por pantalla
            DrawText.drawText("Con la tecla F se dibuja el bounding box.", 0, 20, Color.OrangeRed);
            DrawText.drawText("Con clic izquierdo subimos la camara [Actual]: " + TGCVector3.PrintVector3(Camara.Position), 0, 30, Color.OrangeRed);

            //Siempre antes de renderizar el modelo necesitamos actualizar la matriz de transformacion.
            //Debemos recordar el orden en cual debemos multiplicar las matrices, en caso de tener modelos jer�rquicos, tenemos control total.
            //Box.Transform = TGCMatrix.Scaling(Box.Scale) * TGCMatrix.RotationYawPitchRoll(Box.Rotation.Y, Box.Rotation.X, Box.Rotation.Z) * TGCMatrix.Translation(Box.Position);
            //A modo ejemplo realizamos toda las multiplicaciones, pero aqu� solo nos hacia falta la traslaci�n.
            //Finalmente invocamos al render de la caja
            Box1.Render();
            Box2.Render();
            Box3.Render();

            //Cuando tenemos modelos mesh podemos utilizar un m�todo que hace la matriz de transformaci�n est�ndar.
            //Es �til cuando tenemos transformaciones simples, pero OJO cuando tenemos transformaciones jer�rquicas o complicadas.
            //Mesh.UpdateMeshTransform();
            //Render del mesh
            //Mesh.Render();

            //Render de BoundingBox, muy �til para debug de colisiones.
            if (BoundingBox)
            {
                Box1.BoundingBox.Render();
                ship.BoundingBox.Render();
            }

            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }

        /// <summary>
        ///     Se llama cuando termina la ejecuci�n del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gr�ficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            //Dispose de la caja.
            Box1.Dispose();
            Box2.Dispose();
            Box3.Dispose();
            //Dispose del mesh.
            ship.Dispose();
        }
    }
}