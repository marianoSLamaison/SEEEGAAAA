using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Data;
using System.Collections.Generic;
using BepuPhysics.Collidables;


namespace Escenografia
{

    class CombatVehicle : Escenografia3D
    {
    protected float velocidad;
        protected float aceleracion;
        protected float velocidadGiro;
        //protected float peso;
        protected bool estaSaltando = false;

        protected float velocidadVertical = 0f;
        protected float altura = 0f;
        protected const float velocidadSalto = 980f;        //Antes era 500f
        protected const float maximaVelocidadPosible = 500f; //Antes era 2536f;

        protected Box limites;
        protected Vector3 direccion;


     ///////Cosas de textureo//////////////   
        protected List<Texture2D> Textures {get;set;} 

        protected Texture2D baseColorTexture;
        protected Texture2D normalTexture;
        protected Texture2D diffuseTexture;

        protected float rotacionRuedasDelanteras;
        protected float revolucionDeRuedas;

        protected Vector3 posicionRuedaDelanteraIzquierda = new Vector3(-0.5f, -0.2f, 1.0f); // Ajusta según tu modelo
        protected Vector3 posicionRuedaDelanteraDerecha = new Vector3(0.5f, -0.2f, 1.0f);
        protected Vector3 posicionRuedaTraseraIzquierda = new Vector3(-0.5f, -0.2f, -1.0f);
        protected Vector3 posicionRuedaTraseraDerecha = new Vector3(0.5f, -0.2f, -1.0f);

        public CombatVehicle(Vector3 posicion, Vector3 direccion, float aceleracion, float velocidadGiro)
        {
            this.direccion = direccion;
            this.posicion = posicion;
            this.aceleracion = aceleracion;
            this.velocidadGiro = velocidadGiro;
        }

    public override Matrix getWorldMatrix()
    {
        return Matrix.CreateFromYawPitchRoll(rotacionY, rotacionX, rotacionZ)* Matrix.CreateTranslation(posicion)*Matrix.CreateScale(0.04f);
    }

     public override void loadModel(string direccionModelo, string direccionEfecto, ContentManager contManager)
     {
            base.loadModel(direccionModelo, direccionEfecto, contManager);

            // Cargar texturas específicas
            baseColorTexture = contManager.Load<Texture2D>("Models/CombatVehicle/" + "Tex_0001_1");
            normalTexture = contManager.Load<Texture2D>("Models/CombatVehicle/" + "Tex_0006_6");
            diffuseTexture = contManager.Load<Texture2D>("Models/CombatVehicle/" + "Tex_0004_4");

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
            //efecto.Parameters["SamplerType+..."].SetValue(diffuseTexture);
        }

        public override void dibujar(Matrix view, Matrix projection, Color color)
        {
            efecto.Parameters["View"].SetValue(view);
            // le cargamos el como quedaria projectado en la pantalla
            efecto.Parameters["Projection"].SetValue(projection);

            foreach( ModelMesh mesh in modelo.Meshes)
            {
                    efecto.Parameters["World"].SetValue(mesh.ParentBone.Transform * getWorldMatrix());
/*
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
                */ 
                mesh.Draw();    
            }
        }




    }
}

