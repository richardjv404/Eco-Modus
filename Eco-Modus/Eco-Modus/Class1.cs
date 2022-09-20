using System;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

public class Eco_Modus : Script
{
    private Ped playerPed = Game.Player.Character;
    private Vehicle veh = Game.Player.Character.CurrentVehicle;
    private int GameTimeRef = Game.GameTime + 40000;
    int LightToggleTimer;
    bool LightAlwaysOff;
    bool VehicleIsStopped = false;
    public Eco_Modus()
    {

        Tick += OnTick;
    }

    public int GameTimeRef1 { get => GameTimeRef; set => GameTimeRef = value; }
    GTA.Control lightControl = GTA.Control.VehicleHeadlight;

    enum VehicleLightsType
    {
        Normal = 0,
        AlwaysOff = 1,
        AlwaysOn = 2
    }

    void OnTick(object sender, EventArgs e)
    {
        if (VehicleIsStopped)
        {
            if (Game.IsControlPressed(GTA.Control.VehicleAccelerate) || Game.IsControlPressed(GTA.Control.VehicleBrake) || Game.IsControlPressed(GTA.Control.VehicleMoveLeft) || Game.IsControlPressed(GTA.Control.VehicleMoveRight)
                || playerPed.IsRagdoll)
            {
                playerPed.Task.ClearAll();
                VehicleIsStopped = false;
                GameTimeRef1 = Game.GameTime;

                Function.Call(Hash.SET_VEHICLE_ENGINE_ON, veh);
                veh.IsDriveable = true;
            }
        }
        else
        {
            if (playerPed.IsInVehicle())
            {
                veh = playerPed.CurrentVehicle;
                if (CanWeUse(veh))
                    if (veh.Model.IsCar || veh.Model.IsBike)
                    {
                        if (!playerPed.IsStopped) GameTimeRef1 = Game.GameTime;
                        else if (Game.GameTime > GameTimeRef1 + 4000)
                        {
                            VehicleIsStopped = true;
                            Function.Call(Hash.SET_VEHICLE_ENGINE_ON, veh, false);
                            veh.IsDriveable = false;
                        }
                    }
            }

            if (playerPed.IsSittingInVehicle())
            {
                veh = playerPed.CurrentVehicle;

                if (isEngineOn())
                {
                    LightsStateController(veh);
                }

            }

            void LightsStateController(Vehicle v)
            {
                if (Game.IsControlJustPressed(lightControl) && LightToggleTimer < Game.GameTime)
                {
                    LightToggleTimer = Game.GameTime + 800;
                }

                if (Game.IsControlJustReleased(lightControl))
                {
                    LightToggleTimer = 0;
                }

                if (CanSwitchLightState())
                {
                    if (LightAlwaysOff)
                    {
                        SetVehicleLightsType(v, (int)VehicleLightsType.Normal);
                        LightAlwaysOff = false;
                    }
                    else
                    {
                        SetVehicleLightsType(v, (int)VehicleLightsType.AlwaysOn);
                        LightAlwaysOff = true;
                    }
                }
            }

            bool CanSwitchLightState()
            {
                if (Game.IsControlPressed(lightControl) && LightToggleTimer < Game.GameTime + 200 && LightToggleTimer >= Game.GameTime)
                {
                    LightToggleTimer = 0;
                    return true;
                }
                else { return false; }
            }

            bool isEngineOn()
            {
                return Function.Call<bool>(Hash.GET_IS_VEHICLE_ENGINE_RUNNING, veh);
            }

            void SetVehicleLightsType(Vehicle v, int type)
            {
                Function.Call(Hash.SET_VEHICLE_LIGHTS, v, type);
            }

            bool CanWeUse(Entity entity)
            {
                return entity != null && entity.Exists();
            }
        }
    }
}
