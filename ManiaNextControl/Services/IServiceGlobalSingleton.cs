using ManiaNextControl.Network;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ManiaNextControl.Services
{
    public interface IServiceGlobalSingleton
    {
        Task ControllerLoad(); 
    }
}
