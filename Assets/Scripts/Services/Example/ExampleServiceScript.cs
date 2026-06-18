using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Must inherit from MonoService with <T> replaced with the Class Type 
public class ExampleServiceScript : MonoService<ExampleServiceScript>
{

    // We must register the service in Awake or Start
    private void Awake()
    {
        RegisterService(); ///handles Registering the service and accidental Duplicates 
    }


    //If not unregistered elsewhere we want to unregister the service on the Object deletion
    private void OnDestroy()
    {
        UnregisterService();// handles Un-registering the service
    }

    public void TestFunction()
    {
        /*
        Services.Resolve<GaneManagert>().StartGame(); // we call resolve<"Type of service">() to access the service we want if it exists and registered 
        Services.ResolveWhenValid<GameManager>(() => { print("GameStart"); }); //we call ResolveWhenValid and supply a lamba or Action when we want to access a service
                                                                               //but are unsure if it will exist at the time of calling or to queue for it it ready

        */


    }
}
