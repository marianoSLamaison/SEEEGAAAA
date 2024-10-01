using System;
using Escenografia;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Control
{
    class Camarografo//laquitoo
    {
        private Control.Camara camaraAsociada;
        private Matrix projeccion;
        private Vector2 FontPos;
        private SpriteFont Font;
        private bool deboDibujarDatos;
        private bool teclaOprimida;
        
        public Camarografo(Vector3 posicion, Vector3 puntoDeFoco, float AspectRatio, float minVista, float maxVista)
        {
            //iniciamos la camara
            camaraAsociada = new Control.Camara(posicion, puntoDeFoco);
            projeccion = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, AspectRatio, minVista, maxVista);
            deboDibujarDatos = false;
            teclaOprimida = false;
        }
        public Matrix getViewMatrix()
        {
            return camaraAsociada.getViewMatrix();
        }
        public Matrix getProjectionMatrix()
        {
            return projeccion;
        }
        public void GetInputs()
        {
            if ( Font == null )
                throw new System.Exception("No puedo escribir sin una fuente");
            if (Keyboard.GetState().IsKeyDown(Keys.O) && !teclaOprimida)
            {
                deboDibujarDatos = !deboDibujarDatos;
                teclaOprimida = true;
            }
            if (Keyboard.GetState().IsKeyUp(Keys.O) && teclaOprimida)
                teclaOprimida = false;
        }
        public void setPuntoAtencion(Vector3 PuntoAtencion)
        {
            camaraAsociada.PuntoAtencion = PuntoAtencion;
        }
        public void DrawDatos(SpriteBatch bathc)
        {

            //despues veo esto
            /*
            if ( deboDibujarDatos )
            {
                bathc.Begin();
                String output = "holaaa";
                bathc.DrawString(Font, output, Vector2.Zero, Color.LightGreen,
        0, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
            }*/
        }
        public void loadTextFont(String CarpetaEfectos, ContentManager contManager)
        {
            Font = contManager.Load<SpriteFont>(CarpetaEfectos + "debugFont");
        }
    }
}