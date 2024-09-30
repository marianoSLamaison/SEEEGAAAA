﻿using System;
using System.Security.Cryptography.X509Certificates;
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

        private Effect _vehicleCombatShader;
        private Effect _terrenoShader;
        

        //Control.Camera camara;
        Control.Camarografo camarografo;
        Escenografia.AutoJugador auto;

        Escenografia.CombatVehicle combatVehicle;
        Control.AdministradorNPCs generadorPrueba;

        AdministradorConos generadorConos;

        AdministradorNPCs generadorCombatVehicleNPC;

        
        Escenografia.Primitiva cuadrado;


        private Escenografia.Plano _plane { get; set; }
        
        private Terreno terreno;

        private AdminUtileria Escenario;
        private Escenografia.Plataforma _plataforma { get; set;}


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

            generadorConos = new AdministradorConos();
            generadorConos.generarConos(Vector3.Zero, 16000f, 200);

            generadorCombatVehicleNPC = new AdministradorNPCs();
            generadorCombatVehicleNPC.generadorNPCsV3();

            auto = new Escenografia.AutoJugador(Vector3.Zero, Vector3.Backward, 1000f, Convert.ToSingle(Math.PI)/3f);
            auto.setLimites(-new Vector3(1f,1f,1f)*10000f, new Vector3(1f,1f,1f)*10000f);
            //combatVehicle = new Escenografia.CombatVehicle(new Vector3(1f,0f,1f)*2000, Vector3.Backward, 1000f,Convert.ToSingle(Math.PI)/3f);
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
            auto.loadModel(ContentFolder3D + "Auto/RacingCar", ContentFolderEffects + "VehicleShader", Content);
            generadorCombatVehicleNPC.loadCombatVehicles(ContentFolder3D + "CombatVehicle/Vehicle", ContentFolderEffects + "VehicleCombatShader", Content);

            cuadrado = Escenografia.Primitiva.RegPoligon(Vector3.Zero, 32, 1f);

            _basicShader = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
            _vehicleShader = Content.Load<Effect>(ContentFolderEffects + "VehicleShader");
            _vehicleCombatShader = Content.Load<Effect>(ContentFolderEffects + "VehicleCombatShader");
            _terrenoShader = Content.Load<Effect>(ContentFolderEffects + "TerrenoShader");
            _plane.SetEffect(_basicShader);
            
            Plataforma.setGScale(15f);
            Escenario.loadPlataformas(ContentFolder3D+"Plataforma/Plataforma", ContentFolderEffects + "BasicShader", Content);
            
            generadorConos.loadModelosConos(ContentFolder3D + "Cono/Traffic Cone/Models and Textures/1", ContentFolderEffects + "BasicShader", Content);
            
            generadorCombatVehicleNPC.loadModelNPC(ContentFolder3D+"CombatVehicle/Vehicle",ContentFolderEffects+"VehicleCombatShader", Content);

            //terreno.CargarTerreno(ContentFolder3D+"Terreno/height2",Content, 20f);
            //terreno.SetEffect(_basicShader);
            /*Intento crear una variante de terreno con textura que no sea un sólo color.  Modificar en Terreno.dibujar() el Diffuse.*/
            terreno.CargarTerreno(ContentFolder3D+"Terreno/OrangeRockTexture", Content, 200f);
            terreno.SetEffect(_terrenoShader);
            

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
            auto.getInputs(Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds));
            //para que el camarografo nos siga siempre
            camarografo.setPuntoAtencion(auto.posicion);
            
            base.Update(gameTime);
        }

        private float Timer{get;set;}= 0f;
        protected override void Draw(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logia de renderizado del juego.
            GraphicsDevice.Clear(Color.LightBlue);

            auto.dibujar(camarografo.getViewMatrix(), camarografo.getProjectionMatrix(), Color.White);
            //combatVehicle.dibujar(camarografo.getViewMatrix(), camarografo.getProjectionMatrix(), Color.White);
            Escenario.Dibujar(camarografo);
            
            generadorConos.drawConos(camarografo.getViewMatrix(), camarografo.getProjectionMatrix());

            generadorCombatVehicleNPC.drawCombatModel(camarografo.getViewMatrix(), camarografo.getProjectionMatrix(), Color.White);

            terreno.dibujar(camarografo.getViewMatrix(), camarografo.getProjectionMatrix(), Color.DarkGray);
            
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