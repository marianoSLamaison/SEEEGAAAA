using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;

namespace Control
{
    
    class Fisicas
    {
        //basicos necesarios para el trabajo del engine
        Simulation simulacion;
        ThreadDispatcher treadDelEngine;
        //listas de objetos controlados por el engine
        Escenografia.Escenografia3D colisionablesActivos;
        Escenografia.Escenografia3D colisionablesInactivos;
        /// <summary>
        /// Aqui vamos a cargar todos los colisionables que tengan que estar desde 0
        /// </summary>
        public void load()
        {
            treadDelEngine = new ThreadDispatcher(1,1);
            
        }
        public void Update(float deltaTime)
        {
            //por favor inicializar el tread antes de todo
            simulacion.Timestep(1/60f,treadDelEngine);

            
        }
    }
}