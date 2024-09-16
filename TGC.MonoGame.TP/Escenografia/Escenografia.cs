using Control;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Escenografia
{
    public abstract class Escenografia3D 
    {
        protected Model modelo; 
        protected Effect efecto;

        public Vector3 posicion;
        protected float rotacionX, rotacionY, rotacionZ;
        /// <summary>
        /// Usado para obtener la matriz mundo de cada objeto
        /// </summary>
        /// <returns>La matriz "world" asociada al objeto que llamo</returns>
        abstract public Matrix getWorldMatrix();
        /// <summary>
        /// Inicializa un modelo junto a sus efectos dado una direccion de archivo para este
        /// </summary>
        /// <param name="direcionModelo"> Direccion en el sistema de archivos para el modelo</param>
        /// <param name="direccionEfecto"> Direccion en el sistema de archivos para el efecto</param>
        /// <param name="contManager"> Content Manager del juego </param>
        /// <remarks> 
        /// Este metodo es virtual, para permitir sobre escribirlo, en caso de que
        /// necesitemos que algun modelo tenga diferentes efectos por mesh
        /// </remarks>
        virtual public void loadModel(String direcionModelo,
                        String direccionEfecto, ContentManager contManager)
        {
            //asignamos el modelo deseado
            modelo = contManager.Load<Model>(direcionModelo);
            //mismo caso para el efecto
            efecto = contManager.Load<Effect>(direccionEfecto);
            //agregamos el efecto deseado a cada parte del modelo
            //por ahora cada modelo, carga una misma textura para todo el modelo
            //luego podemos re escribir esto para hacerlo de otra forma
            //podria mover esta parte a los hijos de la clase y solo dejar la carga generica
            //esto sera aplicado por cada clase hija
            /*
            foreach ( ModelMesh mesh in modelo.Meshes )
            {
                foreach ( ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = efecto;
                }
            }
            */
        }
        /// <summary>+
        /// Funcion para dibujar los modelos
        /// </summary>
        /// <param name="view">la matriz de la camara</param>
        /// <param name="projection">la matriz que define el como se projecta sobre la camara</param>
        /// <param name="color">el color que queremos que tenga el modelo de base</param>
        virtual public void dibujar(Matrix view, Matrix projection, Color color)
        {
            efecto.Parameters["View"].SetValue(view);
            // le cargamos el como quedaria projectado en la pantalla
            efecto.Parameters["Projection"].SetValue(projection);
            // le pasamos el color ( repasar esto )
            efecto.Parameters["DiffuseColor"].SetValue(color.ToVector3());
            foreach( ModelMesh mesh in modelo.Meshes)
            {
                efecto.Parameters["World"].SetValue(mesh.ParentBone.Transform * getWorldMatrix());
                mesh.Draw();
            }
        }
    }
    /// <summary>
    /// Esta es la clase que te permite generar las figuras primitivas
    /// triangulos, cuadrados, circulo, una mesh cuadrada, etc.
    /// No tenemos un poligono general, solo poligonos regulares. 
    /// </summary>
    class Primitiva 
    {
        private GraphicsDevice device;
        private short[] indices;
        private Effect effect;
        private VertexPositionColor[] vertices;
        private Color color;
        private int numeroTriangulos;

        public static Primitiva Triangulo(Vector3 vertice1, Vector3 vertice2, Vector3 vertice3)
        {
            Primitiva ret = new Primitiva();
            ret.vertices = new VertexPositionColor[3];
            ret.vertices[0] = new VertexPositionColor(vertice1, Color.Black);
            ret.vertices[1] = new VertexPositionColor(vertice2, Color.Black);
            ret.vertices[2] = new VertexPositionColor(vertice3, Color.Black);
            ret.indices = new short[] {0, 1, 2};
            ret.numeroTriangulos = 1;
            return ret;
        }
        public static Primitiva Cuad(Vector3 vertice1, Vector3 vertice2, Vector3 vertice3, Vector3 vertice4)
        {
            Primitiva ret = new Primitiva();
            ret.vertices = new VertexPositionColor[4];
            ret.vertices[0] = new VertexPositionColor(vertice1, Color.Black);
            ret.vertices[1] = new VertexPositionColor(vertice2, Color.Black);
            ret.vertices[2] = new VertexPositionColor(vertice3, Color.Black);
            ret.vertices[3] = new VertexPositionColor(vertice4, Color.Black);
            ret.indices = new short[] { 0, 1, 2, 2, 3, 0 };
            ret.numeroTriangulos = 2;
            return ret;
        }

        public static Primitiva RegPoligon(Vector3 centro, int caras, float radio)
        {
            if ( caras < 3 ) throw new Exception("No puedes hacer una figura cerrada con 2 caras rectas.\n");
            float anguloPorCara = Convert.ToSingle(Math.Tau / caras);
            Primitiva ret = new Primitiva();
            int numeroVertices = caras + 1;
            ret.vertices = new VertexPositionColor[numeroVertices];//uno de mas por el centro
            ret.indices = new short[caras * 3];//hay 3 indices por cara ( tienes un triangulo por cara)
            Vector3 vectorDireccion;
            //creamos los vertices del circulo
            ret.vertices[0] = new VertexPositionColor(centro, Color.Black);
            for( int i=1; i < numeroVertices; i++)
            {
                vectorDireccion = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(-anguloPorCara * i)) * radio;
                ret.vertices[i] = new VertexPositionColor(vectorDireccion + centro, Color.Black);
            }
            //cargamos los indices
            for (int i=0 ; i < caras; i++)
            {
                ret.indices[i * 3] = 0;
                ret.indices[i * 3 + 1] = (short)(i + 1);
                ret.indices[i * 3 + 2] = (short)(i < (caras - 1) ? i + 2 : 1);
            }
            ret.numeroTriangulos = caras;
            return ret;
        }

        public void loadPrimitiva(GraphicsDevice device, Effect effect, Color color)
        {
            this.device = device;
            this.effect = effect;
            this.color = color;
        }

        public void dibujar(Camarografo camarografo)
        {
            effect.Parameters["Projection"].SetValue(camarografo.getProjectionMatrix());
            effect.Parameters["View"].SetValue(camarografo.getViewMatrix());
            effect.Parameters["World"].SetValue(Matrix.CreateTranslation(Vector3.Zero) * Matrix.CreateScale(1000f));
            effect.Parameters["DiffuseColor"].SetValue(color.ToVector3());
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, numeroTriangulos);
            }
        }
        public void Dispose()
        {

        } 
    }
}