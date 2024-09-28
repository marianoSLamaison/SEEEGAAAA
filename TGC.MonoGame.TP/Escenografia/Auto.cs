using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using BepuPhysics.Collidables;
using BepuPhysics;
using Control;
using System;


namespace Escenografia
{
    struct LimBox
    {
        public Vector3 minVertice;
        public Vector3 maxVertice;
        public LimBox(Vector3 minVertice, Vector3 maxVertice)
        {
            this.minVertice = minVertice;
            this.maxVertice = maxVertice;
        }
    }

    /// <summary>
    /// Lo separe de Escenografia 3D para poder tener la Posicion ligada a nuestro objeto
    /// </summary>
    abstract class Auto 
    {
        //ralacionadas con movimiento
        /// <summary>
        /// Este es el handler del cuerpo, hay mas cosas a demas de la ref a si que nos lo quedamos por si acaso
        /// </summary>
        protected BodyHandle handlerCuerpo;
        /// <summary>
        /// esta es la referencia al cuerpo, con esto es con lo que aplicamos fuerzas y demas
        /// </summary>
        protected BodyReference refACuerpo;
        /// <summary>
        /// Esto es para ligar la posicion con la que trabaja Bepu a nuestro modelo visible
        /// No tengo idea como valla a reaccionar Bepu si ustedes simplemente le setean una posicion a si que solo pueden consultarla
        /// </summary>
        public Vector3 Posicion { get{return AyudanteSimulacion.NumericsToMicrosofth(refACuerpo.Pose.Position);}}
        /// <summary>
        /// Aproximo vamos a tener que usarla en algun momento para rotar bien el auto cuando este pegue saltos
        /// </summary>
        public System.Numerics.Quaternion orientacion  { get{ return refACuerpo.Pose.Orientation;}}
        /// <summary>
        /// Para mover al auto
        /// </summary>
        protected float fuerzaDireccional;
        protected float velocidadGiro;
        //protected float peso;
        protected bool estaSaltando = false;

        protected float velocidadVertical = 0f;
        protected Vector3 direccion;

        protected float rotacionRuedasDelanteras;
        protected float revolucionDeRuedas;


     ///////Cosas de textureo//////////////   
        protected List<Texture2D> Textures {get;set;} 

        protected Model modelo; 
        protected Effect efecto;
        protected Texture2D baseColorTexture;
        protected Texture2D normalTexture;
        protected Texture2D metallicTexture;
        protected Texture2D roughnessTexture;
        protected Texture2D aoTexture;
        protected Texture2D emissionTexture;

        protected Vector3 posicionRuedaDelanteraIzquierda => new Vector3(-0.5f, -0.2f, 1.0f); // Ajusta según tu modelo
        protected Vector3 posicionRuedaDelanteraDerecha => new Vector3(0.5f, -0.2f, 1.0f);
        protected Vector3 posicionRuedaTraseraIzquierda => new Vector3(-0.5f, -0.2f, -1.0f);
        protected Vector3 posicionRuedaTraseraDerecha => new Vector3(0.5f, -0.2f, -1.0f);



        //para limitar el movimiento de objetos
        //esto es una constante
        static protected Vector3 esquinaInferiorEsc = new Vector3(1f,0f,1f) * -10000f;
        static protected Vector3 esquinaSuperiorEsc = new Vector3(1f, 0f, 1f) * 10000f;
        //vector unitario

        //esto lo implementan los hijos de la clase
        abstract public void mover(float fuerzaAAplicar);
        abstract public Matrix getWorldMatrix();
        abstract public void loadModel(string direccionModelo, string direccionEfecto, ContentManager contManager);
        abstract public void dibujar(Matrix view, Matrix projection, Color color);
        /// <summary>
        /// se encarga de asignar un cuerpo para el auto, siempre hara que no duerma por que 
        /// Bepu pone a "dormir" todo lo que no este moviendose y por culpa de eso, luego no puedes moverlo
        /// Sigue siendo algo con lo que puedes chocar, solo que no puedes aplicarle impulso
        /// con las funciones apply linear impulsa, y bepu no se molestara en moverlo
        /// </summary>
        /// <param name="handler"> es solo el handler del objeto </param>
        public void darCuerpo(BodyHandle handler)
        {
            handlerCuerpo = handler;
            refACuerpo = AyudanteSimulacion.getRefCuerpoDinamico(handler);
            refACuerpo.Activity.SleepThreshold = -1;//esto es lo que permite que el objeto no sea 
                                                    //puesto a dormir
                                                    //valores negativos lo haceno No durmiente
                                                    //valores positivos solo le dan un tiempo hasta que duerma
        }

    }

    class AutoJugador : Auto
    {   
        float rotacionX,rotacionY,rotacionZ;

        public AutoJugador(Vector3 direccion, float velocidadGiro, float fuerzaDireccional)
        {
            this.direccion = direccion;
            this.velocidadGiro = velocidadGiro;
            this.fuerzaDireccional = fuerzaDireccional;
        }
        public void setVelocidadGiro(float velocidadGiro)
        {
            this.velocidadGiro = velocidadGiro;
        }

        public override Matrix getWorldMatrix() => Matrix.CreateRotationY(rotacionY) * Matrix.CreateTranslation(Posicion);

        public override void loadModel(string direccionModelo, string direccionEfecto, ContentManager contManager){
            //asignamos el modelo deseado
            modelo = contManager.Load<Model>(direccionModelo);
            //mismo caso para el efecto
            efecto = contManager.Load<Effect>(direccionEfecto);

            // Cargar texturas específicas
            baseColorTexture = contManager.Load<Texture2D>("Models/Auto/" + "Vehicle_basecolor_0");
            normalTexture = contManager.Load<Texture2D>("Models/Auto/" + "Vehicle_normal");
            metallicTexture = contManager.Load<Texture2D>("Models/Auto/" + "Vehicle_metallic");
            roughnessTexture = contManager.Load<Texture2D>("Models/Auto/" + "Vehicle_rougness");
            aoTexture = contManager.Load<Texture2D>("Models/Auto/" + "Vehicle_ao");
            emissionTexture = contManager.Load<Texture2D>("Models/Auto/" + "Vehicle_emission");

            this.ApplyTexturesToShader();

            // Asignar el shader a cada parte del modelo
            foreach (ModelMesh mesh in modelo.Meshes)
            {   
                //Console.WriteLine(mesh.Name);
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = efecto;
                }
            }
        }

        public void ApplyTexturesToShader()
        {
            efecto.Parameters["SamplerType+BaseColorTexture"].SetValue(baseColorTexture);
            //efecto.Parameters["SamplerType+NormalTexture"].SetValue(normalTexture);
            //efecto.Parameters["SamplerType+MetallicTexture"].SetValue(metallicTexture);
            //efecto.Parameters["SamplerType+RoughnessTexture"].SetValue(roughnessTexture);
            //efecto.Parameters["SamplerType+AOTexture"].SetValue(aoTexture);
            //efecto.Parameters["SamplerType+EmissionTexture"].SetValue(emissionTexture);
        }

        public override void dibujar(Matrix view, Matrix projection, Color color)
        {
            efecto.Parameters["View"].SetValue(view);
            // le cargamos el como quedaria projectado en la pantalla
            efecto.Parameters["Projection"].SetValue(projection);

            foreach( ModelMesh mesh in modelo.Meshes)
            {
                if(mesh.Name == "Car")
                    efecto.Parameters["World"].SetValue(mesh.ParentBone.Transform * getWorldMatrix());

                if (mesh.Name.StartsWith("Wheel"))
                {
                    Vector3 posicionRueda = Vector3.Zero;
                    float rotacionYRueda = 0f;

                    // Determinar la posición de la rueda según su nombre
                    if (mesh.Name == "WheelB") {// Rueda delantera izquierda
                        posicionRueda = posicionRuedaDelanteraIzquierda;
                        rotacionYRueda = rotacionRuedasDelanteras;
                    }
                    else if (mesh.Name == "WheelA"){ // Rueda delantera derecha
                        posicionRueda = posicionRuedaDelanteraDerecha;
                        rotacionYRueda = rotacionRuedasDelanteras;
                    }
                    else if (mesh.Name == "WheelD") {
                        // Rueda trasera izquierda
                        posicionRueda = posicionRuedaTraseraIzquierda;
                        rotacionYRueda = 0;
                    }
                    else if (mesh.Name == "WheelC"){ // Rueda trasera derecha
                        posicionRueda = posicionRuedaTraseraDerecha;
                        rotacionYRueda = 0;
                    }
                    // Calcular la matriz de transformación para la rueda
                    Matrix wheelWorld = Matrix.CreateRotationY(rotacionY) * // cargamos su rotacion con respecto del eje XZ con respecto del auto
                                        Matrix.CreateTranslation(Posicion + posicionRueda); // cargamos su posicion con respcto del auto
        
                    efecto.Parameters["World"].SetValue(Matrix.CreateRotationX(revolucionDeRuedas) * //primero la rotamos sobre su propio eje 
                                                        Matrix.CreateRotationY(rotacionYRueda ) * // segundo la rotamos sobre el plano XZ
                                                        mesh.ParentBone.Transform * // luego la hacemos heredar la transformacion del padre
                                                        wheelWorld); // pos ultimo
                }
                mesh.Draw();    
            }
        }
/*
        public override void mover(float deltaTime)
        {
            //incremento de velocidad
            if ( Keyboard.GetState().IsKeyDown(Keys.S))
            {
                velocidad -= aceleracion * deltaTime;
            }
            else if ( Keyboard.GetState().IsKeyDown(Keys.W))
            {
                velocidad += aceleracion * deltaTime;
            }
                //la velocidad siempre se reducira por algun factor, en este caso por 4%
            else 
                velocidad *= 0.96f;
            //los elvis operators/ ifinlines / ternaris. Estan solo para que el auto se mueva como un auto de verdad
            if ( Keyboard.GetState().IsKeyDown(Keys.A))
            {
                //rotacionY += (velocidad >= 0 ? velocidadGiro : -velocidadGiro) * deltaTime;
                rotacionRuedasDelanteras += (velocidad >= 0 ? velocidadGiro : -velocidadGiro) * deltaTime;
            }
            if ( Keyboard.GetState().IsKeyDown(Keys.D))
            {
                //rotacionY += (velocidad >= 0 ? -velocidadGiro : velocidadGiro) * deltaTime;
                rotacionRuedasDelanteras += (velocidad >= 0 ? -velocidadGiro : velocidadGiro) * deltaTime;
            }


            rotacionX += velocidad * 0.001f;
            float escalarDeDerrape = Math.Clamp(velocidad * 0.000025f, 0.001f, 0.05f);

            //limitamos el giro de las ruedas
            rotacionRuedasDelanteras = (float)Math.Clamp(rotacionRuedasDelanteras, -Math.PI/4, Math.PI/4);
            //reducimos por un 2% su giro
            rotacionRuedasDelanteras *= 0.98f;
            //chequemoa si 
            if(velocidad >= 10f || velocidad <= -10f)
                rotacionY += rotacionRuedasDelanteras * escalarDeDerrape;

            if (estaSaltando)
            {
                // Si está saltando, la velocidad vertical disminuye por gravedad
                altura += velocidadVertical * deltaTime;
                gravedad += 7.5f * deltaTime;
                velocidadVertical -= gravedad;

                // Si cae al suelo, termina el salto
                if (altura <= piso)
                {
                    altura = piso;
                    estaSaltando = false;
                    velocidadVertical = 0f;
                }
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                estaSaltando = true;
                velocidadVertical = velocidadSalto;
                gravedad = 7.5f;
            }
            
            posicion += Vector3.Transform(direccion, Matrix.CreateFromYawPitchRoll(
                rotacionY, 0, 0) ) * velocidad * deltaTime + new Vector3(0, altura, 0);
            velocidad = Math.Clamp(velocidad, -2000f, 2000f);
            posicion = Utils.Matematicas.clampV(posicion, limites.minVertice, limites.maxVertice);
        }
*/
        /// <summary>
        /// Aqui olo recivimos inputs, no aplicamos fuerzas, no cambiamos posiciones
        /// solo seteamos valores para luego mover
        /// </summary>
        /// <param name="deltaTime"></param>
        public void getInputs(float deltaTime)
        {
            float fuerzaDeseada = 0; 
            if ( !estaSaltando )
            {
                //negativo si estamos llendo para atras
                float velocidadDeGiroDefinitiva;
                float velocidadDeGiroInstantanea = 0f;
                if ( Keyboard.GetState().IsKeyDown(Keys.W))
                    fuerzaDeseada += fuerzaDireccional;
                else if ( Keyboard.GetState().IsKeyDown(Keys.S))
                    fuerzaDeseada -= fuerzaDireccional;
                //de la des aceleracion nos encargamos en "mover"

                velocidadDeGiroDefinitiva = fuerzaDireccional >= 0 ? velocidadGiro : -velocidadGiro;
                if ( Keyboard.GetState().IsKeyDown(Keys.A))
                    velocidadDeGiroInstantanea +=  velocidadDeGiroDefinitiva * deltaTime;
                if ( Keyboard.GetState().IsKeyDown(Keys.D))
                    velocidadDeGiroInstantanea -= velocidadDeGiroDefinitiva * deltaTime;
                if ( Keyboard.GetState().IsKeyDown(Keys.Space))
                    estaSaltando = true;

                rotacionRuedasDelanteras += velocidadDeGiroInstantanea * (fuerzaDireccional >= 0 ? 1 : -1);
                const float maximaVelocidadPosible = 5000f;
                float escalarDeDerrape = Math.Abs(fuerzaDeseada / maximaVelocidadPosible);
                //limitamos el giro de las ruedas
                rotacionRuedasDelanteras = (float)Math.Clamp(rotacionRuedasDelanteras, -Math.PI/4, Math.PI/4);
                //si estamos moviendonos, aplicamos rotacion al auto
                rotacionY += velocidadDeGiroInstantanea ;
                /*
                if(fuerzaDireccional != 0f)
                {
                    rotacionY += velocidadDeGiroInstantanea * escalarDeDerrape;
                    revolucionDeRuedas += ((float)Math.PI / 10) *deltaTime;
                }*/
                //reducimos por un 2% su giro
                rotacionRuedasDelanteras *= 0.98f;
                
            } else {
                //solo no hacemos cosas, de la caida se encargara Bepu
            }
            //mover(deltaTime);
            mover( fuerzaDeseada);
        }
        //TODO: Rework del movimiento del auto
        /// <summary>
        /// Esta esta destinada a aplicarle fuerzas al auto
        /// </summary>
        /// <param name="fuerzaDeseada"></param>
        override public void mover( float fuerzaDeseada)
        {
            
            refACuerpo.ApplyLinearImpulse(Vector3.Transform(direccion, Matrix.CreateRotationY(rotacionY)).ToNumerics() * fuerzaDeseada);
            refACuerpo.ApplyAngularImpulse(new System.Numerics.Vector3(0f, 1f, 0f) * fuerzaDeseada * 1500f);
        }
    /*
        override public void mover(float deltaTime)
        {
            const float G = -500.5f;
            velocidadVertical += G * deltaTime ;
            altura += velocidadVertical * deltaTime;
            altura = Math.Clamp(altura, 0, limites.maxVertice.Y);
            //posicion += Vector3.Transform(direccion, Matrix.CreateRotationY(rotacionY)) * velocidad * deltaTime;
            posicion.Y = altura;
            posicion = Utils.Matematicas.clampV(posicion, limites.minVertice, limites.maxVertice);
            //limitamos la rotacion para que no ocurra que te quedas girando en un lado por ciempre
            rotacionY = Convert.ToSingle(Utils.Matematicas.wrapf(rotacionY, 0, Math.Tau));
        }
    */
    }

    class JugadorColisionable 
    {
        float fuerza;
        Primitiva figuraAsociada;
        public System.Numerics.Vector3 Posicion { get { return figuraAsociada.Posicion;}}

        public void setForma(Vector3 minVer, Vector3 maxVer, Vector3 posInicial)
        {
            figuraAsociada = Primitiva.Prisma(minVer, maxVer);
            figuraAsociada.setearCuerpoPrisma( minVer,maxVer,posInicial);
            fuerza = 500f;
        }

        public void dibujar(Camarografo camarografo)
        {
            figuraAsociada.dibujar(camarografo, Posicion);
        }

        public void getInputs(float deltaTime)
        {
            //aplicamos la fuerza al auto
            float fuerzaInstantanea = fuerza * deltaTime;
            
            //AdminFisicas.AplicarFuerzaLineal(Vector3.Backward.ToNumerics() * 0.5f,figuraAsociada.referenciaCuerpo);
            if ( Keyboard.GetState().IsKeyDown(Keys.W))
            {
                AyudanteSimulacion.AplicarFuerzaLineal(Vector3.Backward.ToNumerics() * fuerzaInstantanea, figuraAsociada.handlerCuerpo);
                Console.WriteLine("Retrocediendo " + Posicion);
            }
            else if ( Keyboard.GetState().IsKeyDown(Keys.S))
            {
                AyudanteSimulacion.AplicarFuerzaLineal(Vector3.Forward.ToNumerics() * fuerzaInstantanea, figuraAsociada.handlerCuerpo);
                Console.WriteLine("Avanzando " + Posicion);
            }
            
                     
        }
        public void loadJugador(GraphicsDevice graphics, Effect efecto)
        {
            figuraAsociada.loadPrimitiva(graphics, efecto, Color.Black);
        }
        
        public void mover(float deltaTime)
        {
            getInputs(deltaTime);
        }
    }
    class AutoNPC : Auto
    {
        public override void dibujar(Matrix view, Matrix projection, Color color)
        {
            throw new NotImplementedException();
        }

        public override Matrix getWorldMatrix()
        {
            throw new NotImplementedException();
        }

        public override void loadModel(string direccionModelo, string direccionEfecto, ContentManager contManager)
        {
            throw new NotImplementedException();
        }

        public override void mover( float fuerzaAAplicar)
        {
            throw new NotImplementedException();
        }
    }

}