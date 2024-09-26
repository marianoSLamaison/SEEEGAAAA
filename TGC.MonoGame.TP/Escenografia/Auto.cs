using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Data;
using System.Collections.Generic;

namespace Escenografia
{
    struct Box
    {
        public Vector3 minVertice;
        public Vector3 maxVertice;
        public Box(Vector3 minVertice, Vector3 maxVertice)
        {
            this.minVertice = minVertice;
            this.maxVertice = maxVertice;
        }
    }

    abstract class Auto : Escenografia3D
    {
        //ralacionadas con movimiento
        protected float velocidad;
        protected float aceleracion;
        protected float velocidadGiro;
        //protected float peso;
        protected bool estaSaltando = false;

        protected float velocidadVertical = 0f;
        protected float altura = 0f;
        protected const float velocidadSalto = 500f;

        protected const float maximaVelocidadPosible = 2536f;

        protected Box limites;
        protected Vector3 direccion;


     ///////Cosas de textureo//////////////   
        protected List<Texture2D> Textures {get;set;} 

        protected Texture2D baseColorTexture;
        protected Texture2D normalTexture;
        protected Texture2D metallicTexture;
        protected Texture2D roughnessTexture;
        protected Texture2D aoTexture;
        protected Texture2D emissionTexture;

        protected float rotacionRuedasDelanteras;
        protected float revolucionDeRuedas;

        protected Vector3 posicionRuedaDelanteraIzquierda = new Vector3(-0.5f, -0.2f, 1.0f); // Ajusta según tu modelo
        protected Vector3 posicionRuedaDelanteraDerecha = new Vector3(0.5f, -0.2f, 1.0f);
        protected Vector3 posicionRuedaTraseraIzquierda = new Vector3(-0.5f, -0.2f, -1.0f);
        protected Vector3 posicionRuedaTraseraDerecha = new Vector3(0.5f, -0.2f, -1.0f);



        //para limitar el movimiento de objetos
        //esto es una constante
        static protected Vector3 esquinaInferiorEsc = new Vector3(1f,0f,1f) * -10000f;
        static protected Vector3 esquinaSuperiorEsc = new Vector3(1f, 0f, 1f) * 10000f;
        //vector unitario

        //esto lo implementan los hijos de la clase
        abstract public void mover(float deltaTime);

    }

    class AutoJugador : Auto
    {

        public AutoJugador(Vector3 posicion, Vector3 direccion)
        {
            this.posicion = posicion;
            this.direccion = direccion;
        }
        public AutoJugador(Vector3 posicion, Vector3 direccion, float aceleracion, float velocidadGiro)
        {
            this.direccion = direccion;
            this.posicion = posicion;
            this.aceleracion = aceleracion;
            this.velocidadGiro = velocidadGiro;
        }
        public void setLimites(Vector3 minLim, Vector3 maxLim)
        {
            limites = new Box(minLim, maxLim);
        }
        public void setAceleracion(float aceleracion)
        {
            this.aceleracion = aceleracion;
        }
        public void setVelocidadGiro(float velocidadGiro)
        {
            this.velocidadGiro = velocidadGiro;
        }

        public override Matrix getWorldMatrix()
        {
            return Matrix.CreateFromYawPitchRoll(rotacionY, 0, rotacionZ) * Matrix.CreateTranslation(posicion);
        }


        public override void loadModel(string direccionModelo, string direccionEfecto, ContentManager contManager){
            base.loadModel(direccionModelo, direccionEfecto, contManager);

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
                                        Matrix.CreateTranslation(posicion + posicionRueda); // cargamos su posicion con respcto del auto
        
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
        public void getInputs(float deltaTime)
        {
            if ( !estaSaltando )
            {
                //negativo si estamos llendo para atras
                float velocidadDeGiroDefinitiva;
                float velocidadDeGiroInstantanea = 0f;
                if ( Keyboard.GetState().IsKeyDown(Keys.W))
                    velocidad += aceleracion * deltaTime;
                else if ( Keyboard.GetState().IsKeyDown(Keys.S))
                    velocidad -= aceleracion * deltaTime;
                else 
                    velocidad *= 0.96f;

                velocidadDeGiroDefinitiva = velocidad >= 0 ? velocidadGiro : -velocidadGiro;
                if ( Keyboard.GetState().IsKeyDown(Keys.A))
                    velocidadDeGiroInstantanea +=  velocidadDeGiroDefinitiva * deltaTime;
                if ( Keyboard.GetState().IsKeyDown(Keys.D))
                    velocidadDeGiroInstantanea -= velocidadDeGiroDefinitiva * deltaTime;
                if ( Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    velocidadVertical = velocidadSalto;
                    estaSaltando = true;
                }
                rotacionRuedasDelanteras += velocidadDeGiroInstantanea * (velocidad >= 0 ? 1 : -1);
                //necesito una explicacion de esto despues
                float escalarDeDerrape = Math.Abs(velocidad / maximaVelocidadPosible);
                //limitamos el giro de las ruedas
                rotacionRuedasDelanteras = (float)Math.Clamp(rotacionRuedasDelanteras, -Math.PI/4, Math.PI/4);
                //si estamos moviendonos, aplicamos rotacion al auto
                if(velocidad > 10f || velocidad < -10f)
                {
                    rotacionY += velocidadDeGiroInstantanea * escalarDeDerrape;
                    revolucionDeRuedas += ((float)Math.PI / 10) *deltaTime;
                }
                //reducimos por un 2% su giro
                rotacionRuedasDelanteras *= 0.98f;
                
            } else {
                if (altura == 0)
                {
                    estaSaltando = false;
                }
            }
            mover(deltaTime);
        }
        override public void mover(float deltaTime)
        {
            const float G = -500.5f;
            velocidadVertical += G * deltaTime ;
            altura += velocidadVertical * deltaTime;
            altura = Math.Clamp(altura, 0, limites.maxVertice.Y);
            posicion += Vector3.Transform(direccion, Matrix.CreateRotationY(rotacionY)) * velocidad * deltaTime;
            posicion.Y = altura;
            posicion = Utils.Matematicas.clampV(posicion, limites.minVertice, limites.maxVertice);
            //limitamos la rotacion para que no ocurra que te quedas girando en un lado por ciempre
            rotacionY = Convert.ToSingle(Utils.Matematicas.wrapf(rotacionY, 0, Math.Tau));
        }
    }
    

    class AutoNPC : Auto
    {
        public Color color;
        public AutoNPC(Vector3 posicion)
        {
            this.posicion = posicion; 
        }
        public AutoNPC(Vector3 posicion, float rotacionX, float rotacionY, float rotacionZ)
        {
            this.posicion = posicion;
            this.rotacionX = rotacionX;
            this.rotacionY = rotacionY;
            this.rotacionZ = rotacionZ;
        }
        public AutoNPC(Vector3 posicion, float rotacionX, float rotacionY, float rotacionZ, Color color)
        {
            this.posicion = posicion;
            this.rotacionX = rotacionX;
            this.rotacionY = rotacionY;
            this.rotacionZ = rotacionZ;
            this.color = color;
        }
        public override Matrix getWorldMatrix()
        {
            return Matrix.CreateFromYawPitchRoll(rotacionY,rotacionX,rotacionZ) * Matrix.CreateTranslation(this.posicion);
        }

        public override void mover(float deltaTime)
        {
            throw new System.NotImplementedException();
        }
        public override void loadModel(string direcionModelo, string direccionEfecto, ContentManager contManager)
        {
            base.loadModel(direcionModelo, direccionEfecto, contManager);
            foreach ( ModelMesh mesh in modelo.Meshes )
            {
                foreach ( ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = efecto;
                }
            }
        }
    }
}