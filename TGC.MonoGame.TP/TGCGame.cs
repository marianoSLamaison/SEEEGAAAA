using System;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Control;
using Escenografia;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Esta es la clase principal del juego.
    ///     Inicialmente puede ser renombrado o copiado para hacer mas ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
    /// </summary>
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";

        private GraphicsDeviceManager Graphics { get; }
        private SpriteBatch SpriteBatch { get; set; }

        
        private Effect _basicShader;
        private Effect _vehicleShader;
        private Effect _terrenoShader;
        
        private Simulation _simulacion;

        //Control.Camera camara;
        Control.Camarografo camarografo;
        Escenografia.AutoJugador auto;
        AdministradorConos generadorConos;

        //Escenografia.Primitiva cuadrado;


        private Escenografia.Plano _plane { get; set; }
        
        private Terreno terreno;

        private AdminUtileria Escenario;
        Primitiva Colisionable1;
        private Escenografia.Plataforma _plataforma { get; set;}

        private Turbo turboPowerUp;
        

        private BepuPhysics.Collidables.Box _box {get; set;}
        private PrismaRectangularEditable _boxVisual {get; set;}
        private BepuPhysics.Collidables.Box _hitboxAuto {get; set;}
        private BufferPool bufferPool;


        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);
            
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            // Para que el juego sea pantalla completa se puede usar Graphics IsFullScreen.
            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
            // Hace que el mous e sea visible.
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.
            // Apago el backface culling.
            // Esto se hace por un problema en el diseno del modelo del logo de la materia.
            // Una vez que empiecen su juego, esto no es mas necesario y lo pueden sacar.
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState; 

            bufferPool= new BufferPool();

            _simulacion = Simulation.Create(bufferPool, 
                                            new Control.AyudanteSimulacion.NarrowPhaseCallbacks(), 
                                            new Control.AyudanteSimulacion.PoseIntegratorCallbacks(new Vector3(0f, -1000f, 0f).ToNumerics()),
                                            new SolveDescription(5,1));

            AyudanteSimulacion.simulacion = _simulacion;



            auto = new Escenografia.AutoJugador( Vector3.Backward,Convert.ToSingle(Math.PI)/2f, 15f);
            //seteamos una figura para el auto
            Box figuraAuto = new BepuPhysics.Collidables.Box(300f, 250f, 500f);
            TypedIndex referenciaAFigura = _simulacion.Shapes.Add(figuraAuto);
            BodyHandle handlerDeCuerpo = AyudanteSimulacion.agregarCuerpoDinamico(new RigidPose( new Vector3(1f,0.5f,0f).ToNumerics() * 1500f),10f,referenciaAFigura,10f);
            auto.darCuerpo(handlerDeCuerpo);

            Colisionable1 = Primitiva.Prisma(new Vector3(100,100,100),- new Vector3(100,100,100));
            AyudanteSimulacion.agregarCuerpoStatico(new RigidPose(Vector3.UnitZ.ToNumerics() * -500f),
                                    _simulacion.Shapes.Add(new Sphere(100f)));

           var Piso = new Box(1000000, 1, 1000000);
           // Add the plane to the simulation
           AyudanteSimulacion.agregarCuerpoStatico(new RigidPose(new Vector3(0f, -1f, 0f).ToNumerics()), _simulacion.Shapes.Add(Piso));
           var Pared1 = new Box(1, 1000000, 1000000);
           // Add the plane to the simulation
           AyudanteSimulacion.agregarCuerpoStatico(new RigidPose(Vector3.Right.ToNumerics()*10000), _simulacion.Shapes.Add(Pared1));
           var Pared2 = new Box(1, 1000000, 1000000);
           // Add the plane to the simulation
           AyudanteSimulacion.agregarCuerpoStatico(new RigidPose(Vector3.Left.ToNumerics()*10000), _simulacion.Shapes.Add(Pared2));
           var Pared3 = new Box(1000000, 1000000, 1);
           // Add the plane to the simulation
           AyudanteSimulacion.agregarCuerpoStatico(new RigidPose(Vector3.Forward.ToNumerics()*10000), _simulacion.Shapes.Add(Pared3));
           var Pared4 = new Box(1000000, 1000000, 1);
           // Add the plane to the simulation
           AyudanteSimulacion.agregarCuerpoStatico(new RigidPose(Vector3.Backward.ToNumerics()*10000), _simulacion.Shapes.Add(Pared4));


            generadorConos = new AdministradorConos();
            generadorConos.generarConos(Vector3.Zero, 16000f, 200);
            camarografo = new Control.Camarografo(new Vector3(1f,1f,1f) * 1500f,Vector3.Zero, GraphicsDevice.Viewport.AspectRatio, 1f, 6000f);
            Escenario = new AdminUtileria(-new Vector3(1f,0f,1f)*10000f, new Vector3(1f,0f,1f)*10000f);
            _plane = new Plano(GraphicsDevice, new Vector3(-11000, -200, -11000));
            terreno = new Terreno();

            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            String[] modelos = {ContentFolder3D + "Auto/RacingCar"};
            String[] efectos = {ContentFolderEffects + "BasicShader"};
            

            _basicShader = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
            _vehicleShader = Content.Load<Effect>(ContentFolderEffects + "VehicleShader");
            _terrenoShader = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
            _plane.SetEffect(_basicShader);
            
            Plataforma.setGScale(15f);
            Escenario.loadPlataformas(ContentFolder3D+"Plataforma/Plataforma", ContentFolderEffects + "BasicShader", Content);
            
            generadorConos.loadModelosConos(ContentFolder3D + "Cono/Traffic Cone/Models and Textures/1", ContentFolderEffects + "BasicShader", Content);


            terreno.CargarTerreno(ContentFolder3D+"Terreno/height2",Content, 15f);
            terreno.SetEffect(_basicShader);

            auto.loadModel(ContentFolder3D + "Auto/RacingCar", ContentFolderEffects + "VehicleShader", Content);
            Colisionable1.loadPrimitiva(Graphics.GraphicsDevice, _basicShader, Color.DarkCyan);
            
            //_plant = Content.Load<Model>(ContentFolder3D + "Plant/indoor plant_02_fbx/plant");

            //_cono.loadModel(ContentFolder3D + "Cono/Traffic Cone/Models and Textures/1", ContentFolderEffects + "BasicShader", Content);
            //_cono.SetScale(20f);
            //_edificio.SetEffect(_basicShader);

            //_rampa.SetEffect(_basicShader);
            //_rampa.SetRotacion(0f,0f,Convert.ToSingle(Math.PI/2));

            //_cilindro.SetEffect(_basicShader);
            //_cilindro.SetPosition(new Vector3(1000f,0f,2000f));
            //_cilindro.SetScale(100f);
            //_cilindro.SetRotacion(0, Convert.ToSingle(Math.PI/2), Convert.ToSingle(Math.PI/2));

            //_palmera.loadModel(ContentFolder3D + "Palmera/palmera2", ContentFolderEffects + "BasicShader", Content);
            //_palmera.SetPosition(new Vector3(1500f, 0f, 1000f));
            //_palmera.SetScale(0.5f);
            
            terreno.CrearCollider(bufferPool, _simulacion);


            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logica de actualizacion del juego.
            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                //Salgo del juego.
                Exit();
            }
            
            auto.Mover(Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds));
            //para que el camarografo nos siga siempre
            camarografo.setPuntoAtencion(auto.Posicion);
            _simulacion.Timestep(1f/60f);//por ahora corre en el mismo thread que todo lo demas
            base.Update(gameTime);
        }

        private float Timer{get;set;}= 0f;
        protected override void Draw(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logia de renderizado del juego.
            GraphicsDevice.Clear(Color.LightBlue);

            Escenario.Dibujar(camarografo);
            
            generadorConos.drawConos(camarografo.getViewMatrix(), camarografo.getProjectionMatrix());

            terreno.dibujar(camarografo.getViewMatrix(), camarografo.getProjectionMatrix(), Color.DarkGray);

            Colisionable1.dibujar(camarografo, new Vector3(0, 0, -500).ToNumerics());
            
            auto.dibujar(camarografo.getViewMatrix(), camarografo.getProjectionMatrix(), Color.White);
            

            Timer += ((float)gameTime.TotalGameTime.TotalSeconds) % 1f;

        }

        /// <summary>
        ///     Libero los recursos que se cargaron en el juego.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();

            base.UnloadContent();
        }
    }
}